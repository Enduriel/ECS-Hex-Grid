using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;

namespace Trideria.Input
{
	public interface IUserInputWithValue<T>
	{
		public T SetValue { set; }

		public bool IsZero()
		{
			throw new System.NotImplementedException();
		}
	}

	public interface IUserInputValue<T>
	{
		public bool IsNotZero();
		public T Value { get; set; }
	}

	public interface IUserInput
	{
		public ButtonState SetState { set; }
	}

	public struct UserFloat2InputValue : IUserInputValue<float2>
	{
		public bool IsNotZero() => math.any(Value);
		public float2 Value { get; set; }
	}

	public struct UserIntInputValue : IUserInputValue<int>
	{
		public bool IsNotZero() => Value != 0;
		public int Value { get; set; }
	}

	public struct UserMovement : IComponentData, IUserInputWithValue<UserFloat2InputValue>
	{
		public float2 Value;

		public UserFloat2InputValue SetValue
		{
			set => Value = value.Value;
		}
	}

	public struct UserMouseMovement : IComponentData, IUserInputWithValue<UserFloat2InputValue>
	{
		public float2 Value;

		public UserFloat2InputValue SetValue
		{
			set => Value = value.Value;
		}
	}

	public struct UserScroll : IComponentData, IUserInputWithValue<UserIntInputValue>
	{
		public int Value;

		public UserIntInputValue SetValue
		{
			set => Value = value.Value;
		}
	}

	public struct UserMouseInfo : IComponentData
	{
		public float2 Position;
		public Ray Ray;
	}

	public struct UserSelect : IComponentData, IUserInput
	{
		public ButtonState State;

		public ButtonState SetState
		{
			set => State = value;
		}
	}

	public struct UserDrag : IComponentData, IUserInput
	{
		public ButtonState State;

		public ButtonState SetState
		{
			set => State = value;
		}
	}
}