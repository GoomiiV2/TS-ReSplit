using UnityEditor;
using UnityEngine;
using System.Linq;
using System;
using TS2Data;

[CustomEditor(typeof(AnimatedModelV2))]
public class SomeEditor : Editor
{
    string[][] ModelNames; 
    int ModelNameIdx;

    void OnEnable()
    {
        CreateNameList();
    }

    void CreateNameList()
    {
        ModelNames = new string[ModelDB.Models.Length][];
        
        for (int i = 0; i < ModelDB.Models.Length; i++)
        {
            var modelsOfType = ModelDB.Models[i];
            ModelNames[i]    = modelsOfType.Keys.ToArray();
        }
    }

    public override void OnInspectorGUI ()
    {
        var targetObj = target as AnimatedModelV2;
        
        if (ModelNames == null) { CreateNameList(); }
        
        var names       = ModelNames[(int)targetObj.ModelType];
        ModelNameIdx    = Array.IndexOf(names, targetObj.ModelName);
        
        targetObj.ModelType = (ModelType)EditorGUILayout.EnumPopup("Model Type", targetObj.ModelType);
        ModelNameIdx        = EditorGUILayout.Popup("Model", ModelNameIdx, names);
        var changedModel    = names[ModelNameIdx] != targetObj.ModelName;
        targetObj.ModelName = names[ModelNameIdx];

        // Draw the default inspector
        DrawDefaultInspector();

        if (changedModel)
        {
            targetObj.LoadModel();
            EditorUtility.SetDirty(targetObj);

            //var sObj = new SerializedObject(targetObj);
            //sObj.ApplyModifiedProperties();
        }
    }
}