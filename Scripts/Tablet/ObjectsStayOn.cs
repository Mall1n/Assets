
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class ObjectsStayOn : UdonSharpBehaviour
{
    public Transform[] Objects = new Transform[0];
    public Transform[] ObjectsStartScripts = new Transform[0];
    void Start()
    {


        for (int i = 0; i < Objects.Length; i++)
        {
            BoxCollider[] Boxes = Objects[i].GetComponentsInChildren<BoxCollider>(true);
            foreach (var item in Boxes)
            {
                if (item == null) continue;
                if (item.transform.name == "AreaTrigger" || item.transform.name == "InteractTrigger")
                    item.gameObject.SetActive(false);
            }
            if (Objects[i] != null)
                Objects[i].gameObject.SetActive(true);
        }


        for (int i = 0; i < ObjectsStartScripts.Length; i++)
        {
            BoxCollider[] Boxes = ObjectsStartScripts[i].GetComponentsInChildren<BoxCollider>(true);
            foreach (var item in Boxes)
            {
                if (item == null) continue;
                if (item.transform.name == "AreaTrigger" || item.transform.name == "InteractTrigger")
                    item.gameObject.SetActive(false);
            }
            if (ObjectsStartScripts[i] != null)
                ObjectsStartScripts[i].gameObject.SetActive(true);
        }


        SendCustomEventDelayedFrames("ObjectsStartScriptsFalse", 1);

    }

    public void ObjectsStartScriptsFalse()
    {
        for (int i = 0; i < ObjectsStartScripts.Length; i++)
            if (ObjectsStartScripts[i] != null)
                ObjectsStartScripts[i].gameObject.SetActive(false);
    }
}
