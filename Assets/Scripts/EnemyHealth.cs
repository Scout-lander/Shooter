using UnityEngine;
using Photon.Pun;
using System.Collections;
using System.Collections.Generic;

public class EnemyHealth : MonoBehaviourPun
{
    public int maxHealth = 100; 
    private int currentHealth;
    private bool hasDied = false;

    [Header("Effects")]
    public GameObject deathEffect;  // Optional effect on death
    public float flinchIntensity = 5f;
    public float flinchDuration = 0.1f;

    private Photon.Realtime.Player lastAttacker;

    void Start()
    {
        currentHealth = maxHealth;
    }

    [PunRPC]
    public void TakeDamage(int damage, Photon.Realtime.Player attacker)
    {
        if (hasDied) return;

        currentHealth -= damage;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        lastAttacker = attacker;

        StartCoroutine(FlinchEffect());

        if (currentHealth <= 0 && !hasDied)
        {
            hasDied = true;
            HandleDeath();
        }
    }

    private void HandleDeath()
    {
        photonView.RPC("BroadcastEnemyDeath", RpcTarget.All, lastAttacker.NickName);

        if (deathEffect != null)
        {
            Instantiate(deathEffect, transform.position, Quaternion.identity);
        }

        PhotonNetwork.Destroy(gameObject);
    }

    [PunRPC]
    public void BroadcastEnemyDeath(string killerName)
    {
        Debug.Log($"{killerName} has killed an enemy!");
    }

    private IEnumerator FlinchEffect()
    {
        // Basic flinch effect when taking damage
        Quaternion originalRotation = transform.localRotation;
        float elapsedTime = 0f;

        while (elapsedTime < flinchDuration)
        {
            float flinchX = Random.Range(-flinchIntensity, flinchIntensity);
            float flinchY = Random.Range(-flinchIntensity, flinchIntensity);

            transform.localRotation = originalRotation * Quaternion.Euler(flinchX, flinchY, 0);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        transform.localRotation = originalRotation;
    }
}
