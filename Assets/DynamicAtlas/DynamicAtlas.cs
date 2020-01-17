
using System.Collections.Generic;
using UnityEngine;

public class DynamicAtlas
{
    private float mUVXDiv, mUVYDiv;
    private List<Texture2D> m_tex2DList = new List<Texture2D>();
    private List<RenderTexture> m_RenderTexList = new List<RenderTexture>();
    private List<Material> m_MaterialList = new List<Material>();

    private bool mTopFirst = true;
    private float offset = 1;
    private int mWidth = 0;
    private int mHeight = 0;
    private int mPadding = 3;
    private Color32[] tmpColor;
    private DynamicAtlasGroup _DynamicAtlasGroup;

    private Dictionary<string, SaveImageData> _usingRect = new Dictionary<string, SaveImageData>();

    private List<GetImageData> mGetImageTasks = new List<GetImageData>();

    private List<List<IntegerRectangle>> mFreeAreasList = new List<List<IntegerRectangle>>();

    public List<Texture2D> tex2DList { get { return m_tex2DList; } }
    public List<RenderTexture> renderTexList { get { return m_RenderTexList; } }
    public int atlasWidth { get { return mWidth; } }
    public int atlasHeight { get { return mHeight; } }

    public int padding { get { return mPadding; } }

    private Material mMaterial;
    private int mBlitParamId;
    void CreateNewAtlas()
    {
        if (AtlasConfig.kUsingCopyTexture)
        {
            Texture2D tex2D = new Texture2D(mWidth, mHeight, AtlasConfig.kTextureFormat, false, true);
            tex2D.filterMode = FilterMode.Bilinear;
            //tex2D.alphaIsTransparency = true;
            tex2D.SetPixels32(0, 0, mWidth, mWidth, tmpColor);
            tex2D.Apply(false);
            m_tex2DList.Add(tex2D);
        }
        else
        {
            RenderTexture renderTexture = CommonUtils.CreateDyanmicAtlasRenderTexture(mWidth, mHeight, 0, AtlasConfig.kRenderTextureFormat);
            renderTexture.name = string.Format("DynamicAtlas {0:G} -- {0:G}", mWidth, mHeight);
            renderTexture.DiscardContents(true, true);
            Material mDefaultMaterial = new Material(Shader.Find("UI/Default"));
            mDefaultMaterial.mainTexture = renderTexture;
            m_MaterialList.Add(mDefaultMaterial);

            mMaterial.SetVector(mBlitParamId, new Vector4(0, 0, 1, 1));
            Texture2D cleared_texture = new Texture2D(2, 2, AtlasConfig.kTextureFormat, false);
            Graphics.Blit(cleared_texture, renderTexture, mMaterial);
            m_RenderTexList.Add(renderTexture);
        }

        IntegerRectangle area = DynamicAtlasManager.Instance.AllocateRectangle(0, 0, mWidth, mHeight);
        List<IntegerRectangle> list = new List<IntegerRectangle>();
        list.Add(area);
        mFreeAreasList.Add(list);
    }

