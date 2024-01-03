using MyNamespace;
using MyNamespace.Input.Enums;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Physics;
using UnityEngine.Rendering;

namespace Input.Jobs
{
	[BurstCompile]
	public partial struct InputUpdateJob : IJobEntity
	{
		public EntityCommandBuffer ECB;
		[ReadOnly] public Ray Ray;
		public NativeHashMap<int, ButtonState> LastFrameState;
		public NativeHashMap<int, bool> LastFrameInput;

		[ReadOnly] public float2 Movement;
		[ReadOnly] public float2 MouseMovement;
		[ReadOnly] public float2 MousePosition;
		[ReadOnly] public int MouseScroll;

		[ReadOnly] public bool SelectPressed;
		[ReadOnly] public bool DragPressed;

		[BurstCompile]
		public void Execute(ref UserMouseInfo userMouseInfo, Entity entity)
		{
			userMouseInfo.Position = MousePosition;
			userMouseInfo.Ray = Ray;

			UpdateComponentWithValue<UserMovement, UserFloat2InputValue, float2>(ECB, entity, InputType.Move,
				new UserFloat2InputValue
				{
					Value = Movement
				});
			UpdateComponentWithValue<UserScroll, UserIntInputValue, int>(ECB, entity, InputType.Scroll,
				new UserIntInputValue
				{
					Value = MouseScroll
				});
			UpdateComponentWithValue<UserMouseMovement, UserFloat2InputValue, float2>(ECB, entity, InputType.MouseMove,
				new UserFloat2InputValue
				{
					Value = MouseMovement
				});
			UpdateComponent<UserSelect>(ECB, entity, InputType.Select, SelectPressed);
			UpdateComponent<UserDrag>(ECB, entity, InputType.Drag, DragPressed);
		}

		private void UpdateComponent<T>(EntityCommandBuffer ecb, Entity entity, InputType inputType, bool pressed)
			where T : unmanaged, IComponentData, IUserInput
		{
			var value = ButtonState.None;
			if (pressed)
			{
				if ((LastFrameState[(int)inputType] & ButtonState.PressedOrHeld) == ButtonState.None)
					value = ButtonState.Pressed;
				else
					value = ButtonState.Held;
			}
			else if ((LastFrameState[(int)inputType] & ButtonState.PressedOrHeld) != ButtonState.None)
			{
				value = ButtonState.Released;
			}

			LastFrameState[(int)inputType] = value;
			if (value == ButtonState.None)
			{
				ecb.RemoveComponent<T>(entity);
			}
			else
			{
				ecb.AddComponent<T>(entity);
				ecb.SetComponent(entity, new T
				{
					SetState = value
				});
			}
		}

		private void UpdateComponentWithValue<T, T1, T2>(EntityCommandBuffer ecb, Entity entity, InputType inputType,
			T1 value)
			where T : unmanaged, IComponentData, IUserInputWithValue<T1>
			where T1 : IUserInputValue<T2>
		{
			if (value.IsNotZero())
			{
				if (!LastFrameInput[(int)inputType])
				{
					ecb.AddComponent<T>(entity);
					LastFrameInput[(int)inputType] = true;
				}

				ecb.SetComponent(entity, new T
				{
					SetValue = value
				});
			}
			else if (LastFrameInput[(int)inputType])
			{
				ecb.RemoveComponent<T>(entity);
				LastFrameInput[(int)inputType] = false;
			}
		}
	}
}