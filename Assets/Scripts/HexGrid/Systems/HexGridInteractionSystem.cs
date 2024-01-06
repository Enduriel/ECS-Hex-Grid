using Trideria.Input;
using Trideria.Mesh;
using Trideria.UI;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;
using UnityEngine;
using RaycastHit = Unity.Physics.RaycastHit;

namespace Trideria.HexGrid
{
	[UpdateInGroup(typeof(SimulationSystemGroup))]
	[UpdateBefore(typeof(BeginSimulationEntityCommandBufferSystem))]
	public partial struct HexGridInteractionSystem : ISystem
	{
		private EntityQuery _hexGridQuery;

		public void OnCreate(ref SystemState state)
		{
			state.RequireForUpdate<HexEditorInputComponent>();
			state.RequireForUpdate<BeginSimulationEntityCommandBufferSystem.Singleton>();
			state.RequireForUpdate<PhysicsWorldSingleton>();
			state.RequireForUpdate<UserMouseInfo>();
			_hexGridQuery = SystemAPI.QueryBuilder().WithAll<UserSelect, UserMouseInfo>().Build();
			state.RequireForUpdate(_hexGridQuery);
		}

		private void OnRayHit(ref SystemState state, RaycastHit hit)
		{
			var buffer = SystemAPI.GetBuffer<HexBuffer>(hit.Entity);
			var localTransform = SystemAPI.GetComponentRO<LocalTransform>(hit.Entity);
			var hexIdx = HexHelpers.GetHexIdxAtPosition(buffer, localTransform.ValueRO, hit.Position);
			buffer[hexIdx] = OnTouchHex(ref state, buffer[hexIdx]);
			var ecb = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>()
				.CreateCommandBuffer(state.WorldUnmanaged);
			ecb.AddComponent<MeshOutdatedTag>(hit.Entity);
		}

		private HexBuffer OnTouchHex(ref SystemState state, HexBuffer hex)
		{
			var data = SystemAPI.GetSingleton<HexEditorInputComponent>();
			hex.Color = HexHelpers.GetColor(data.Color);
			hex.Height = data.Height;
			return hex;
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
				OnRayHit(ref state, hit);
			}
		}
	}
}