    public void Clear()
    {
        foreach (var item in _usingRect)
        {
            DynamicAtlasManager.Instance.ReleaseSaveImageData(item.Value);
        }
        _usingRect.Clear();
    }
    public DynamicAtlas(DynamicAtlasGroup group, bool topFirst)
    {
        mTopFirst = topFirst;
        _DynamicAtlasGroup = group;

        int length = (int)group;
        tmpColor = new Color32[length * length];
        for (int k = 0; k < tmpColor.Length; ++k)
        {
            tmpColor[k] = Color.clear;
        }
        if (AtlasConfig.kUsingCopyTexture == false)
        {
            mMaterial = new Material(Shader.Find("DynamicAtlas/GraphicBlit"));
            mBlitParamId = Shader.PropertyToID("_DrawRect");
        }

        mWidth = length;
        mHeight = length;
        mPadding = padding;
        mUVXDiv = 1f / mWidth;
        mUVYDiv = 1f / mHeight;
        CreateNewAtlas();
    }
    private void CopyTexture(int posX, int posY, int index, Texture2D srcTex)
    {
        Texture2D dstTex = m_tex2DList[index];
        Graphics.CopyTexture(srcTex, 0, 0, 0, 0, srcTex.width, srcTex.height, dstTex, 0, 0, posX, posY);
    }
    private void BlitTexture(int posX, int posY, int index, Texture2D srcTex)
    {
        Rect uv = new Rect(posX * mUVXDiv, posY * mUVYDiv, srcTex.width * mUVXDiv, srcTex.height * mUVYDiv);
        RenderTexture dest = m_RenderTexList[index];
        GraphicsBlit(uv, srcTex, dest, mMaterial, 0);
        //GLBlit(uv, srcTex, dest, mMaterial, 0);
    }
    public void ClearTextureWithBlit()
    {
        List<List<IntegerRectangle>> freeLists = GetFreeAreas();
        int freeListsCount = freeLists.Count;
        for (int i = 0; i < freeListsCount; i++)
        {
            var freeList = freeLists[i];
            int freeListCount = freeList.Count;
            RenderTexture dest = renderTexList[i];
            foreach (IntegerRectangle freeItem in freeList)
            {
                Rect uv = new Rect(freeItem.x * mUVXDiv, freeItem.y * mUVYDiv, freeItem.width * mUVXDiv, freeItem.height * mUVYDiv);
                Texture2D srcTex = new Texture2D(2, 2, AtlasConfig.kTextureFormat, false);
                GraphicsBlit(uv, srcTex, dest, mMaterial, 0);
                //GLBlit( uv, srcTex, dest, mMaterial, 0 );
            }
        }
    }
    void GraphicsBlit(Rect rc, Texture source, RenderTexture dest, Material fxMaterial, int passNr)
    {
        fxMaterial.SetVector(mBlitParamId, new Vector4(rc.xMin, rc.yMin, rc.xMax, rc.yMax));
#if UNITY_EDITOR
        dest.DiscardContents();
#endif
        Graphics.Blit(source, dest, fxMaterial);
    }

    void GLBlit(Rect rc, Texture source, RenderTexture dest, Material fxMaterial, int passNr)
    {
        Rect uv = new Rect(0, 0, 1, 1);
        dest.MarkRestoreExpected();
        Graphics.SetRenderTarget(dest);
        fxMaterial.SetTexture("_MainTex", source);

        GL.PushMatrix();
        GL.LoadOrtho();

        fxMaterial.SetPass(passNr);//Activate the given pass for rendering

        GL.Begin(GL.QUADS);

        GL.TexCoord2(uv.xMin, uv.yMin);
        GL.Vertex3(rc.xMin, rc.yMin, 0.1f); // BL

        GL.TexCoord2(uv.xMax, uv.yMin);
        GL.Vertex3(rc.xMax, rc.yMin, 0.1f); // BR

        GL.TexCoord2(uv.xMax, uv.yMax);
        GL.Vertex3(rc.xMax, rc.yMax, 0.1f); // TR

        GL.TexCoord2(uv.xMin, uv.yMax);
        GL.Vertex3(rc.xMin, rc.yMax, 0.1f); // TL

        GL.End();
        GL.PopMatrix();
    }
    //bilt
    public void SetTexture(string path, Texture _texture, OnCallBackMetRect callback)
    {
        if (_texture == null || _texture.width > mWidth || _texture.height > mHeight)//不符合规格
        {
            if (callback != null)
            {
                callback(null, new Rect(0, 0, 0, 0), path);//完成一个任务，就回调一个任务
            }
            Debug.Log("Texture Does not meet the standard:" + path);
            return;
        }
        if (_usingRect.ContainsKey(path))
        {
            if (callback != null)
            {
                SaveImageData imagedata = _usingRect[path];
                imagedata.referenceCount++;
                Material material = m_MaterialList[imagedata.texIndex];
                callback(material, _usingRect[path].rect, path);
            }
            return;
        }
        GetImageData data = DynamicAtlasManager.Instance.AllocateGetImageData();
        data.path = path;
        data.BlitCallback = callback;
        mGetImageTasks.Add(data);
        Texture2D tex = (Texture2D)_texture;
        OnRenderTexture(path, tex, null);
    }
    //copytexture
    public void SetTexture(string path, Texture _texture, OnCallBackTexRect callback)
    {
        if (_texture == null || _texture.width > mWidth || _texture.height > mHeight)//不符合规格
        {
            if (callback != null)
            {
                callback(null, new Rect(0, 0, 0, 0), path);//完成一个任务，就回调一个任务
            }
            Debug.Log("Texture Does not meet the standard:" + path);
            return;
        }
        if (_usingRect.ContainsKey(path))
        {
            if (callback != null)
            {
                SaveImageData imagedata = _usingRect[path];
                imagedata.referenceCount++;
                Texture2D tex2D = m_tex2DList[imagedata.texIndex];
                callback(tex2D, _usingRect[path].rect, path);
            }
            return;
        }
        GetImageData data = DynamicAtlasManager.Instance.AllocateGetImageData();
        data.path = path;
        data.callback = callback;
        mGetImageTasks.Add(data);
        Texture2D tex = (Texture2D)_texture;
        OnRenderTexture(path, tex, null);
    }
    //bilt
    public void GetImage(string path, OnCallBackMetRect callback)
    {
        if (_usingRect.ContainsKey(path))
        {
            if (callback != null)
            {
                SaveImageData imagedata = _usingRect[path];
                imagedata.referenceCount++;
                Material material = m_MaterialList[imagedata.texIndex];
                callback(material, _usingRect[path].rect, path);
            }
            return;
        }
        GetImageData data = DynamicAtlasManager.Instance.AllocateGetImageData();
        data.path = path;
        data.BlitCallback = callback;
        mGetImageTasks.Add(data);
        if (mGetImageTasks.Count > 1)
        {
            return;
        }
        OnGetImage();
    }

