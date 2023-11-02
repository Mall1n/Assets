
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class TriggerBottomLR : UdonSharpBehaviour
{
    public VacuumV2 robotVacuumScript;
    public bool left = true;
    public bool back = true;


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
            if (!back)
            {
                if (left) robotVacuumScript.triggerBottomLeft = true;
                else robotVacuumScript.triggerBottomRight = true;
            }
            else robotVacuumScript.BothPits = false;

            //robotVacuumScript.triggerBottom = true;
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
            if (!back)
            {
                if (left) robotVacuumScript.triggerBottomLeft = false;
                else robotVacuumScript.triggerBottomRight = false;
            }
            else robotVacuumScript.BothPits = true;
            //robotVacuumScript.triggerBottom = false;
            //Debug.Log("Sensor Detected a pit!");у
        }
    }
}
