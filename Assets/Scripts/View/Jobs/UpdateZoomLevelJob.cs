using Trideria.Input;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

namespace Trideria.View
{
	[BurstCompile]
	public struct UpdateZoomLevelJob : IJob
	{
		public NativeReference<ViewSystemInputAspect> Aspect;
		[ReadOnly] public NativeArray<ZoomLevelDescriptorElement> ZoomLevels;

		public void OnScroll(UserScroll scroll, ref ZoomLevel zoomLevel)
		{
			var delta = scroll.Value;
			zoomLevel.Value = math.clamp(zoomLevel.Value + delta, 0, ZoomLevels.Length - 1);
		}

		[BurstCompile]
		public void Execute()
		{
			if (Aspect.Value.UserScroll.IsValid)
				OnScroll(Aspect.Value.UserScroll.ValueRO, ref Aspect.Value.ZoomLevel.ValueRW);
		}
	}
}