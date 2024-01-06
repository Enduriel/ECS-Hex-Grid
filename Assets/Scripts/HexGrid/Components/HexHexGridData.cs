using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

namespace Trideria.HexGrid
{
	public struct HexHexGridData : IComponentData, IHexGridData
	{
		public ushort Radius;

		public readonly AABB GetBounds(NativeArray<HexBuffer> hexes)
		{
			// todo improve this function, it's almost certainly overestimating
			var x = HexHelpers.OuterRadius * 1.5f * (Radius - 1) + HexHelpers.OuterRadius * 0.5f;
			var z = HexHelpers.InnerRadius * (Radius - 1) + HexHelpers.InnerRadius;
			var diag = math.sqrt(x * x + z * z);

			return new AABB
			{
				Center = float3.zero,
				Extents = new float3(diag, 0f, diag)
			};
		}

		public readonly int GetNumHexes()
		{
			return 1 + 3 * Radius * (Radius - 1);
		}

		// based on https://www.redblobgames.com/x/2317-hexagon-shaped-hex-map-storage and
		// https://old.reddit.com/r/gamedev/comments/133gv7l/i_might_have_figured_out_new_way_to_store_hex/
		public readonly bool TryGetHexIndex(HexCoordinates hex, out int idx)
		{
			if (HexHelpers.GetDistance(HexCoordinates.Zero, hex) > Radius - 1)
			{
				idx = -1;
				return false;
			}

			idx = hex switch
			{
				{ Q: >= 0, R: < 0 } => (Radius - 1) * hex.Q - hex.R,
				{ R: >= 0, S: < 0 } => (Radius - 1) * (Radius + hex.R) - hex.S,
				{ S: >= 0, Q: < 0 } => (Radius - 1) * (2 * Radius + hex.S) - hex.Q,
				{ Q: 0, R : 0 } => 0,
				_ => -1
			};

			return true;
		}

		// not burst-compatible for now
		public readonly IEnumerable<HexCoordinates> GetHexes()
		{
			yield return HexCoordinates.Zero;
			var rectStart = HexCoordinates.North;
			for (var rect = 0; rect < 3; rect++)
			{
				var rectCol = rectStart;
				for (var i = 0; i < Radius; i++)
				{
					var rectRow = rectCol;
					for (var j = 0; j < Radius - 1; j++)
					{
						yield return rectRow;
						rectRow += rectStart;
					}

					rectCol += HexHelpers.DirectionMap[HexHelpers.DirectionMap[rectStart]];
				}

				rectStart = HexHelpers.DirectionMap[HexHelpers.DirectionMap[rectStart]];
			}
		}
	}
}