using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using UnityEngine;
using UnityEngine.Rendering;

namespace Trideria.HexGrid
{
	public static class HexGridDataExtensions
	{
		public static void Init<T>(this T grid, DynamicBuffer<HexBuffer> hexes)
			where T : unmanaged, IHexGridData
		{
			hexes.ResizeUninitialized(grid.GetNumHexes());
			grid.Init(hexes.AsNativeArray());
		}

		public static void Init<T>(this T grid, NativeArray<HexBuffer> hexes)
			where T : unmanaged, IHexGridData
		{
			var idx = 0;
			foreach (var hexCoords in grid.GetHexes())
			{
				hexes[idx++] = new HexBuffer(hexCoords, Color.white);
			}
		}

		public static bool TryGetNeighbor<T>(this T grid, NativeArray<HexBuffer> hexes, HexBuffer hex,
			HexDirection direction, out HexBuffer neighbor)
			where T : unmanaged, IHexGridData
		{
			return grid.TryGetNeighbor(hexes, hex, direction.ToCoordinates(), out neighbor);
		}

		public static bool TryGetNeighbor<T>(this T grid, NativeArray<HexBuffer> hexes, HexBuffer hex,
			HexCoordinates direction, out HexBuffer neighbor)
			where T : unmanaged, IHexGridData
		{
			var neighborCoords = hex.Coords + direction;
			if (grid.TryGetHexIndex(neighborCoords, out var idx))
			{
				neighbor = hexes[idx];
				if (!neighbor.Coords.Equals(neighborCoords))
				{
					Debug.Log(
						$"Failed {neighborCoords.Q}, {neighborCoords.R}, {neighborCoords.S} -> {neighbor.Coords.Q}, {neighbor.Coords.R}, {neighbor.Coords.S}");
				}

				return true;
			}

			neighbor = default;
			return false;
		}

		private static void AddHexBorder<T>(this T grid, NativeArray<HexBuffer> hexes,
			HexGridMeshDataWrapper meshWrapper, HexBuffer hex, HexDirection direction, float3 v1, float3 v2)
			where T : unmanaged, IHexGridData
		{
			if (!grid.TryGetNeighbor(hexes, hex, direction, out var neighbor))
			{
				return;
			}

			var bridge = HexHelpers.GetBridge(direction);
			var v3 = v1 + bridge;
			var v4 = v2 + bridge;

			meshWrapper.AddQuad(v1, v2, v3, v4);
			meshWrapper.AddQuadColors(hex.Color, neighbor.Color);

			direction = direction.Next();

			if (direction < HexDirection.SE || !grid.TryGetNeighbor(hexes, hex, direction, out var nextNeighbor))
			{
				return;
			}

			meshWrapper.AddTriangle(v2, v4, v2 + HexHelpers.GetBridge(direction));
			meshWrapper.AddTriangleColors(hex.Color, neighbor.Color, nextNeighbor.Color);
		}

		private static void AddTriangles<T>(this T grid, NativeArray<HexBuffer> hexes,
			HexGridMeshDataWrapper meshWrapper, HexBuffer hex)
			where T : unmanaged, IHexGridData
		{
			var vCenter = HexHelpers.GetRelativePosition(HexCoordinates.Zero, hex.Value.Coords, hex.Value.Height);
			for (var i = HexDirection.N; i <= HexDirection.NW; i++)
			{
				var v1 = vCenter + HexHelpers.GetFirstSolidVertex(i);
				var v2 = vCenter + HexHelpers.GetSecondSolidVertex(i);

				meshWrapper.AddTriangle(
					vCenter,
					v1,
					v2);

				meshWrapper.AddTriangleColor(hex.Color);

				if (i < HexDirection.S)
				{
					grid.AddHexBorder(hexes, meshWrapper, hex, i, v1, v2);
				}
			}
		}

		public static void FillMeshData<T>(this T grid, NativeArray<HexBuffer> hexes, ref RenderBounds renderBounds,
			UnityEngine.Mesh.MeshData meshData)
			where T : unmanaged, IHexGridData
		{
			var meshWrapper = new HexGridMeshDataWrapper();
			meshWrapper.Init(hexes.Length * 18, hexes.Length * 18, Allocator.Temp);

			foreach (var hexBufferElement in hexes)
			{
				grid.AddTriangles(hexes, meshWrapper, hexBufferElement);
			}

			meshWrapper.FillMeshData(meshData);

			meshData.subMeshCount = 1;
			var aabb = grid.GetBounds(hexes);
			meshData.SetSubMesh(0, new SubMeshDescriptor(0, meshWrapper.Triangles.Length)
			{
				bounds = aabb.ToBounds()
			}, MeshUpdateFlags.DontRecalculateBounds);

			renderBounds.Value = aabb;
		}

		public static void Init(this HexGridAspect hexGridAspect, DynamicBuffer<HexBuffer> hexes)
		{
			if (hexGridAspect.RectHexGridData.IsValid)
			{
				hexGridAspect.RectHexGridData.ValueRO.Init(hexes);
			}
			else if (hexGridAspect.HexHexGridData.IsValid)
			{
				hexGridAspect.HexHexGridData.ValueRO.Init(hexes);
			}
			else
			{
				Debug.LogError("HexGridData is not valid");
			}
		}

		public static void FillMeshData(this HexGridAspect hexGridAspect, NativeArray<HexBuffer> hexes,
			ref RenderBounds renderBounds, UnityEngine.Mesh.MeshData meshData)
		{
			if (hexGridAspect.RectHexGridData.IsValid)
			{
				hexGridAspect.RectHexGridData.ValueRO.FillMeshData(hexes, ref renderBounds, meshData);
			}
			else if (hexGridAspect.HexHexGridData.IsValid)
			{
				hexGridAspect.HexHexGridData.ValueRO.FillMeshData(hexes, ref renderBounds, meshData);
			}
			else
			{
				Debug.LogError("HexGridData is not valid");
			}
		}
	}
}