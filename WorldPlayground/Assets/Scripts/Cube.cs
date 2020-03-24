using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cube : MonoBehaviour {

	public MeshRenderer meshRenderer;
	public MeshFilter meshFilter;

	void Start ()
	{
		meshFilter.mesh = GetCubeMesh();
	}

	private static Mesh GetCubeMesh()
	{
		List<Vector3> verticesInMesh = new List<Vector3>(); // All mesh vertices
		List<int> indexOfVerticesToFormTriangles = new List<int>(); // Index of the vertices that will form each of the triangles (that form all the faces). They must be in order (and grouped by 3) to form every triangle, so the total size must be a multiple of 3)
		List<Vector2> uvs = new List<Vector2>(); // Texture mapping values (texture coordinates for each vertex with the same index as the coordinate in this list)

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
