using Grasshopper.Kernel;
using marmot;
using System;
using System.Reflection;

namespace Marmot
{
	public class PlanSettingsComponent : GH_Component
	{
		public PlanSettingsComponent()
		  : base("PlanSettings", "PlanSettings",
			  "Provides advanced settings for the planmaker component.",
			  "Marmot", "Plan")
		{
		}

		protected override void RegisterInputParams(GH_InputParamManager pManager)
		{
			pManager.AddNumberParameter(
				"Weight Position",
				"wfR",
				"relative weight of the position of fixed rooms " +
				"- higher weight means that fixed rooms will be closer to fixed positions",
				GH_ParamAccess.item,
				1.0
				);
			pManager.AddNumberParameter(
				"Weight Area",
				"wA",
				"relative weight of the areas of the rooms " +
				"- higher weight means that areas of rooms will match required areas more closely",
				GH_ParamAccess.item,
				1.0
				);
			pManager.AddNumberParameter(
				"Weight Proportion",
				"wP",
				"relative weight of the proportion of the rooms " +
				"- higher weight means that the proportions of the rooms will be more squarish",
				GH_ParamAccess.item,
				1.0
				);
			pManager.AddNumberParameter(
				"Min Wall Length",
				"M",
				"Minimum wall lenght between two connected rooms, 1 by default",
				GH_ParamAccess.item,
				1.0
				);
			pManager.AddIntegerParameter(
				"Timeout duration",
				"T",
				"Maximum time (s) that the planmaker component is allowed to optimize the floorplan" +
				"- a short timeout can lead to less optimal results",
				GH_ParamAccess.item,
				60
				);
			pManager[0].Optional = true;
			pManager[1].Optional = true;
			pManager[2].Optional = true;
			pManager[3].Optional = true;
			pManager[4].Optional = true;
		}


		protected override void RegisterOutputParams(GH_OutputParamManager pManager)
		{
			pManager.AddGenericParameter("Settings", "S", "Advanced settings for planmaker", GH_ParamAccess.item);
		}

		protected override void SolveInstance(IGH_DataAccess DA)
		{
			double? wfR = null;
			double? wA = null;
			double? wP = null;
			double? M = null;
			int? T = null;

			if (!DA.GetData(0, ref wfR)) { };
			if (!DA.GetData(1, ref wA)) { };
			if (!DA.GetData(2, ref wP)) { };
			if (!DA.GetData(3, ref M)) { };
			if (DA.GetData(4, ref T)) { };

			Settings planSettings = new Settings(wfR, wA, wP, M, T);

			DA.SetData(0, planSettings);
		}

		public override GH_Exposure Exposure => GH_Exposure.primary;

		protected override System.Drawing.Bitmap Icon
		{
			get
			{
				string resourceName = "marmot.Icons.planSettings.png";
				var assembly = Assembly.GetExecutingAssembly();
				using (var stream = assembly.GetManifestResourceStream(resourceName))
				{
					return new System.Drawing.Bitmap(stream);
				}
			}
		}

		public override Guid ComponentGuid => new Guid("b51c40ce-3690-4488-8e4b-3073b825146f");
	}
}
