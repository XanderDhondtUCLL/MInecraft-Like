using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using Unity.Mathematics;
using UnityEngine;
using static VoxelData;


public static class TerrainGenerator
{
    public static List<Vector3> GenerateTerrain(BlockType[,,] blocks, int heightMultiplier, Vector3 chunkCoordinate)
    {
        List<Vector3> blockCoords = new List<Vector3>();
        for (int x = 0; x < blocks.GetLength(0); x++)
        {
            for (int z = 0; z < blocks.GetLength(2); z++)
            {
                int rndYheight = (int)getHeight(x, z, heightMultiplier, chunkCoordinate);
                // clamped height so it doesn't round out of chunk bounds
                if (rndYheight > 128)
                    rndYheight = 127;
                for (int y = 0; y <= rndYheight; y++)
                {
                    int rnd = UnityEngine.Random.Range(0, 10);
                    // Ensure bottom layer is always bedrock
                    if (y == 0)
                    {
                        blocks[x, y, z] = BlockType.Bedrock;
                    }
                    else if (rnd > 2 && rnd < 6 && y < 3)
                    {
                        blocks[x, y, z] = BlockType.Bedrock;
                    }

                    // Make rest of the blocks stone
                    else if (blocks[x, y, z] != BlockType.Bedrock)
                    {
                        blocks[x, y, z] = BlockType.Stone;
                    }

                    // Change dirt and grass layers where appropriate
                    if (y > rndYheight - 4 && y != rndYheight)
                    {
                        blocks[x, y, z] = BlockType.Dirt;
                    }
                    else if (y == rndYheight)
                    {
                        blocks[x, y, z] = BlockType.Grass;
                    }
                    else if (y == rndYheight - 1)
                    {
                        blocks[x, y, z] = BlockType.Sand;
                    }

                    // Convert dirt/grass below height 14 to sand
                    if (y < 18 && (blocks[x, y, z] == BlockType.Dirt || blocks[x, y, z] == BlockType.Grass))
                    {
                        blocks[x, y, z] = BlockType.Sand;
                    }

                    // if the blocks are not air, add them to the list of blocks to be rendered
                    if (blocks[x, y, z] != BlockType.Empty)
                        blockCoords.Add(new Vector3(x, y, z));
                }
            }
        }
        return blockCoords;
    }

    public static Mesh GenerateTesselatedPlane(GameObject chunkObject, Vector3 chunkCoord, int xAmountSquares, int zAmountSquares, int height, Material material)
    {
        Mesh tesselatedPlane = new Mesh();

        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();
        Vector3 chunkOffset = new Vector3(chunkCoord.x * 32, 0, chunkCoord.z * 32);

        // Generate vertices (note: <= to create (x+1)*(z+1) vertices)
        for (int z = 0; z <= zAmountSquares; z++)
        {
            for (int x = 0; x <= xAmountSquares; x++)
            {
                vertices.Add(new Vector3(x, height - 0.09f, z) + chunkOffset);
            }
        }

        // Generate triangles
        for (int z = 0; z < zAmountSquares; z++)
        {
            for (int x = 0; x < xAmountSquares; x++)
            {
                int topLeft = z * (xAmountSquares + 1) + x;
                int topRight = topLeft + 1;
                int bottomLeft = topLeft + (xAmountSquares + 1);
                int bottomRight = bottomLeft + 1;

                // Triangle 1
                triangles.Add(topLeft);
                triangles.Add(bottomLeft);
                triangles.Add(topRight);

                // Triangle 2
                triangles.Add(topRight);
                triangles.Add(bottomLeft);
                triangles.Add(bottomRight);
            }
        }

        tesselatedPlane.vertices = vertices.ToArray();
        tesselatedPlane.triangles = triangles.ToArray();
        //tesselatedPlane.RecalculateNormals();

        MeshFilter meshFilter = chunkObject.AddComponent<MeshFilter>();
        MeshRenderer meshRenderer = chunkObject.AddComponent<MeshRenderer>();

        meshRenderer.material = material;
        meshFilter.mesh = tesselatedPlane;

        return tesselatedPlane;
    }

    public static float getHeight(float x, float z, int heightMultiplier, Vector3 chunkCoordinate)
    {
        float globalX = x + chunkCoordinate.x * 32;
        float globalZ = z + chunkCoordinate.z * 32;

        float height = NoiseGenerator.GeneratePerlinNoise(globalX, globalZ);

        return height + 12f; // Slightly increased base height
    }

