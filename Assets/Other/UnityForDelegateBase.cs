using UnityEngine.EventSystems;

public delegate void OnCallBackType(System.Type a);

public delegate void OnCallBack();
public delegate void OnCallBackInt(int a);
public delegate void OnCallBackFloat(float a);
public delegate void OnCallBackDouble(double a);
public delegate void OnCallBackString(string a);
public delegate void OnCallBackStringStringIntLong(string a,string b,int c,long d);
public delegate void OnCallBackStringStringInt(string a, string b, int c);
public delegate void OnCallBackBool(bool a);
public delegate void OnCallBackII(int a, int b);
public delegate void OnCallBackFF(float a, float b);
public delegate void OnCallBackSS(string a, string b);
public delegate void OnCallBackCharInt( char c, int i );
public delegate void OnCallBackStringIntInt( string s, int i0, int i1 );
public delegate void OnCallBackWWW(UnityEngine.WWW a);
public delegate void OnCallBackTransform(UnityEngine.Transform a);
public delegate void OnCallBackAudioClip(UnityEngine.AudioClip a);
public delegate void OnCallBackGameObject(UnityEngine.GameObject a);
public delegate void OnCallBackGameObjectInt(UnityEngine.GameObject a, int b);

public delegate void OnCallBackObject(UnityEngine.Object a);
public delegate void OnCallBackObjectS(UnityEngine.Object a, string b);
public delegate void OnCallBackSObject(string a, UnityEngine.Object b);
public delegate void OnCallBackGameObjectIntPointerEventData(UnityEngine.GameObject a, int b,PointerEventData c);
public delegate void OnCallBackGameObjectPointerEventData(UnityEngine.GameObject a, PointerEventData c);

public delegate void OnCallBackStringStringIntPED( string a, string b, int c, PointerEventData d );
public delegate void OnCallBackMetRect(UnityEngine.Material a, UnityEngine.Rect b, string path);
public delegate void OnCallBackTexRect(UnityEngine.Texture tex, UnityEngine.Rect b, string path);

