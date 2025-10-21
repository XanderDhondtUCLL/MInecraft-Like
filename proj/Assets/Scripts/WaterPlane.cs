using UnityEngine;
using Unity.Mathematics;

public class WaterPlane
{
    public Vector3 chunkCoordinate;
    public Mesh planeMesh;
    private Vector3[] originalVertices;
    private Vector3[] animatedVertices;
    private MeshFilter meshFilter;

    public WaterPlane(int2 chunkCoord)
    {
        this.chunkCoordinate = new Vector3(chunkCoord.x, 0, chunkCoord.y);
    }

    public Mesh RenderWaterPlane(GameObject planeObject, int height, Material water)
    {
        planeMesh = TerrainGenerator.GenerateTesselatedPlane(planeObject, chunkCoordinate, 32, 32, height, water);
        meshFilter = planeObject.GetComponent<MeshFilter>();

        originalVertices = planeMesh.vertices;
        animatedVertices = new Vector3[originalVertices.Length];
        return planeMesh;
    }

    private void Update()
    {
    }
}