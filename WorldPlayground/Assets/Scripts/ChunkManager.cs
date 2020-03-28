using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class ChunkManager : MonoBehaviour
{
    public static Vector2Int size = new Vector2Int(6,6); // XZ, Y     //    16, 256 // 10, 128
    public Vector2Int position { get; private set; }
    private byte[,,] chunkData = new byte[size.x,size.y,size.x]; //Max qty of cubes = 256. Id range = [0, 255]
    [SerializeField] private MeshFilter meshFilter;
    
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.white;
        Gizmos.DrawWireCube(transform.position+new Vector3(size.x-1, size.y-1, size.x-1)/2, new Vector3(size.x, size.y, size.x));
        //Gizmos.DrawWireCube(transform.position, new Vector3(size.x, 0, size.x));

        /*Gizmos.color = new Color(35/255f, 110/225f, 34/225f);
        for (int y = 0; y < size.y; y++)
            for (int x = 0; x < size.x; x++)
                for (int z = 0; z < size.x; z++)
                {
                    Cube c = chunkData[x, y, z];
                    //if (c != null)
                        Gizmos.DrawSphere(GetPositionInRealWorldOfChunkDataAt(new Vector3Int(x,y,z)), 0.25f);
                }*/
    }

    public Vector3 GetPositionInRealWorldOfChunkDataAt(Vector3Int vector3Int)
    {
        return transform.position + new Vector3(vector3Int.x, vector3Int.y, vector3Int.z);
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
        //Clear previous data
        chunkData = new byte[size.x,size.y,size.x];
        
        //Create new data
        for (int y = 0; y < size.y; y++)
            for (int x = 0; x < size.x; x++)
                for (int z = 0; z < size.x; z++)
                    //if (x >= y)
                        chunkData[x, y, z] = 0;
    }

    
    
    
    
    
    List<Vector3> verticesInMesh = new List<Vector3>(); // All mesh vertices
    List<int> indexOfVerticesToFormTriangles = new List<int>(); // Index of the vertices that will form each of the triangles (that form all the faces). They must be in order (and grouped by 3) to form every triangle, so the total size must be a multiple of 3)
    List<Vector2> uvs = new List<Vector2>(); // Texture mapping values (texture coordinates for each vertex with the same index as the coordinate in this list)
    int currentVertexIndex = 0;
    
    private void UpdateMesh()
    {
        // Create mesh data
        currentVertexIndex = 0;
        for (int y = 0; y < size.y; y++)
            for (int x = 0; x < size.x; x++)
                for (int z = 0; z < size.x; z++)
                    //if (chunkData[x, y, z] != null) //If there is a cube in that place
                        AddVoxelDataToChunk(new Vector3(x, y, z));
        
        // Update the mesh
        meshFilter.mesh = GetChunkMeshWithCurrentData();
    }

    private void AddVoxelDataToChunk(Vector3 relativePosition)
    {
        for (int face = 0; face < 6; face++) // 6 faces in a cube
		{
            if (!IsCubeOpaque(relativePosition+VoxelData.faceChecks[face])) //Check if face is visible 
            {
                verticesInMesh.Add(relativePosition + VoxelData.vertexPosition[VoxelData.verticesOfFace[face, 0]]);
                verticesInMesh.Add(relativePosition + VoxelData.vertexPosition[VoxelData.verticesOfFace[face, 1]]);
                verticesInMesh.Add(relativePosition + VoxelData.vertexPosition[VoxelData.verticesOfFace[face, 2]]);
                verticesInMesh.Add(relativePosition + VoxelData.vertexPosition[VoxelData.verticesOfFace[face, 3]]);
                
                uvs.Add(VoxelData.textureCoordinates[0]);
                uvs.Add(VoxelData.textureCoordinates[1]);
                uvs.Add(VoxelData.textureCoordinates[2]);
                uvs.Add(VoxelData.textureCoordinates[3]);
                
                indexOfVerticesToFormTriangles.Add(currentVertexIndex);
                indexOfVerticesToFormTriangles.Add(currentVertexIndex + 1);
                indexOfVerticesToFormTriangles.Add(currentVertexIndex + 2);
                indexOfVerticesToFormTriangles.Add(currentVertexIndex + 2);
                indexOfVerticesToFormTriangles.Add(currentVertexIndex + 1);
                indexOfVerticesToFormTriangles.Add(currentVertexIndex + 3);
                currentVertexIndex += 4;

                /* // OLD METHOD - Adding duplicated vertices but easier tu underestand
                for (int vertexNumber = 0; vertexNumber < 6; vertexNumber++) // 6 vertex per face
                {
                    // Save the vertex position
                    int vertexIndexOfTriangle = VoxelData.verticesOfFace[face, vertexNumber];
                    verticesInMesh.Add(VoxelData.vertexPosition[vertexIndexOfTriangle] + relativePosition);

                    // Assign the vertex to the correspondent triangle
                    indexOfVerticesToFormTriangles.Add(currentVertexIndex);
                    currentVertexIndex++;
				
                    // Assign the texture coordinates for that vertex
                    uvs.Add(VoxelData.textureCoordinates[vertexNumber]);
                }*/
            } 

		}
    }

    private bool IsCubeOpaque(Vector3 cubePositionRelativeToTheChunk)
    {
        Cube cube = GetCubeFromRelativePosition(new Vector3Int((int) cubePositionRelativeToTheChunk.x,
            (int) cubePositionRelativeToTheChunk.y, (int) cubePositionRelativeToTheChunk.z));
        
        return cube != null && cube.isOpaque;
    }

    public Cube GetCubeFromRelativePosition(Vector3Int relativePositionToTheChunk)
    {
        //TODO: get cube from adjacent chunk
        if (relativePositionToTheChunk.x >= size.x) return null;
        else if (relativePositionToTheChunk.z >= size.x) return null;
        else if (relativePositionToTheChunk.y >= size.y) return null;
        else if (relativePositionToTheChunk.x < 0) return null;
        else if (relativePositionToTheChunk.z < 0) return null;
        else if (relativePositionToTheChunk.y < 0) return null;
        return WorldManager.Instance.GetCube(chunkData[relativePositionToTheChunk.x, relativePositionToTheChunk.y, relativePositionToTheChunk.z]);
    }

    // Create a new mesh with the current mesh data of the chunk
    private Mesh GetChunkMeshWithCurrentData()
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
