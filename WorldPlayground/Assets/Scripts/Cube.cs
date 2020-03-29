using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
[CreateAssetMenu(fileName = "New Cube", menuName = "Cube")]
public class Cube : ScriptableObject 
{
    [HideInInspector] public int id = -1;
    public byte byteId => (byte) id;
    public string cubeName = "DefaultName";
    public bool isOpaque = true;
    
    [Header("Texture values")]
    public int backFaceTexture;
    public int frontFaceTexture;
    public int topFaceTexture;
    public int bottomFaceTexture;
    public int leftFaceTexture;
    public int rightFaceTexture;
    
    // Back, Front, Top, Bottom, Left, Right
    public int GetTextureId(int faceIndex)
    {
        switch (faceIndex)
        {
            case 0: return backFaceTexture;
            case 1: return frontFaceTexture;
            case 2: return topFaceTexture;
            case 3: return bottomFaceTexture;
            case 4: return leftFaceTexture;
            case 5: return rightFaceTexture;
            default: throw new IndexOutOfRangeException("The given index (" + faceIndex + ") does not correspond do any face. The index must be [0,5].");
        }
    }
}
