using System.Collections;
using Photon.Pun;
using UnityEngine;
using UnityEngine.Animations.Rigging;

public class WeaponSwitcher : MonoBehaviour
{
    [Header("Set Up")]
    public PhotonView playerSetupView;

    [Header("IK Constraints for FP")]
    public Animator fpAnimator; // First-Person Animator
    public TwoBoneIKConstraint fpRightHandIKConstraint;
    public TwoBoneIKConstraint fpLeftHandIKConstraint;
    public RigBuilder fpRigBuilder; // First-Person RigBuilder

    private int selectedWeapon = 0;
    private int totalWeapons;
    void Start()
    {
        totalWeapons = transform.childCount; // Get the total number of weapons
        SelectWeapon();
    }

    void Update()
    {
        if (!playerSetupView.IsMine) return;

        int previousSelectedWeapon = selectedWeapon;

        HandleNumberKeyInput();
        HandleScrollInput();

        if (previousSelectedWeapon != selectedWeapon)
        {
            SelectWeapon();
        }
    }

    private void HandleNumberKeyInput()
    {
        for (int i = 0; i < totalWeapons; i++)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1 + i))
            {
                selectedWeapon = i;
                break;
            }
        }
    }

    private void HandleScrollInput()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll > 0f)
        {
            selectedWeapon = (selectedWeapon + 1) % totalWeapons;
        }
        else if (scroll < 0f)
        {
            selectedWeapon = (selectedWeapon - 1 + totalWeapons) % totalWeapons;
        }
    }

    void SelectWeapon()
    {
        if (playerSetupView.IsMine)
        {
            playerSetupView.RPC("SetTPWeapon", RpcTarget.All, selectedWeapon);
        }

        int index = 0;
        foreach (Transform weapon in transform)
        {
            bool isActive = index == selectedWeapon;
            weapon.gameObject.SetActive(isActive);

            if (isActive)
            {
                StartCoroutine(PlayDrawAnimation());
                UpdateWeaponUI(weapon);

                // Get the script that holds IK targets for the first-person model
                Weapon weaponScript = weapon.GetComponent<Weapon>();
                // Assuming the third-person weapons are synced with the TPweaponHolder indices

                if (weaponScript != null)
                {
                    UpdateIKConstraints(weaponScript.rightHandIK, weaponScript.leftHandIK);
                }
            }
            index++;
        }
    }

    private void UpdateIKConstraints(Transform fpRightTarget, Transform fpLeftTarget)
    {
        // Update first-person IK targets
        if (fpRightHandIKConstraint != null && fpRightTarget != null)
            fpRightHandIKConstraint.data.target = fpRightTarget;
        if (fpLeftHandIKConstraint != null && fpLeftTarget != null)
            fpLeftHandIKConstraint.data.target = fpLeftTarget;

        // Rebuild the IK rigs
        if (fpRigBuilder != null)
            fpRigBuilder.Build();

    }


    private IEnumerator PlayDrawAnimation()
    {
        if (fpAnimator != null)
        {
            fpAnimator.SetTrigger("drawTrigger");

            yield return new WaitForSeconds(1f); // Wait for the animation length

            fpAnimator.ResetTrigger("drawTrigger");
        }
    }

    private void UpdateWeaponUI(Transform weapon)
    {
        IWeapon weaponScript = weapon.GetComponent<IWeapon>();
        if (weaponScript != null)
        {
            weaponScript.UpdateWeaponUI(); // Update the UI based on the weapon
        }
    }
}
