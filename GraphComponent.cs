using Grasshopper.Kernel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Marmot
{
	public class GraphComponent : GH_Component
	{
		public GraphComponent()
		  : base("Graph", "Graph",
			  "Create a graph from given nodes, connections and areas.",
			  "Marmot", "Graph")
		{
		}

		protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
		{
			pManager.AddTextParameter("Nodes", "N", "Nodes of the graph", GH_ParamAccess.list);
			pManager.AddTextParameter("Edges", "E", "Edges of the graph", GH_ParamAccess.list);
			pManager.AddNumberParameter("Areas", "A", "Areas of the graph", GH_ParamAccess.list);
		}

		protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
		{
			pManager.AddGenericParameter("Graph", "G", "Graph object", GH_ParamAccess.item);
		}

		protected override void SolveInstance(IGH_DataAccess DA)
		{
			List<string> nodesInput = new List<string>();
			List<string> edgesInput = new List<string>();
			List<double> areasInput = new List<double>();

			if (!DA.GetDataList(0, nodesInput)) return;
			if (!DA.GetDataList(1, edgesInput)) return;
			if (!DA.GetDataList(2, areasInput)) return;

			// Error handling
			if (nodesInput.Count > 7)
			{
				throw new Exception("Maximum 7 nodes allowed per graph.");
			}
			if (nodesInput.Count > 4 && edgesInput.Count < nodesInput.Count - 2)
			{
				throw new Exception(string.Format(
						"Minimum {0} edges required for graph with {1} nodes.",
						nodesInput.Count - 2, nodesInput.Count
					)
				);
			}
			if (nodesInput.Count != areasInput.Count)
			{
				throw new Exception("Amount of nodes should be equal to areas.");
			}

			// Define graph
			Graph myGraph = new Graph(nodes: nodesInput, areas: areasInput);
			foreach (string edge in edgesInput)
			{

				// Check if string contains one dash
				if (edge.Count(f => f == '-') != 1)
				{
					throw new Exception("Edge should be formatted 'node1-node2'");
				}
				string[] nodeNames = edge.Split('-');

				myGraph.AddEdge(new Tuple<string, string>(nodeNames[0], nodeNames[1]));
			};

			DA.SetData(0, myGraph);
		}

		public override GH_Exposure Exposure => GH_Exposure.primary;

		protected override System.Drawing.Bitmap Icon
		{
			get
			{
				string resourceName = "marmot.Icons.graph.png";
				var assembly = Assembly.GetExecutingAssembly();
				using (var stream = assembly.GetManifestResourceStream(resourceName))
				{
					return new System.Drawing.Bitmap(stream);
				}
			}
		}

		public override Guid ComponentGuid => new Guid("f4616a8a-b80b-4b49-9691-5716b7e3680f");
	}
}