    //copytexture
    public void GetImage(string path, OnCallBackTexRect callback)
    {
        if (_usingRect.ContainsKey(path))
        {
            if (callback != null)
            {
                SaveImageData imagedata = _usingRect[path];
                imagedata.referenceCount++;
                Texture2D tex2D = m_tex2DList[imagedata.texIndex];
                callback(tex2D, _usingRect[path].rect, path);
            }
            return;
        }
        GetImageData data = DynamicAtlasManager.Instance.AllocateGetImageData();
        data.path = path;
        data.callback = callback;
        mGetImageTasks.Add(data);
        if (mGetImageTasks.Count > 1)
        {
            return;
        }
        OnGetImage();
    }

    void ClearTexture(int index, Rect rect)
    {
        Texture2D dstTex = m_tex2DList[index];
        int width = (int)rect.width;
        int height = (int)rect.height;
        Color32[] colors = new Color32[width * height];
        for (int k = 0; k < colors.Length; ++k)
        {
            colors[k] = Color.clear;
        }
        dstTex.SetPixels32((int)rect.x, (int)rect.y, width, height, colors);
        dstTex.Apply();
    }
    //image用完之后销毁，同时要在atlas上同步数据
    public void RemoveImage(string path, bool isClearRange = false)
    {
        if (_usingRect.ContainsKey(path))
        {
            SaveImageData imageData = _usingRect[path];
            imageData.referenceCount--;
            if (imageData.referenceCount == 0)//引用计数为空，则清除
            {
                if (isClearRange)
                {
                    ClearTexture(imageData.texIndex, imageData.rectangle.rect);
                }
                OnMergeArea(imageData.rectangle, imageData.texIndex);
                _usingRect.Remove(path);
                DynamicAtlasManager.Instance.ReleaseSaveImageData(imageData);
            }
        }
    }

    void OnRenderTexture(string path, Texture2D data, GetImageData imageData)
    {
        if (data == null)
        {
            for (int i = mGetImageTasks.Count - 1; i >= 0; i--)
            {
                GetImageData item = mGetImageTasks[i];
                if (item.path.Equals(path))
                {
                    if (item.callback != null)
                    {
                        item.callback(null, new Rect(0, 0, 0, 0), path);
                    }
                    if (AtlasConfig.kUsingCopyTexture)
                    {
                        if (item.callback != null)
                        {
                            item.callback(null, new Rect(0, 0, 0, 0), path);//完成一个任务，就回调一个任务
                        }
                    }
                    else
                    {
                        if (item.BlitCallback != null)
                        {
                            item.BlitCallback(null, new Rect(0, 0, 0, 0), path);//完成一个任务，就回调一个任务
                        }
                    }

                    DynamicAtlasManager.Instance.ReleaseGetImageData(item);
                    mGetImageTasks.RemoveAt(i);
                }
            }
            return;
        }
        int index;
        IntegerRectangle useArea = InsertArea(data.width, data.height, out index);

        Rect uv = new Rect((useArea.x + offset) * mUVXDiv, (useArea.y + offset) * mUVYDiv, (useArea.width - mPadding - offset * 2) * mUVXDiv, (useArea.height - mPadding - offset * 2) * mUVYDiv);
        if (AtlasConfig.kUsingCopyTexture)
        {
            CopyTexture(useArea.x, useArea.y, index, data);
        }
        else
        {
            BlitTexture(useArea.x, useArea.y, index, data);
        }

        SaveImageData _SaveImageData = DynamicAtlasManager.Instance.AllocateSaveImageData(uv);
        _SaveImageData.texIndex = index;
        _SaveImageData.rectangle = useArea;
        _usingRect[path] = _SaveImageData;

        for (int i = mGetImageTasks.Count - 1; i >= 0; i--)
        {
            GetImageData item = mGetImageTasks[i];
            if (item.path.Equals(path))
            {
                _usingRect[path].referenceCount = _usingRect[path].referenceCount + 1;
                if (AtlasConfig.kUsingCopyTexture)
                {
                    if (item.callback != null)
                    {
                        Texture2D dstTex = m_tex2DList[index];
                        item.callback(dstTex, uv, path);//完成一个任务，就回调一个任务
                    }
                }
                else
                {
                    if (item.BlitCallback != null)
                    {
                        Material material = m_MaterialList[index];
                        item.BlitCallback(material, uv, path);//完成一个任务，就回调一个任务
                    }
                }
                DynamicAtlasManager.Instance.ReleaseGetImageData(item);
                mGetImageTasks.RemoveAt(i);
            }
        }
    }
    void OnGetImage()
    {
        if (mGetImageTasks.Count == 0)
        {
            return;
        }
        GetImageData imageData = mGetImageTasks[0];
        ResourcesManager.Instance.LoadResAsync(imageData.path, (string newPath, Object obj) =>
        {
            Texture2D data = null;
            if (obj != null)
            {
                data = (Texture2D)obj;
            }
            OnRenderTexture(newPath, data, imageData);
            OnGetImage();
        });
    }

