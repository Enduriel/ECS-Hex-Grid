namespace Trideria.HexGrid
{
	public enum HexDirection
	{
		N,
		NE,
		SE,
		S,
		SW,
		NW
	}

	public static class HexDirectionExtensions
	{
		public static HexDirection Previous(this HexDirection direction)
		{
			return direction == HexDirection.N ? HexDirection.NW : direction - 1;
		}

		public static HexDirection Next(this HexDirection direction)
		{
			return direction == HexDirection.NW ? HexDirection.N : direction + 1;
		}
	}
}