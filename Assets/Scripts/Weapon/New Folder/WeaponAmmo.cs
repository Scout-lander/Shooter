using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using TMPro;
using UnityEngine;

public class WeaponAmmo : MonoBehaviour
{
    public int mag = 5;
    public int ammo = 30;
    public int magAmmo = 30;
    public TextMeshProUGUI magText;
    public TextMeshProUGUI ammoText;

    private bool isReloading = false;
    public Animator animator;
    private string reloadTrigger = "reload";

    void Start()
    {
        UpdateWeaponUI();
    }

     // Method to check if the weapon can fire
    public bool CanFire()
    {
        return ammo > 0;
    }

    public bool UseAmmo()
    {
        if (ammo > 0)
        {
            ammo--;
            UpdateWeaponUI();
            return true;
        }
        return false;
    }

    public void Reload()
    {
        if (mag > 0 && !isReloading)
        {
            isReloading = true;
            animator.SetTrigger(reloadTrigger);
            StartCoroutine(ReloadRoutine(animator.GetCurrentAnimatorStateInfo(0).length));
        }
    }

    IEnumerator ReloadRoutine(float reloadTime)
    {
        yield return new WaitForSeconds(reloadTime);
        mag--;
        ammo = magAmmo;
        UpdateWeaponUI();
        isReloading = false;
    }

    public void UpdateWeaponUI()
    {
        magText.text = mag.ToString();
        ammoText.text = $"{ammo}/{magAmmo}";
    }
}
