
using UnityEngine;
using UnityEngine.UI;

public class UIDynamicImage : Image
{
    public DynamicAtlasGroup mGroup = DynamicAtlasGroup.Size_1024;
    [SerializeField]
    private string _currentPath = null;
    private DynamicAtlasGroup m_Group;
    private DynamicAtlas m_Atlas;
    private OnCallBack onCallBack;
    private bool isSetImage = false;
    private Sprite defaultSprite;
    protected override void Start()
    {
        base.Start();
#if UNITY_EDITOR
        if (Application.isPlaying)
        {
            OnPreDoImage();
        }
#else
       OnPreDoImage();
#endif
    }
    void OnPreDoImage()
    {
        if (mainTexture != null && isSetImage == false) //说明这个rawimage事先挂了texture，此时需要给copy到图集中
        {
            SetGroup(mGroup);
            SetImage();
        }
    }
    public void SetGroup(DynamicAtlasGroup group)
    {
        if (m_Atlas != null)
        {
            return;
        }
        m_Group = group;
        m_Atlas = DynamicAtlasManager.Instance.GetDynamicAtlas(group);
    }

    public DynamicAtlasGroup GetGroup()
    {
        return m_Group;
    }
    public void OnGetImageCallBack(Texture tex, Rect rect, string path)
    {
        int length = (int)m_Group;
        Rect spriteRect = rect;
        spriteRect.x *= length;
        spriteRect.y *= length;
        spriteRect.width *= length;
        spriteRect.height *= length;
        //Create(Texture2D texture, Rect rect, Vector2 pivot, float pixelsPerUnit, uint extrude, SpriteMeshType meshType, Vector4 border);
        sprite = Sprite.Create((Texture2D)tex, spriteRect, defaultSprite.pivot, defaultSprite.pixelsPerUnit, 1, SpriteMeshType.Tight, defaultSprite.border);
        gameObject.SetActiveVirtual(true);

        if (onCallBack != null)
        {
            onCallBack();
        }
    }
    private void SetImage()
    {
        defaultSprite = sprite;
        if (string.IsNullOrEmpty(currentPath))
        {
            return;
        }
        if (AtlasConfig.kUsingCopyTexture)//Sprite 只支持using copytexture
        {
            m_Atlas.SetTexture(currentPath, mainTexture, OnGetImageCallBack);
        }
        else
        {
            gameObject.SetActiveVirtual(false);
        }

        //此时可以卸载自己的引用计数
    }
    public void SetImage(string path, OnCallBack callBack = null)
    {
        isSetImage = true;
        gameObject.SetActiveVirtual(false);
        onCallBack = callBack;
        currentPath = path;
        if (m_Atlas == null)
        {
            SetGroup(mGroup);
        }
        m_Atlas.GetImage(path, OnGetImageCallBack);
    }
    public void SetImageNoHide(string path, OnCallBack callBack = null)
    {
        onCallBack = callBack;
        currentPath = path;
        if (m_Atlas == null)
        {
            SetGroup(mGroup);
        }
        m_Atlas.GetImage(path, OnGetImageCallBack);
    }
    public void OnDispose()
    {
        if (string.IsNullOrEmpty(currentPath) == false && m_Atlas != null)
        {
            m_Atlas.RemoveImage(currentPath, false);
            //m_Atlas.RemoveImage(this._currentPath, true);
            currentPath = null;
        }
        isSetImage = false;
    }
    protected override void OnDestroy()
    {
        base.OnDestroy();
        OnDispose();
    }
    public string currentPath
    {
        set
        {
            _currentPath = value;
        }
        get
        {
            return _currentPath;
        }
    }
}
