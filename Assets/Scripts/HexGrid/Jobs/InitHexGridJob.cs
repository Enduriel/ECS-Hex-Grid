using Trideria.Mesh;
using Unity.Burst;
using Unity.Entities;
using Unity.Rendering;
using UnityEngine;
using Random = Unity.Mathematics.Random;

namespace Trideria.HexGrid
{
	[BurstCompile]
	public partial struct InitHexHexGridJob : IJobEntity
	{
		// Output
		public EntityCommandBuffer.ParallelWriter ECB;

		[BurstCompile]
		public void Execute(
			[ChunkIndexInQuery] int chunkIdx,
			[EntityIndexInQuery] int idx,
			Entity e,
			ref RenderBounds renderBounds,
			in HexHexGridData hexGridData)
		{
			ECB.AddBuffer<HexBuffer>(chunkIdx, e);
			var hexes = ECB.SetBuffer<HexBuffer>(chunkIdx, e);
			// CommandBuffer.RemoveComponent<RenderMeshArray>(chunkIdx, e);
			InitHexGrid(hexes, hexGridData);
			ECB.AddComponent<MeshOutdatedTag>(chunkIdx, e);
			ECB.AddComponent<SyncColliderWithMeshTag>(chunkIdx, e);
		}

		private HexBuffer CreateHex(HexCoordinates coords)
		{
			return new HexBuffer(coords, Color.white);
		}

		private HexBuffer RandomizeColor(HexBuffer element)
		{
			// very inefficient and only for testing
			var random = new Random((uint)element.Value.Coords.GetHashCode());
			switch (random.NextInt(1, 5))
			{
				case 1:
					element.Value.Color = Color.red;
					break;
				case 2:
					element.Value.Color = Color.green;
					break;
				case 3:
					element.Value.Color = Color.blue;
					break;
				case 4:
					element.Value.Color = Color.cyan;
					break;
			}

			return element;
		}

		private void InitHexGrid(DynamicBuffer<HexBuffer> hexes, HexHexGridData hexGridData)
		{
			hexes.ResizeUninitialized(HexHelpers.GetNumHexes(hexGridData.Radius));
			var direction = HexCoordinates.NorthEast;
			var currentCoords = new HexCoordinates(0, 0);
			var idx = 0;
			hexes[idx++] = CreateHex(currentCoords);
			for (var i = 0; i < hexGridData.Radius; i++)
			{
				// each edge within cycle
				for (var j = 0; j < 6; j++)
				{
					// each hex within edge
					direction = HexHelpers.DirectionMap[direction];
					for (var k = 0; k < i; k++)
					{
						currentCoords += direction;
						hexes[idx++] = RandomizeColor(CreateHex(currentCoords));
					}
				}

				currentCoords += HexCoordinates.North;
			}
		}
	}
}