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
			var idx = 0;
			foreach (var hexCoords in grid.GetHexes())
			{
				hexes[idx++] = new HexBuffer(hexCoords, Color.white);
			}
		}

		public static void FillMeshData<T>(this T grid, DynamicBuffer<HexBuffer> hexes, ref RenderBounds renderBounds,
			UnityEngine.Mesh.MeshData meshData)
			where T : unmanaged, IHexGridData
		{
			var vertexAttributeCount = 3;
			var vertexCount = hexes.Length * 18;
			var triangleIndexCount = vertexCount;
			var vertexAttributes = new NativeArray<VertexAttributeDescriptor>(
				vertexAttributeCount, Allocator.Temp, NativeArrayOptions.UninitializedMemory
			);
			vertexAttributes[0] = new VertexAttributeDescriptor(dimension: 3);
			vertexAttributes[1] = new VertexAttributeDescriptor(
				VertexAttribute.Normal, dimension: 3, stream: 1
			);
			vertexAttributes[2] = new VertexAttributeDescriptor(
				VertexAttribute.Color, dimension: 4, stream: 2
			);

			meshData.SetVertexBufferParams(vertexCount, vertexAttributes);
			var vertices = meshData.GetVertexData<float3>();
			var normals = meshData.GetVertexData<float3>(1);
			var colors = meshData.GetVertexData<Color>(2);

			meshData.SetIndexBufferParams(triangleIndexCount, IndexFormat.UInt16);
			var triangles = meshData.GetIndexData<ushort>();

			var baseCoords = new HexCoordinates(0, 0);
			var j = 0;
			foreach (var hexBufferElement in hexes)
			{
				var hexOrigin = HexHelpers.GetRelativePosition(baseCoords, hexBufferElement.Value.Coords,
					hexBufferElement.Value.Height);
				for (var i = 0; i < 6; i++)
				{
					HexHelpers.AddTriangle(
						vertices,
						triangles,
						normals,
						colors,
						j + i * 3,
						hexOrigin,
						hexOrigin + HexHelpers.Vertices[i],
						hexOrigin + HexHelpers.Vertices[i + 1],
						hexBufferElement.Value.Color);
				}

				j += 18;
			}

			meshData.subMeshCount = 1;
			var aabb = grid.GetBounds(hexes);
			meshData.SetSubMesh(0, new SubMeshDescriptor(0, triangleIndexCount)
			{
				bounds = aabb.ToBounds()
			}, MeshUpdateFlags.DontRecalculateBounds);

			renderBounds.Value = aabb;
		}
	}
}