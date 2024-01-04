using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;

namespace Trideria.HexGrid
{
	public struct HexGridMeshDataWrapper
	{
		public NativeArray<float3> Vertices;
		public NativeArray<float3> Normals;
		public NativeArray<Color> Colors;
		public NativeArray<ushort> Triangles;

		public HexGridMeshDataWrapper(UnityEngine.Mesh.MeshData meshData, int numVertices)
		{
			var vertexAttributeCount = 3;
			var vertexCount = numVertices;
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
			Vertices = meshData.GetVertexData<float3>();
			Normals = meshData.GetVertexData<float3>(1);
			Colors = meshData.GetVertexData<Color>(2);

			meshData.SetIndexBufferParams(triangleIndexCount, IndexFormat.UInt16);
			Triangles = meshData.GetIndexData<ushort>();
		}
	}
}