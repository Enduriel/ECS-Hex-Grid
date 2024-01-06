using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;

namespace Trideria.View
{
	public class EntitySetterAndFollower<T> : EntityFollower<T>
	where T : IComponentData
	{
		protected override void TryAwake()
		{
			base.TryAwake();
			if (!IsAwake)
			{
				return;
			}

			var gameObjectTransform = transform;
			Manager.SetComponentData(_myTarget, new LocalTransform()
			{
				Position = gameObjectTransform.position,
				Rotation = gameObjectTransform.rotation,
				Scale = Manager.GetComponentData<LocalTransform>(_myTarget).Scale
			});
		}
	}
}