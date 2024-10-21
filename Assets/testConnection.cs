using UnityEngine;
using Photon.Pun; // Photon Unity Networking
using Photon.Realtime; // For handling real-time events

public class PhotonConnectionTest : MonoBehaviourPunCallbacks
{
    private void Start()
    {
        Debug.Log("Starting connection to Photon...");

        // Make sure we connect using the settings from PhotonServerSettings
        PhotonNetwork.ConnectUsingSettings();
    }

    // Called when the connection to the Master Server is successful
    public override void OnConnectedToMaster()
    {
        Debug.Log("Connected to Master Server!");
    }

    // Called when joining a lobby
    public override void OnJoinedLobby()
    {
        Debug.Log("Joined a Lobby!");
    }

    // Called when failed to connect to the Photon servers
    public override void OnDisconnected(DisconnectCause cause)
    {
        Debug.LogError("Disconnected from Photon: " + cause);
    }

    // Called when the player has joined a room
    public override void OnJoinedRoom()
    {
        Debug.Log("Joined a Room!");
    }

    // Called if there is a failure to join or create a room
    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        Debug.LogError("Failed to join a room: " + message);
    }

    // Called if creating a room fails
    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        Debug.LogError("Failed to create a room: " + message);
    }

    // Call this method if you want to join a random room after connecting
    public void JoinRandomRoom()
    {
        PhotonNetwork.JoinRandomRoom();
    }

    // Call this method if you want to create a new room after connecting
    public void CreateRoom(string roomName)
    {
        PhotonNetwork.CreateRoom(roomName, new RoomOptions { MaxPlayers = 4 });
    }
}
