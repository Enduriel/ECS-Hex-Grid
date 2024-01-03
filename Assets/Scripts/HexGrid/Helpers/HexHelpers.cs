using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Trideria.HexGrid
{
	public static class HexHelpers
	{
		public const float OuterRadius = 1f;
		public const float InnerRadius = OuterRadius * 0.866025404f;
		public const float Height = 0.5f;

		public static readonly float3[] Vertices =
		{
			new(-OuterRadius * 0.5f, 0f, InnerRadius),
			new(OuterRadius * 0.5f, 0f, InnerRadius),
			new(OuterRadius, 0f, 0f),
			new(OuterRadius * 0.5f, 0f, -InnerRadius),
			new(-OuterRadius * 0.5f, 0f, -InnerRadius),
			new(-OuterRadius, 0f, 0f),
			new(-OuterRadius * 0.5f, 0f, InnerRadius)
		};

		public static readonly (ushort, ushort, ushort)[] Triangles =
		{
			(0, 1, 2),
			(0, 2, 3),
			(0, 3, 4),
			(0, 4, 5),
			(0, 5, 6),
			(0, 6, 1)
		};

		public static readonly NativeHashMap<HexCoordinates, HexCoordinates> DirectionMap = new(6, Allocator.Persistent)
		{
			{ HexCoordinates.North, HexCoordinates.NorthEast },
			{ HexCoordinates.NorthEast, HexCoordinates.SouthEast },
			{ HexCoordinates.SouthEast, HexCoordinates.South },
			{ HexCoordinates.South, HexCoordinates.SouthWest },
			{ HexCoordinates.SouthWest, HexCoordinates.NorthWest },
			{ HexCoordinates.NorthWest, HexCoordinates.North }
		};

		public static int GetDistance(HexCoordinates a, HexCoordinates b)
		{
			return (math.abs(a.Q - b.Q)
			        + math.abs(a.Q + a.R - b.Q - b.R)
			        + math.abs(a.R - b.R)) / 2;
		}

		public static int GetNumHexes(int radius)
		{
			return 1 + 3 * radius * (radius - 1);
		}

		public static float3 GetRelativePosition(HexCoordinates origin, HexCoordinates target, int elevation = 0)
		{
			return new float3(
				(target.Q - origin.Q) * OuterRadius * 1.5f,
				elevation * Height,
				(target.Q - origin.Q) * InnerRadius + (target.R - origin.R) * InnerRadius * 2
			);
		}

		// todo improve this function, it's almost certainly overestimating
		public static float3 GetMaxDistanceFromCenter(HexHexGridData hexGridData)
		{
			var x = OuterRadius * 1.5f * (hexGridData.Radius - 1) + OuterRadius * 0.5f;
			var z = InnerRadius * (hexGridData.Radius - 1) + InnerRadius;
			var diag = math.sqrt(x * x + z * z);
			return new float3(diag, 0f, diag);
		}

		public static float3 GetLocalPosition(HexCoordinates coords, int elevation = 0)
		{
			return GetRelativePosition(HexCoordinates.Zero, coords, elevation);
		}

		public static float3 GetWorldPosition(HexCoordinates coords, LocalTransform hexGridTransform, int elevation = 0)
		{
			return math.mul(hexGridTransform.Rotation, GetLocalPosition(coords, elevation)) + hexGridTransform.Position;
		}

		public static int GetHexIdxAtPosition(DynamicBuffer<HexBuffer> buffer, LocalTransform hexGridTransform,
			float3 position)
		{
			var minDistance = float.PositiveInfinity;
			var minIdx = 0;
			for (var i = 0; i < buffer.Length; i++)
			{
				var distance =
					math.distance(GetWorldPosition(buffer[i].Value.Coords, hexGridTransform, buffer[i].Value.Height),
						position);
				if (minDistance > distance)
				{
					minDistance = distance;
					minIdx = i;
				}
			}

			return minIdx;
		}
	}
}