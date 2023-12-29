using Components;
using MyNamespace.Jobs;
using Unity.Entities;
using Unity.Rendering;
using UnityEngine;

namespace MyNamespace
{
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateAfter(typeof(BeginSimulationEntityCommandBufferSystem))]
    [UpdateBefore(typeof(EndSimulationEntityCommandBufferSystem))]
    public partial struct UpdateHexMeshSystem : ISystem
    {
        private EntityQuery _entityQuery;
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<EndSimulationEntityCommandBufferSystem.Singleton>();
            _entityQuery = SystemAPI.QueryBuilder()
                .WithAll<HexBuffer, MaterialMeshInfo, MeshOutdatedTag>()
                .WithNone<MeshDataArrayComponent>()
                .Build();
            state.RequireForUpdate(_entityQuery);
        }

        public void OnUpdate(ref SystemState state)
        {
            var ecb = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>()
                .CreateCommandBuffer(state.WorldUnmanaged);
            var id = state.World.GetExistingSystemManaged<AssignMeshSystem>().AllocateMeshDataArray(out var meshDataArray, _entityQuery.CalculateEntityCount());

            new CreateHexMeshJob()
            {
                ECB = ecb.AsParallelWriter(),
                MeshDataArrayID = id,
                MeshDataArray = meshDataArray
            }.ScheduleParallel();

            // var ecb2 = SystemAPI.GetSingleton<BeginPresentationEntityCommandBufferSystem.Singleton>()
            //     .CreateCommandBuffer(state.WorldUnmanaged);
            // new CreateHexMeshJob()
            // {
            //     CommandBuffer = ecb2.AsParallelWriter()
            // }.ScheduleParallel(SystemAPI.QueryBuilder().WithAll<UpdateHexMeshTag>().Build(), ecb.);

        }
    }
}