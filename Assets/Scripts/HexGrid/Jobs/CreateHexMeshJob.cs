using Trideria.Mesh;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using UnityEngine;
using UnityEngine.Rendering;

namespace Trideria.HexGrid
{
	[BurstCompile]
	public partial struct CreateHexMeshJob : IJobEntity
	{
		public EntityCommandBuffer.ParallelWriter ECB;
		public UnityEngine.Mesh.MeshDataArray MeshDataArray;
		public MeshDataArrayID MeshDataArrayID;

		[BurstCompile]
		public void Execute(
			[ChunkIndexInQuery] int chunkIdx,
			[EntityIndexInQuery] int idx,
			ref RenderBounds renderBounds,
			in DynamicBuffer<HexBuffer> hexes,
			in HexHexGridData hexHexGridData,
			Entity e)
		{
			hexHexGridData.FillMeshData(hexes, ref renderBounds, MeshDataArray[idx]);
			ECB.AddComponent(chunkIdx, e, new MeshDataArrayComponent
			{
				ID = MeshDataArrayID,
				Index = idx
			});
			ECB.RemoveComponent<MeshOutdatedTag>(chunkIdx, e);
		}
	}
}