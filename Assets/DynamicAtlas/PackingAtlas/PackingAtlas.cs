
using System.Collections.Generic;
using UnityEngine;

/**
    * Class used to pack rectangles within container rectangle with close to optimal solution.
    */
public class PackingAtlas
{
    private float mUVXDiv, mUVYDiv;
    private int mWidth = 0;
    private int mHeight = 0;
    private int mPadding = 8;
    private float offset = 1;
    private Material mMaterial;
    private int mBlitParamId;
    private DynamicAtlasGroup _PackingAtlasGroup;
    private Color32[] tmpColor;

    private List<Texture2D> m_tex2DList = new List<Texture2D>();
    private List<RenderTexture> m_RenderTexList = new List<RenderTexture>();
    private List<Material> m_MaterialList = new List<Material>();

    private List<Vector2> m_FindRange = new List<Vector2>();

    private List<IntegerRectangle> mNewFreeAreas = new List<IntegerRectangle>();
    private List<GetImageData> mGetImageTasks = new List<GetImageData>();

    private List<List<IntegerRectangle>> mFreeAreasList = new List<List<IntegerRectangle>>();

    private IntegerRectangle mOutsideRectangle;

    private Dictionary<string, SaveImageData> _usingRect = new Dictionary<string, SaveImageData>();
    public List<Texture2D> tex2DList { get { return m_tex2DList; } }

