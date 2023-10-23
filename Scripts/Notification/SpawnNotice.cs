
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class SpawnNotice : UdonSharpBehaviour
{

    public Transform[] objects;

    private int Count = 0;
    private int Length = 0;
    private Transform[] images;

    void Start() 
    {
        Length = objects.Length;
        images = new Transform[objects.Length];
        for (int i = 0; i < images.Length; i++)
        {
            images[i] = objects[i].Find("Image");
        }
    }
    void Interact()
    {
        Spawn();
    }

    public void Spawn()
    {
        if (Count < Length)
        {
            objects[Count].GetComponent<Animator>().SetBool("Spawn", true);
            Count++;
            objects = ChangeArray(objects, Count);
            images = ChangeArray(images, Count);
        }
        else
        {
            images[Length - 1].localPosition = new Vector3(0, 0, 0);
            objects[Length - 1].GetComponent<Animator>().SetBool("Spawn", true);
            objects[Length - 1].GetComponent<Animator>().SetBool("Despawn", false);
            objects = ChangeArray(objects, Count);
            images = ChangeArray(images, Count);
        }
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
}
