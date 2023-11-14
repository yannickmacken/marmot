using Accord.Math.Optimization;
using Grasshopper.Kernel;
using marmot;
using Newtonsoft.Json;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using static marmot.Helpers;
using Point = Rhino.Geometry.Point;

namespace Marmot
{

	public class PlanMaker : GH_Component
	{
		public PlanMaker()
		  : base("PlanMaker", "PlanMaker",
			  "Create an optimized rectangular plan from a graph with rooms, edges and areas.",
			  "Marmot", "Plan")
		{
		}

		protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
		{
			pManager.AddGenericParameter("Graph", "G", "Graph object", GH_ParamAccess.item);
			pManager.AddRectangleParameter(
				"Boundary", "B",
				"Rectangle for outer boundary of plan",
				GH_ParamAccess.item
				);
			pManager.AddTextParameter("Fixed Rooms", "fR", "Rooms with fixed position", GH_ParamAccess.list);
			pManager.AddTextParameter("Fixed Points", "fP", "Points for position of fixed rooms", GH_ParamAccess.list);
			pManager.AddGenericParameter("PlanSettings", "S", "Advanced PlanSettings", GH_ParamAccess.item);
			pManager[2].Optional = true;
			pManager[3].Optional = true;
			pManager[4].Optional = true;
		}

		protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
		{
			pManager.AddRectangleParameter("Rectangles", "R", "Rectangles representing the rooms", GH_ParamAccess.list);
		}

