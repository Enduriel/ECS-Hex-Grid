using Mesh.Components;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Rendering;
using UnityEngine;
using UnityEngine.Rendering;
using MeshCollider = Unity.Physics.MeshCollider;

namespace Mesh.Jobs
{
    [BurstCompile]
    public partial struct SyncColliderWithMeshJob : IJobEntity
    {
        public EntityCommandBuffer.ParallelWriter ECB;
        [ReadOnly] public UnityEngine.Mesh.MeshDataArray MeshDataArray;
        [BurstCompile]
        public void Execute(
            [EntityIndexInQuery] int idx, 
            [ChunkIndexInQuery] int chunkIdx, 
            Entity e,
            in ColliderOutdatedTag _)
        {
            var meshData = MeshDataArray[idx];
            if (meshData.indexFormat == IndexFormat.UInt32)
            {
                Debug.LogError($"Mesh index format is UInt32, but only UInt16 is supported");
                return;
            }
            var indexData = meshData.GetIndexData<ushort>();
            var triangleData = new NativeArray<int3>(indexData.Length / 3, Allocator.Temp);
            for (int i = 0; i < indexData.Length; i+=3)
            {
                triangleData[i / 3] = new int3(indexData[i], indexData[i + 1], indexData[i + 2]);
            }
            
            ECB.AddComponent<PhysicsCollider>(chunkIdx, e);
            ECB.SetComponent(chunkIdx, e, new PhysicsCollider
            {
                Value = MeshCollider.Create(meshData.GetVertexData<float3>(), triangleData, CollisionFilter.Default)
            });
            ECB.RemoveComponent<ColliderOutdatedTag>(chunkIdx, e);
        }
    }
}