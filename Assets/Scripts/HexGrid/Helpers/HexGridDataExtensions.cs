﻿using Unity.Collections;
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
			return grid.TryGetNeighbor(hexes, hex, HexHelpers.DirectionCoordinateMap[(int)direction], out neighbor);
		}

		public static bool TryGetNeighbor<T>(this T grid, NativeArray<HexBuffer> hexes, HexBuffer hex, HexCoordinates direction, out HexBuffer neighbor)
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

		private static void AddTriangleColors(HexGridMeshDataWrapper meshDataWrapper, Color c1, Color c2, Color c3)
		{
			meshDataWrapper.Colors.Add(c1);
			meshDataWrapper.Colors.Add(c2);
			meshDataWrapper.Colors.Add(c3);
		}

		private static void AddTriangle(
			HexGridMeshDataWrapper meshWrapper,
			float3 v1,
			float3 v2,
			float3 v3)
		{
			var idx = meshWrapper.Vertices.Length;
			meshWrapper.Vertices.Add(v1);
			meshWrapper.Vertices.Add(v2);
			meshWrapper.Vertices.Add(v3);

			meshWrapper.Normals.AddReplicate(math.normalize(math.cross(v2 - v1, v3 - v1)), 3);
			meshWrapper.Triangles.Add((ushort) idx);
			meshWrapper.Triangles.Add((ushort) (idx + 1));
			meshWrapper.Triangles.Add((ushort) (idx + 2));
		}

		private static void AddTriangles<T>(this T grid, NativeArray<HexBuffer> hexes, HexGridMeshDataWrapper meshWrapper, HexBuffer hex)
		where T : unmanaged, IHexGridData
		{
			var hexOrigin = HexHelpers.GetRelativePosition(HexCoordinates.Zero, hex.Value.Coords, hex.Value.Height);
			for (var i = HexDirection.N; i <= HexDirection.NW; i++)
			{
				AddTriangle(meshWrapper,
					hexOrigin,
					hexOrigin + HexHelpers.GetFirstVertex(i),
					hexOrigin + HexHelpers.GetSecondVertex(i));

				if (i == HexDirection.S)
				{
					AddTriangleColors(meshWrapper, Color.white, Color.red, Color.red);
				}
				else if (grid.TryGetNeighbor(hexes, hex, i, out var neighbor))
				{
					AddTriangleColors(meshWrapper, hex.Color, neighbor.Color, neighbor.Color);
				}
				else
				{
					AddTriangleColors(meshWrapper, hex.Color, hex.Color, hex.Color);
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