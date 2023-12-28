using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;

namespace MyNamespace
{
    public readonly partial struct UserInput : IAspect
    {
        public readonly RefRO<UserMovement> Movement;
        public readonly RefRO<UserZoom> Zoom;
        public readonly RefRO<UserMouseInfo> MousePosition;
    }
    
    public readonly partial struct UserMouseClickInfo : IAspect
    {
        public readonly RefRO<UserMouseInfo> MousePosition;
        public readonly RefRO<UserClick> Click;
    }
    
    public struct UserMovement : IComponentData
    {
        public float2 Value;
    }

    public struct UserZoom : IComponentData
    {
        public sbyte Value;
    }
    
    public struct UserMouseInfo : IComponentData
    {
        public float2 Position;
        public Ray Ray;
    }

    public struct UserClick : IComponentData
    {
    }
}