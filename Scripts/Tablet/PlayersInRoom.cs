using UnityEngine.UI;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class PlayersInRoom : UdonSharpBehaviour
{
    public Text playerCountText = null;
    public Text playerLocal = null;
    private int playerCount = 0;

    void Start()
    {
        if (Networking.LocalPlayer != null)
            playerLocal.text = Networking.LocalPlayer.displayName;
        //playerLocal.text = $"{Networking.LocalPlayer.displayName} - {Networking.LocalPlayer.playerId}";
    }

    public override void OnPlayerJoined(VRCPlayerApi player)
    {
        playerCount++;
        PlayerCount();
    }

    public override void OnPlayerLeft(VRCPlayerApi player)
    {
        playerCount--;
        PlayerCount();
    }

    void PlayerCount()
    {
        if (playerCount < 10)
            playerCountText.text = playerCount.ToString();
        else playerCountText.text = "9+";
    }
}
