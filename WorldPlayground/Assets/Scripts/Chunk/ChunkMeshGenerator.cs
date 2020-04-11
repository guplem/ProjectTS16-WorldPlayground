using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ChunkMeshGenerator
{
    public static void PopulateMeshDataToDrawChunk(Chunk chunk, CustomMesh chunkCustomMesh, byte[,,] chunkData)
    {
        chunkCustomMesh.ClearData();
        
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
                    
                    chunkCustomMesh.verticesInMesh.Add(relativePosition + VoxelData.vertexPosition[VoxelData.verticesOfFace[face, 0]]);
                    chunkCustomMesh.verticesInMesh.Add(relativePosition + VoxelData.vertexPosition[VoxelData.verticesOfFace[face, 1]]);
                    chunkCustomMesh.verticesInMesh.Add(relativePosition + VoxelData.vertexPosition[VoxelData.verticesOfFace[face, 2]]);
                    chunkCustomMesh.verticesInMesh.Add(relativePosition + VoxelData.vertexPosition[VoxelData.verticesOfFace[face, 3]]);
                    
                    AddTexture(CubesManager.Instance.GetCube(blockId).GetTextureId(face), chunkCustomMesh.uvs);
                    
                    chunkCustomMesh.indexOfVerticesToFormTriangles.Add(currentVertexIndex);
                    chunkCustomMesh.indexOfVerticesToFormTriangles.Add(currentVertexIndex + 1);
                    chunkCustomMesh.indexOfVerticesToFormTriangles.Add(currentVertexIndex + 2);
                    chunkCustomMesh.indexOfVerticesToFormTriangles.Add(currentVertexIndex + 2);
                    chunkCustomMesh.indexOfVerticesToFormTriangles.Add(currentVertexIndex + 1);
                    chunkCustomMesh.indexOfVerticesToFormTriangles.Add(currentVertexIndex + 3);
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
