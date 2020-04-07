using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomMesh
{
    public List<Vector3> verticesInMesh = new List<Vector3>(); // All mesh vertices
    public List<int> indexOfVerticesToFormTriangles = new List<int>(); // Index of the vertices that will form each of the triangles (that form all the faces). They must be in order (and grouped by 3) to form every triangle, so the total size must be a multiple of 3)
    public List<Vector2> uvs = new List<Vector2>(); // Texture mapping values (texture coordinates for each vertex with the same index as the coordinate in this list)
    
    private MeshFilter meshFilter;
    private MeshRenderer meshRenderer;

    public CustomMesh(MeshFilter meshFilter, MeshRenderer meshRenderer)
    {
        this.meshFilter = meshFilter;
        this.meshRenderer = meshRenderer;
    }

    public void ClearData()
    {
        verticesInMesh.Clear();
        indexOfVerticesToFormTriangles.Clear();
        uvs.Clear();
    }
    
    public void DisableMeshRenderer()
    {
        meshRenderer.enabled = false;
    }
    
    public void EnableMeshRenderer()
    {
        meshRenderer.enabled = true;
    }
    
    public void UpdateMesh()
    {
        meshFilter.mesh = GenerateMesh();
    }

    private Mesh GenerateMesh()
    {
        Mesh mesh = new Mesh
        {
            vertices = verticesInMesh.ToArray(),
            uv = uvs.ToArray(),
            triangles = indexOfVerticesToFormTriangles.ToArray(),
        };

        mesh.RecalculateNormals();
        return mesh;
    }
}
