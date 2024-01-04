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
			var idx = 0;
			foreach (var hexCoords in grid.GetHexes())
			{
				hexes[idx++] = new HexBuffer(hexCoords, Color.white);
			}
		}

		private static void AddTriangle(
			HexGridMeshDataWrapper meshWrapper,
			int idx,
			float3 v1,
			float3 v2,
			float3 v3,
			Color color)
		{
			meshWrapper.Vertices[idx] = v1;
			meshWrapper.Vertices[idx + 1] = v2;
			meshWrapper.Vertices[idx + 2] = v3;

			meshWrapper.Normals[idx + 2] = meshWrapper.Normals[idx + 1] = meshWrapper.Normals[idx] = math.normalize(math.cross(v2 - v1, v3 - v1));
			meshWrapper.Triangles[idx] = (ushort)idx;
			meshWrapper.Triangles[idx + 1] = (ushort)(idx + 1);
			meshWrapper.Triangles[idx + 2] = (ushort)(idx + 2);
			meshWrapper.Colors[idx] = meshWrapper.Colors[idx + 1] = meshWrapper.Colors[idx + 2] = color;
		}

		private static void AddTriangles(HexGridMeshDataWrapper meshWrapper, HexBuffer hex, int idx)
		{
			var hexOrigin = HexHelpers.GetRelativePosition(HexCoordinates.Zero, hex.Value.Coords, hex.Value.Height);
			for (var i = HexDirection.N; i <= HexDirection.NW; i++)
			{
				AddTriangle(meshWrapper,
					idx * 18 + (int)i * 3,
					hexOrigin,
					hexOrigin + HexHelpers.GetFirstVertex(i),
					hexOrigin + HexHelpers.GetSecondVertex(i),
					hex.Value.Color);
			}
		}

		public static void FillMeshData<T>(this T grid, DynamicBuffer<HexBuffer> hexes, ref RenderBounds renderBounds,
			UnityEngine.Mesh.MeshData meshData)
			where T : unmanaged, IHexGridData
		{
			var meshWrapper = new HexGridMeshDataWrapper(meshData, hexes.Length * 18);

			var j = 0;
			foreach (var hexBufferElement in hexes)
			{
				AddTriangles(meshWrapper, hexBufferElement, j++);
			}

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

		public static void FillMeshData(this HexGridAspect hexGridAspect, DynamicBuffer<HexBuffer> hexes,
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