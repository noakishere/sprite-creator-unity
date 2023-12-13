using UnityEngine;
using UnityEditor;
using System.Runtime.InteropServices.ComTypes;

[CustomEditor(typeof(StatueRulesDB)), CanEditMultipleObjects]
public class StatueDataEditor : Editor
{
    public override void OnInspectorGUI()
    {
        StatueRulesDB statueData = (StatueRulesDB)target;

        if (GUILayout.Button("Initialize Statue Groups Data"))
        {
            bool userConfirmed = EditorUtility.DisplayDialog("Attention!!!!",
                "Please beware that this will reinstantiate the statue groups" +
                " and you will LOSE data!", "Yes", "No");

            if (userConfirmed)
            {
                statueData.InitializeStatueGroups();
                EditorUtility.SetDirty(statueData);
            }
        }

        if (GUILayout.Button("Match parent parts with the options"))
        {
            statueData.UpdateImagesZIndex();
            EditorUtility.SetDirty(statueData);
        }

        base.OnInspectorGUI();
    }
}
