using UnityEngine;

public class WeaponFireControl : MonoBehaviour
{
    public enum FireMode { Single, Auto, Burst }
    public FireMode currentFireMode = FireMode.Single;
    public float fireRate = 0.3f;
    public int burstCount = 3;
    public float burstFireRate = 0.1f;

    private float nextFire;
    private Coroutine burstCoroutine;

    // References to other weapon components
    public WeaponAmmo weaponAmmo;
    public Recoil weaponRecoil;
    public Animator animator;
    private string recoilTrigger = "recoil";

    void Update()
    {
        if (nextFire > 0)
        {
            nextFire -= Time.deltaTime;
        }

        switch (currentFireMode)
        {
            case FireMode.Auto:
                HandleAutomaticFire();
                break;
            case FireMode.Single:
                HandleSingleShotFire();
                break;
            case FireMode.Burst:
                HandleBurstFire();
                break;
        }
    }

    public void HandleSingleShotFire()
    {
        // Implementation for single-shot fire
    }

    public void HandleAutomaticFire()
    {
        // Implementation for automatic fire
    }

    public void HandleBurstFire()
    {
        // Implementation for burst fire
    }

    public void Fire()
    {
        if (weaponAmmo.UseAmmo())
        {
            animator.SetTrigger(recoilTrigger);
            weaponRecoil.PerformRecoil();
            // Further fire logic
        }
    }

    public void SwitchFireMode()
    {
        // Logic to switch fire modes
    }
}
