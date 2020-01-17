using UnityEditor;
using UnityEditor.UI;
using UnityEngine;

[CustomEditor(typeof(UIPackingImage))]
public class UIPackingImageEditor : ImageEditor
{
    private Sprite lastTexture = null;
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        UIPackingImage myScript = (UIPackingImage)target;
        GUILayout.Space(5);
        EditorGUILayout.LabelField("--------------------------------------------------------------------------------------------------------------------");
        GUILayout.Space(5);
        myScript.mGroup = (DynamicAtlasGroup)EditorGUILayout.EnumPopup("Group", myScript.mGroup);
        myScript.mAndroidTextureFormat = (TextureFormat)EditorGUILayout.EnumPopup("Android TextureFormat", myScript.mAndroidTextureFormat);
        myScript.mIosTextureFormat = (TextureFormat)EditorGUILayout.EnumPopup("IOS TextureFormat", myScript.mIosTextureFormat);
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
