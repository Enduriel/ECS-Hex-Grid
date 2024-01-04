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
			hexGridData.Init(hexes);
			ECB.AddComponent<MeshOutdatedTag>(chunkIdx, e);
			ECB.AddComponent<SyncColliderWithMeshTag>(chunkIdx, e);
		}
	}
}