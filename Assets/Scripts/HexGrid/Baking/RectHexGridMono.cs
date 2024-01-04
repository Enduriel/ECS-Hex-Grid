using Unity.Entities;
using UnityEngine;
using UnityEngine.Serialization;

namespace Trideria.HexGrid
{
	public class RectHexGridMono : MonoBehaviour
	{
		public ushort width;
		public ushort height;

		public class RectHexGridDataBaker : Baker<RectHexGridMono>
		{
			public override void Bake(RectHexGridMono authoring)
			{
				var entity = GetEntity(TransformUsageFlags.Dynamic);
				AddComponent(entity, new RectHexGridData { Width = authoring.width, Height = authoring.height });
				AddComponent(entity, new HexGridTag());
			}
		}
	}
}