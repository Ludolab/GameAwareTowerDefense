using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Reflection;
using System.Linq;



namespace GameAware {



    [CustomEditor(typeof(MetaDataTrackable), true)]
    public class MetaDataTrackableEditor : Editor {
        /*private static HashSet<string> RESERVED_WORDS = new HashSet<string> {
            "rigidbody",
            "rigidbody2D",
            "camera",
            "light",
            "animation",
            "constantForce",
            "gameObject",
            "renderer",
            "audio",
            "networkView",
            "collider",
            "collider2D",
            "hingeJoint",
            "particleSystem",
            "transform"
        };

        private bool props_expanded = false;
        

        private static string GameObjectScenePath(Transform tfm) {
            string ret = tfm.name;
            while(tfm.parent != null) {
                ret = tfm.parent.name + "." + ret;
                tfm = tfm.parent;
            }
            return ret;
        }
*/
        private bool gen_setting_expanded = false;

        public override void OnInspectorGUI() {
            DrawDefaultInspector();

            MetaDataTrackable mdt = (MetaDataTrackable)target;

            gen_setting_expanded = EditorGUILayout.BeginFoldoutHeaderGroup(gen_setting_expanded, "MetaData Settings");
            if (gen_setting_expanded) {
                mdt.FrameType = (MetaDataFrameType)EditorGUILayout.EnumPopup("Frame Type", mdt.FrameType);
                mdt.ScreenRectStyle = (ScreenSpaceReference)EditorGUILayout.EnumPopup("Screen Rect Style", mdt.ScreenRectStyle);
                mdt.PersistAcrossScenes = EditorGUILayout.Toggle("Persist Across Scenes", mdt.PersistAcrossScenes);
            }

            EditorGUILayout.EndFoldoutHeaderGroup();


            //props_expanded = EditorGUILayout.BeginFoldoutHeaderGroup(props_expanded, "Track Properties:");
            //Transform Properties
                //ScreenSpace Position / box
                //TransformPosition
                //TransformRotation
                //TrasnformScale
            //GameObject Properties
                //name
                    //Name as ObjectKey
                //tag
            //Other MonoBehaviors
                //ignore built ins and stuff from the GameObject
                //Do ComponentsInChildren from transform.root?
            //Check out: https://docs.unity3d.com/Manual/TreeViewAPI.html


            //expanded = EditorGUILayout.Foldout(expanded, "Track Properties");
          /*  if (props_expanded) {
                // Put in the common settings
                EditorGUI.indentLevel++;
                foreach (Component comp in mdt.GetComponents<Component>()) {
                    *//*if(comp.GetType() == typeof(Transform)) {
                        continue;
                    }*//*
                    EditorGUILayout.LabelField(comp.GetType().Name);
                    EditorGUI.indentLevel++;
                    foreach (PropertyInfo pi in comp.GetType().GetProperties().Where(pi => !RESERVED_WORDS.Contains(pi.Name))) {
                        EditorGUILayout.ToggleLeft(string.Format("{0}({1}): {2}", pi.Name, pi.PropertyType.Name, pi.GetValue(comp)), false);
                    }
                    EditorGUI.indentLevel--;
                }
                EditorGUI.indentLevel--;
            }
            EditorGUILayout.EndFoldoutHeaderGroup();*/
        }
    }
}