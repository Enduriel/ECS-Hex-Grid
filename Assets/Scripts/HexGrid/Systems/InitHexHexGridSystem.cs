using System;
using System.Linq;
using MyNamespace.Jobs;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Jobs;
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
            // this must complete before the BeginSimulationEntityCommandBufferSystem
            // is run
            var ecb = SystemAPI.GetSingleton<EndInitializationEntityCommandBufferSystem.Singleton>()
                .CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter();

            new InitHexHexGridJob
            {
                ECB = ecb
            }.ScheduleParallel();
        }
    }
}