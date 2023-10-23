
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using UnityEngine.UI;

public class MoveNotice : UdonSharpBehaviour
{

    public Transform[] Notifications;
    private Transform[] Images;
    private Transform[] Points;
    private Animator[] Anims;
    public float speed = 1.0f;
    public float diff = - 250.0f;
    public string Title = "";
    public Font TitleFont;
    public Font MainFont;
    public string LastJoin = "";
    public string TextJoinLeft = "";
    private int Count = -1;
    private int Length = -1;

    void OnEnable() 
    {
        Count++;
        if (Count == 1)
        {
            PlayerJoin();
            Stop();
        }
        else if (Count == Length)
        {
            Anims[Length - 2].SetBool("Despawn", true);
            Anims[Length - 2].SetBool("Spawn", false);
            Points[Length - 2].localPosition = new Vector3(0, 0, 0);
            ChangesArray();
            Count--;
            MoveTarget();
        }
        else if (Count == 0)
        {
            Length = Notifications.Length;
            Images = new Transform[Notifications.Length];
            for (int i = 0; i < Notifications.Length; i++)
            {
                Images[i] = Notifications[i].Find("Image");
                Images[i].Find("Title").GetComponent<Text>().text = Title;
                Images[i].Find("Title").GetComponent<Text>().font = TitleFont;

                Images[i].Find("PlayerText").GetComponent<Text>().font = MainFont;
                Images[i].Find("MESSAGES").GetComponent<Text>().font = MainFont;
                Images[i].Find("Time").GetComponent<Text>().font = MainFont;
            }
            Points = new Transform[Notifications.Length];
            for (int i = 0; i < Notifications.Length; i++)
            {
                Points[i] = Notifications[i].Find("Point");
            }
            Anims = new Animator[Notifications.Length];
            for (int i = 0; i < Notifications.Length; i++)
            {
                Anims[i] = Notifications[i].GetComponent<Animator>();
            }
            Stop();
            return;
        }
        else 
        {
            ChangesArray();
            MoveTarget();
        }
    }


    void ChangesArray()
    {
        Images = ChangeArray(Images, Count);
        Points = ChangeArray(Points, Count);
        Notifications = ChangeArray(Notifications, Count);
        Anims = ChangeArrayAnims(Anims, Count);
        PlayerJoin();
    }

    void PlayerJoin()
    {
        if (LastJoin.Length > 15)
        {
            LastJoin = $"{LastJoin.Substring(0, 15)}...";
        }
        Images[0].Find("PlayerText").GetComponent<Text>().text = $"{LastJoin} {TextJoinLeft}";
    }

    void MoveTarget()
    {
        for (int i = Count - 1; i > 0; i--)
        {
            Points[i].localPosition = new Vector3(0, i * diff, 0);
        }
    }


    void Update()
    {
        for (int i = 1; i < Count; i++)
        {
            Images[i].position = Vector3.Lerp(Images[i].position, Points[i].position, speed * Time.deltaTime);
            if (Images[i].localPosition.y < Points[i].localPosition.y + 1)
            {
                Stop();
            }
        }
        
    }

    public void Stop()
    {
        this.enabled = false;
    }


    Transform[] ChangeArray(Transform[] inp, int n)
    {
        Transform[] temp = (Transform[]) inp.Clone();
        temp[0] = inp[n - 1];
        for (int i = 1; i < n; i++)
        {
            temp[i] = inp[i - 1];
        }
        return temp;
    }

    Animator[] ChangeArrayAnims(Animator[] inp, int n)
    {
        Animator[] temp = (Animator[]) inp.Clone();
        temp[0] = inp[n - 1];
        for (int i = 1; i < n; i++)
        {
            temp[i] = inp[i - 1];
        }
        return temp;
    }
}