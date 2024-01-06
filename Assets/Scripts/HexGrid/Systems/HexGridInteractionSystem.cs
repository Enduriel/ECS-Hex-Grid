using Trideria.Input;
using Trideria.Mesh;
using Trideria.UI;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;
using UnityEngine;

namespace Trideria.HexGrid
{
	[UpdateInGroup(typeof(SimulationSystemGroup))]
	[UpdateBefore(typeof(BeginSimulationEntityCommandBufferSystem))]
	public partial struct HexGridInteractionSystem : ISystem
	{
		private EntityQuery _hexGridQuery;

		public void OnCreate(ref SystemState state)
		{
			state.RequireForUpdate<AllowedColorComponent>();
			state.RequireForUpdate<BeginSimulationEntityCommandBufferSystem.Singleton>();
			state.RequireForUpdate<PhysicsWorldSingleton>();
			state.RequireForUpdate<UserMouseInfo>();
			_hexGridQuery = SystemAPI.QueryBuilder().WithAll<UserSelect, UserMouseInfo>().Build();
			state.RequireForUpdate(_hexGridQuery);
		}

		public void OnUpdate(ref SystemState state)
		{
			var userSelect = _hexGridQuery.GetSingleton<UserSelect>();
			if (userSelect.State != ButtonState.Pressed)
			{
				return;
			}

			var userMouseInfo = _hexGridQuery.GetSingleton<UserMouseInfo>();
			var collisionWorld = SystemAPI.GetSingleton<PhysicsWorldSingleton>().CollisionWorld;
			var test = new RaycastInput
			{
				Start = userMouseInfo.Ray.Origin,
				End = userMouseInfo.Ray.Origin + userMouseInfo.Ray.Displacement * 1000,
				Filter = CollisionFilter.Default
			};


			if (collisionWorld.CastRay(test, out var hit))
			{
				var buffer = SystemAPI.GetBuffer<HexBuffer>(hit.Entity);
				var localTransform = SystemAPI.GetComponentRO<LocalTransform>(hit.Entity);
				var hexIdx = HexHelpers.GetHexIdxAtPosition(buffer, localTransform.ValueRO, hit.Position);
				var hex = buffer[hexIdx];
				hex.Value.Color = HexHelpers.GetColor(SystemAPI.GetSingleton<AllowedColorComponent>().Value);
				buffer[hexIdx] = hex;
				var ecb = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>()
					.CreateCommandBuffer(state.WorldUnmanaged);
				ecb.AddComponent<MeshOutdatedTag>(hit.Entity);
			}
		}
	}
}