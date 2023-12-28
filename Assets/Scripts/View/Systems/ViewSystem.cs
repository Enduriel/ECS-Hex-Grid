using System.Linq;
using MyNamespace;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

namespace View.Systems
{
    [UpdateInGroup(typeof(InitializationSystemGroup))]
    [UpdateAfter(typeof(InputSystem))]
    public partial struct ViewSystem : ISystem
    {
        public static float MoveSpeed = 10f;
        public static float ZoomSpeed = 50f;

        private EntityQuery _query;
        
        public void OnCreate(ref SystemState state)
        {
            _query = SystemAPI.QueryBuilder().WithAny<UserMovement, UserScroll>().Build();
            state.RequireForUpdate(_query);
        }

        public void OnUpdate(ref SystemState state)
        {
            var camera = CameraSingleton.Instance;
            var pos = camera.GetPosition();
            if (SystemAPI.TryGetSingleton(out UserScroll scroll))
            {
                pos.y -= scroll.Value * (ZoomSpeed * SystemAPI.Time.DeltaTime);
            }

            if (SystemAPI.TryGetSingleton(out UserMovement movement))
            {
                pos.x += movement.Value.x * (MoveSpeed * SystemAPI.Time.DeltaTime);
                pos.z += movement.Value.y * (MoveSpeed * SystemAPI.Time.DeltaTime);
            }
            camera.SetPosition(pos);
        }
    }
}