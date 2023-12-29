using BovineLabs.Core.Collections;
using Components;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Rendering;
using UnityEngine;
using UnityEngine.Rendering;

namespace MyNamespace.Jobs
{
    public partial struct InitHexHexGridJob : IJobEntity
    {

        // Output
        public EntityCommandBuffer.ParallelWriter ECB;

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
            ECB.AddComponent(chunkIdx, e, new MeshOutdatedTag());
        }

        private HexBuffer CreateHex(HexCoordinates coords)
        {
            return new HexBuffer(coords);
        }

        private void InitHexGrid(DynamicBuffer<HexBuffer> hexes, HexHexGridData hexGridData)
        {
            hexes.ResizeUninitialized(HexHelpers.GetNumHexes(hexGridData.Radius));
            var direction = HexCoordinates.NE;
            var currentCoords = new HexCoordinates(0, 0);
            var idx = 0;
            hexes[idx++] = CreateHex(currentCoords);
            for (int i = 0; i < hexGridData.Radius; i++)
            {
                // each edge within cycle
                for (int j = 0; j < 6; j++)
                {
                    // each hex within edge
                    direction = HexHelpers.DirectionMap[direction];
                    for (int k = 0; k < i; k++)
                    {
                        currentCoords += direction;
                        hexes[idx++] = CreateHex(currentCoords);
                    }
                }

                currentCoords += HexCoordinates.N;
            }
        }
    }
}