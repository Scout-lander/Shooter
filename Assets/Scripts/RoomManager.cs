using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEngine.UI;

public class RoomManager : MonoBehaviourPunCallbacks
{
    public static RoomManager instance;
    public GameObject player;
    [Space]
    public Transform[] spawnPoints;

    [Space]
    public GameObject roomCam;
    [Space]
    public GameObject nameUI;
    public GameObject connectingUI;
    public GameObject respawnScreen; // Reference to the respawn screen UI

    [Header("UI Elements")]
    public TMP_InputField nicknameInputField; // Reference to the TMP_InputField for nickname
    public Button joinRoomButton; // Reference to the button for joining the room
    public Button respawnButton; // Reference to the button for respawning

    [HideInInspector]
    public int kills = 0;
    [HideInInspector]
    public int deaths = 0;
    [HideInInspector]
    public int assists = 0;
    public string roomNameToJoin = ""; // Default room name
    private string nickname = "Unnamed"; // Default nickname
    private bool hasSpawnedPlayer = false;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        // Add a listener for the button click
        joinRoomButton.onClick.AddListener(JoinRoomButtonPressed);
        respawnButton.onClick.AddListener(RespawnPlayer); // Add respawn button listener

        // Start with the name UI active and the connecting UI hidden
        nameUI.SetActive(true);
        connectingUI.SetActive(false);
        respawnScreen.SetActive(false); // Start with the respawn screen hidden
    }

    public void OnPlayerDeath()
    {
        // Reset spawn flag on death
        hasSpawnedPlayer = false;

        // Show the respawn screen
        respawnScreen.SetActive(true);
        roomCam.SetActive(true);

        // Enable and show the mouse cursor
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void RespawnPlayer()
    {
        // Hide the respawn screen when respawning
        respawnScreen.SetActive(false);
        roomCam.SetActive(false);

        // Lock and hide the mouse cursor after respawn
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // Respawn the player
        SpawnPlayer();
    }

    // Method to set the room name when a room button is pressed
    public void SetRoomName(string roomName)
    {
        roomNameToJoin = roomName; // Assign the room name from the button press
    }

    public void JoinRoomButtonPressed()
    {
        // Get the nickname from the input field if it's not empty
        if (!string.IsNullOrEmpty(nicknameInputField.text))
        {
            nickname = nicknameInputField.text;
        }
        else
        {
            Debug.LogWarning("Nickname is empty, using default nickname.");
            nickname = "Unnamed"; // Fallback to default nickname
        }

        Debug.Log("Connecting...");

        // Check if we're already connected to Photon to avoid reconnecting
        if (PhotonNetwork.IsConnected)
        {
            JoinRoom();
        }
        else
        {
            // Connect to the Photon server using the settings
            PhotonNetwork.ConnectUsingSettings();
        }

        // Hide the name UI and show the connecting UI
        nameUI.SetActive(false);
        connectingUI.SetActive(true);
    }

    public override void OnConnectedToMaster()
    {
        base.OnConnectedToMaster();
        Debug.Log("Connected to server");

        // Join the lobby if not in one already, or proceed to join the room
        if (!PhotonNetwork.InLobby)
        {
            PhotonNetwork.JoinLobby();
        }
        else
        {
            JoinRoom();
        }
    }

    public override void OnJoinedLobby()
    {
        base.OnJoinedLobby();
        Debug.Log("In the Lobby");
        JoinRoom();
    }

    private void JoinRoom()
    {
        if (string.IsNullOrEmpty(roomNameToJoin))
        {
            Debug.LogError("Room name is not set. Can't join room.");
            return;
        }

        // Set the room options and join the room selected
        RoomOptions roomOptions = new RoomOptions();
        roomOptions.MaxPlayers = 4; // Set the max players as needed

        // Join the room passed in from the room button
        PhotonNetwork.JoinOrCreateRoom(roomNameToJoin, roomOptions, TypedLobby.Default);
    }

    public override void OnJoinedRoom()
    {
        base.OnJoinedRoom();
        Debug.Log("In a Room");

        // Disable the room camera and spawn the player
        roomCam.SetActive(false);
        connectingUI.SetActive(false);
        SpawnPlayer();
    }

     public void SpawnPlayer()
    {
        if (hasSpawnedPlayer)
        {
            Debug.LogWarning("Player already spawned. Skipping spawn.");
            return;
        }

        if (spawnPoints.Length == 0)
        {
            Debug.LogError("No spawn points set!");
            return;
        }

        // Choose a random spawn point from the available ones
        Transform spawnPoint = spawnPoints[UnityEngine.Random.Range(0, spawnPoints.Length)];

        // Instantiate the player object at the chosen spawn point
        GameObject _player = PhotonNetwork.Instantiate(player.name, spawnPoint.position, Quaternion.identity);

        // Set up the player if the PlayerSetup script exists
        PlayerSetup playerSetup = _player.GetComponent<PlayerSetup>();
        if (playerSetup != null)
        {
            playerSetup.IsLocalPlayer(); // Ensure this is the correct method
        }
        else
        {
            Debug.LogError("PlayerSetup script missing on the player prefab.");
        }

        // Set the player as the local player in the Health script if it exists
        Health playerHealth = _player.GetComponent<Health>();
        if (playerHealth != null)
        {
            playerHealth.isLocalPlayer = true;
        }
        else
        {
            Debug.LogError("Health script missing on the player prefab.");
        }

        // Set the nickname for the player over the network
        PhotonView photonView = _player.GetComponent<PhotonView>();
        if (photonView != null)
        {
            photonView.RPC("SetNickname", RpcTarget.AllBuffered, nickname);
            PhotonNetwork.LocalPlayer.NickName = nickname;
        }
        else
        {
            Debug.LogError("PhotonView missing on the player prefab.");
        }

        // Mark player as spawned
        hasSpawnedPlayer = true;
    }

    public void SetHashes()
    {
        try
        {
            // Create a new hashtable to store custom properties using Photon Hashtable
            ExitGames.Client.Photon.Hashtable hash = new ExitGames.Client.Photon.Hashtable(); // Explicit use of Photon Hashtable

            // Set values for custom properties based on RoomManager
            hash["kills"] = kills;
            hash["deaths"] = deaths;
            hash["assists"] = assists;

            // Apply the custom properties to the local player
            PhotonNetwork.LocalPlayer.SetCustomProperties(hash);
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Failed to set custom properties: {ex.Message}");
        }
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        base.OnJoinRoomFailed(returnCode, message);
        Debug.LogError($"Failed to join room: {message}");
        // Show some UI to inform the user, or fallback to the name UI
        nameUI.SetActive(true);
        connectingUI.SetActive(false);
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        base.OnDisconnected(cause);
        Debug.LogError($"Disconnected from Photon. Cause: {cause}");
        // Return to the name UI if disconnected
        nameUI.SetActive(true);
        connectingUI.SetActive(false);
    }
}
