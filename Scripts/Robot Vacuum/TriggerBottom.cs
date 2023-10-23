
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class TriggerBottom : UdonSharpBehaviour
{
    public RobotVacuumScript robotVacuumScript;


    public TriggerBottom triggerBottomSecond;
    private int CountColliders = 0;
    public int countColliders { get => CountColliders; }

    void Start()
    {
        if (robotVacuumScript == null || triggerBottomSecond == null) this.enabled = false;
        GetComponent<BoxCollider>().enabled = false;
        GetComponent<BoxCollider>().enabled = true;
    }



    private void OnTriggerEnter(Collider other)
    {
        if (other == null) return;
        if (other.isTrigger) return;

        CountColliders++;
        if (CountColliders == 1 && triggerBottomSecond.countColliders > 0)
        {
            robotVacuumScript.triggerBottom = true;
            //Debug.Log("Sensor Not Detected Pit!");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other == null) return;
        if (other.isTrigger) return;

        CountColliders--;
        if (CountColliders == 0)
        {
            robotVacuumScript.triggerBottom = false;
            //Debug.Log("Sensor Detected Pit!");
        }
    }
}
