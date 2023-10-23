using UnityEngine.UI;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class LerpObject : UdonSharpBehaviour
{

    public GameObject CanvasMain = null;
    public GameObject CanvasProgram = null;
    public float speedSpawning = 7.0f, speedDeSpawning = 10.0f;
    public bool switchMain = true;
    public bool switchProgram = true;
    public bool Scaling = false;

    [Header("Additional Settings")]
    public UdonBehaviour UdonSecond = null;
    public string NameEvent = "";
    public GameObject time = null;
    public bool timeColorSwitch = false;


    private Transform image = null;
    private Transform ActualPosition = null;
    private BoxCollider[] MainTriggers;
    private BoxCollider[] ProgramTriggers;
    private Vector3 primalPosition;
    private Vector3 VectorZero;
    private bool enabledStart = false, FirstEnable = true, spawn = true;


    private void OnEnable() 
    {
        if (!FirstEnable)
        {
            if (spawn) 
            {
                if (switchProgram) CanvasProgram.SetActive(true);
                TriggersSwitch(MainTriggers, false);
                //Debug.Log($"Spawning program | where MainTriggers.Length = {MainTriggers.Length}");
            }
            else 
            {
                if (switchMain) CanvasMain.SetActive(true);
                TriggersSwitch(ProgramTriggers, false);
                if (time != null && !timeColorSwitch) time.SetActive(true);
                if (timeColorSwitch) 
                {
                    time.transform.Find("Time").GetComponent<Text>().color = Color.white;
                    time.transform.Find("pmam").GetComponent<Text>().color = Color.white;
                }
            }
            enabledStart = true;
        }
        else
        {
            image = CanvasProgram.transform.Find("ImageButtons").GetComponent<RectTransform>();
            MainTriggers = CanvasMain.GetComponentsInChildren<BoxCollider>(true);
            ProgramTriggers = CanvasProgram.GetComponentsInChildren<BoxCollider>(true);
            MainTriggers = ResizeT(MainTriggers, "InteractTrigger", "AreaTrigger");
            ProgramTriggers = ResizeT(ProgramTriggers, "InteractTrigger", "AreaTrigger");
            //primalPosition = image.localPosition;
            VectorZero = new Vector3(0, 0, image.localPosition.z);
            ActualPosition = CanvasProgram.transform.Find("ActualPosition");
            primalPosition = ActualPosition.localPosition;
            image.transform.localPosition = ActualPosition.localPosition;
            if (Scaling) image.transform.localScale = Vector3.zero;
            FirstEnable = false;
            enabled = false;
        }
    }

    private void TriggersSwitch(BoxCollider[] t, bool b)
    {
        foreach (BoxCollider item in t)
            item.gameObject.SetActive(b);
    }

    private void Update() 
    {
        if (enabledStart)
        {
            if (spawn == true) Spawn();
            else DeSpawn();
        }
    }

    private void Spawn()
    {
        image.localPosition = Vector3.Lerp(image.localPosition, VectorZero, speedSpawning * Time.deltaTime);
        if (Scaling) image.localScale = Vector3.Lerp(image.localScale, Vector3.one, speedSpawning * Time.deltaTime);
        if (image.localPosition.y < 0.1f && image.localPosition.y > - 0.1f)
        {
            image.localPosition = VectorZero;
            if (Scaling) image.localScale = Vector3.one;
            if (timeColorSwitch) 
            {
                time.transform.Find("Time").GetComponent<Text>().color = Color.black;
                time.transform.Find("pmam").GetComponent<Text>().color = Color.black;
            }
            //if (switchProgram) 
            if (switchMain) CanvasMain.SetActive(false);
            TriggersSwitch(ProgramTriggers, true);
            if (time != null && !timeColorSwitch) time.SetActive(false);
            Stop();
        }

    }

    private void DeSpawn()
    {
        image.localPosition = Vector3.Lerp(image.localPosition, primalPosition, speedDeSpawning * Time.deltaTime);
        if (Scaling) image.localScale = Vector3.Lerp(image.localScale, Vector3.zero, speedDeSpawning * Time.deltaTime);
        if (image.localPosition.y < primalPosition.y + 0.1f && image.localPosition.y > primalPosition.y - 0.1f)
        {
            image.localPosition = primalPosition;
            if (Scaling) image.localScale = Vector3.zero;
            if (switchProgram) CanvasProgram.SetActive(false);
            TriggersSwitch(MainTriggers, true);
            Stop();
        }
    }

    public void Stop()
    {
        //if (UdonSecond != null) UdonSecond.SendCustomEvent("CustomEvent");NameEvent
        if (UdonSecond != null && NameEvent != "") UdonSecond.SendCustomEvent(NameEvent);
        spawn = !spawn;
        this.enabled = false;
    }

    private BoxCollider[] ResizeT(BoxCollider[] t, string s, string ss)
    {
        int Count = 0;
        foreach (BoxCollider item in t)
            if (item.name.IndexOf(s) != -1 || item.name.IndexOf(ss) != -1)
                Count++;
        BoxCollider[] temp = new BoxCollider[Count];
        Count = 0;
        foreach (BoxCollider item in t)
            if (item.name.IndexOf(s) != -1 || item.name.IndexOf(ss) != -1) 
            {
                temp[Count] = item;
                Count++;
            }
        return t;
    }
}