using UnityEngine;
using TMPro; // Assuming you're using TextMeshPro for text fields
using Photon.Realtime;

public class RoomButton : MonoBehaviour
{
    public TMP_Text roomNameText;  // Reference to the room name text in the prefab
    public TMP_Text playersText;   // Reference to the player count text in the prefab
    private string roomName;
    private RoomInfo roomInfo;

    // Method to initialize the button with room info
    public void Setup(RoomInfo room)
    {
        roomInfo = room;

        // Update the room name and player count text
        roomNameText.text = roomInfo.Name;
        playersText.text = $"Players: {roomInfo.PlayerCount}/{roomInfo.MaxPlayers}";
        roomName = roomInfo.Name;
    }

    // Method to handle the button press
    public void OnButtonPressed()
    {
        // Call the RoomList to pass the room name to the RoomManager
        RoomList.Instance.JoinRoomByName(roomName);
    }
}
