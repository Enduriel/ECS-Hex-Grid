using Components;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using UnityEngine;
using UnityEngine.Rendering;

namespace MyNamespace.Jobs
{
    [BurstCompile]
    public partial struct CreateHexMeshJob : IJobEntity
    {
        public EntityCommandBuffer.ParallelWriter ECB;
        public Mesh.MeshDataArray MeshDataArray;
        public MeshDataArrayID MeshDataArrayID;

        [BurstCompile]
        public void Execute(
            [ChunkIndexInQuery] int chunkIdx,
            [EntityIndexInQuery] int idx,
            ref RenderBounds renderBounds,
            in DynamicBuffer<HexBuffer> hexes,
            in HexHexGridData hexHexGridData,
            Entity e)
        {
            CreateMesh(hexes, hexHexGridData, ref renderBounds, idx);
            ECB.AddComponent(chunkIdx, e, new MeshDataArrayComponent
            {
                ID = MeshDataArrayID,
                Index = idx
            });
            ECB.RemoveComponent<MeshOutdatedTag>(chunkIdx, e);
        }
        
        private void CreateMesh(
            DynamicBuffer<HexBuffer> hexes,
            HexHexGridData hexHexGridData,
            ref RenderBounds renderBounds,
            int meshDataArrayIndex)
        {
            int vertexAttributeCount = 2;
            var vertexCount = hexes.Length * 18;
            var triangleIndexCount = vertexCount;
            var meshData = MeshDataArray[meshDataArrayIndex];
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
                var hexOrigin = HexHelpers.GetRelativePosition(baseCoords, hexBufferElement.Value.Coords, hexBufferElement.Value.Height);
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