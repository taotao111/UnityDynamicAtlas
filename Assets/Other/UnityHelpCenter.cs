using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnityHelpCenter : MonoBehaviour
{
    private static volatile UnityHelpCenter ms_Instance;
    public static UnityHelpCenter Instance
    {
        get
        {
            return ms_Instance;
        }
    }

    private class ResCallBackData
    {
        public AssetBundle assetBundle = null;
        public OnCallBackSObject callBack;
        public string filePath;
        public System.Type systemTypeInstance;
    }
    private List<ResCallBackData> loadResTasks = new List<ResCallBackData>();

    private void Awake()
    {
        ms_Instance = this;
    }

    //load from resource synchronous
    public UnityEngine.Object LoadResSync(string path, System.Type systemTypeInstance = null)
    {
        path = CommonUtils.RemoveFileExtension(path);
        UnityEngine.Object resObject = null;
        if (systemTypeInstance == null)
        {
            resObject = Resources.Load(path);
        }
        else
        {
            resObject = Resources.Load(path, systemTypeInstance);
        }
        return resObject;
    }


    //load from resource asynchronous
    public void LoadResourceAsync(string fileName, OnCallBackSObject callBack)
    {
        LoadResourceAsync(fileName, callBack, null);
    }

    public void LoadResourceAsync(string filePath, OnCallBackSObject callBack, System.Type systemTypeInstance)
    {
        if (string.IsNullOrEmpty(filePath))
        {
            if (callBack != null)
            {
                callBack(null, null);
            }
            return;
        }
        ResCallBackData data = new ResCallBackData();
        data.filePath = filePath;
        data.callBack = callBack;
        data.systemTypeInstance = systemTypeInstance;
        loadResTasks.Add(data);

        if (loadResTasks.Count == 1)
        {
            StartLoadResourceAsync();
        }
    }

    void StartLoadResourceAsync()
    {
        if (loadResTasks.Count == 0)
        {
            return;
        }
        ResCallBackData data = loadResTasks[0];
        StartCoroutine(OnLoadResourceAsync(data));
    }
    IEnumerator OnLoadResourceAsync(ResCallBackData data)
    {
        string resPath = CommonUtils.RemoveFileExtension(data.filePath);
        ResourceRequest request = null;
        if (data.systemTypeInstance == null)
        {
            request = Resources.LoadAsync(resPath);
        }
        else
        {
            request = Resources.LoadAsync(resPath, data.systemTypeInstance);
        }
        yield return request;
        int count = loadResTasks.Count;
        for (int i = count - 1; i >= 0; i--)
        {
            ResCallBackData item = loadResTasks[i];
            if (item.filePath == data.filePath)
            {
                item.callBack(data.filePath, request.asset);
                loadResTasks.RemoveAt(i);
            }
        }
        StartLoadResourceAsync();
    }

    public void Clear()
    {
        loadResTasks.Clear();
      
        OnStopAllCoroutines();
    }

    public void OnStopAllCoroutines()
    {
        StopAllCoroutines();
    }
   
}
