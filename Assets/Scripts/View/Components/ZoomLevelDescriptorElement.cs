﻿using Unity.Entities;
using Unity.Mathematics;

namespace Trideria.View
{
	[InternalBufferCapacity(10)]
	public struct ZoomLevelDescriptorElement : IBufferElementData
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