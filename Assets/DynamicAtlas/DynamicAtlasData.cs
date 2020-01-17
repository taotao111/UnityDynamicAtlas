using UnityEngine;

public enum DynamicAtlasGroup
{
    Size_256 = 256,
    Size_512 = 512,
    Size_1024 = 1024,
    Size_2048 = 2048
}
public class GetImageData
{
    public string path;
    public OnCallBackTexRect callback;
    public OnCallBackMetRect BlitCallback;
}
public class SaveImageData
{
    public int texIndex = -1;
    public int referenceCount = 0;
    public Rect rect;
    public IntegerRectangle rectangle;//with padding
}
public class SortableSize
{
    public int width;
    public int height;
    public int id;

    public SortableSize(int width, int height)
    {
        this.width = width;
        this.height = height;
    }
}

public class IntegerRectangle
{
    public int x;
    public int y;
    public int width;
    public int height;
    public int id;
    public int right
    {
        get
        {
            return x + width;
        }
    }
    public int top
    {
        get
        {
            return y + height;
        }
    }
    public int size
    {
        get
        {
            return width * height;
        }
    }
    public UnityEngine.Rect rect
    {
        get
        {
            return new UnityEngine.Rect(x, y, width, height);
        }

    }
    public IntegerRectangle(int x = 0, int y = 0, int width = 0, int height = 0)
    {
        this.x = x;
        this.y = y;
        this.width = width;
        this.height = height;
    }
    public override string ToString()
    {
        return string.Format("x{0}_y:{1}_width:{2}_height{3}_top:{4}_right{5}", x, y, width, height, top, right);
    }
}

