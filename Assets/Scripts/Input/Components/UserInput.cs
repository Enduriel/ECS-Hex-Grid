using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;

namespace MyNamespace
{
    public readonly partial struct UserInput : IAspect
    {
        public readonly RefRO<UserMovement> Movement;
        public readonly RefRO<UserScroll> Scroll;
        public readonly RefRO<UserMouseInfo> MousePosition;
    }
    
    public readonly partial struct UserMouseClickInfo : IAspect
    {
        public readonly RefRO<UserMouseInfo> MousePosition;
        public readonly RefRO<UserSelect> Click;
    }
    
    public interface IUserInputWithValue<T>
    {
        public T ValueFunc { get; set; }
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
        public UserFloat2InputValue ValueFunc { get => new() {Value = Value}; set => Value = value.Value; }
    }

    public struct UserScroll : IComponentData, IUserInputWithValue<UserIntInputValue>
    {
        public int Value;
        public UserIntInputValue ValueFunc { get => new() {Value = Value}; set => Value = value.Value; }
    }
    
    public struct UserMouseInfo : IComponentData
    {
        public float2 Position;
        public Ray Ray;
    }

    public struct UserSelect : IComponentData
    {
    }

    public struct UserDrag : IComponentData
    {
    }
    
}