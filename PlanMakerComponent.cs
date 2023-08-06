using System;
using System.Collections.Generic;
using Grasshopper.Kernel;
using marmot;
using Rhino.Geometry;

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
            if (!DA.GetDataList(2, fixedRooms)) {};
            if (!DA.GetDataList(3, fixedPoints)) {};
            if (!DA.GetData(4, ref settings)) {};

            // Component logic .. 

            DA.SetDataList(0, new List<Rectangle3d>());
        }

        public override GH_Exposure Exposure => GH_Exposure.primary;

        protected override System.Drawing.Bitmap Icon => null;

        public override Guid ComponentGuid => new Guid("ded68cf8-aeb0-4a18-a2e3-be2c7a7f843b");
    }
}
