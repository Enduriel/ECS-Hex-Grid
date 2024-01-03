using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace Trideria.View
{
	public abstract class EntityFollower<T> : MonoBehaviour
		where T : IComponentData
	{
		protected EntityQuery EntityQuery;
		protected EntityManager Manager;

		protected void Awake()
		{
			Manager = World.DefaultGameObjectInjectionWorld.EntityManager;
			EntityQuery = new EntityQueryBuilder(Allocator.Temp).WithAll<T, LocalTransform>().Build(Manager);
			var tempQuery = new EntityQueryBuilder(Allocator.Temp).WithAll<T>().WithAllRW<LocalTransform>()
				.Build(Manager);

			var entityTransform = tempQuery.GetSingletonRW<LocalTransform>();
			tempQuery.Dispose();
			var gameObjectTransform = transform;

			entityTransform.ValueRW.Position = gameObjectTransform.position;
			entityTransform.ValueRW.Rotation = gameObjectTransform.rotation;
		}

		protected void Update()
		{
			var test = World.DefaultGameObjectInjectionWorld.GetExistingSystem<ViewSystem>();
			World.DefaultGameObjectInjectionWorld.Unmanaged.GetUnsafeSystemRef<ViewSystem>(test).test.Complete();
			if (EntityQuery.TryGetSingletonEntity<LocalTransform>(out var entity))
			{
				var localTransform = Manager.GetComponentData<LocalTransform>(entity);
				if (Manager.HasComponent<Parent>(entity))
				{
					var parentLocalTransform =
						Manager.GetComponentData<LocalTransform>(Manager.GetComponentData<Parent>(entity).Value);
					localTransform.Rotation = math.mul(parentLocalTransform.Rotation, localTransform.Rotation);
					localTransform.Position += parentLocalTransform.Position;
				}

				transform.position = localTransform.Position;
				transform.rotation = localTransform.Rotation;
			}
		}
	}
}