    public static void RenderChunk(GameObject chunkObject, Material[] materials, int heightMultiplier, BlockType[,,] blocks, Vector3 chunkCoordinate)
    {
        Mesh mesh = new Mesh();

        List<Vector3> vertices = new List<Vector3>();
        List<Vector2> uvs = new List<Vector2>(); // List to store UV coordinates

        List<List<int>> submeshTriangles = new List<List<int>>();

        // Initialize submesh triangle lists for each material
        for (int i = 0; i < materials.Length; i++)
        {
            submeshTriangles.Add(new List<int>());
        }

        List<Vector3> BlockCoords = TerrainGenerator.GenerateTerrain(blocks, heightMultiplier, chunkCoordinate);

        int vertexOffset = 0;
        foreach (var blockCoord in BlockCoords)
        {
            int x = (int)blockCoord.x;
            int y = (int)blockCoord.y;
            int z = (int)blockCoord.z;

            BlockType blockType = blocks[x, y, z];
            int submeshIndex = (int)blockType - 1; // Map BlockType to submesh index (e.g., Bedrock = 1 -> index 0)

            // Add faces with appropriate UVs for each face type
            if (z == 0 || blocks[x, y, z - 1] == BlockType.Empty) // Front face
            {
                Vector2[] faceUVs = GetFaceUVs(blockType, FaceType.Front);
                AddFace(
                    blocks,
                    chunkCoordinate,
                    vertices,
                    uvs,
                    submeshTriangles[submeshIndex],
                    blockCoord,
                    new Vector3(0, 0, 0), new Vector3(1, 0, 0), new Vector3(1, 1, 0), new Vector3(0, 1, 0),
                    faceUVs,
                    ref vertexOffset
                    );
            }

            // Left face
            if (x == 0 || blocks[x - 1, y, z] == BlockType.Empty)
            {
                Vector2[] faceUVs = GetFaceUVs(blockType, FaceType.Left);
                AddFace(
                    blocks,
                    chunkCoordinate,
                    vertices,
                    uvs,
                    submeshTriangles[submeshIndex],
                    blockCoord,
                    new Vector3(0, 0, 0), new Vector3(0, 1, 0), new Vector3(0, 1, 1), new Vector3(0, 0, 1),
                    faceUVs,
                    ref vertexOffset);
            }

            // Right face
            if (x == blocks.GetLength(0) - 1 || blocks[x + 1, y, z] == BlockType.Empty)
            {
                Vector2[] faceUVs = GetFaceUVs(blockType, FaceType.Right);
                AddFace(
                    blocks,
                    chunkCoordinate,
                    vertices,
                    uvs,
                    submeshTriangles[submeshIndex],
                    blockCoord,
                    new Vector3(1, 0, 0), new Vector3(1, 0, 1), new Vector3(1, 1, 1), new Vector3(1, 1, 0),
                    faceUVs,
                    ref vertexOffset);
            }

            // Bottom face
            if (y == 0 || blocks[x, y - 1, z] == BlockType.Empty)
            {
                Vector2[] faceUVs = GetFaceUVs(blockType, FaceType.Bottom);
                AddFace(
                    blocks,
                    chunkCoordinate,
                    vertices,
                    uvs,
                    submeshTriangles[submeshIndex],
                    blockCoord,
                    new Vector3(0, 0, 0), new Vector3(0, 0, 1), new Vector3(1, 0, 1), new Vector3(1, 0, 0),
                    faceUVs,
                    ref vertexOffset);
            }

            // Top face
            if (y == blocks.GetLength(1) - 1 || blocks[x, y + 1, z] == BlockType.Empty)
            {
                Vector2[] faceUVs = GetFaceUVs(blockType, FaceType.Top);
                AddFace(
                    blocks,
                    chunkCoordinate,
                    vertices,
                    uvs,
                    submeshTriangles[submeshIndex],
                    blockCoord,
                    new Vector3(0, 1, 0), new Vector3(1, 1, 0), new Vector3(1, 1, 1), new Vector3(0, 1, 1),
                    faceUVs,
                    ref vertexOffset);
            }

            // Back face
            if (z == blocks.GetLength(2) - 1 || blocks[x, y, z + 1] == BlockType.Empty)
            {
                Vector2[] faceUVs = GetFaceUVs(blockType, FaceType.Back);
                AddFace(
                    blocks,
                    chunkCoordinate,
                    vertices,
                    uvs,
                    submeshTriangles[submeshIndex],
                    blockCoord,
                    new Vector3(0, 0, 1), new Vector3(0, 1, 1), new Vector3(1, 1, 1), new Vector3(1, 0, 1),
                    faceUVs,
                    ref vertexOffset);
            }
        }

        mesh.vertices = vertices.ToArray();
        mesh.uv = uvs.ToArray(); // Assign UVs to the mesh

        // Assign triangles to submeshes
        mesh.subMeshCount = submeshTriangles.Count;
        for (int i = 0; i < submeshTriangles.Count; i++)
        {
            mesh.SetTriangles(submeshTriangles[i], i);
        }

        mesh.RecalculateNormals();

        MeshFilter meshFilter = chunkObject.AddComponent<MeshFilter>();
        MeshRenderer meshRenderer = chunkObject.AddComponent<MeshRenderer>();

        meshFilter.mesh = mesh;
        meshRenderer.materials = materials;
    }

