using Unity.Entities;
using UnityEngine;

namespace MyNamespace
{
	public struct Hex
	{
		public HexCoordinates Coords;
		public int Height;
		public Color Color;
	}

	public enum HexTerrainEnum
	{
		White,
		Green,
		Blue,
		Yellow,
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