using UnityEngine;

public static class PathUtil
{
    public static string GetDiskPath(string assetPath)
    {
        if(string.IsNullOrEmpty(assetPath))
        {
            return string.Empty;
        }
        if(!assetPath.StartsWith("Assets"))
        {
            return string.Empty;
        }
        return Application.dataPath + assetPath.Substring(assetPath.IndexOf("Assets") + 6);
    }
    public static string RemoveResourcePath(string fullPath)
    {
        if (string.IsNullOrEmpty(fullPath))
        {
            return null;
        }
        string subName = "Resources";
        int startIndex = fullPath.LastIndexOf(subName);
        if (startIndex == -1)
        {
            return null;
        }
        string sourceAssetPath = fullPath.Substring(startIndex + subName.Length + 1);
        sourceAssetPath = Replace(sourceAssetPath);
        return sourceAssetPath;
    }

    public static string Replace(string s)
    {
        if (string.IsNullOrEmpty(s))
        {
            return null;
        }
        return s.Replace("\\", "/");
    }
    public static string GetAssetPath(string fullPath)
    {
        if (string.IsNullOrEmpty(fullPath))
        {
            return null;
        }
        string subName = "Assets";
        int startIndex = fullPath.LastIndexOf(subName);
        if (startIndex == -1)
        {
            return null;
        }
        string sourceAssetPath = fullPath.Substring(startIndex);
        sourceAssetPath = Replace(sourceAssetPath);
        return sourceAssetPath;
    }
}

