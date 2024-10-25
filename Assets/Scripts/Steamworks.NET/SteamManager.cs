using UnityEngine;
using Steamworks;

public class SteamManager : MonoBehaviour
{
    private static SteamManager _instance;

    private void Awake()
    {
        if (_instance != null)
        {
            Destroy(gameObject);
            return;
        }

        _instance = this;
        DontDestroyOnLoad(gameObject);

        if (!SteamAPI.Init())
        {
            Debug.LogError("SteamAPI Init failed!");
            return;
        }
        Debug.Log("SteamAPI Initialized successfully.");
    }

    private void OnDisable()
    {
        if (SteamAPI.IsSteamRunning())
        {
            SteamAPI.Shutdown();
        }
    }

    private void Update()
    {
        if (SteamAPI.IsSteamRunning())
        {
            SteamAPI.RunCallbacks();
        }
    }
}
