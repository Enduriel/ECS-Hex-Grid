using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

namespace Trideria.HexGrid
{
	public interface IHexGridData
	{
		public AABB GetBounds(NativeArray<HexBuffer> hexes);

		public int GetNumHexes();

		public bool TryGetHexIndex(HexCoordinates hex, out int idx);

		public IEnumerable<HexCoordinates> GetHexes();
	}
}