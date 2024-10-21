using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using TMPro;
using UnityEngine.UI;

public class Health : MonoBehaviour
{
    public int health;
    public bool isLocalPlayer;

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

    [PunRPC]
    public void TakeDamage(int _damage)
    {
        health -= _damage;

        // Update health UI
        if (healthText != null)
        {
            healthText.text = health.ToString();
        }

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
                // Check if RoomManager exists before trying to respawn
                if (RoomManager.instance != null)
                {   
                    RoomManager.instance.OnPlayerDeath();
                    RoomManager.instance.SpawnPlayer();
                    RoomManager.instance.deaths++;
                    RoomManager.instance.SetHashes();
                }
                else
                {
                    Debug.LogError("RoomManager instance is null. Cannot respawn the player.");
                }
            }

            // Destroy the gameObject after the respawn process
            Destroy(gameObject);
        }
    }

    // Coroutine for the camera flinch effect
    private IEnumerator CameraFlinch()
    {
        if (playerCamera != null)
        {
            // Get original rotation
            Quaternion originalRotation = playerCamera.transform.localRotation;

            // Apply random rotation for the flinch effect
            float elapsedTime = 0f;
            while (elapsedTime < flinchDuration)
            {
                float flinchX = Random.Range(-flinchIntensity, flinchIntensity);
                float flinchY = Random.Range(-flinchIntensity, flinchIntensity);

                playerCamera.transform.localRotation = originalRotation * Quaternion.Euler(flinchX, flinchY, 0);

                elapsedTime += Time.deltaTime;
                yield return null;
            }

            // Return camera to original rotation
            playerCamera.transform.localRotation = originalRotation;
        }
    }

    // Coroutine for the red flash effect
    private IEnumerator RedFlashEffect()
    {
        if (redFlashImage != null)
        {
            // Set the initial red flash color
            redFlashImage.color = flashColor;

            // Fade the red flash out over time
            float elapsedTime = 0f;
            Color originalColor = redFlashImage.color;
            while (elapsedTime < redFlashDuration)
            {
                float alpha = Mathf.Lerp(flashColor.a, 0, elapsedTime / redFlashDuration);
                redFlashImage.color = new Color(flashColor.r, flashColor.g, flashColor.b, alpha);
                elapsedTime += Time.deltaTime;
                yield return null;
            }

            // Ensure the flash effect is fully gone
            redFlashImage.color = new Color(flashColor.r, flashColor.g, flashColor.b, 0);
        }
    }
}
