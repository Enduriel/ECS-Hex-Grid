using Unity.Entities;

namespace Trideria.HexGrid
{
	public readonly partial struct HexGridAspect : IAspect
	{
		[Optional] public readonly RefRO<HexHexGridData> HexHexGridData;
		[Optional] public readonly RefRO<RectHexGridData> RectHexGridData;
		public readonly RefRO<HexGridTag> HexGridTag;
	}
}