using System;
using Unity.Entities;
using UnityEditor.AnimatedValues;

namespace MyNamespace
{
	public struct HexCoordinates : IComponentData, IEquatable<HexCoordinates>
	{
		public int q;
		public int r;
		public int s => -q - r;

		public HexCoordinates(int q, int r)
		{
			this.q = q;
			this.r = r;
		}

		public static HexCoordinates operator +(HexCoordinates a, HexCoordinates b)
		{
			return new HexCoordinates
			{
				q = a.q + b.q,
				r = a.r + b.r
			};
		}

		public static readonly HexCoordinates Zero = new();

		public static readonly HexCoordinates N = new() { q = 0, r = -1 };

		public static readonly HexCoordinates NE = new() { q = 1, r = -1 };

		public static readonly HexCoordinates SE = new() { q = 1, r = 0 };

		public static readonly HexCoordinates S = new() { q = 0, r = 1 };

		public static readonly HexCoordinates SW = new() { q = -1, r = 1 };

		public static readonly HexCoordinates NW = new() { q = -1, r = 0 };

		public bool Equals(HexCoordinates other)
		{
			return q == other.q && r == other.r;
		}

		public override bool Equals(object obj)
		{
			return obj is HexCoordinates other && Equals(other);
		}

		public override int GetHashCode()
		{
			return HashCode.Combine(q, r);
		}
	}
}