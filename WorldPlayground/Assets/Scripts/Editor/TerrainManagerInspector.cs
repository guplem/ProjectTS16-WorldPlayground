using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(TerrainManager))]
[CanEditMultipleObjects]
public class TerrainManagerInspector : Editor
{
    //SerializedProperty lookAtPoint;
    private TerrainManager terrainManager;
    
    void OnEnable()
    {
        //lookAtPoint = serializedObject.FindProperty("lookAtPoint");
        terrainManager = (TerrainManager) target;
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        
        //serializedObject.Update();

        if(GUILayout.Button("Destroy world"))
            terrainManager.DestroyWorld();
        
        if(GUILayout.Button("Generate world"))
            terrainManager.GenerateWorld();

        if (GUILayout.Button("Regenerate world"))
        {
            terrainManager.DestroyWorld();
            terrainManager.GenerateWorld();
        }

        //serializedObject.ApplyModifiedProperties();
    }
}
