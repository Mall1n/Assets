
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class ColliderFloor : UdonSharpBehaviour
{
    public RobotVacuumScript robotVacuumScript;
    public float delay = 2.0f;

    private int CountColliders = 0;
    private BoxCollider boxCollider;

    void Start()
    {
        if (robotVacuumScript == null || boxCollider == null) this.enabled = false;
        boxCollider = GetComponent<BoxCollider>();
        boxCollider.enabled = false;
        boxCollider.enabled = true;
    }



    private void OnTriggerEnter(Collider other)
    {
        if (other == null) return;
        if (other.isTrigger) return;

        CountColliders++;
        if (CountColliders == 1)
            robotVacuumScript.onFloor = true;

        //Debug.Log("Sensor Detected Floor!");
    }

    private void OnTriggerExit(Collider other)
    {
        if (other == null) return;
        if (other.isTrigger) return;

        Debug.Log(other);

        CountColliders--;
        if (CountColliders == 0)
        {
            ResetVacuumScript();
            robotVacuumScript.ResetRotate();
            robotVacuumScript.SetAlert(true);
            //Debug.Log("Sensor Not Detected Floor!");
        }
    }

    public void ResetFloorDetector()
    {
        ResetVacuumScript();
        boxCollider.enabled = false;
        CountColliders = 0;
        boxCollider.enabled = true;
    }

    private void ResetVacuumScript()
    {
        robotVacuumScript.onFloor = false;
        robotVacuumScript.delay = delay;
    }

}
