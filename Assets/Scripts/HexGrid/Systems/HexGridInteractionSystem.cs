using System.Linq;
using Unity.Entities;
using Unity.Physics;

namespace MyNamespace
{
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateBefore(typeof(BeginSimulationEntityCommandBufferSystem))]
    public partial struct HexGridInteractionSystem : ISystem
    {
        private EntityQuery _hexGridQuery;
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<PhysicsWorldSingleton>();
            state.RequireForUpdate<UserMouseClickInfo>();
            _hexGridQuery = SystemAPI.QueryBuilder().WithAll<UserClick, UserMouseInfo>().Build();
            state.RequireForUpdate(_hexGridQuery);
        }

        public void OnUpdate(ref SystemState state)
        {
            var singleton = SystemAPI.Query<UserMouseClickInfo>().First();
            var collisionWorld = SystemAPI.GetSingleton<PhysicsWorldSingleton>().CollisionWorld;
            RaycastInput test = new RaycastInput()
            {
                Start = singleton.MousePosition.ValueRO.Ray.Origin,
                End = singleton.MousePosition.ValueRO.Ray.Origin + singleton.MousePosition.ValueRO.Ray.Displacement * 1000,
                Filter = new CollisionFilter()
                {
                    BelongsTo = ~0u,
                    CollidesWith = ~0u,
                    GroupIndex = 0
                }
            };

            RaycastHit hit = new RaycastHit();
            bool haveHit = collisionWorld.CastRay(test, out hit);
            if (haveHit)
            {
                
            }
        }
    }
}