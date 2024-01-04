using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;

namespace Trideria.HexGrid
{
	public struct HexGridMeshDataWrapper
	{
		public NativeList<float3> Vertices;
		public NativeList<float3> Normals;
		public NativeList<Color> Colors;
		public NativeList<ushort> Triangles;

		public void Init(int numVertices, int numTriangles, Allocator allocator)
		{
			Vertices = new NativeList<float3>(numVertices, allocator);
			Normals = new NativeList<float3>(numVertices, allocator);
			Colors = new NativeList<Color>(numVertices, allocator);
			Triangles = new NativeList<ushort>(numTriangles, allocator);
		}

		public void FillMeshData(UnityEngine.Mesh.MeshData meshData)
		{
			var vertexAttributes = new NativeArray<VertexAttributeDescriptor>(
				3, Allocator.Temp, NativeArrayOptions.UninitializedMemory
			);
			vertexAttributes[0] = new VertexAttributeDescriptor(dimension: 3);
			vertexAttributes[1] = new VertexAttributeDescriptor(
				VertexAttribute.Normal, dimension: 3, stream: 1
			);
			vertexAttributes[2] = new VertexAttributeDescriptor(
				VertexAttribute.Color, dimension: 4, stream: 2
			);
			meshData.SetVertexBufferParams(Vertices.Length, vertexAttributes);
			meshData.SetIndexBufferParams(Triangles.Length, IndexFormat.UInt16);

			meshData.GetVertexData<float3>().CopyFrom(Vertices);
			meshData.GetVertexData<float3>(1).CopyFrom(Normals);
			meshData.GetVertexData<Color>(2).CopyFrom(Colors);
			meshData.GetIndexData<ushort>().CopyFrom(Triangles);
		}
	}
}