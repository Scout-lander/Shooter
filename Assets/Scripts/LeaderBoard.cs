using UnityEngine;
using System.Linq;
using Photon.Pun;
using TMPro;
using System.Collections.Generic;
using Photon.Pun.UtilityScripts;

public class LeaderBoard : MonoBehaviour
{
    public GameObject PlayerLayout; // The layout where player cards are placed
    public GameObject playerCardPrefab; // The prefab for each player card
    public Transform playerCardParent; // The parent object for instantiated player cards

    [Header("Options")]
    public float refreshRate = 1f; // How often to refresh the leaderboard

    // Dictionary to keep track of player cards by player ID
    private Dictionary<int, GameObject> playerCards = new Dictionary<int, GameObject>();

    private void Start()
    {
        // Start refreshing the leaderboard at the specified refresh rate
        InvokeRepeating(nameof(Refresh), 1f, refreshRate);
    }

    public void Refresh()
    {
        // Sort players by score in descending order (or modify as per your needs)
        var sortedPlayerList = (from player in PhotonNetwork.PlayerList
                                orderby player.GetScore() descending
                                select player).ToList();

        // Create or update player cards
        foreach (var player in sortedPlayerList)
        {
            // If the player doesn't have a card yet, create one
            if (!playerCards.ContainsKey(player.ActorNumber))
            {
                CreatePlayerCard(player);
            }

            // Update the player's card with the current score and name
            UpdatePlayerCard(player);
        }

        // Remove cards for players who left
        var playerIDs = sortedPlayerList.Select(p => p.ActorNumber).ToList();
        foreach (var cardKey in playerCards.Keys.ToList())
        {
            if (!playerIDs.Contains(cardKey))
            {
                Destroy(playerCards[cardKey]); // Remove the card from the UI
                playerCards.Remove(cardKey); // Remove the entry from the dictionary
            }
        }
    }

    // Create a new player card
    private void CreatePlayerCard(Photon.Realtime.Player player)
    {
        // Instantiate a new player card prefab and set its parent to the playerCardParent
        GameObject newCard = Instantiate(playerCardPrefab, playerCardParent);

        // Set the new card to inactive initially (it will be shown when data is updated)
        newCard.SetActive(true);

        // Add the card to the dictionary with the player's ActorNumber as the key
        playerCards.Add(player.ActorNumber, newCard);
    }

    // Update an existing player's card
    private void UpdatePlayerCard(Photon.Realtime.Player player)
    {
        if (playerCards.ContainsKey(player.ActorNumber))
        {
            GameObject card = playerCards[player.ActorNumber];

            // Find the TMP components in the prefab and update them with the player's data
            TextMeshProUGUI nameText = card.transform.Find("NameText").GetComponent<TextMeshProUGUI>();
            TextMeshProUGUI scoreText = card.transform.Find("ScoreText").GetComponent<TextMeshProUGUI>();

            nameText.text = string.IsNullOrEmpty(player.NickName) ? "unnamed" : player.NickName;

            // Access custom properties for kills and deaths
            if (player.CustomProperties.ContainsKey("kills") && player.CustomProperties.ContainsKey("deaths"))
            {
                int kills = (int)player.CustomProperties["kills"];
                int deaths = (int)player.CustomProperties["deaths"];

                // Display Kills/Deaths format
                scoreText.text = $"{kills} / {deaths}";
            }
            else
            {
                // Default display if no kills or deaths are available
                scoreText.text = "0 / 0";
            }
        }
    }

    private void Update()
    {
        // Show/hide the player layout when the Tab key is pressed
        PlayerLayout.SetActive(Input.GetKey(KeyCode.Tab));
    }
}
