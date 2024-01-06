using System;

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

		public static HexDirection Opposite(this HexDirection direction)
		{
			return (int) direction < 3 ? direction + 3 : direction - 3;
		}

		public static HexCoordinates ToCoordinates(this HexDirection direction)
		{
			return direction switch
			{
				HexDirection.N => HexCoordinates.North,
				HexDirection.NE => HexCoordinates.NorthEast,
				HexDirection.SE => HexCoordinates.SouthEast,
				HexDirection.S => HexCoordinates.South,
				HexDirection.SW => HexCoordinates.SouthWest,
				HexDirection.NW => HexCoordinates.NorthWest,
				_ => throw new ArgumentOutOfRangeException(nameof(direction), direction, null)
			};
		}
	}
}
