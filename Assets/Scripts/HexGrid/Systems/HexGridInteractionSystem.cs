using Trideria.Input;
using Trideria.Mesh;
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
			Debug.DrawLine(userMouseInfo.Ray.Origin, userMouseInfo.Ray.Origin + userMouseInfo.Ray.Displacement * 1000,
				Color.red, 100f);
			var test = new RaycastInput
			{
				Start = userMouseInfo.Ray.Origin,
				End = userMouseInfo.Ray.Origin + userMouseInfo.Ray.Displacement * 1000,
				Filter = CollisionFilter.Default
			};


			if (collisionWorld.CastRay(test, out var hit))
			{
				Debug.DrawLine(hit.Position, hit.Position + hit.SurfaceNormal * 1, Color.green, 100f);
				var buffer = SystemAPI.GetBuffer<HexBuffer>(hit.Entity);
				var localTransform = SystemAPI.GetComponentRO<LocalTransform>(hit.Entity);
				var hexIdx = HexHelpers.GetHexIdxAtPosition(buffer, localTransform.ValueRO, hit.Position);
				Debug.DrawLine(localTransform.ValueRO.Position, localTransform.ValueRO.Position + new float3(0, 1, 0),
					Color.black, 100f);
				Debug.DrawLine(HexHelpers.GetWorldPosition(buffer[hexIdx].Value.Coords, localTransform.ValueRO),
					HexHelpers.GetWorldPosition(buffer[hexIdx].Value.Coords, localTransform.ValueRO) +
					new float3(0, 1, 0), Color.blue, 100f);
				var hex = buffer[hexIdx];
				hex.Value.Height += 1;
				buffer[hexIdx] = hex;
				var ecb = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>()
					.CreateCommandBuffer(state.WorldUnmanaged);
				ecb.AddComponent<MeshOutdatedTag>(hit.Entity);
			}
		}
	}
}