using MyNamespace;
using MyNamespace.Input.Enums;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using View.Aspects;
using View.Components;

namespace View.Jobs
{
	[BurstCompile]
	public partial struct UpdateCameraPivotJob : IJobEntity
	{
		[ReadOnly] public NativeReference<ViewSystemInputAspect> ConfigAspect;
		[ReadOnly] public NativeArray<ZoomLevelDescriptorElement> ZoomLevels;

		[ReadOnly] public float RotationSpeed;
		[ReadOnly] public float DeltaTime;

		[BurstCompile]
		public void Execute(ref LocalTransform localTransform, in CameraPivotTag _)
		{
			if (ConfigAspect.Value.UserMovement.IsValid)
				OnMove(ref localTransform, ConfigAspect.Value.UserMovement.ValueRO);
			if (ConfigAspect.Value.UserDrag.IsValid && ConfigAspect.Value.UserMouseMovement.IsValid)
				OnDrag(ref localTransform, ConfigAspect.Value.UserDrag.ValueRO,
					ConfigAspect.Value.UserMouseMovement.ValueRO);
		}

		public void OnMove(ref LocalTransform localTransform, UserMovement movement)
		{
			localTransform.Position +=
				math.mul(localTransform.Rotation, new float3(movement.Value.x, 0, movement.Value.y)) *
				(ZoomLevels[ConfigAspect.Value.ZoomLevel.ValueRO.Value].MoveSpeed * DeltaTime);
		}

		public void OnDrag(ref LocalTransform localTransform, UserDrag drag, UserMouseMovement mouseMovement)
		{
			if (drag.State != ButtonState.Held)
				return;
			localTransform.Rotation = math.mul(localTransform.Rotation,
				quaternion.RotateY(RotationSpeed * mouseMovement.Value.x));
		}
	}
}