		protected override void SolveInstance(IGH_DataAccess DA)
		{
			Graph graph = null;
			Rectangle3d inRectangle = new Rectangle3d();
			List<string> fixedRooms = new List<string>();
			List<Point> fixedPoints = new List<Point>();
			Settings settings = null;

			if (!DA.GetData(0, ref graph)) return;
			if (!DA.GetData(1, ref inRectangle)) return;
			if (!DA.GetDataList(2, fixedRooms)) { };
			if (!DA.GetDataList(3, fixedPoints)) { };
			if (!DA.GetData(4, ref settings)) { };

			// Error handling
			if (fixedRooms.Count != fixedPoints.Count)
			{
				throw new Exception(
					"Amount of fixed rooms should be equal to amount of fixed points."
				);
			}
			if (fixedRooms.Count > graph.Nodes.Count)
			{
				throw new Exception(
					"Amount of fixed rooms can not be more than graph nodes."
				);
			}

			// Load dissection data from resource into memory
			int numberOfRooms = graph.Nodes.Count;
			string resourceName = $"marmot.dissections.dissections_{numberOfRooms}_rooms.json";
			var assembly = Assembly.GetExecutingAssembly();
			var stream = assembly.GetManifestResourceStream(resourceName);
			var reader = new StreamReader(stream);
			string dissectionData = reader.ReadToEnd();

			// Parse json with dissection data
			var root = JsonConvert.DeserializeObject<List<Dissection>>(dissectionData);
			if (root == null)
			{
				return;
			}

			// Get geometry inputs
			var xTotal = inRectangle.Width;
			var yTotal = inRectangle.Height;
			var moveVector = new Vector3d(xTotal / 2, yTotal / 2, 0);
			Transform transform = Transform.ChangeBasis(Plane.WorldXY, inRectangle.Plane);
			Transform reverseTransform = transform.TryGetInverse(out Transform inverse) ? inverse : Transform.Unset;
			Vector3d reverseVector = -moveVector;

			// Determine weights
			double relativeFixedRoomWeight;
			double relativeAreaWeight;
			double relativeProportionWeight;
			double minSize;
			if (settings == null)
			{
				relativeFixedRoomWeight = 1.0;
				relativeAreaWeight = 1.0;
				relativeProportionWeight = 1.0;
				minSize = 1.0;
			}
			else
			{
				double fixedRoomWeight = settings.WFixedRooms ?? 1.0;
				double areaWeight = settings.WAreas ?? 1.0;
				double proportionWeight = settings.WProportions ?? 1.0;
				minSize = settings.MinSize ?? 1.0;
				double totalWeight = fixedRoomWeight + areaWeight + proportionWeight;
				relativeFixedRoomWeight = fixedRoomWeight / totalWeight;
				relativeAreaWeight = areaWeight / totalWeight;
				relativeProportionWeight = proportionWeight / totalWeight;
			}

			// Determine fixed rooms
			var roomsFixed = fixedRooms.Any() & fixedPoints.Any();
			if (roomsFixed)
			{
				foreach (var point in fixedPoints)
				{
					point.Transform(transform);
					point.Translate(moveVector);
				}
			}

			// Loop through dissections
			var mappedGraphs = new List<Graph>();
			foreach (var dissection in root)
			{
				// Create new Graph from dissection
				var dissectionGraph = new Graph(
					nodes: dissection.Nodes.Select(node => node.ToString()).ToList(),
					edges: dissection.Edges.Select(
						edge => new Tuple<string, string>(edge[0].ToString(), edge[1].ToString())
					).ToList(),
					rooms: dissection.Rooms.Select(
						room => new Tuple<List<int>, List<int>>(room[0], room[1])
					).ToList()
				);

				// Map requirement graph onto dissection graph
				var mappedGraphsTemp = graph.MapOnto(dissectionGraph);
				mappedGraphs.AddRange(mappedGraphsTemp);

				// Get equivalent graphs from mapped graphs
				foreach (var mappedGraph in mappedGraphsTemp)
				{
					var rotatedGraph = mappedGraph.RotateGraph();
					mappedGraphs.Add(rotatedGraph);
					if (roomsFixed)  // If rooms fixed, orientation matters
					{
						var mirroredGraphs = mappedGraph.MirrorGraph();
						mappedGraphs.AddRange(mirroredGraphs);
					}
				}
			}

			// Raise exception if no solutions found
			if (mappedGraphs.Count < 1)
			{
				throw new Exception(
					"No possible solutions found for requirement graph. Try removing some edges."
				);
			}

			// Starting vals
			double topScore = double.MaxValue;
			Graph topGraph = null;
			List<double> topX = new List<double>();
			List<double> topY = new List<double>();

			// Loop through mapped graphs
			foreach (var mappedGraph in mappedGraphs)
			{
				// Determine starting values of room sizes
				var xSpacing = new List<List<int>>();
				var ySpacing = new List<List<int>>();
				foreach (var room in mappedGraph.Rooms)
				{
					xSpacing.Add(room.Item1);
					ySpacing.Add(room.Item2);
				}
				int xLen = xSpacing.Max(space => space.Last());
				int yLen = ySpacing.Max(space => space.Last());
				List<double> StartingValues = Enumerable.Repeat(xTotal / (xLen + 1), xLen)
					.Concat(Enumerable.Repeat(yTotal / (yLen + 1), yLen)).ToList();

				// Dynamically define objective function to optimize
				double Objective(double[] z)
				{

					// Split values in x and y, subtract remaining length of edge
					List<double> xVals = z.Take(xLen).Concat(new[] { xTotal - z.Take(xLen).Sum() }).ToList();
					List<double> yVals = z.Skip(xLen).Take(yLen).Concat(new[] { yTotal - z.Skip(xLen).Take(yLen).Sum() }).ToList();
					List<double> totalDiff = new List<double>();

					// Add punishment for distance of fixedRoom to fixedRoomPoint
					if (roomsFixed)
					{
						var constraintDistance = ConstraintDistanceRoomToPoint(
							mappedGraph, fixedRooms, fixedPoints, xVals, yVals
							);
						totalDiff.Add(relativeFixedRoomWeight * constraintDistance);
					}

					// Add punishment for stretched proportion of rooms
					for (int i = 0; i < mappedGraph.Rooms.Count; i++)
					{
						var room = mappedGraph.Rooms[i];
						double xDimension = room.Item1.Sum(a => Math.Abs(xVals[a]));
						double yDimension = room.Item2.Sum(a => Math.Abs(yVals[a]));

						totalDiff.Add(relativeAreaWeight * Math.Pow(
							Math.Abs(mappedGraph.Areas[i] - xDimension * yDimension), 2) * 0.1
							);
						totalDiff.Add(relativeProportionWeight * ConstraintProportion(
							xDimension, yDimension
							));
					}

					return totalDiff.Sum();
				}

				// Optimize room sizes
				var optimizer = new NelderMead(numberOfVariables: StartingValues.Count)
				{
					Function = Objective,
				};
				optimizer.Convergence.StartTime = DateTime.Now;
				optimizer.Convergence.MaximumTime = TimeSpan.FromSeconds(10);
				bool success = optimizer.Minimize(StartingValues.ToArray());
				if (!success) { continue; };

				// Unpack values
				double score = optimizer.Value;
				double[] optimized = optimizer.Solution;
				if (score < topScore)
				{
					// Save top option
					topScore = score;
					topX = CalculateSpacing(optimized, 0, xLen, minSize, xTotal);
					topY = CalculateSpacing(optimized, xLen, optimized.Length, minSize, yTotal);
					topGraph = mappedGraph;
				}
			}

			// Draw rectangles for rooms of top solution
			List<Rectangle3d> roomRectangles = new List<Rectangle3d>();
			for (int i = 0; i < topGraph.Nodes.Count; i++)
			{
				Plane originalPlane = Plane.WorldXY;
				Point3d insertionPoint = new Point3d(
					topX.Take(topGraph.Rooms[i].Item1[0]).Sum(),
					topY.Take(topGraph.Rooms[i].Item2[0]).Sum(),
					0
				);
				Plane insertionPlane = new Plane(insertionPoint, originalPlane.Normal);
				double xSize = topX.GetRange(
					topGraph.Rooms[i].Item1[0],
					topGraph.Rooms[i].Item1.Count
					).Sum();
				double ySize = topY.GetRange(
					topGraph.Rooms[i].Item2[0],
					topGraph.Rooms[i].Item2.Count
					).Sum();
				Rectangle3d roomRectangle = new Rectangle3d(
					insertionPlane,
					new Interval(0, xSize),
					new Interval(0, ySize)
					);
				roomRectangle.Transform(
					Transform.Multiply(
						Transform.Translation(reverseVector),
						reverseTransform
						)
					);
				roomRectangles.Add(roomRectangle);
			}

			// Return rectangle list
			DA.SetDataList(0, roomRectangles);
		}

		public override GH_Exposure Exposure => GH_Exposure.primary;

		protected override System.Drawing.Bitmap Icon => null;

		public override Guid ComponentGuid => new Guid("ded68cf8-aeb0-4a18-a2e3-be2c7a7f843b");
	}
}
