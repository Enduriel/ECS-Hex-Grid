using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

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
			return (math.abs(a.Q - b.Q) + math.abs(a.Q + a.R - b.Q - b.R) + math.abs(a.R - b.R)) / 2;
		}

		public static float3 GetRelativePosition(HexCoordinates origin, HexCoordinates target, int elevation = 0)
		{
			return new float3(
				(target.Q - origin.Q) * OuterRadius * 1.5f,
				elevation * Height,
				(target.Q - origin.Q) * InnerRadius + (target.R - origin.R) * InnerRadius * 2
			);
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

		public static Color GetColor(AllowedColor allowedColor)
		{
			return allowedColor switch
			{
				AllowedColor.Green => Color.green,
				AllowedColor.Blue => Color.blue,
				AllowedColor.Yellow => Color.yellow,
				_ => Color.white
			};
		}

		// not a fan of having this here but honestly not sure where to put this
		// so this will do for now
		public static void AddTriangle(
			NativeArray<float3> vertices,
			NativeArray<ushort> triangles,
			NativeArray<float3> normals,
			NativeArray<Color> colors,
			int idx,
			float3 v1,
			float3 v2,
			float3 v3,
			Color color)
		{
			vertices[idx] = v1;
			vertices[idx + 1] = v2;
			vertices[idx + 2] = v3;
			normals[idx + 2] = normals[idx + 1] = normals[idx] = math.normalize(math.cross(v2 - v1, v3 - v1));
			triangles[idx] = (ushort)idx;
			triangles[idx + 1] = (ushort)(idx + 1);
			triangles[idx + 2] = (ushort)(idx + 2);
			colors[idx] = colors[idx + 1] = colors[idx + 2] = color;
		}
	}
}