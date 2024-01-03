using Trideria.Input;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using Parent = Unity.Transforms.Parent;

namespace Trideria.View
{
	// todo fix frame of input lag
	[UpdateInGroup(typeof(InitializationSystemGroup))]
	[UpdateBefore(typeof(EndInitializationEntityCommandBufferSystem))]
	public partial struct ViewSystem : ISystem
	{
		public static float ZoomSpeed = 1f;
		public static float RotationSpeed = 0.01f;

		private EntityQuery _requireForUpdateQuery;
		private EntityQuery _onUpdateQuery;

		public JobHandle test;

		public void OnCreate(ref SystemState state)
		{
			// state.RequireForUpdate<ViewSystemInputAspect>();
			_requireForUpdateQuery = SystemAPI.QueryBuilder()
				.WithAll<UserMouseInfo>()
				.WithAny<UserMovement, UserScroll, UserDrag, UserMouseMovement>()
				.AddAdditionalQuery()
				.WithAllRW<LocalTransform>()
				.WithAny<CameraPivotTag, CameraTag>()
				.Build();

			// ReSharper disable once Unity.Entities.SingletonMustBeRequested
			var configSingleton = SystemAPI.GetSingletonEntity<UserMouseInfo>();

			var ecb = new EntityCommandBuffer(Allocator.Temp);

			ecb.AddBuffer<ZoomLevelDescriptorElement>(configSingleton);
			var zoomLevelBuffer = ecb.SetBuffer<ZoomLevelDescriptorElement>(configSingleton);
			// this should be set up to read from something in the editor probably
			zoomLevelBuffer.Add(new ZoomLevelDescriptorElement(
				new float3(0, 50f, 0),
				quaternion.Euler(math.radians(85), 0, 0),
				50f));
			zoomLevelBuffer.Add(new ZoomLevelDescriptorElement(
				new float3(0, 30f, 0),
				quaternion.Euler(math.radians(80), 0, 0),
				30f));
			zoomLevelBuffer.Add(new ZoomLevelDescriptorElement(
				new float3(0, 20f, 0),
				quaternion.Euler(math.radians(70), 0, 0),
				20f));
			zoomLevelBuffer.Add(new ZoomLevelDescriptorElement(
				new float3(0, 15f, 0),
				quaternion.Euler(math.radians(75), 0, 0),
				15f));
			zoomLevelBuffer.Add(new ZoomLevelDescriptorElement(
				new float3(0, 10f, 0),
				quaternion.Euler(math.radians(60), 0, 0),
				10f));
			zoomLevelBuffer.Add(new ZoomLevelDescriptorElement(
				new float3(0, 5f, 0),
				quaternion.Euler(math.radians(30), 0, 0),
				10f));
			ecb.AddComponent<ZoomLevel>(configSingleton);

			var camera = ecb.CreateEntity();
			ecb.SetName(camera, "Camera");
			var cameraPivot = ecb.CreateEntity();
			ecb.SetName(cameraPivot, "CameraPivot");
			ecb.AddComponent<LocalTransform>(camera);
			ecb.AddComponent<CameraTag>(camera);
			ecb.AddComponent<LocalToWorld>(camera);
			ecb.AddComponent<Parent>(camera);
			ecb.SetComponent(camera, new Parent { Value = cameraPivot });
			ecb.AddComponent<LocalTransform>(cameraPivot);
			ecb.AddComponent<LocalToWorld>(cameraPivot);
			ecb.AddComponent<CameraPivotTag>(cameraPivot);
			// ecb.AddBuffer<Child>(cameraPivot)
			ecb.Playback(state.EntityManager);
			ecb.Dispose();
			state.RequireForUpdate(_requireForUpdateQuery);
		}

		public void OnUpdate(ref SystemState state)
		{
			var entity = SystemAPI.GetSingletonEntity<ViewSystemInputAspect>();
			var aspect = SystemAPI.GetAspect<ViewSystemInputAspect>(entity);
			var aspectRef = new NativeReference<ViewSystemInputAspect>(aspect, Allocator.TempJob);
			var zoomLevels = SystemAPI.GetBuffer<ZoomLevelDescriptorElement>(entity).AsNativeArray();
			var handle = new UpdateZoomLevelJob
			{
				Aspect = aspectRef,
				ZoomLevels = zoomLevels
			}.Schedule(state.Dependency);
			var time = SystemAPI.Time.DeltaTime;

			handle = new UpdateCameraPivotJob
			{
				ConfigAspect = aspectRef,
				ZoomLevels = zoomLevels,
				DeltaTime = time,
				RotationSpeed = RotationSpeed
			}.Schedule(handle);

			handle = new UpdateCameraJob
			{
				ConfigAspect = aspectRef,
				ZoomLevels = zoomLevels,
				DeltaTime = time,
				ZoomSpeed = ZoomSpeed
			}.Schedule(handle);

			aspectRef.Dispose(handle);
			zoomLevels.Dispose(handle);
			state.Dependency = handle;
			test = handle;
		}
	}
}