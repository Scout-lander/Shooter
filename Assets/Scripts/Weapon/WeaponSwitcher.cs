using System.Collections;
using Photon.Pun;
using UnityEngine;
using UnityEngine.Animations.Rigging;

public class WeaponSwitcher : MonoBehaviour
{
    [Header("Photon & Animation")]
    public PhotonView playerSetupView;
    public Animator weaponAnimator; // Reference to the Animator

    [Header("IK Constraints")]
    public TwoBoneIKConstraint rightHandIKConstraint;
    public TwoBoneIKConstraint leftHandIKConstraint;

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

                Weapon weaponScript = weapon.GetComponent<Weapon>();
                if (weaponScript != null)
                {
                    UpdateIKConstraints(weaponScript.rightHandIK, weaponScript.leftHandIK);
                }
            }
            index++;
        }
    }

    public RigBuilder rigBuilder; // Assign this in the inspector

    private void UpdateIKConstraints(Transform rightTarget, Transform leftTarget)
    {
        if (rightHandIKConstraint != null && rightTarget != null)
        {
            rightHandIKConstraint.data.target = rightTarget;
        }
        if (leftHandIKConstraint != null && leftTarget != null)
        {
            leftHandIKConstraint.data.target = leftTarget;
        }

        // Force the rig to update
        if (rigBuilder != null)
        {
            rigBuilder.Build();
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

    private void UpdateWeaponUI(Transform weapon)
    {
        IWeapon weaponScript = weapon.GetComponent<IWeapon>();
        if (weaponScript != null)
        {
            weaponScript.UpdateWeaponUI(); // Update the UI based on the weapon
        }
    }
}