    void AddFreeArea(int index, IntegerRectangle data)
    {
        List<IntegerRectangle> list = mFreeAreasList[index];
        list.Add(data);
    }
    void RemoveFreeArea(int index, IntegerRectangle data)
    {
        DynamicAtlasManager.Instance.ReleaseRectangle(data);
        mFreeAreasList[index].Remove(data);
    }

    bool OnMergeArea(IntegerRectangle newRect, int texIndex)
    {
        IntegerRectangle mergeRect = newRect;
        bool isMerged = false;
        while (mergeRect != null)
        {
            mergeRect = OnMergeAreaRecursive(mergeRect, texIndex);
            if (mergeRect != null)
            {
                isMerged = true;
            }
        }
        return isMerged;
    }

    IntegerRectangle OnMergeAreaRecursive(IntegerRectangle target, int texIndex)
    {
        List<IntegerRectangle> data = mFreeAreasList[texIndex];
        IntegerRectangle mergeRect = null;
        foreach (var freeArea in data)
        {
            if (target.right == freeArea.x && target.y == freeArea.y && target.height == freeArea.height) //右
            {
                mergeRect = DynamicAtlasManager.Instance.AllocateRectangle(target.x, target.y, target.width + freeArea.width, target.height);
            }
            else if (target.x == freeArea.x && target.top == freeArea.y && target.width == freeArea.width)//上
            {
                mergeRect = DynamicAtlasManager.Instance.AllocateRectangle(target.x, target.y, target.width, target.height + freeArea.height);
            }
            else if (target.x == freeArea.right && target.y == freeArea.y && target.height == freeArea.height)//左
            {
                mergeRect = DynamicAtlasManager.Instance.AllocateRectangle(freeArea.x, freeArea.y, freeArea.width + target.width, freeArea.height);
            }
            else if (target.x == freeArea.x && target.y == freeArea.top && target.width == freeArea.width)//下
            {
                mergeRect = DynamicAtlasManager.Instance.AllocateRectangle(freeArea.x, freeArea.y, target.width, target.height + freeArea.height);
            }
            if (mergeRect != null)
            {
                RemoveFreeArea(texIndex, freeArea);
                return mergeRect;
            }
        }
        if (mergeRect == null)
        {
            AddFreeArea(texIndex, target);
        }
        return mergeRect;
    }

    public IntegerRectangle InsertArea(int width, int height, out int index)
    {
        IntegerRectangle result;
        bool justRightSize;
        IntegerRectangle freeArea = GetFreeArea(width, height, out index, out justRightSize);
        if (justRightSize == false)
        {
            int resultWidth = (width + mPadding) > freeArea.width ? freeArea.width : (width + mPadding);
            int resultHeight = (height + mPadding) > freeArea.height ? freeArea.height : (height + mPadding);
            result = DynamicAtlasManager.Instance.AllocateRectangle(freeArea.x, freeArea.y, resultWidth, resultHeight);
            if (mTopFirst)  //切割此freeArea，得到新的freeArea: mNewFreeAreas
            {
                GenerateDividedAreasTopFirst(index, result, freeArea);
            }
            else
            {
                GenerateDividedAreasRightFirst(index, result, freeArea);
            }
        }
        else
        {
            result = DynamicAtlasManager.Instance.AllocateRectangle(freeArea.x, freeArea.y, freeArea.width, freeArea.height); ;
        }
        RemoveFreeArea(index, freeArea);//回收此freeArea,无论是正好符合还是切割，都要回收
        return result;
    }

