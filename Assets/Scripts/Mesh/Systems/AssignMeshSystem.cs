using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using Unity.Entities;
using Unity.Rendering;
using UnityEngine;
using UnityEngine.Rendering;
using Debug = System.Diagnostics.Debug;

namespace MyNamespace
{
    public struct MeshDataArrayID
    {
        public int Value;
    }
    
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateBefore(typeof(EndSimulationEntityCommandBufferSystem))]
    public partial class AssignMeshSystem : SystemBase
    {
        private EntityQuery _hexGridQuery;
        
        private List<Mesh.MeshDataArray?> _meshDataArrays;

        protected override void OnCreate()
        {
            RequireForUpdate<EndSimulationEntityCommandBufferSystem.Singleton>();
            _hexGridQuery = SystemAPI.QueryBuilder()
                .WithAllRW<MaterialMeshInfo>()
                .WithAll<MeshDataArrayComponent>()
                .Build();
            RequireForUpdate(_hexGridQuery);
            
            _meshDataArrays = new List<Mesh.MeshDataArray?>();
        }
        
        public MeshDataArrayID AllocateMeshDataArray(out Mesh.MeshDataArray meshDataArray, int size)
        {
            meshDataArray = Mesh.AllocateWritableMeshData(size);
            var idx = _meshDataArrays.FindIndex(x => x == null);
            if (idx == -1)
            {
                _meshDataArrays.Add(meshDataArray);
                idx = _meshDataArrays.Count - 1;
            }
            else
            {
                _meshDataArrays[idx] = meshDataArray;
            }
            return new MeshDataArrayID { Value = idx };
        }
        
        private Mesh.MeshDataArray? UseMeshDataArray(MeshDataArrayID id)
        {
            var value = _meshDataArrays[id.Value];
            _meshDataArrays[id.Value] = null;
            return value;
        }
        
        protected override void OnUpdate()
        {
            var ecb = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>()
                .CreateCommandBuffer(World.Unmanaged);
            var system = World.GetExistingSystemManaged<EntitiesGraphicsSystem>();
            
            var meshDataArrayToMeshMap = new Dictionary<MeshDataArrayID, (Mesh.MeshDataArray, Mesh[])>();
            foreach (var (materialMeshInfo, meshDataArrayComponent, entity)
                     in SystemAPI.Query<RefRW<MaterialMeshInfo>, RefRO<MeshDataArrayComponent>>().WithEntityAccess())
            {
                var meshDataArrayID = meshDataArrayComponent.ValueRO.ID;
                if (!meshDataArrayToMeshMap.ContainsKey(meshDataArrayID))
                {
                    if (UseMeshDataArray(meshDataArrayID) is { } meshDataArray)
                    {
                        meshDataArrayToMeshMap[meshDataArrayID] = (meshDataArray, new Mesh[meshDataArray.Length]);
                    }
                    else
                    {
                        Debug.Fail($"MeshDataArrayID {meshDataArrayID} is invalid");
                    }
                }
                var mesh = new Mesh();
                meshDataArrayToMeshMap[meshDataArrayID].Item2[meshDataArrayComponent.ValueRO.Index] = mesh;
                materialMeshInfo.ValueRW.MeshID = system.RegisterMesh(mesh);
                ecb.RemoveComponent<MeshDataArrayComponent>(entity);
            }
            
            foreach (var (key, (meshDataArray, meshes)) in meshDataArrayToMeshMap)
            {
                Mesh.ApplyAndDisposeWritableMeshData(meshDataArray, meshes);
            }
        }
    }
}