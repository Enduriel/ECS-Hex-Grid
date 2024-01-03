using System.Collections.Generic;
using Unity.Entities;
using Unity.Rendering;

namespace Trideria.Mesh
{
	[UpdateInGroup(typeof(InitializationSystemGroup))]
	[UpdateBefore(typeof(EndInitializationEntityCommandBufferSystem))]
	public partial struct AssignColliderSystem : ISystem
	{
		private EntityQuery _query;

		public void OnCreate(ref SystemState state)
		{
			state.RequireForUpdate<EndInitializationEntityCommandBufferSystem.Singleton>();
			_query = SystemAPI.QueryBuilder().WithAll<ColliderOutdatedTag, MaterialMeshInfo, SyncColliderWithMeshTag>()
				.Build();
			state.RequireForUpdate(_query);
		}

		public void OnUpdate(ref SystemState state)
		{
			// this is a frame late, maybe AssignMeshSystem should be moved earlier in the update order
			// so this can execute in the same frame
			var ecb = SystemAPI.GetSingleton<EndInitializationEntityCommandBufferSystem.Singleton>()
				.CreateCommandBuffer(state.WorldUnmanaged);
			var system = state.World.GetExistingSystemManaged<EntitiesGraphicsSystem>();
			var meshList = new List<UnityEngine.Mesh>();
			foreach (var materialMeshInfo in SystemAPI.Query<RefRO<MaterialMeshInfo>>()
				         .WithAll<ColliderOutdatedTag, SyncColliderWithMeshTag>())
			{
				meshList.Add(system.GetMesh(materialMeshInfo.ValueRO.MeshID));
			}

			var meshDataArray = UnityEngine.Mesh.AcquireReadOnlyMeshData(meshList);
			new SyncColliderWithMeshJob
			{
				ECB = ecb.AsParallelWriter(),
				MeshDataArray = meshDataArray
			}.ScheduleParallel(_query);
		}
		
		public void OnDestroy(ref SystemState state)
		{
			_query.Dispose();
		}
	}
}