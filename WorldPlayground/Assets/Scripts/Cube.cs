using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
[CreateAssetMenu(fileName = "New Cube", menuName = "Cube")]
public class Cube : ScriptableObject 
{
    [HideInInspector] public int id = -1;
    public string cubeName = "DefaultName";
    public bool isOpaque = true;

}
