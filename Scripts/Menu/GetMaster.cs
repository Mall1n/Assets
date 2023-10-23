using UnityEngine.UI;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class GetMaster : UdonSharpBehaviour
{

    private Text text = null;
    private VRCPlayerApi[] players = null;
    private int playerCount = 1;

    private void Start()
    {
        text = transform.GetComponent<Text>();
    }
    public override void OnPlayerJoined(VRCPlayerApi player)
    {
        playerCount++;
        CheckMaster();
    }

    public override void OnPlayerLeft(VRCPlayerApi player)
    {
        playerCount--;
        CheckMaster();
    }


    void CheckMaster()
    {
        players = new VRCPlayerApi[playerCount * 2];
        players = VRCPlayerApi.GetPlayers(players);
        for (int i = 0; i < players.Length; i++)
        {
            if (players[i] != null && players[i].isMaster)
            {
                text.text = $"Master: {players[i].displayName}";
                return;
            }
        }
        text.text = "Master:";
    }
}
