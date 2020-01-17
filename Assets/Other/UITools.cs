//using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

static public class UITools{

    public static string SecondsToHMS(float seconds)
    {
        System.TimeSpan time = System.TimeSpan.FromSeconds(seconds);

        //here backslash is must to tell that colon is
        //not the part of format, it just a character that we want in output
        string str = string.Format("{0:D2}:{1:D2}:{2:D2}", time.Hours, time.Minutes, time.Seconds);

        return str;
    }
    static public void SetActiveVirtual(this GameObject obj,bool isShow)
    {
        if(obj.activeSelf != isShow)
        {
            obj.SetActive(isShow);
        }
    }

    public static Vector2 roundVec2(Vector2 v)
    {
        return new Vector2(Mathf.Round(v.x), Mathf.Round(v.y));
    }

    static public int RandomRange(int min, int max)
    {
        if (min == max) return min;
        return UnityEngine.Random.Range(min, max + 1);
    }
    static public string GetHierarchy(GameObject obj)
    {
        if (obj == null) return "";
        string path = obj.name;

        while (obj.transform.parent != null)
        {
            obj = obj.transform.parent.gameObject;
            path = obj.name + "\\" + path;
        }
        return path;
    }

    static public void ReSetRectTransform(this RectTransform rect)
    {
        rect.anchoredPosition = Vector3.zero;
        rect.offsetMin = Vector3.zero;
        rect.offsetMax = Vector3.zero;
    }
    static public GameObject AddChild(this Transform parent, GameObject prefab, bool isNeedClone = true)
    {
        if(isNeedClone)
        {
            prefab = Object.Instantiate(prefab.gameObject) as GameObject;
        }

        if (prefab != null)
        {
            prefab.name = prefab.name;

            if (parent != null)
            {
                Transform t = prefab.transform;
                t.SetParent(parent);
                t.localPosition = Vector3.zero;
                t.localRotation = Quaternion.identity;
                t.localScale = Vector3.one;
            }
            prefab.SetActive(true);
        }
        return prefab;
    }
    static public void ResetRectTransSize(RectTransform rectTrans, float width, float height)
    {
        if(rectTrans != null){
            rectTrans.anchorMin = new Vector2(0.5f, 0.5f);
            rectTrans.anchorMax = new Vector2(0.5f, 0.5f);
            rectTrans.sizeDelta = new Vector2(width, height);
        }
    }
    static public void ResetRectTransScale(RectTransform rectTrans, float scale)
    {
        if (rectTrans != null)
        {
            rectTrans.anchorMin = new Vector2(0.5f, 0.5f);
            rectTrans.anchorMax = new Vector2(0.5f, 0.5f);
            rectTrans.sizeDelta = new Vector2(rectTrans.sizeDelta.x * scale, rectTrans.sizeDelta.y * scale);
        }
    }
}
