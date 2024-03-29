﻿using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Trideria.View
{
	[BurstCompile]
	public partial struct UpdateCameraJob : IJobEntity
	{
		[ReadOnly] public NativeReference<ViewSystemInputAspect> ConfigAspect;
		[ReadOnly] public NativeArray<ZoomLevelDescriptorElement> ZoomLevels;

		[ReadOnly] public float ZoomSpeed;
		[ReadOnly] public float DeltaTime;

		[BurstCompile]
		public void Execute(ref LocalTransform localTransform, in CameraTag _)
		{
			UpdatePosition(ZoomLevels[ConfigAspect.Value.ZoomLevel.ValueRO.Value], ref localTransform);
		}

		public void UpdatePosition(ZoomLevelDescriptorElement target, ref LocalTransform current)
		{
			current.Position = math.lerp(current.Position, target.Position, math.min(DeltaTime * 5f, 1f));
			current.Rotation = math.slerp(current.Rotation, target.Rotation, math.min(DeltaTime * 5f, 1f));
		}
	}
}