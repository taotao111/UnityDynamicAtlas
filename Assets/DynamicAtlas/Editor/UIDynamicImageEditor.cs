using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.UI;

[CustomEditor(typeof(UIDynamicImage))]
public class UIDynamicImageEditor : ImageEditor
{
    private Sprite lastTexture = null;
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        UIDynamicImage myScript = (UIDynamicImage)target;
        GUILayout.Space(5);
        EditorGUILayout.LabelField("--------------------------------------------------------------------------------------------------------------------");
        GUILayout.Space(5);
        myScript.mGroup = (DynamicAtlasGroup)EditorGUILayout.EnumPopup("Group", myScript.mGroup);

        //EditorGUILayout.LabelField("Texture Path", myScript.currentPath);

        GUILayout.Space(5);


        if (EditorApplication.isPlaying)
        {
            if (GUILayout.Button("Show DynamicAtlas"))
            {
                DynamicAtlasWindow.ShowWindow(myScript.mGroup);
            }
        }
        else
        {
            if ((lastTexture != myScript.sprite) || (myScript.sprite != null && myScript.currentPath == null))
            {
                lastTexture = myScript.sprite;
                string path = PathUtil.RemoveResourcePath(AssetDatabase.GetAssetPath(myScript.sprite));
                myScript.currentPath = path;
            }

            if (myScript.sprite == null)
            {
                lastTexture = null;
                myScript.currentPath = null;
            }
        }
        EditorGUILayout.LabelField("--------------------------------------------------------------------------------------------------------------------");

        EditorGUILayout.LabelField("--------------------------------------------------------------------------------------------------------------------");
    }
}
