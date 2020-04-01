using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ChunkMeshGenerator
{
    public static Mesh GetMeshOf(Chunk chunk, byte[,,] chunkData)
    {
        List<Vector3> verticesInMesh = new List<Vector3>(); // All mesh vertices
        List<int> indexOfVerticesToFormTriangles = new List<int>(); // Index of the vertices that will form each of the triangles (that form all the faces). They must be in order (and grouped by 3) to form every triangle, so the total size must be a multiple of 3)
        List<Vector2> uvs = new List<Vector2>(); // Texture mapping values (texture coordinates for each vertex with the same index as the coordinate in this list)
        int currentVertexIndex = 0;

        // Create mesh data
        currentVertexIndex = 0;
        for (int y = 0; y < Chunk.size.y; y++)
        for (int x = 0; x < Chunk.size.x; x++)
        for (int z = 0; z < Chunk.size.x; z++)
        {
            Vector3Int relativePosition = new Vector3Int(x, y, z);
            
            if (chunk.GetCubeFromRelativePosition(relativePosition).isVisible)
            //Add each voxel data to mesh data
            for (int face = 0; face < 6; face++) // 6 faces in a cube
		    {
                if (!IsCubeOpaque(chunk, relativePosition+VoxelData.faceChecks[face])) //Check if face is visible 
                {
                    byte blockId = chunkData[(int) relativePosition.x, (int) relativePosition.y, (int) relativePosition.z];
                    
                    verticesInMesh.Add(relativePosition + VoxelData.vertexPosition[VoxelData.verticesOfFace[face, 0]]);
                    verticesInMesh.Add(relativePosition + VoxelData.vertexPosition[VoxelData.verticesOfFace[face, 1]]);
                    verticesInMesh.Add(relativePosition + VoxelData.vertexPosition[VoxelData.verticesOfFace[face, 2]]);
                    verticesInMesh.Add(relativePosition + VoxelData.vertexPosition[VoxelData.verticesOfFace[face, 3]]);
                    
                    AddTexture(CubesManager.Instance.GetCube(blockId).GetTextureId(face), uvs);
                    
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


        
        // Create the mesh itself
        Mesh mesh = new Mesh
        {
            vertices = verticesInMesh.ToArray(),
            uv = uvs.ToArray(),
            triangles = indexOfVerticesToFormTriangles.ToArray(),
        };

        mesh.RecalculateNormals();
        return mesh;
    }

    
    private static void AddTexture(int textureId, List<Vector2> uvs)
    {
        // ReSharper disable once PossibleLossOfFraction //The loss of fraction is intended
        float y = textureId / CubesManager.textureAtlasSizeInBlocks;
        float x = textureId - (y * CubesManager.textureAtlasSizeInBlocks);

        x *= CubesManager.normalizedBlockTextureSize;
        y *= CubesManager.normalizedBlockTextureSize;

        y = 1f - y - CubesManager.normalizedBlockTextureSize;

        uvs.Add(new Vector2(x, y));
        uvs.Add(new Vector2(x, y + CubesManager.normalizedBlockTextureSize));
        uvs.Add(new Vector2(x + CubesManager.normalizedBlockTextureSize, y));
        uvs.Add(new Vector2(x + CubesManager.normalizedBlockTextureSize, y + CubesManager.normalizedBlockTextureSize));
    }
    
    
    private static bool IsCubeOpaque(Chunk chunk, Vector3 cubePositionRelativeToTheChunk)
    {
        Cube cube = chunk.GetCubeFromRelativePosition(new Vector3Int((int) cubePositionRelativeToTheChunk.x,
            (int) cubePositionRelativeToTheChunk.y, (int) cubePositionRelativeToTheChunk.z));
        
        return cube != null && cube.isOpaque;
    }
}
