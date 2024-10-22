using System.Collections;
using UnityEngine;
using Photon.Pun;
using TMPro;
using UnityEngine.UI;

public class Health : MonoBehaviourPun
{
    public int health;
    public bool isLocalPlayer;
    private bool hasDied;

    [Header("UI")]
    public TextMeshProUGUI healthText;
    public RoomManager roomManager;

    [Header("Camera Effects")]
    public GameObject playerCamera; // The player's camera object
    public float flinchIntensity = 5f; // Intensity of the flinch effect
    public float flinchDuration = 0.1f; // Duration of the flinch effect

    [Header("Red Flash Effect")]
    public Image redFlashImage; // UI Image overlay for red flash
    public float redFlashDuration = 0.3f; // Duration of the red flash
    public Color flashColor = new Color(1f, 0f, 0f, 0.5f); // Red color with transparency

    private Photon.Realtime.Player lastAttacker; // Reference to store the last attacker

    void Update()
    {
        // Test for damage by pressing the 'P' key
        if (isLocalPlayer && Input.GetKeyDown(KeyCode.P))
        {
            TakeDamage(30, PhotonNetwork.LocalPlayer); // Take 30 damage when 'P' is pressed
        }
    }

    [PunRPC]
    public void TakeDamage(int _damage, Photon.Realtime.Player attacker)
    {
        if (hasDied) return; // If the player has already died, ignore further damage

        health -= _damage;

        // Update health UI
        if (healthText != null)
        {
            healthText.text = health.ToString();
        }

        // Track the last player who attacked
        lastAttacker = attacker;

        // Trigger the camera flinch and red flash effect if this is the local player
        if (isLocalPlayer)
        {
            StartCoroutine(CameraFlinch());
            StartCoroutine(RedFlashEffect());
        }

        if (health <= 0)
        {
            if (isLocalPlayer)
            {
                hasDied = true;
                HandlePlayerDeath();
            }

            // Synchronize death with other players
            PhotonNetwork.Destroy(gameObject); // Destroy the player object across the network
        }
    }

    // Method to handle player death logic
    private void HandlePlayerDeath()
    {
        // Broadcast the kill notification to all players
        photonView.RPC("BroadcastKillNotification", RpcTarget.All, lastAttacker.NickName, PhotonNetwork.LocalPlayer.NickName);

        // Ensure RoomManager exists before trying to handle respawn
        if (RoomManager.instance != null)
        {
            RoomManager.instance.OnPlayerDeath(); // Open the respawn screen
            RoomManager.instance.deaths++;
            RoomManager.instance.SetHashes(); // Update room stats
        }
        else
        {
            Debug.LogError("RoomManager instance is null. Cannot open respawn screen.");
        }
    }

    // RPC to broadcast a kill notification
    [PunRPC]
    public void BroadcastKillNotification(string killerName, string victimName)
    {
        KillFeedManager.Instance.AddKillNotification(killerName, victimName);
    }

    // Coroutine for the camera flinch effect
    private IEnumerator CameraFlinch()
    {
        if (playerCamera != null)
        {
            Quaternion originalRotation = playerCamera.transform.localRotation;

            float elapsedTime = 0f;
            while (elapsedTime < flinchDuration)
            {
                float flinchX = Random.Range(-flinchIntensity, flinchIntensity);
                float flinchY = Random.Range(-flinchIntensity, flinchIntensity);

                // Apply flinch to the camera
                playerCamera.transform.localRotation = originalRotation * Quaternion.Euler(flinchX, flinchY, 0);

                elapsedTime += Time.deltaTime;
                yield return null;
            }

            // Reset camera to original rotation
            playerCamera.transform.localRotation = originalRotation;
        }
    }

    // Coroutine for the red flash effect
    private IEnumerator RedFlashEffect()
    {
        if (redFlashImage != null)
        {
            // Set initial red flash color
            redFlashImage.color = flashColor;

            float elapsedTime = 0f;
            Color originalColor = redFlashImage.color;
            while (elapsedTime < redFlashDuration)
            {
                float alpha = Mathf.Lerp(flashColor.a, 0, elapsedTime / redFlashDuration);
                redFlashImage.color = new Color(flashColor.r, flashColor.g, flashColor.b, alpha);
                elapsedTime += Time.deltaTime;
                yield return null;
            }

            // Ensure the red flash is completely gone
            redFlashImage.color = new Color(flashColor.r, flashColor.g, flashColor.b, 0);
        }
    }
}
