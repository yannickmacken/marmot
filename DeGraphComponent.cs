using Grasshopper.Kernel;
using System;
using System.Linq;

namespace Marmot
{
	public class DeGraphComponent : GH_Component
	{
		public DeGraphComponent()
		  : base("DeGraph", "DeGraph",
			  "Deconstruct a graph into its subcomponents.",
			  "Marmot", "Graph")
		{
		}

		protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
		{
			pManager.AddGenericParameter("Graph", "G", "Graph object", GH_ParamAccess.item);
		}

		protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
		{
			pManager.AddTextParameter("Rooms", "R", "Rooms of the graph", GH_ParamAccess.list);
			pManager.AddTextParameter("Edges", "E", "Edges of the graph", GH_ParamAccess.list);
			pManager.AddNumberParameter("Areas", "A", "Areas of the graph", GH_ParamAccess.list);
		}

		protected override void SolveInstance(IGH_DataAccess DA)
		{
			Graph graph = null;

			if (!DA.GetData(0, ref graph)) return;

			// Transforming the list of tuples into the desired format
			var transformedEdges = graph.Edges.Select(
				edge => $"{edge.Item1}-{edge.Item2}"
			).ToList();

			DA.SetDataList(0, graph.Nodes);
			DA.SetDataList(1, transformedEdges);
			DA.SetDataList(2, graph.Areas);
		}

		public override GH_Exposure Exposure => GH_Exposure.primary;

		protected override System.Drawing.Bitmap Icon => null;

		public override Guid ComponentGuid => new Guid("c35ca9bd-e0f0-4b38-9741-e98598f58831");
	}
}