    public List<RenderTexture> renderTexList { get { return m_RenderTexList; } }
    public int atlasWidth { get { return mWidth; } }
    public int atlasHeight { get { return mHeight; } }
    public int padding { get { return mPadding; } }
    public PackingAtlas(DynamicAtlasGroup group)
    {
        int length = (int)group;
        mWidth = length;
        mHeight = length;
        tmpColor = new Color32[length * length];
        for (int k = 0; k < tmpColor.Length; ++k)
        {
            tmpColor[k] = Color.clear;
        }
        mUVXDiv = 1f / mWidth;
        mUVYDiv = 1f / mHeight;
        mOutsideRectangle = new IntegerRectangle(mWidth + 1, mWidth + 1, 0, 0);

        if (AtlasConfig.kUsingCopyTexture == false)
        {
            mMaterial = new Material(Shader.Find("DynamicAtlas/GraphicBlit"));
            mBlitParamId = Shader.PropertyToID("_DrawRect");
        }

        CreateNewAtlas();
    }
    void CreateNewAtlas()
    {
        if (AtlasConfig.kUsingCopyTexture)
        {
            var tex2D = new Texture2D(mWidth, mHeight, AtlasConfig.kTextureFormat, false, true);
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
        Vector2 range = Vector2.zero;
        m_FindRange.Add(range);
    }
    public void ClearAtlas()
    {
        foreach (List<IntegerRectangle> items in mFreeAreasList)
        {
            while (items.Count > 0)
            {
                DynamicAtlasManager.Instance.ReleaseRectangle(items.Pop());
            }
            items.Clear();
            IntegerRectangle area = DynamicAtlasManager.Instance.AllocateRectangle(0, 0, mWidth, mHeight);
            items.Add(area);
        }
        int count = m_FindRange.Count;
        for (int i = 0; i < count; i++)
        {
            m_FindRange[i] = Vector2.zero;
        }
        foreach (var item in _usingRect)
        {
            DynamicAtlasManager.Instance.ReleaseSaveImageData(item.Value);
        }
        _usingRect.Clear();
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

    void CallBackNull(GetImageData item, string path)
    {
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
                    CallBackNull(item, path);
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

    IntegerRectangle InsertArea(int width, int height, out int index)
    {
        bool justRightSize;//宽高正好是图集大小,一般不建议这样干,但是为了兼容.
        IntegerRectangle freeArea = GetFreeArea(width, height, out index, out justRightSize);
        IntegerRectangle result;
        mNewFreeAreas.Clear();
        if (justRightSize == false)
        {
            int resultWidth = (width + mPadding) > freeArea.width ? freeArea.width : (width + mPadding);
            int resultHeight = (height + mPadding) > freeArea.height ? freeArea.height : (height + mPadding);
            result = DynamicAtlasManager.Instance.AllocateRectangle(freeArea.x, freeArea.y, resultWidth, resultHeight);
            generateNewFreeAreas(index, result);
        }
        else
        {
            result = DynamicAtlasManager.Instance.AllocateRectangle(freeArea.x, freeArea.y, freeArea.width, freeArea.height);
        }
        //RemoveFreeArea(index, freeArea);//回收此freeArea,无论是正好符合还是切割，都要回收
        while (mNewFreeAreas.Count > 0)
        {
            AddFreeArea(index, mNewFreeAreas.Pop());
        }
        Vector2 range = m_FindRange[index];
        if (result.right > range.x)//尽量的往左，往高了找
            range.x = result.right;

        if (result.top > range.y)
            range.y = result.top;
        m_FindRange[index] = range;
        return result;
    }

    private void filterSelfSubAreas(int index)
    {
        List<IntegerRectangle> areas = mFreeAreasList[index];
        for (int i = areas.Count - 1; i >= 0; i--)
        {
            IntegerRectangle filtered = areas[i];
            for (int j = areas.Count - 1; j >= 0; j--)
            {
                if (i != j)
                {
                    IntegerRectangle area = areas[j];
                    if (filtered.x >= area.x && filtered.y >= area.y && filtered.right <= area.right && filtered.top <= area.top)
                    {
                        DynamicAtlasManager.Instance.ReleaseRectangle(filtered);
                        IntegerRectangle topOfStack = areas.Pop();
                        if (i < areas.Count)
                        {
                            // Move the one on the top to the freed position
                            areas[i] = topOfStack;
                        }
                        break;
                    }
                }
            }
        }
    }

    private void generateNewFreeAreas(int index, IntegerRectangle divider)
    {
        List<IntegerRectangle> areas = mFreeAreasList[index];
        for (int i = areas.Count - 1; i >= 0; i--)
        {
            IntegerRectangle area = areas[i];
            if ((divider.x >= area.right || divider.right <= area.x || divider.y >= area.top || divider.top <= area.y) == false)//divider 在area里面
            {
                generateDividedAreas(divider, area);
                areas.RemoveAt(i);
                DynamicAtlasManager.Instance.ReleaseRectangle(area);
            }
        }
        filterSelfSubAreas(index);
    }

    private void generateDividedAreas(IntegerRectangle divider, IntegerRectangle area)
    {
        //int count = 0;
        int rightDelta = area.right - divider.right;
        if (rightDelta > 0)
        {
            mNewFreeAreas.Add(DynamicAtlasManager.Instance.AllocateRectangle(divider.right, area.y, rightDelta, area.height));
            //count++;
        }

        int leftDelta = divider.x - area.x;
        if (leftDelta > 0)
        {
            mNewFreeAreas.Add(DynamicAtlasManager.Instance.AllocateRectangle(area.x, area.y, leftDelta, area.height));
            //count++;
        }

        int bottomDelta = area.top - divider.top;
        if (bottomDelta > 0)
        {
            mNewFreeAreas.Add(DynamicAtlasManager.Instance.AllocateRectangle(area.x, divider.top, area.width, bottomDelta));
            //count++;
        }

        int topDelta = divider.y - area.y;
        if (topDelta > 0)
        {
            mNewFreeAreas.Add(DynamicAtlasManager.Instance.AllocateRectangle(area.x, area.y, area.width, topDelta));
            //count++;
        }

        //if (count == 0 && (divider.width < area.width || divider.height < area.height))//没有切割
        //{
        //    // Only touching the area, store the area itself
        //    mNewFreeAreas.Add(area);
        //}
        //else
        //{
        //    DynamicAtlasManager.Instance.ReleaseRectangle(area);
        //}
    }

    IntegerRectangle GetFreeArea(int width, int height, out int index, out bool justRightSize)
    {
        IntegerRectangle best = mOutsideRectangle;
        index = -1;
        justRightSize = false;
        int paddedWidth = width + mPadding;
        int paddedHeight = height + mPadding;
        int count = mFreeAreasList.Count;
        bool isFindResult = false;
        for (int i = 0; i < count; i++)//从第一个图集开始查找
        {
            var item = mFreeAreasList[i];
            index = i;
            Vector2 range = m_FindRange[index];
            foreach (IntegerRectangle free in item)
            {
                if (free.x < range.x || free.y < range.y)
                {
                    if (free.x < best.x && paddedWidth <= free.width && paddedHeight <= free.height)
                    {
                        best = free;
                        isFindResult = true;
                        if ((paddedWidth == free.width && free.width <= free.height && free.right < mWidth) || (paddedHeight == free.height && free.height <= free.width))
                        {
                            IntegerRectangle area = item.Pop();
                            DynamicAtlasManager.Instance.ReleaseRectangle(area);
                            justRightSize = true;
                            break;
                        }
                    }
                }
                else
                {
                    // Outside the current packed area, no padding required
                    if (free.x < best.x && width <= free.width && height <= free.height)
                    {
                        best = free;
                        isFindResult = true;
                        if ((width == free.width && free.width <= free.height && free.right < mWidth) || (height == free.height && free.height <= free.width))
                        {
                            IntegerRectangle area = item.Pop();
                            DynamicAtlasManager.Instance.ReleaseRectangle(area);
                            justRightSize = true;
                            break;
                        }
                    }
                }
            }
            if (isFindResult)//找到了就往下一个图集找了
            {
                break;
            }
        }
        if (best == mOutsideRectangle) //没有找到适合的区域，那么就要申请新的图集了
        {
            CreateNewAtlas();
            index = mFreeAreasList.Count - 1;
            justRightSize = false;//不支持，也不允许让 图片大小直接等于图集大小
            return mFreeAreasList[index][0];
        }
        return best;
    }

    void AddFreeArea(int index, IntegerRectangle data)
    {
        List<IntegerRectangle> list = mFreeAreasList[index];
        list.Add(data);
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
        //GraphicsBlit( uv, srcTex, dest, mMaterial, 0 );
        GLBlit(uv, srcTex, dest, mMaterial, 0);
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
    public List<List<IntegerRectangle>> GetFreeAreas()
    {
        return mFreeAreasList;
    }
}
