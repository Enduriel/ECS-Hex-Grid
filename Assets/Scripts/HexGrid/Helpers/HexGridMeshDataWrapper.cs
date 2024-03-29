﻿using Unity.Collections;
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
				2, Allocator.Temp, NativeArrayOptions.UninitializedMemory
			);
			vertexAttributes[0] = new VertexAttributeDescriptor(dimension: 3);
			// vertexAttributes[1] = new VertexAttributeDescriptor(
			// 	VertexAttribute.Normal, dimension: 3, stream: 1
			// );
			vertexAttributes[1] = new VertexAttributeDescriptor(
				VertexAttribute.Color, dimension: 4, stream: 1
			);
			meshData.SetVertexBufferParams(Vertices.Length, vertexAttributes);
			meshData.SetIndexBufferParams(Triangles.Length, IndexFormat.UInt16);

			meshData.GetVertexData<float3>().CopyFrom(Vertices);
			// meshData.GetVertexData<float3>(1).CopyFrom(Normals);
			meshData.GetVertexData<Color>(1).CopyFrom(Colors);
			meshData.GetIndexData<ushort>().CopyFrom(Triangles);
		}

		public void AddTriangle(int idx, int idx1, int idx2)
		{
			Triangles.Add((ushort) idx);
			Triangles.Add((ushort) idx1);
			Triangles.Add((ushort) idx2);
		}

		public void AddTriangleColors(Color c1, Color c2, Color c3)
		{
			Colors.Add(c1);
			Colors.Add(c2);
			Colors.Add(c3);
		}

		public void AddTriangleColor(Color c)
		{
			AddTriangleColors(c, c, c);
		}

		public void AddTriangle(float3 v1, float3 v2, float3 v3)
		{
			var idx = Vertices.Length;
			Vertices.Add(v1);
			Vertices.Add(v2);
			Vertices.Add(v3);
			// placeholder normals

			// Normals.AddReplicate(math.cross(math.cross(v1, v2), v3), 3);
			AddTriangle(idx, idx + 1, idx + 2);
		}

		public void AddQuadColors(Color c1, Color c2, Color c3, Color c4)
		{
			Colors.Add(c1);
			Colors.Add(c2);
			Colors.Add(c3);
			Colors.Add(c4);
		}

		public void AddQuadColors(Color c1, Color c2)
		{
			AddQuadColors(c1, c1, c2, c2);
		}

		public void AddQuad(float3 v1, float3 v2, float3 v3, float3 v4)
		{
			var idx = Vertices.Length;
			Vertices.Add(v1);
			Vertices.Add(v2);
			Vertices.Add(v3);
			Vertices.Add(v4);

			// Normals.AddReplicate(new float3(0, 1, 0), 4);

			AddTriangle(idx, idx + 2, idx + 1);
			AddTriangle(idx + 1, idx + 2, idx + 3);
		}
	}
}