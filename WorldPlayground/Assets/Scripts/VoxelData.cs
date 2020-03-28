using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct VoxelData {
	
	public static readonly Vector3Int[] vertexPosition = new Vector3Int[8] {
		new Vector3Int(0, 0, 0),
		new Vector3Int(1, 0, 0),
		new Vector3Int(1, 1, 0),
		new Vector3Int(0, 1, 0),
		new Vector3Int(0, 0, 1),
		new Vector3Int(1, 0, 1),
		new Vector3Int(1, 1, 1),
		new Vector3Int(0, 1, 1),
	};

	public static readonly Vector3Int[] faceChecks = new Vector3Int[6] { // Offset that represents the voxel next to the face
		new Vector3Int(0, 0, -1), // Back Face
		new Vector3Int(0, 0, 1), // Front Face
		new Vector3Int(0, 1, 0), // Top Face
		new Vector3Int(0, -1, 0), // Bottom Face
		new Vector3Int(-1, 0, 0), // Left Face
		new Vector3Int(1, 0, 0), // Right Face
	};
	
	public static readonly int[,] verticesOfFace = new int[6,6] { // [face, vertex number] (the vertices are in order)
		{0, 3, 1, 1, 3, 2}, // Back Face
		{5, 6, 4, 4, 6, 7}, // Front Face
		{3, 7, 2, 2, 7, 6}, // Top Face
		{1, 5, 0, 0, 5, 4}, // Bottom Face
		{4, 7, 0, 0, 7, 3}, // Left Face
		{1, 2, 5, 5, 2, 6} // Right Face
	};

	public static readonly Vector2Int[] textureCoordinates = new Vector2Int[6] {
		new Vector2Int (0, 0),
		new Vector2Int (0, 1),
		new Vector2Int (1, 0),
		new Vector2Int (1, 0),
		new Vector2Int (0, 1),
		new Vector2Int (1, 1)
	};


}
