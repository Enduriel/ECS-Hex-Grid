using Unity.Entities;
using UnityEngine;

namespace Trideria.HexGrid
{
	public struct Hex
	{
		public HexCoordinates Coords;
		public int Height;
		public Color Color;
	}

	public enum AllowedColor
	{
		Yellow,
		Green,
		Blue,
		White
	}

	public enum HexTerrainEnum
	{
		White,
		Green,
		Blue,
		Yellow
	}

	public struct HexTerrain
	{
		public HexTerrainEnum Value;

		public static implicit operator HexTerrain(HexTerrainEnum value)
		{
			return new HexTerrain { Value = value };
		}

		public static implicit operator HexTerrainEnum(HexTerrain value)
		{
			return value.Value;
		}
	}

	[InternalBufferCapacity(100)]
	public struct HexBuffer : IBufferElementData
	{
		public Hex Value;

		public HexCoordinates Coords => Value.Coords;
		public Color Color => Value.Color;
		public int Height => Value.Height;

		public HexBuffer(HexCoordinates coords)
		{
			Value = new Hex { Coords = coords };
		}

		public HexBuffer(HexCoordinates coords, Color color)
		{
			Value = new Hex
			{
				Coords = coords,
				Color = color
			};
		}
	}
}