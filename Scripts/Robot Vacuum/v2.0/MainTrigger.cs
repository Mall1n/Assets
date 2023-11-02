
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class MainTrigger : UdonSharpBehaviour
{
    public VacuumV2 robotVacuumScript;
    public bool delaySensor = true;
    public float delaySensorTime = 1.0f;


    private int CountColliders = 0;
    private BoxCollider boxCollider;

    void Start()
    {
        if (robotVacuumScript == null) this.enabled = false;
        boxCollider = GetComponent<BoxCollider>();
        if (boxCollider == null) this.enabled = false;
        boxCollider.enabled = false;
        boxCollider.enabled = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other == null) return;
        if (other.isTrigger) return;

        robotVacuumScript._MainTrigger = true;
        CountColliders++;

        //Debug.Log($"Sensor Detected Collider! INFO: {other.name}");
    }

    private void OnTriggerExit(Collider other)
    {
        if (other == null) return;
        if (other.isTrigger) return;

        CountColliders--;
        if (CountColliders == 0)
            robotVacuumScript._MainTrigger = false;

        //Debug.Log($"Sensor Exit Collider! INFO: {other.name}");

    }

    // public void SensorOff()
    // {
    //     if (!delaySensor) return;
    //     boxCollider.enabled = false;
    //     SendCustomEventDelayedSeconds("SensorOn", delaySensorTime);
    // }

    public void SensorOn() => boxCollider.enabled = true;
}
