namespace marmot
{
	internal class Settings
	{
		public double? WFixedRooms { get; set; }
		public double? WAreas { get; set; }
		public double? WProportions { get; set; }
		public double? MinSize { get; set; }

		public Settings(
			double? wFixedRooms = null,
			double? wAreas = null,
			double? wProportions = null,
			double? minSize = null
			)
		{
			WFixedRooms = wFixedRooms;
			WAreas = wAreas;
			WProportions = wProportions;
			MinSize = minSize;
		}

		public override string ToString()
		{
			string str = "planSettings";
			return str;
		}
	}
}
