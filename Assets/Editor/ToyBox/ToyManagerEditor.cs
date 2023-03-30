using System.Collections.Generic;
using UnityEditor;
using UnityEngine;



[CustomEditor(typeof(ToyManager))]
public class ToyManagerEditor : Editor {


    public override void OnInspectorGUI() {
        ToyManager manager = (ToyManager)target;

        EditorGUILayout.BeginVertical();

        if (GUILayout.Button("Update Scene List")) {
            //This code was modified from: https://answers.unity.com/questions/1128694/how-can-i-get-a-list-of-all-scenes-in-the-build.html
            List<string> scenes = new List<string>();
            foreach (EditorBuildSettingsScene scene in EditorBuildSettings.scenes) {
                if (scene.enabled) {
                    scenes.Add(System.IO.Path.GetFileNameWithoutExtension(scene.path));
                }
            }
            manager.scenes = scenes.ToArray();
            EditorUtility.SetDirty(manager);
            PrefabUtility.RecordPrefabInstancePropertyModifications(manager);
        }
        EditorGUILayout.EndVertical();
        DrawDefaultInspector();
    }


}
