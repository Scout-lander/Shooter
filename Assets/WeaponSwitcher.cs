using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using Unity.VisualScripting;
using UnityEngine;

public class WeaponSwitcher : MonoBehaviour
{

    public PhotonView playerSetupView;
    private int selectedWeapon = 0;
    private int totalWeapons;

    void Start()
    {
        totalWeapons = transform.childCount; // Get the total number of weapons
        SelectWeapon();
    }

    void Update()
    {
        int previousSelectedWeapon = selectedWeapon;

        // Handle weapon selection using number keys
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            selectedWeapon = 0;
        }
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            selectedWeapon = 1;
        }
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            selectedWeapon = 2;
        }
        if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            selectedWeapon = 3;
        }

        // Handle weapon selection using mouse scroll wheel
        float scroll = Input.GetAxis("Mouse ScrollWheel");

        if (scroll > 0f)
        {
            selectedWeapon = (selectedWeapon + 1) % totalWeapons; // Scroll up to the next weapon
        }
        else if (scroll < 0f)
        {
            selectedWeapon = (selectedWeapon - 1 + totalWeapons) % totalWeapons; // Scroll down to the previous weapon
        }

        // If the selected weapon has changed, select the new weapon
        if (previousSelectedWeapon != selectedWeapon)
        {
            SelectWeapon();
        }
    }

    void SelectWeapon()
    {
        playerSetupView.RPC("SetTPWeapon", RpcTarget.All, selectedWeapon);
        int i = 0;

        foreach (Transform weapon in transform)
        {
            if (i == selectedWeapon)
            {
                weapon.gameObject.SetActive(true);

                // Trigger the draw animation
                Animator weaponAnimator = weapon.GetComponent<Animator>();
                if (weaponAnimator != null)
                {
                    weaponAnimator.SetTrigger("drawTrigger");
                }

                // Update UI for the selected weapon
                IWeapon weaponScript = weapon.GetComponent<IWeapon>();
                if (weaponScript != null)
                {
                    weaponScript.UpdateWeaponUI(); // Update the UI based on the weapon
                }
            }
            else
            {
                weapon.gameObject.SetActive(false);
            }

            i++;
        }
    }
}
