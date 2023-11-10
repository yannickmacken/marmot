using Grasshopper.Kernel;
using System;
using System.Drawing;

namespace marmot
{
    public class marmotInfo : GH_AssemblyInfo
    {
        public override string Name => "marmot";

        //Return a 24x24 pixel bitmap to represent this GHA library.
        public override Bitmap Icon => null;

        //Return a short string describing the purpose of this GHA library.
        public override string Description => "";

        public override Guid Id => new Guid("3f9b3d6f-0430-451c-ae3a-36feb370c5dc");

        //Return a string identifying you or your company.
        public override string AuthorName => "Yannick Macken";

        //Return a string representing your preferred contact details.
        public override string AuthorContact => "";
    }
}