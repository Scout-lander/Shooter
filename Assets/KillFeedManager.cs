using UnityEngine;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class KillFeedManager : MonoBehaviour
{
    public static KillFeedManager Instance;

    public GameObject killNotificationPrefab; // Prefab for the kill notification UI
    public Transform notificationParent; // Parent object to hold the notifications
    public float notificationDuration = 5f; // Duration of the notification on screen

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // Method to add a kill notification
    public void AddKillNotification(string killerName, string victimName)
    {
        // Instantiate a new notification from the prefab
        GameObject newNotification = Instantiate(killNotificationPrefab, notificationParent);

        // Find the TMP component and set the text to display the kill message
        TextMeshProUGUI textComponent = newNotification.GetComponentInChildren<TextMeshProUGUI>();
        textComponent.text = $"{killerName} killed {victimName}";

        // Start the coroutine to remove the notification after a duration
        StartCoroutine(RemoveNotificationAfterDelay(newNotification, notificationDuration));
    }

    // Coroutine to remove the notification after the set duration
    private IEnumerator RemoveNotificationAfterDelay(GameObject notification, float delay)
    {
        yield return new WaitForSeconds(delay);

        Destroy(notification); // Remove the notification from the UI
    }
}
