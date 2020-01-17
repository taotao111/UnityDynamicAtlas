
using System.Collections.Generic;
using UnityEngine;

public class DynamicAtlasManager : Singleton<DynamicAtlasManager>
{
    private Dictionary<DynamicAtlasGroup, DynamicAtlas> m_DynamicAtlas = new Dictionary<DynamicAtlasGroup, DynamicAtlas>();
    private Dictionary<DynamicAtlasGroup, PackingAtlas> m_PackingAtlas = new Dictionary<DynamicAtlasGroup, PackingAtlas>();

    public DynamicAtlas GetDynamicAtlas(DynamicAtlasGroup group, bool topFirst = true)
    {
        DynamicAtlas data;
        if (m_DynamicAtlas.ContainsKey(group))
        {
            data = m_DynamicAtlas[group];
        }
        else
        {
            data = new DynamicAtlas(group, topFirst);
            m_DynamicAtlas[group] = data;
        }
        return data;
    }

    //--------------------------------
    public PackingAtlas GetPackingAtlas(DynamicAtlasGroup group)
    {
        PackingAtlas data;
        if (m_PackingAtlas.ContainsKey(group))
        {
            data = m_PackingAtlas[group];
        }
        else
        {
            data = new PackingAtlas(group);
            m_PackingAtlas[group] = data;
        }
        return data;
    }
    //--------------------------------------
    private List<IntegerRectangle> mRectangleStack = new List<IntegerRectangle>();
    private List<SaveImageData> mSaveImageDataStack = new List<SaveImageData>();
    private List<GetImageData> mGetImageDataStack = new List<GetImageData>();

    public IntegerRectangle AllocateRectangle(int x, int y, int width, int height)
    {
        if (mRectangleStack.Count > 0)
        {
            IntegerRectangle rectangle = mRectangleStack.Pop();
            rectangle.x = x;
            rectangle.y = y;
            rectangle.width = width;
            rectangle.height = height;
            return rectangle;
        }
        return new IntegerRectangle(x, y, width, height);
    }

    public void ReleaseRectangle(IntegerRectangle rectangle)
    {
        mRectangleStack.Add(rectangle);
    }

    public SaveImageData AllocateSaveImageData(Rect rect)
    {
        if (mSaveImageDataStack.Count > 0)
        {
            SaveImageData popData = mSaveImageDataStack.Pop();
            popData.rect = rect;
            return popData;
        }
        SaveImageData data = new SaveImageData();
        data.rect = rect;
        return data;
    }

    public void ReleaseSaveImageData(SaveImageData data)
    {
        data.rectangle = null;
        data.referenceCount = 0;
        mSaveImageDataStack.Add(data);
    }

    public GetImageData AllocateGetImageData()
    {
        if (mGetImageDataStack.Count > 0)
        {
            GetImageData popData = mGetImageDataStack.Pop();
            return popData;
        }
        GetImageData data = new GetImageData();
        return data;
    }

    public void ReleaseGetImageData(GetImageData data)
    {
        mGetImageDataStack.Add(data);
    }
    public void ClearAllCache()
    {
        mSaveImageDataStack.Clear();
        mGetImageDataStack.Clear();
        mRectangleStack.Clear();
    }
}
