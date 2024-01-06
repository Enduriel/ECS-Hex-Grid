using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;

namespace Trideria.HexGrid
{
	public interface IHexGridData
	{
		public AABB GetBounds(NativeArray<HexBuffer> hexes);

		public int GetNumHexes();

		public int GetHexIndex(HexCoordinates hex);

		public IEnumerable<HexCoordinates> GetHexes();
	}
}