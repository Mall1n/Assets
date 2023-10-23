
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class LerpEllipse : UdonSharpBehaviour
{
    public Transform settings = null;
    public float speed = 5.0f;

    [Tooltip("If you want change the LEFT position of the bubble then make changes in *Left_position_button* above")]
    public bool left = true;

    [Header("Udon")]
    public UdonBehaviour Udon = null;

    [Header("Colors")]
    [ColorUsage(true, true)]
    public Color SliderColor;
    [ColorUsage(true, true)]
    public Color DisabledColor;
    [ColorUsage(true, true)]
    public Color EnabledColor;

    private Transform Ellipse = null;
    private Transform EllipseLong = null;
    private Vector3 LeftPosition;
    private Vector3 RightPosition;
    private Vector3 target;
    private bool enabledStart = false;
    private float Range = 0.03f;


    private void Start()
    {
        EllipseLong = settings.Find("EllipseLong").GetComponent<Transform>();
        Ellipse = settings.Find("Ellipse").GetComponent<Transform>();
        LeftPosition = new Vector3(-Range, 0, 0);
        RightPosition = new Vector3(Range, 0, 0);
        Udon.SetProgramVariable("left", left);
        enabled = false;
    }

    public void FullChange()
    {
        MainChange();

        Udon.SendCustomEvent("FullChange");

        //Sync_Canvas();
    }

    public void ChangeWithoutSync()
    {
        MainChange();

        Udon.SendCustomEvent("ChangeWithoutSync");
    }

    // public void ChangeWithoutConflict()
    // {
    //     MainChange();

    //     Udon.SendCustomEvent("ChangeWithoutConflict");
    // }

    private void MainChange()
    {
        if (left)
        {
            EllipseLong.GetComponent<SpriteRenderer>().color = EnabledColor;
            target = RightPosition;
        }
        else
        {
            EllipseLong.GetComponent<SpriteRenderer>().color = DisabledColor;
            target = LeftPosition;
        }
        left = !left;
        enabledStart = true;
    }

    private void Update()
    {
        if (enabledStart) Move();
    }

    private void Move()
    {
        Ellipse.localPosition = Vector3.Lerp(Ellipse.localPosition, target, speed * Time.deltaTime);
        if (Ellipse.localPosition.x < target.x + 0.0001f && Ellipse.localPosition.x > target.x - 0.0001f)
        {
            Ellipse.localPosition = target;
            Stop();
        }
    }

    public void Stop() => this.enabled = false;

}
