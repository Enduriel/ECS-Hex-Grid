using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Rendering;
using UnityEngine;
using UnityEngine.Rendering;

namespace MyNamespace.Jobs
{
    public partial struct InitHexHexGridJob : IJobEntity
    {

        // Output
        public EntityCommandBuffer.ParallelWriter CommandBuffer;
        public Mesh.MeshDataArray MeshDataArray;

        public void Execute([ChunkIndexInQuery] int chunkIdx,[EntityIndexInQuery] int idx, Entity e, ref RenderBounds renderBounds, in HexHexGridData hexGridData)
        {
            CommandBuffer.AddBuffer<HexBuffer>(chunkIdx, e);
            var hexes = CommandBuffer.SetBuffer<HexBuffer>(chunkIdx, e);
            // CommandBuffer.RemoveComponent<RenderMeshArray>(chunkIdx, e);
            InitHexGrid(hexes, hexGridData);
            CreateMesh(hexes, hexGridData, ref renderBounds, idx);
        }
        
        private HexBuffer CreateHex(HexCoordinates coords)
        {
            return new HexBuffer(coords);
        }

        private void InitHexGrid(DynamicBuffer<HexBuffer> hexes, HexHexGridData hexGridData)
        {
            hexes.ResizeUninitialized(HexHelpers.GetNumHexes(hexGridData.Radius));
            var direction = HexCoordinates.NE;
            var currentCoords = new HexCoordinates(0, 0);
            var idx = 0;
            hexes[idx++] = CreateHex(currentCoords);
            for (int i = 0; i < hexGridData.Radius; i++)
            {
                // each edge within cycle
                for (int j = 0; j < 6; j++)
                {
                    // each hex within edge
                    direction = HexHelpers.DirectionMap[direction];
                    for (int k = 0; k < i; k++)
                    {
                        currentCoords += direction;
                        hexes[idx++] = CreateHex(currentCoords);
                    }
                }
                currentCoords += HexCoordinates.N;
            }
        }

        private void CreateMesh(DynamicBuffer<HexBuffer> hexes, HexHexGridData hexHexGridData, ref RenderBounds renderBounds, int idx)
        {
            int vertexAttributeCount = 2;
            var vertexCount = hexes.Length * 18;
            var triangleIndexCount = vertexCount;
            var meshData = MeshDataArray[idx];
            var vertexAttributes = new NativeArray<VertexAttributeDescriptor>(
                vertexAttributeCount, Allocator.Temp, NativeArrayOptions.UninitializedMemory
            );
            vertexAttributes[0] = new VertexAttributeDescriptor(dimension: 3);
            vertexAttributes[1] = new VertexAttributeDescriptor(
			    VertexAttribute.Normal, dimension: 3, stream: 1
		    );
            meshData.SetVertexBufferParams(vertexCount, vertexAttributes);
            var vertices = meshData.GetVertexData<float3>();
            var normals = meshData.GetVertexData <float3>(1);
            
            meshData.SetIndexBufferParams(triangleIndexCount, IndexFormat.UInt16);
            var triangles = meshData.GetIndexData<ushort>();
            
            var baseCoords = new HexCoordinates(0, 0);
            var j = 0;
            foreach (var hexBufferElement in hexes)
            {
                var hexOrigin = HexHelpers.GetRelativePositionFloat3(baseCoords, hexBufferElement.Value.Coords);
                for (int i = 0; i < 6; i++)
                {
                    AddTriangle(
                        vertices,
                        triangles,
                        normals,
                        j + i * 3,
                        hexOrigin,
                        hexOrigin + HexHelpers.Vertices[i],
                        hexOrigin + HexHelpers.Vertices[i + 1]);
                }

                j += 18;
            }
            meshData.subMeshCount = 1;
            var aabb = new AABB()
            {
                Center = float3.zero,
                Extents = HexHelpers.GetMaxDistanceFromCenter(hexHexGridData)
            };
            meshData.SetSubMesh(0, new SubMeshDescriptor(0, triangleIndexCount)
            {
                bounds = aabb.ToBounds()
            }, MeshUpdateFlags.DontRecalculateBounds);

            renderBounds.Value = aabb;
            
             //       vertexAttributes[1] = new VertexAttributeDescriptor(
			   //  VertexAttribute.Normal, dimension: 3, stream: 1
		    // );
		    // vertexAttributes[2] = new VertexAttributeDescriptor(
			   //  VertexAttribute.Tangent, dimension: 4, stream: 2
		    // );
		    // vertexAttributes[3] = new VertexAttributeDescriptor(
			   //  VertexAttribute.TexCoord0, dimension: 2, stream: 3
		    // );
        }
        
        private void AddTriangle(NativeArray<float3> vertices, NativeArray<ushort> triangles, NativeArray<float3> normals, int idx, float3 v1, float3 v2, float3 v3)
        {
            vertices[idx] = v1;
            vertices[idx + 1] = v2;
            vertices[idx + 2] = v3;
            normals[idx + 2] = normals[idx + 1] = normals[idx] = math.normalize(math.cross(v2 - v1, v3 - v1));
            triangles[idx] = (ushort) idx;
            triangles[idx + 1] = (ushort) (idx + 1);
            triangles[idx + 2] = (ushort) (idx + 2);
        }
    }
}