    /// <summary>
    /// Add a face to the mesh with the specified vertices, UVs, and triangles.
    /// The vertices, uvs, and triangles lists are used to store the mesh data.
    /// </summary>
    /// <param name="vertices">The face is defined by four vertices (v0, v1, v2, v3) and the UV coordinates for each vertex</param>
    /// <param name="uvs"></param>
    /// <param name="triangles"></param>
    /// <param name="blockCoord">The blockCoord is the position of the block in the world</param>
    /// <param name="v0"></param>
    /// <param name="v1"></param>
    /// <param name="v2"></param>
    /// <param name="v3"></param>
    /// <param name="faceUVs">The faceUVs are the UV coordinates for the face</param>
    /// <param name="vertexOffset">The vertexOffset is used to calculate the correct indices for the triangles, automatically gets incremented by 4</param>
    public static void AddFace(BlockType[,,] blocks, Vector3 chunkCoordinate, List<Vector3> vertices, List<Vector2> uvs, List<int> triangles, Vector3 blockCoord, Vector3 v0, Vector3 v1, Vector3 v2, Vector3 v3, Vector2[] faceUVs, ref int vertexOffset)
    {
        // Scale chunkCoordinate by the chunk's dimensions
        // Reason for the y coord in the z is because the coord is in 2D
        Vector3 chunkOffset = new Vector3(chunkCoordinate.x * blocks.GetLength(0), 0, chunkCoordinate.z * blocks.GetLength(2));

        vertices.Add(blockCoord + v0 + chunkOffset);
        vertices.Add(blockCoord + v1 + chunkOffset);
        vertices.Add(blockCoord + v2 + chunkOffset);
        vertices.Add(blockCoord + v3 + chunkOffset);

        uvs.Add(faceUVs[0]);
        uvs.Add(faceUVs[1]);
        uvs.Add(faceUVs[2]);
        uvs.Add(faceUVs[3]);

        triangles.Add(vertexOffset + 0);
        triangles.Add(vertexOffset + 2);
        triangles.Add(vertexOffset + 1);
        triangles.Add(vertexOffset + 0);
        triangles.Add(vertexOffset + 3);
        triangles.Add(vertexOffset + 2);

        vertexOffset += 4;
    }

    /// <summary>
    /// Get the UV coordinates for a given block type and face type.
    /// </summary>
    /// <param name="blockType"></param>
    /// <param name="faceType"></param>
    /// <returns>An array of vec2 with UV data.</returns>
    public static Vector2[] GetFaceUVs(BlockType blockType, FaceType faceType)
    {
        // Define the size of each cell in the atlas
        float cellSize = 1.0f / 16.0f; // 16x16 grid

        // Get the starting UV coordinates for the block type
        Vector2 uvStart;
        switch (blockType)
        {
            case BlockType.Bedrock:
                uvStart = new Vector2(1 * cellSize, 14 * cellSize);
                break;

            case BlockType.Stone:
                uvStart = new Vector2(1 * cellSize, 15 * cellSize);
                break;

            case BlockType.Dirt:
                uvStart = new Vector2(2 * cellSize, 15 * cellSize);
                break;

            case BlockType.Grass:
                if (faceType == FaceType.Top)
                    uvStart = new Vector2(0 * cellSize, 15 * cellSize);
                else if (faceType == FaceType.Bottom)
                    uvStart = new Vector2(2 * cellSize, 15 * cellSize);
                else
                    uvStart = new Vector2(3 * cellSize, 15 * cellSize);
                break;

            case BlockType.Sand:
                uvStart = new Vector2(2 * cellSize, 14 * cellSize);
                break;


            default:
                uvStart = new Vector2(0, 0); // Default to the first cell
                break;
        }

        // Adjust UV order for left, back and bottom faces
        // rotated 90 degrees counter-clockwise
        if (faceType == FaceType.Back || faceType == FaceType.Left || faceType == FaceType.Bottom)
        {
            return new Vector2[]
            {
            uvStart + new Vector2(cellSize, 0),       // Bottom-right
            uvStart + new Vector2(cellSize, cellSize), // Top-right
            uvStart + new Vector2(0, cellSize),       // Top-left
            uvStart,                                  // Bottom-left
            };
        }

        // Default UV order for other faces
        return new Vector2[]
        {
        uvStart,                                    // Bottom-left
        uvStart + new Vector2(cellSize, 0),         // Bottom-right
        uvStart + new Vector2(cellSize, cellSize),  // Top-right
        uvStart + new Vector2(0, cellSize)          // Top-left
        };
    }
}
