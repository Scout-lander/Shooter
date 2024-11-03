using System.Collections;
using UnityEngine;
using Photon.Pun;
using TMPro;
using UnityEngine.UI;

public class Health : MonoBehaviourPun
{
    public int health;
    public int maxHealth = 100; // Define the maximum health
    public bool isLocalPlayer;
    private bool hasDied;

    [Header("UI")]
    public TextMeshProUGUI healthText;
    public Slider healthBar; // Slider for health bar
    public RoomManager roomManager;

    [Header("Camera Effects")]
    public GameObject playerCamera; 
    public float flinchIntensity = 5f;
    public float flinchDuration = 0.1f;

    [Header("Red Flash Effect")]
    public Image redFlashImage;
    public float redFlashDuration = 0.3f;
    public Color flashColor = new Color(1f, 0f, 0f, 0.5f);

    private Photon.Realtime.Player lastAttacker;

    void Start()
    {
        // Initialize health and health bar on start
        health = maxHealth;
        if (healthBar != null)
        {
            healthBar.maxValue = maxHealth;
            healthBar.value = health;
        }
        UpdateHealthUI();
    }

    void Update()
    {
        if (isLocalPlayer && Input.GetKeyDown(KeyCode.P))
        {
            TakeDamage(30, "DamageTest");
        }
    }

    [PunRPC]
    public void TakeDamage(int damage, string attackerName, bool isHeadshot = false)
    {
        if (hasDied) return;

        // Apply headshot multiplier if true
        if (isHeadshot) {
            damage *= 2; // For example, double the damage
        }

        health -= damage;
        health = Mathf.Clamp(health, 0, maxHealth);
        UpdateHealthUI();

        if (isLocalPlayer)
        {
            StartCoroutine(CameraFlinch());
            StartCoroutine(RedFlashEffect());
        }

        if (health <= 0 && !hasDied && photonView.IsMine)
        {
            hasDied = true;
            photonView.RPC("BroadcastKillNotification", RpcTarget.All, attackerName, PhotonNetwork.LocalPlayer.NickName);
            HandlePlayerDeath();
            PhotonNetwork.Destroy(gameObject);
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        // Check if the hit is on a player
        if (collision.gameObject.CompareTag("Player"))
        {
            bool isHeadshot = collision.collider.CompareTag("Head");
            int damageAmount = isHeadshot ? 50 : 20; // Example damage values
            string attackerName = PhotonNetwork.LocalPlayer.NickName;

            collision.gameObject.GetComponent<Health>().photonView.RPC("TakeDamage", RpcTarget.AllBuffered, damageAmount, attackerName, isHeadshot);
        }
    }

    private void UpdateHealthUI()
    {
        // Update health text and health bar slider
        if (healthText != null)
        {
            healthText.text = health.ToString();
        }
        if (healthBar != null)
        {
            healthBar.value = health;
        }
    }

    private void HandlePlayerDeath()
    {
        // Broadcast the kill notification to all clients
        if (lastAttacker != null && photonView.IsMine)
        {
            photonView.RPC("BroadcastKillNotification", RpcTarget.All, lastAttacker.NickName, PhotonNetwork.LocalPlayer.NickName);
        }

        if (RoomManager.instance != null)
        {
            RoomManager.instance.OnPlayerDeath();
            RoomManager.instance.deaths++;
            RoomManager.instance.SetHashes();
        }
        else
        {
            Debug.LogError("RoomManager instance is null. Cannot open respawn screen.");
        }
    }

    [PunRPC]
    public void BroadcastKillNotification(string killerName, string victimName)
    {
        KillFeedManager.Instance.AddKillNotification(killerName, victimName);
    }

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

                playerCamera.transform.localRotation = originalRotation * Quaternion.Euler(flinchX, flinchY, 0);
                elapsedTime += Time.deltaTime;
                yield return null;
            }

            playerCamera.transform.localRotation = originalRotation;
        }
    }

    private IEnumerator RedFlashEffect()
    {
        if (redFlashImage != null)
        {
            redFlashImage.color = flashColor;
            float elapsedTime = 0f;

            while (elapsedTime < redFlashDuration)
            {
                float alpha = Mathf.Lerp(flashColor.a, 0, elapsedTime / redFlashDuration);
                redFlashImage.color = new Color(flashColor.r, flashColor.g, flashColor.b, alpha);
                elapsedTime += Time.deltaTime;
                yield return null;
            }

            redFlashImage.color = new Color(flashColor.r, flashColor.g, flashColor.b, 0);
        }
    }
}
