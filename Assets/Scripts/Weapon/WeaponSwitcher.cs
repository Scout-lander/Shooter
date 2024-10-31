using System.Collections;
using Photon.Pun;
using UnityEngine;
using UnityEngine.Animations.Rigging;

public class WeaponSwitcher : MonoBehaviour
{
    public PhotonView playerSetupView;
    public Animator weaponAnimator; // Reference to the Animator

    // References to Two Bone IK Constraints
    public Transform rightHandIKTarget;
    public Transform leftHandIKTarget;

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

        // If the selected weapon has changed, select the new weapon
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

                // Set the IK targets for the active weapon
                Weapon weaponScript = weapon.GetComponent<Weapon>();
                if (weaponScript != null)
                {
                    UpdateIKConstraints(weaponScript.rightHandIK, weaponScript.leftHandIK);
                }
            }
            index++;
        }
    }

    private IEnumerator PlayDrawAnimation()
    {
        if (weaponAnimator != null)
        {
            weaponAnimator.SetTrigger("drawTrigger");

            // Wait for the animation length before continuing
            yield return new WaitForSeconds(1f);

            // After waiting, reset trigger or proceed to the next state
            weaponAnimator.ResetTrigger("drawTrigger");
            //weaponAnimator.Play("WeaponChange");
        }
    }
    private void UpdateIKConstraints(Transform rightTarget, Transform leftTarget)
    {
        if (rightTarget != null && rightHandIKTarget != null)
        {
            rightHandIKTarget.position = rightTarget.position;
            rightHandIKTarget.rotation = rightTarget.rotation;
        }

        if (leftTarget != null && leftHandIKTarget != null)
        {
            leftHandIKTarget.position = leftTarget.position;
            leftHandIKTarget.rotation = leftTarget.rotation;
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
