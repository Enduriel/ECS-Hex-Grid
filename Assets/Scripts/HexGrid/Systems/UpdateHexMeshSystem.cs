﻿using Trideria.Mesh;
using Unity.Entities;
using Unity.Rendering;

namespace Trideria.HexGrid
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
				.WithAll<HexBuffer, MaterialMeshInfo, MeshOutdatedTag, RenderBounds>()
				.WithAspect<HexGridAspect>()
				.WithNone<MeshDataArrayComponent>()
				.Build();
			state.RequireForUpdate(_entityQuery);
		}

		public void OnUpdate(ref SystemState state)
		{
			var ecb = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>()
				.CreateCommandBuffer(state.WorldUnmanaged);
			var id = state.World.GetExistingSystemManaged<AssignMeshSystem>()
				.AllocateMeshDataArray(out var meshDataArray, _entityQuery.CalculateEntityCount());
			new CreateHexMeshJob
			{
				ECB = ecb.AsParallelWriter(),
				MeshDataArrayID = id,
				MeshDataArray = meshDataArray
			}.ScheduleParallel(_entityQuery);
		}
	}
}