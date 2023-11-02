
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class TriggerTop : UdonSharpBehaviour
{
    public VacuumV2 robotVacuumScript;
    public bool left = true;

    private int CountColliders = 0;
    public int countColliders { get => CountColliders; set => CountColliders = value; }

    void Start()
    {
        if (robotVacuumScript == null) this.enabled = false;
        GetComponent<BoxCollider>().enabled = false;
        GetComponent<BoxCollider>().enabled = true;
    }


    private void OnTriggerEnter(Collider other)
    {
        if (other == null) return;
        if (other.isTrigger) return;

        CountColliders++;
        if (CountColliders == 1)
        {
            if (left)
            {
                robotVacuumScript.triggerTopLeft = true;
                if (robotVacuumScript.Turning == false)
                {
                    robotVacuumScript.turningLeftSide = false;
                    robotVacuumScript.Turning = true;
                }
            }
            else
            {
                robotVacuumScript.triggerTopRight = true;
                if (robotVacuumScript.Turning == false)
                {
                    robotVacuumScript.turningLeftSide = true;
                    robotVacuumScript.Turning = true;
                }
            }


            //robotVacuumScript.triggerTop = true;
            //robotVacuumScript.leftTrigger = left;
            //Debug.Log("Sensor Not Detected a pit!");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other == null) return;
        if (other.isTrigger) return;

        CountColliders--;
        if (CountColliders == 0)
        {
            if (left) robotVacuumScript.triggerTopLeft = false;
            else robotVacuumScript.triggerTopRight = false;
            //robotVacuumScript.Turning = false;
            //robotVacuumScript.triggerTop = false;
            //Debug.Log("Sensor Detected a pit!");
        }
    }
}
