using System.Collections.Generic;
using Mesh.Components;
using Unity.Entities;
using Unity.Rendering;
using UnityEngine;

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

		private List<UnityEngine.Mesh.MeshDataArray?> _meshDataArrays;

		protected override void OnCreate()
		{
			RequireForUpdate<EndSimulationEntityCommandBufferSystem.Singleton>();
			_hexGridQuery = SystemAPI.QueryBuilder()
				.WithAllRW<MaterialMeshInfo>()
				.WithAll<MeshDataArrayComponent>()
				.Build();
			RequireForUpdate(_hexGridQuery);

			_meshDataArrays = new List<UnityEngine.Mesh.MeshDataArray?>();
		}

		public MeshDataArrayID AllocateMeshDataArray(out UnityEngine.Mesh.MeshDataArray meshDataArray, int size)
		{
			meshDataArray = UnityEngine.Mesh.AllocateWritableMeshData(size);
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

		private UnityEngine.Mesh.MeshDataArray? UseMeshDataArray(MeshDataArrayID id)
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

			var meshDataArrayToMeshMap =
				new Dictionary<MeshDataArrayID, (UnityEngine.Mesh.MeshDataArray, UnityEngine.Mesh[])>();
			foreach (var (materialMeshInfo, meshDataArrayComponent, entity)
			         in SystemAPI.Query<RefRW<MaterialMeshInfo>, RefRO<MeshDataArrayComponent>>().WithEntityAccess())
			{
				var meshDataArrayID = meshDataArrayComponent.ValueRO.ID;
				if (!meshDataArrayToMeshMap.ContainsKey(meshDataArrayID))
				{
					if (UseMeshDataArray(meshDataArrayID) is { } meshDataArray)
					{
						meshDataArrayToMeshMap[meshDataArrayID] =
							(meshDataArray, new UnityEngine.Mesh[meshDataArray.Length]);
					}
					else
					{
						Debug.LogError($"MeshDataArrayID {meshDataArrayID} is invalid");
					}
				}

				var mesh = new UnityEngine.Mesh();
				meshDataArrayToMeshMap[meshDataArrayID].Item2[meshDataArrayComponent.ValueRO.Index] = mesh;
				materialMeshInfo.ValueRW.MeshID = system.RegisterMesh(mesh);
				ecb.RemoveComponent<MeshDataArrayComponent>(entity);
				if (SystemAPI.HasComponent<SyncColliderWithMeshTag>(entity))
					ecb.AddComponent<ColliderOutdatedTag>(entity);
			}

			foreach (var (_, (meshDataArray, meshes)) in meshDataArrayToMeshMap)
			{
				UnityEngine.Mesh.ApplyAndDisposeWritableMeshData(meshDataArray, meshes);
			}
		}
	}
}