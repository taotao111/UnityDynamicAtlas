

using UnityEngine;

public class CommonUtils
{
    public static RenderTexture CreateDyanmicAtlasRenderTexture(int width, int height, int depth, RenderTextureFormat format)
    {
        RenderTexture data = new RenderTexture(width, height, depth, format);
        data.autoGenerateMips = false;
        data.useMipMap = false;
        data.filterMode = FilterMode.Bilinear;
        data.wrapMode = TextureWrapMode.Clamp;
        data.Create();
        return data;
    }
    public static string RemoveFileExtension(string path)
    {
        if (path == null)
        {
            return null;
        }
        int __indexDot = path.LastIndexOf('.');
        if (__indexDot != -1)
        {
            return path.Substring(0, __indexDot);
        }
        return path;
    }
}
