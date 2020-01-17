using UnityEditor;
using UnityEditor.UI;
using UnityEngine;

[CustomEditor(typeof(UIPackingRawImage))]
public class UIPackingRawImageEditor : RawImageEditor
{
    private Texture lastTexture = null;
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        UIPackingRawImage myScript = (UIPackingRawImage)target;
        GUILayout.Space(5);
        EditorGUILayout.LabelField("--------------------------------------------------------------------------------------------------------------------");
        GUILayout.Space(5);
        myScript.mGroup = (DynamicAtlasGroup)EditorGUILayout.EnumPopup("Group", myScript.mGroup);
        //EditorGUILayout.LabelField("Texture Path", myScript.currentPath);

        GUILayout.Space(5);

        if (EditorApplication.isPlaying)
        {
            if (GUILayout.Button("Show PackingAtlas"))
            {
                PackingAtlasWindow.ShowWindow(myScript.mGroup);
            }
        }
        else
        {
            if ((lastTexture != myScript.texture) || (myScript.texture != null && myScript.currentPath == null))
            {
                lastTexture = myScript.texture;
                string path = PathUtil.RemoveResourcePath(AssetDatabase.GetAssetPath(myScript.texture));
                myScript.currentPath = path;
            }

            if (myScript.texture == null)
            {
                lastTexture = null;
                myScript.currentPath = null;
            }
        }
        EditorGUILayout.LabelField("--------------------------------------------------------------------------------------------------------------------");

        EditorGUILayout.LabelField("--------------------------------------------------------------------------------------------------------------------");
    }
}
