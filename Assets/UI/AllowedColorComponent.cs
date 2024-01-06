using Trideria.HexGrid;
using Unity.Entities;

namespace Trideria.UI
{
	public struct AllowedColorComponent : IComponentData
	{
		public AllowedColor Value;
	}
}