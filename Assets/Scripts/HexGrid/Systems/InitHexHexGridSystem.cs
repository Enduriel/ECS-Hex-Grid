using Unity.Entities;
using Unity.Rendering;

namespace Trideria.HexGrid
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
		
		public void OnDestroy(ref SystemState state)
		{
			_hexGridQuery.Dispose();
		}
	}
}