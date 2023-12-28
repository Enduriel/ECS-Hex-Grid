using System;
using MyNamespace.Jobs;
using Unity.Collections;
using Unity.Entities;
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine;

namespace MyNamespace
{
    [UpdateInGroup(typeof(InitializationSystemGroup))]
    [UpdateBefore(typeof(EndInitializationEntityCommandBufferSystem))]
    public partial struct InitHexHexGridSystem : ISystem
    {
        private EntityQuery _hexGridQuery;
        public void OnCreate(ref SystemState state)
        {
            _hexGridQuery = SystemAPI.QueryBuilder()
                .WithAllRW<HexHexGridData, RenderBounds>()
                .WithNone<HexBuffer>()
                .Build();
            
            state.RequireForUpdate<EndInitializationEntityCommandBufferSystem.Singleton>();
            state.RequireForUpdate(_hexGridQuery);
        }

        public void OnUpdate(ref SystemState state)
        {
            // can be partially done in OnCreate
             var ecb = SystemAPI.GetSingleton<EndInitializationEntityCommandBufferSystem.Singleton>()
                .CreateCommandBuffer(state.WorldUnmanaged);
            // this must complete before the BeginSimulationEntityCommandBufferSystem
            // is run
            var meshDataArray = Mesh.AllocateWritableMeshData(_hexGridQuery.CalculateEntityCount());
            var e = ecb.CreateEntity();
            ecb.AddComponent(e, new MeshDataComponent(meshDataArray));
            new InitHexHexGridJob
            {
                CommandBuffer = ecb.AsParallelWriter(),
                MeshDataArray = meshDataArray
            }.ScheduleParallel(_hexGridQuery);
        }
    }
}