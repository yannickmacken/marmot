using System;
using System.Collections.Generic;
using Grasshopper.Kernel;
using Rhino.Geometry;

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

            List<Tuple<string, string>> edges = new List<Tuple<string, string>>();
            foreach (string edge in edgesInput)
            {
                // assuming edge is formatted like "node1-node2"
                string[] nodeNames = edge.Split('-');
                edges.Add(new Tuple<string, string>(nodeNames[0], nodeNames[1]));
            }

            // Assuming Graph is a class defined earlier.
            List<string> rooms = new List<string>();
            Graph myGraph = new Graph(nodesInput, edges, rooms, areasInput);

            DA.SetData(0, myGraph);
        }

        public override GH_Exposure Exposure => GH_Exposure.primary;

        protected override System.Drawing.Bitmap Icon => null;

        public override Guid ComponentGuid => new Guid("f4616a8a-b80b-4b49-9691-5716b7e3680f");
    }
}