    //切割freeArea ，如果没有成功，说明没有切割，那只有一种解释，正好待切割的区域和该区域正好匹配大小,
    private void GenerateDividedAreasRightFirst(int index, IntegerRectangle divider, IntegerRectangle freeArea)
    {
        int rightDelta = freeArea.right - divider.right;//找到空闲区域2
        if (rightDelta > 0)
        {
            IntegerRectangle area = DynamicAtlasManager.Instance.AllocateRectangle(divider.right, divider.y, rightDelta, freeArea.height);
            AddFreeArea(index, area);
        }

        int topDelta = freeArea.top - divider.top;//找到空闲区域1
        if (topDelta > 0)
        {
            IntegerRectangle area = DynamicAtlasManager.Instance.AllocateRectangle(freeArea.x, divider.top, divider.width, topDelta);
            AddFreeArea(index, area);
        }
    }

    //切割，优先上面的区域大一些
    private void GenerateDividedAreasTopFirst(int index, IntegerRectangle divider, IntegerRectangle freeArea)
    {
        int rightDelta = freeArea.right - divider.right;//找到空闲区域2
        if (rightDelta > 0)
        {
            IntegerRectangle area = DynamicAtlasManager.Instance.AllocateRectangle(divider.right, divider.y, rightDelta, divider.height);
            AddFreeArea(index, area);
        }

        int topDelta = freeArea.top - divider.top;//找到空闲区域1
        if (topDelta > 0)
        {
            IntegerRectangle area = DynamicAtlasManager.Instance.AllocateRectangle(freeArea.x, divider.top, freeArea.width, topDelta);
            AddFreeArea(index, area);
        }
    }
    //获得最适合放texture的区域
    private IntegerRectangle GetFreeArea(int width, int height, out int index, out bool justRightSize)
    {
        index = -1;
        justRightSize = false;

        if (width > mWidth || height > mHeight)
        {
            Debug.LogError("ERROR 图片尺寸大于图集大小: 图片大小为" + width + "x" + height + "  图集大小为" + mWidth + "x" + mHeight);
            return null;
        }
        int paddedWidth = width + mPadding;
        int paddedHeight = height + mPadding;
        int count = mFreeAreasList.Count;
        IntegerRectangle tempArea = null;
        for (int i = 0; i < count; i++)//从第一个图集开始查找
        {
            var item = mFreeAreasList[i];
            bool isFindResult = false;
            foreach (var listItem in item)
            {
                IntegerRectangle area = listItem;
                bool isJustFullWidth = (width == area.width || width == mWidth);
                bool isJustFullHeight = (height == area.height || height == mHeight);
                bool isFitWidth = isJustFullWidth || paddedWidth <= area.width;
                bool isFitHeight = paddedHeight <= area.height || isJustFullHeight;
                if (isFitWidth && isFitHeight)//用最快的方式，找到能放进去的区域,如果宽或者高恰好等于图集的宽或者高，则没有paddling
                {
                    index = i;
                    justRightSize = (isJustFullWidth || paddedWidth == area.width) && (isJustFullHeight || paddedHeight == area.height);
                    if (isJustFullWidth && isJustFullHeight)
                    {
                        return area;
                    }
                    isFindResult = true;
                    if (tempArea != null)
                    {
                        if (mTopFirst)
                        {
                            if (tempArea.height > area.height)
                            {
                                tempArea = area;
                            }
                        }
                        else
                        {
                            if (tempArea.width > area.width)
                            {
                                tempArea = area;
                            }
                        }
                    }
                    else
                    {
                        tempArea = area;
                    }
                }
            }
            if (isFindResult)//找到了就往下一个图集找了
            {
                break;
            }
        }
        if (tempArea != null)
        {
            return tempArea;
        }
        //没有找到适合的区域，那么就要申请新的图集了
        CreateNewAtlas();
        index = mFreeAreasList.Count - 1;
        justRightSize = false;//不支持，也不允许让 图片大小直接等于图集大小
        return mFreeAreasList[index][0];
    }

    public List<List<IntegerRectangle>> GetFreeAreas()
    {
        return mFreeAreasList;
    }
}