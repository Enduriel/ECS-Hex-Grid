using System;
using Unity.Entities;
using Unity.Mathematics;

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

		public static bool operator ==(HexCoordinates left, HexCoordinates right)
		{
			return left.Equals(right);
		}

		public static bool operator !=(HexCoordinates left, HexCoordinates right)
		{
			return !(left == right);
		}

		private static HexDirection GetDirection(HexCoordinates src, HexCoordinates dest)
		{
			if (src == dest)
			{
				throw new ArgumentException("Source and destination hexes are the same.");
			}

			var absDQ = math.abs(dest.Q - src.Q);
			var absDR = math.abs(dest.R - src.R);
			var absDS = math.abs(dest.S - src.S);
			var maxAbs = math.max(math.max(absDQ, absDR), absDS);

			return maxAbs switch
			{
				_ when maxAbs == absDQ => dest.Q > src.Q ? HexDirection.SE : HexDirection.NW,
				_ when maxAbs == absDR => dest.R > src.R ? HexDirection.S : HexDirection.N,
				_ => dest.S > src.S ? HexDirection.SW : HexDirection.NE
			};
		}

		public HexDirection GetDirection(HexCoordinates destination)
		{
			return GetDirection(this, destination);
		}

		public HexDirection GetDirection()
		{
			return GetDirection(Zero, this);
		}
	}
}