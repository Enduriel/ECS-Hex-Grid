using Unity.Entities;
using UnityEngine;

namespace Trideria.HexGrid
{
	public class HexHexGridMono : MonoBehaviour
	{
		public ushort radius;
	}

	public class HexHexGridBaker : Baker<HexHexGridMono>
	{
		public override void Bake(HexHexGridMono authoring)
		{
			// this can probably be Renderable but would require reworking other stuff, todo later
			var hexGridEntity = GetEntity(TransformUsageFlags.Dynamic);
			AddComponent(hexGridEntity, new HexHexGridData
			{
				Radius = authoring.radius
			});
		}
	}
}