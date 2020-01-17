using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DynamicAtlasDemo : MonoBehaviour
{

    public GameObject image;
    public Transform imageParent;
    private int index = 0;
    private List<string> paths = new List<string>();
    private List<UIDynamicRawImage> images = new List<UIDynamicRawImage>();

    private int pathIndex = 0;

    public void OnAddClick()
    {
        index++;
        var item = imageParent.AddChild(image.gameObject);
        var pos = image.transform.localPosition;
        item.transform.localPosition = new Vector3(pos.x + (index + 1) * 100, pos.y + (index + 1) * 100, 0);

        UIDynamicRawImage data = item.transform.Find("RawImage/DynamicImage").GetComponent<UIDynamicRawImage>();
        string imageName = paths[pathIndex];
        data.SetImage(imageName);
        data.gameObject.name = imageName;
        pathIndex++;
        if (pathIndex >= paths.Count)
        {
            pathIndex = 0;
        }
        images.Add(data);
    }

    public void OnSubClick()
    {
        if (images.Count == 0)
        {
            return;
        }
        //int removeIndex = Random.Range(0, images.Count - 1);
        int removeIndex = 0;
        UIDynamicRawImage obj = images[removeIndex];
        images.RemoveAt(removeIndex);
        GameObject.Destroy(obj.transform.parent.parent.gameObject);
    }

    private void Awake()
    {
        paths.Add("test");
        for (int i = 1; i < 6; i++)
        {
            paths.Add("BUFF/" + (12100000 + i));
        }
        for (int i = 1; i < 8; i++)
        {
            paths.Add("NPC/" + (11000685 + i));
        }
        for (int i = 1; i < 10; i++)
        {
            paths.Add("skill/" + (11200000 + i));
        }
    }
   
}
