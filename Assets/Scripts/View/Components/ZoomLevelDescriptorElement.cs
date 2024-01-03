using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace View.Components
{
	[InternalBufferCapacity(10)]
	public partial struct ZoomLevelDescriptorElement : IBufferElementData
	{
		public float3 Position;
		public quaternion Rotation;
		public float MoveSpeed;

		public ZoomLevelDescriptorElement(float3 position, quaternion rotation, float moveSpeed)
		{
			Position = position;
			Rotation = rotation;
			MoveSpeed = moveSpeed;
		}
	}
}