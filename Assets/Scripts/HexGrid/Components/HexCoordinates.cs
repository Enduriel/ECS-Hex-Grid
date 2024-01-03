using System;
using Unity.Entities;

namespace Trideria.HexGrid
{
	public struct HexCoordinates : IComponentData, IEquatable<HexCoordinates>
	{
		public int Q;
		public int R;
		public int S => -Q - R;

		public HexCoordinates(int q, int r)
		{
			Q = q;
			R = r;
		}

		public static HexCoordinates operator +(HexCoordinates a, HexCoordinates b)
		{
			return new HexCoordinates
			{
				Q = a.Q + b.Q,
				R = a.R + b.R
			};
		}

		public static readonly HexCoordinates Zero = new();

		public static readonly HexCoordinates North = new() { Q = 0, R = -1 };

		public static readonly HexCoordinates NorthEast = new() { Q = 1, R = -1 };

		public static readonly HexCoordinates SouthEast = new() { Q = 1, R = 0 };

		public static readonly HexCoordinates South = new() { Q = 0, R = 1 };

		public static readonly HexCoordinates SouthWest = new() { Q = -1, R = 1 };

		public static readonly HexCoordinates NorthWest = new() { Q = -1, R = 0 };

		public bool Equals(HexCoordinates other)
		{
			return Q == other.Q && R == other.R;
		}

		public override bool Equals(object obj)
		{
			return obj is HexCoordinates other && Equals(other);
		}

		public override int GetHashCode()
		{
			return HashCode.Combine(Q, R);
		}
	}
}