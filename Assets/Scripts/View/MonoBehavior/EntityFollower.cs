using System;
using System.Collections.Generic;
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
		protected EntityManager Manager;
		protected static Dictionary<Type, int> Counters = new();
		protected Entity _myTarget;
		protected bool IsAwake => _myTarget != Entity.Null;

		protected virtual void Awake()
		{
			Manager = World.DefaultGameObjectInjectionWorld.EntityManager;
			if (!Counters.ContainsKey(typeof(T)))
			{
				Counters[typeof(T)] = 0;
			}
			TryAwake();
		}

		protected virtual void TryAwake()
		{
			var tempQuery = new EntityQueryBuilder(Allocator.Temp).WithAll<T>().WithAllRW<LocalTransform>()
				.Build(Manager);
			var array = tempQuery.ToEntityArray(Allocator.Temp);
			if (array.Length <= Counters[typeof(T)])
			{
				return;
			}

			_myTarget = tempQuery.ToEntityArray(Allocator.Temp)[Counters[typeof(T)]++];
			tempQuery.Dispose();
		}

		protected virtual void Update()
		{
			if (!IsAwake)
			{
				TryAwake();
				return;
			}
			var test = World.DefaultGameObjectInjectionWorld.GetExistingSystem<ViewSystem>();
			World.DefaultGameObjectInjectionWorld.Unmanaged.GetUnsafeSystemRef<ViewSystem>(test).test.Complete();
			Manager.GetComponentData<LocalTransform>(_myTarget);
			var localTransform = Manager.GetComponentData<LocalTransform>(_myTarget);
			if (Manager.HasComponent<Parent>(_myTarget))
			{
				var parentLocalTransform =
						Manager.GetComponentData<LocalTransform>(Manager.GetComponentData<Parent>(_myTarget).Value);
					localTransform.Rotation = math.mul(parentLocalTransform.Rotation, localTransform.Rotation);
					localTransform.Position += parentLocalTransform.Position;
			}
			transform.position = localTransform.Position;
			transform.rotation = localTransform.Rotation;
		}

		public void OnDestroy()
		{
			Counters[typeof(T)]--;
		}
	}
}