
using System;
using UnityEngine;

public class ResourcesManager {

    private static volatile ResourcesManager ms_Instance;
    public static ResourcesManager Instance
    {
        get
        {
            if(ms_Instance == null)
            {
                ms_Instance = new ResourcesManager();
            }
            return ms_Instance;
        }
    }
    
    public UnityEngine.Object LoadResSync(string path, System.Type systemTypeInstance = null)
    {
        return UnityHelpCenter.Instance.LoadResSync(path, systemTypeInstance);
    }
    public void LoadResAsync(string path, OnCallBackSObject callBack, Type systemTypeInstance = null)
    {
        UnityHelpCenter.Instance.LoadResourceAsync(path, callBack, systemTypeInstance);
    }
}
