using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace Trideria.HexGrid
{
	public static class HexHelpers
	{
		public const float OuterRadius = 10f;
		public const float InnerRadius = OuterRadius * 0.866025404f;
		public const float Height = 5f;

		public const float SolidFraction = 0.8f;
		public const float BlendFraction = 1f - SolidFraction;

		private static readonly float3[] Vertices =
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

		public static int GetDistance(HexCoordinates a, HexCoordinates b)
		{
			return (math.abs(a.Q - b.Q) + math.abs(a.Q + a.R - b.Q - b.R) + math.abs(a.R - b.R)) / 2;
		}

		public static float3 GetRelativePosition(HexCoordinates origin, HexCoordinates target, int elevation = 0)
		{
			return new float3(
				(target.Q - origin.Q) * OuterRadius * 1.5f,
				elevation * Height,
				- ((target.Q - origin.Q) * InnerRadius + (target.R - origin.R) * InnerRadius * 2)
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

		public static float3 GetFirstVertex(HexDirection direction)
		{
			return Vertices[(int) direction];
		}

		public static float3 GetSecondVertex(HexDirection direction)
		{
			return Vertices[(int) direction + 1];
		}

		public static float3 GetFirstSolidVertex(HexDirection direction)
		{
			return Vertices[(int) direction] * SolidFraction;
		}

		public static float3 GetSecondSolidVertex(HexDirection direction)
		{
			return Vertices[(int) direction + 1] * SolidFraction;
		}

		public static float3 GetBridge (HexDirection direction)
		{
			return (Vertices[(int)direction] + Vertices[(int)direction + 1]) * BlendFraction;
		}
	}
}