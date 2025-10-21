using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class VoxelData
{
    public enum BlockType
    {
        Empty = 0,
        Bedrock = 1,
        Stone = 2,
        Dirt = 3,
        Grass = 4,
        Sand = 5,
    }

    public enum FaceType
    {
        Front,
        Back,
        Left,
        Right,
        Top,
        Bottom
    }
}
