using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using Unity.Entities;
using Unity.Rendering;
using UnityEngine;

namespace MyNamespace
{
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateBefore(typeof(EndSimulationEntityCommandBufferSystem))]
    public partial struct AssignHexMeshSystem : ISystem
    {
        private EntityQuery _hexGridQuery;
        
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<EndSimulationEntityCommandBufferSystem.Singleton>();
            
            _hexGridQuery = SystemAPI.QueryBuilder()
                .WithAll<MeshDataComponent>()
                .Build();
            state.RequireForUpdate(_hexGridQuery);
        }

        public void OnUpdate(ref SystemState state)
        {
            var ecb = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>()
                .CreateCommandBuffer(state.WorldUnmanaged);
            var system = state.World.GetExistingSystemManaged<EntitiesGraphicsSystem>();
            var meshDataArray = SystemAPI.GetSingleton<MeshDataComponent>().Value;
            // probably not a great approach
            ecb.DestroyEntity(SystemAPI.GetSingletonEntity<MeshDataComponent>());
            int i = -1;
            var meshes = new Mesh[meshDataArray.Length];
            foreach (var (materialMeshInfo, entity) in SystemAPI.Query<RefRW<MaterialMeshInfo>>().WithEntityAccess())
            {
                meshes[++i] = new Mesh();
                materialMeshInfo.ValueRW.MeshID = system.RegisterMesh(meshes[i]);
                // this won't work reliably if there are multiple meshes, the order isn't guaranteed to match up
            }
            Mesh.ApplyAndDisposeWritableMeshData(meshDataArray, meshes);
        }
    }
}