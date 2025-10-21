using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using static VoxelData;



public class Chunk
{
    public Vector3 chunkCoordinate;
    private BlockType[,,] blocks = new BlockType[32, 128, 32];

    public Chunk(int2 ChunkCoord)
    {
        this.chunkCoordinate = new Vector3(ChunkCoord.x, 0, ChunkCoord.y);
    }

    // Renders the chunk
    public void RenderChunk(GameObject chunkObject, Material[] materials, int heightMultiplier)
    { 
        TerrainGenerator.RenderChunk(chunkObject, materials, heightMultiplier, blocks, chunkCoordinate);
    }
}
