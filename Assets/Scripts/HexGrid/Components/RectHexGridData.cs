﻿using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Trideria.HexGrid
{
	public struct RectHexGridData : IComponentData, IHexGridData
	{
		public ushort Width;
		public ushort Height;

		// heavily overestimates
		public readonly AABB GetBounds(NativeArray<HexBuffer> hexes)
		{
			var width = Width * HexHelpers.InnerRadius * 2;
			var height = Height * HexHelpers.OuterRadius * 2;
			var diag = math.sqrt(width * width + height * height) / 2;
			return new AABB
			{
				Center = new float3(width / 2, 0f, height / 2),
				Extents = new float3(diag, 0f, diag)
			};
		}

		public readonly int GetNumHexes()
		{
			return Width * Height;
		}

		public readonly bool TryGetHexIndex(HexCoordinates hex, out int idx)
		{
			var col = hex.Q;
			var row = hex.R + (hex.Q - (hex.Q & 1)) / 2;
			idx = row * Width + col;

			return col >= 0 && row >= 0 && col < Width && row < Height;
		}

		public readonly HexCoordinates GetCoordsForIdx(int idx)
		{
			var col = idx / Height;
			var row = idx % Height;

			var q = col;
			var r = row - (col - (col & 1)) / 2;
			return new HexCoordinates(q, r);
		}

		public readonly IEnumerable<HexCoordinates> GetHexes()
		{
			var rowStart = HexCoordinates.Zero;
			for (var i = 0; i < Height; i++)
			{
				var hex = rowStart;
				for (var j = 0; j < Width; j++)
				{
					yield return hex;
					hex += j % 2 == 0 ? HexCoordinates.SouthEast : HexCoordinates.NorthEast;
				}

				rowStart += HexCoordinates.South;
			}
		}
	}
}