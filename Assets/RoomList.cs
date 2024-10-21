using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class RoomList : MonoBehaviourPunCallbacks
{
    public static RoomList Instance { get; private set; } // Singleton instance
    public RoomManager roomManager; // Reference to RoomManager
    public GameObject roomManagerGameObject; // UI for the room manager
    [Header("UI")]
    public Transform roomListParent; // Parent of the room list UI
    public GameObject roomButtonPrefab; // Prefab for room buttons
    public GameObject lobbyPanel; // The entire lobby UI to disable when joining a game

    private List<RoomInfo> cachedRoomList = new List<RoomInfo>();

    public void ChangeRoomToCreateName(string _roomName)
    {
        roomManager.roomNameToJoin = _roomName;
    }

    void Awake()
    {
        // Ensure only one instance of RoomList (Singleton pattern)
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    IEnumerator Start()
    {
        // Precaution: Leave any room and disconnect if already connected
        if (PhotonNetwork.InRoom)
        {
            PhotonNetwork.LeaveRoom();
            PhotonNetwork.Disconnect();
        }

        // Wait until disconnected
        yield return new WaitUntil(() => !PhotonNetwork.IsConnected);

        // Connect to the Photon server
        PhotonNetwork.ConnectUsingSettings();
    }

    // Called when connected to the master server
    public override void OnConnectedToMaster()
    {
        base.OnConnectedToMaster();
        PhotonNetwork.JoinLobby(); // Join the lobby to get room list updates
    }

    // Called when the room list gets updated
    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        base.OnRoomListUpdate(roomList);

        // Clear cached room list
        cachedRoomList.Clear();

        // Update the cache with available rooms
        foreach (RoomInfo room in roomList)
        {
            if (!room.RemovedFromList) // Add only active rooms
            {
                cachedRoomList.Add(room);
            }
        }

        // Update the UI to reflect the current room list
        UpdateUI();
    }

    // Updates the room list UI
    void UpdateUI()
    {
        // Clear the UI room list parent first
        foreach (Transform child in roomListParent)
        {
            Destroy(child.gameObject); // Remove previous room buttons
        }

        // Add new room buttons for all available rooms
        foreach (RoomInfo room in cachedRoomList)
        {
            GameObject roomButton = Instantiate(roomButtonPrefab, roomListParent);

            // Get the RoomButton component from the prefab and set it up
            RoomButton roomButtonScript = roomButton.GetComponent<RoomButton>();
            roomButtonScript.Setup(room);
        }
    }

    // Method to join the room by name, passed from RoomButton
    public void JoinRoomByName(string roomName)
    {
        // Set the room name in RoomManager and show the RoomManager UI
        roomManager.roomNameToJoin = roomName;
        roomManagerGameObject.SetActive(true); // Enable the RoomManager UI

        // Optionally, disable the lobby UI
        lobbyPanel.SetActive(false);
    }
}
