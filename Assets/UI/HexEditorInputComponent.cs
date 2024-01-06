using Trideria.HexGrid;
using Unity.Entities;

namespace Trideria.UI
{
	public struct HexEditorInputComponent : IComponentData
	{
		public AllowedColor Color;
		public int Height;
	}
}