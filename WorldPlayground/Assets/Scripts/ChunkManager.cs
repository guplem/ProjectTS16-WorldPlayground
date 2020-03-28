using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class ChunkManager : MonoBehaviour
{
    public static Vector2Int size = new Vector2Int(10,128); // XZ, Y     //    16, 256
    public Vector2Int position { get; private set; }
    private Cube[,,] chunkData = new Cube[size.x,size.y,size.x];
    private Cube cube = new Cube();
    [SerializeField] private MeshFilter meshFilter;
    
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.white;
        Gizmos.DrawWireCube(transform.position+Vector3.up*size.y/2, new Vector3(size.x, size.y, size.x));
        //Gizmos.DrawWireCube(transform.position, new Vector3(size.x, 0, size.x));

        /*Gizmos.color = new Color(35/255f, 110/225f, 34/225f);
        for (int y = 0; y < size.y; y++)
            for (int x = 0; x < size.x; x++)
                for (int z = 0; z < size.x; z++)
                {
                    Cube c = chunkData[x, y, z];
                    if (c != null)
                        Gizmos.DrawSphere(GetPositionInRealWorldOfChunkDataAt(new Vector3Int(x,y,z)), 0.25f);
                }*/
    }

    public Vector3 GetPositionInRealWorldOfChunkDataAt(Vector3Int vector3Int)
    {
        return transform.position - new Vector3((size.x-1) / 2f, -0.5f, (size.x-1) / 2f) + new Vector3(vector3Int.x, vector3Int.y, vector3Int.z);
    }

    public void InitializeAt(Vector2Int position)
    {
        // Set the new position
        transform.position = new Vector3(position.x, 0, position.y);
        this.position = position;
        
        // Generate the data
        GenerateChunkData();
        
        
        //Build the mesh with the chunk data //TODO: remove. Shouldn't happen 
        UpdateMesh();
    }

    private void GenerateChunkData()
    {
        for (int y = 0; y < size.y; y++)
            for (int x = 0; x < size.x; x++)
                for (int z = 0; z < size.x; z++)
                    if (y < 100)
                        chunkData[x, y, z] = cube;
    }

    
    
    
    
    
    List<Vector3> verticesInMesh = new List<Vector3>(); // All mesh vertices
    List<int> indexOfVerticesToFormTriangles = new List<int>(); // Index of the vertices that will form each of the triangles (that form all the faces). They must be in order (and grouped by 3) to form every triangle, so the total size must be a multiple of 3)
    List<Vector2> uvs = new List<Vector2>(); // Texture mapping values (texture coordinates for each vertex with the same index as the coordinate in this list)
    
    private void UpdateMesh()
    {
        GenerateMeshData();
        meshFilter.mesh = GetChunkMeshWithCurrentData();
    }

    private void GenerateMeshData()
    {
		int vertexIndex = 0;
		for (int face = 0; face < 6; face++) // 6 faces in a cube
		{
			for (int vertexNumber = 0; vertexNumber < 6; vertexNumber++) // 6 vertex per face
			{
				// Save the vertex position
				int vertexIndexOfTriangle = VoxelData.verticesOfFace[face, vertexNumber];
				verticesInMesh.Add(VoxelData.vertexPosition[vertexIndexOfTriangle]);
				
				// Assign the vertex to the correspondent triangle
				indexOfVerticesToFormTriangles.Add(vertexIndex);
				vertexIndex++;
				
				// Assign the texture coordinates for that vertex
				uvs.Add(VoxelData.textureCoordinates[vertexNumber]);
			}
		}
    }
    
    private Mesh GetChunkMeshWithCurrentData()
    {
        // Create a new mesh with all the information built before
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
