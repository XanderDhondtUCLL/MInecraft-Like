using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class ChunkData : MonoBehaviour
{
    public Material bedrockMat;
    public Material stoneMat;
    public Material dirtMat;
    public Material grassMat;
    public Material sandMat;
    public Material waterMat;

    public int heightMultiplier = 10;
    private Material[] materials;

    public int renderDistance = 15;

    // Start is called before the first frame update
    void Start()
    {
        // Initialize the materials array in the Start method to avoid referencing non-static fields in a field initializer
        materials = new Material[5] { bedrockMat, stoneMat, dirtMat, grassMat, sandMat };

        // Loop to create chunks from -5, -5 to 5, 5
        for (int i = -renderDistance; i <= renderDistance; i++)
        {
            for (int j = -renderDistance; j <= renderDistance; j++)
            {
                GameObject chunkObject = new GameObject($"Chunk {i}{j}");
                GameObject waterObject = new GameObject($"Plane {i}{j}");
                Chunk chunk = new Chunk(new int2(i, j));
                chunk.RenderChunk(chunkObject, materials, heightMultiplier);

                WaterPlane waterPlane = new WaterPlane(new int2(i, j));
                waterPlane.RenderWaterPlane(waterObject, 18, waterMat);
            }
        }

        // GameObject testObject = new GameObject("Test Object");
        // WaterPlane waterPlane = new WaterPlane(new int2(0, 0));
        // waterPlane.RenderWaterPlane(testObject, 18, waterMat);
    }

    // Update is called once per frame
    void Update() { }
}
