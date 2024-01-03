using System;
using System.Collections;
using System.Collections.Generic;
using Input.Jobs;
using MyNamespace.Input.Enums;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using View.Aspects;
using Ray = Unity.Physics.Ray;

namespace MyNamespace
{
	[UpdateInGroup(typeof(InitializationSystemGroup), OrderFirst = true)]
	[UpdateBefore(typeof(BeginInitializationEntityCommandBufferSystem))]
	public partial class InputSystem : SystemBase
	{
		private DefaultMovementActions _movementActions;

		private NativeHashMap<int, ButtonState> _lastFrameState = new(2, Allocator.Persistent)
		{
			{ (int)InputType.Select, ButtonState.None },
			{ (int)InputType.Drag, ButtonState.None }
		};

		private NativeHashMap<int, bool> _lastFrameInput = new(2, Allocator.Persistent)
		{
			{ (int)InputType.Scroll, false },
			{ (int)InputType.Move, false },
			{ (int)InputType.MouseMove, false }
		};

		// private Dictionary<InputType, Func<>>

		protected override void OnCreate()
		{
			_movementActions = new DefaultMovementActions();
			var ecb = new EntityCommandBuffer(Allocator.Temp);
			var singleton = ecb.CreateEntity();
			ecb.SetName(singleton, new FixedString64Bytes("UserInputSingleton"));
			ecb.AddComponent<UserMouseInfo>(singleton);
			ecb.SetComponent(singleton, new UserMouseInfo());

			ecb.Playback(EntityManager);
		}

		// Start is called before the first frame update
		protected override void OnStartRunning()
		{
			_movementActions.Enable();
		}

		protected override void OnUpdate()
		{
			var mousePos = Mouse.current.position.ReadValue();
			var managedRay = Camera.main!.ScreenPointToRay(new Vector3(mousePos.x, mousePos.y, 0));
			new InputUpdateJob
			{
				ECB = SystemAPI.GetSingleton<BeginInitializationEntityCommandBufferSystem.Singleton>()
					.CreateCommandBuffer(World.Unmanaged),
				LastFrameState = _lastFrameState,
				LastFrameInput = _lastFrameInput,
				Movement = _movementActions.DefaultMap.Movement.ReadValue<Vector2>(),
				MouseMovement = _movementActions.DefaultMap.MouseMovement.ReadValue<Vector2>(),
				MousePosition = mousePos,
				Ray = new Ray { Origin = managedRay.origin, Displacement = managedRay.direction },
				MouseScroll = (int)Mouse.current.scroll.ReadValue().normalized.y,
				SelectPressed = _movementActions.DefaultMap.Select.IsPressed(),
				DragPressed = _movementActions.DefaultMap.Drag.IsPressed()
			}.Schedule();
		}
	}
}