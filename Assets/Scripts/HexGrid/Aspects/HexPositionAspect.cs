using Unity.Entities;
using Unity.Transforms;

namespace MyNamespace
{
    public readonly partial struct HexPositionAspect : IAspect
    {
        public readonly Entity Self;

        public readonly RefRW<LocalTransform> Transform;
        public readonly RefRW<HexCoordinates> Coords;
    }
}