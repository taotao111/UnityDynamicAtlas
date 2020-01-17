using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
public class DynamicAtlasWindow : EditorWindow
{
    private static DynamicAtlasWindow _DynamicAtlasWindow;
    private DynamicAtlasGroup _mGroup;
    private float scale = 0.2f;
    private List<Color32[]> texColorsList = new List<Color32[]>();
    private List<Texture2D> texList = new List<Texture2D>();
    private Color32[] mFillColor;
    private List<Texture2D> tempTex2DList;
    private bool isShowFreeAreas = false;
    private bool isRefreshFreeAreas = true;
    private float formPosY = 62;
    public static void ShowWindow(DynamicAtlasGroup mGroup)
    {
        if (_DynamicAtlasWindow == null)
        {
            _DynamicAtlasWindow = GetWindow<DynamicAtlasWindow>();
        }
        _DynamicAtlasWindow.Show();
        _DynamicAtlasWindow.Init(mGroup);
        _DynamicAtlasWindow.titleContent.text = "PackingAtlas";
    }
    public void Init(DynamicAtlasGroup mGroup)
    {
        _mGroup = mGroup;
    }
    public void OnGUI()
    {
        if (EditorApplication.isPlaying == false)
        {
            _DynamicAtlasWindow.Close();
            return;
        }
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.BeginVertical();
        EditorGUILayout.LabelField("--------------------------------------------------------------------------------");

        DynamicAtlas dynamicAtlas = DynamicAtlasManager.Instance.GetDynamicAtlas(_mGroup);
        EditorGUILayout.LabelField("图集尺寸：" + dynamicAtlas.atlasWidth + " x " + dynamicAtlas.atlasHeight);
        EditorGUILayout.LabelField("--------------------------------------------------------------------------------");
        EditorGUILayout.EndVertical();
        GUILayout.Space(10);
        EditorGUILayout.BeginVertical();
        isShowFreeAreas = GUILayout.Toggle(isShowFreeAreas, "Show Free Areas", GUILayout.Width(200), GUILayout.Height(20));

        EditorGUILayout.BeginHorizontal();
        if (isShowFreeAreas)
        {
            if (GUILayout.Button("Refresh and Clear Free Area", GUILayout.Width(200), GUILayout.Height(20)))
            {
                isRefreshFreeAreas = true;
                ClearFreeAreas();
            }
        }
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.EndVertical();
        scale = EditorGUILayout.Slider(scale, 0.2f, 1);
        EditorGUILayout.EndHorizontal();


        if (AtlasConfig.kUsingCopyTexture)
        {
            List<Texture2D> tex2DList = dynamicAtlas.tex2DList;
            int count = tex2DList.Count;
            for (int i = 0; i < count; i++)
            {
                Texture2D tex2D = tex2DList[i];
                float poxX = (i + 1) * 10 + i * dynamicAtlas.atlasWidth * scale;
                if (isShowFreeAreas)
                {
                    DrawFreeArea(i, dynamicAtlas);
                }
                GUI.DrawTexture(new Rect(poxX, formPosY, dynamicAtlas.atlasWidth * scale, dynamicAtlas.atlasHeight * scale), tex2D);
            }
        }
        else
        {
            List<RenderTexture> renderTexList = dynamicAtlas.renderTexList;
            int count = renderTexList.Count;
            for (int i = 0; i < count; i++)
            {
                float poxX = (i + 1) * 10 + i * dynamicAtlas.atlasWidth * scale;
                if (isShowFreeAreas)
                {
                    DrawFreeArea(i, dynamicAtlas);
                }
                GUI.DrawTexture(new Rect(poxX, formPosY, dynamicAtlas.atlasWidth * scale, dynamicAtlas.atlasHeight * scale), renderTexList[i]);
            }
        }

        if (isShowFreeAreas)
        {
            isRefreshFreeAreas = false;
        }
    }

    void DrawFreeArea(int index, DynamicAtlas dynamicAtlas)
    {
        Texture2D tex2D = null;
        if (texList.Count < index + 1)
        {
            tex2D = new Texture2D((int)_mGroup, (int)_mGroup, TextureFormat.ARGB32, false, true);
            texList.Add(tex2D);
            if (mFillColor == null)
            {
                mFillColor = tex2D.GetPixels32();
                for (int i = 0; i < mFillColor.Length; ++i)
                    mFillColor[i] = Color.clear;
            }
        }
        else
        {
            tex2D = texList[index];
        }
        tex2D.SetPixels32(mFillColor);
        if (isRefreshFreeAreas)
        {
            Color32[] tmpColor;
            List<IntegerRectangle> freeList = dynamicAtlas.GetFreeAreas()[index];
            foreach (IntegerRectangle item in freeList)
            {
                int size = item.width * item.height;
                tmpColor = new Color32[size];
                for (int k = 0; k < size; ++k)
                {
                    tmpColor[k] = Color.green;//画边
                }
                tex2D.SetPixels32(item.x, item.y, item.width, item.height, tmpColor);
                int outLineSize = 2;
                if (item.width < outLineSize * 2 || item.height < outLineSize * 2)
                {
                    outLineSize = 0;
                }

                size -= outLineSize * 4;
                tmpColor = new Color32[size];
                for (int k = 0; k < size; ++k)
                {
                    tmpColor[k] = Color.yellow;
                }
                tex2D.SetPixels32(item.x + outLineSize, item.y + outLineSize, item.width - outLineSize * 2, item.height - outLineSize * 2, tmpColor);
                tex2D.Apply();
            }
        }

        float poxX = (index + 1) * 10 + index * dynamicAtlas.atlasWidth * scale;
        GUI.DrawTexture(new Rect(poxX, formPosY, dynamicAtlas.atlasWidth * scale, dynamicAtlas.atlasHeight * scale), tex2D);
    }
    void ClearFreeAreas()
    {
        DynamicAtlas dynamicAtlas = DynamicAtlasManager.Instance.GetDynamicAtlas(_mGroup);
        if (AtlasConfig.kUsingCopyTexture)
        {
            List<List<IntegerRectangle>> freeLists = dynamicAtlas.GetFreeAreas();
            int freeListsCount = freeLists.Count;
            List<Texture2D> tex2DList = dynamicAtlas.tex2DList;
            for (int i = 0; i < freeListsCount; i++)
            {
                var freeList = freeLists[i];
                int freeListCount = freeList.Count;
                Texture2D dstTex = tex2DList[i];
                for (int j = 0; j < freeListCount; j++)
                {
                    IntegerRectangle item = freeList[j];
                    Color32[] colors = new Color32[item.width * item.height];
                    for (int k = 0; k < colors.Length; ++k)
                    {
                        colors[k] = Color.clear;
                    }
                    dstTex.SetPixels32((int)item.x, (int)item.y, item.width, item.height, colors);
                    dstTex.Apply();
                }
            }
        }
        else
        {
            dynamicAtlas.ClearTextureWithBlit();
        }
    }
}
