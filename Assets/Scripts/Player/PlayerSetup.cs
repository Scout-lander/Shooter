using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using TMPro;
using ExitGames.Client.Photon;
using UnityEngine.Animations.Rigging;


public class PlayerSetup : MonoBehaviour
{
    public Movement movement;
    public GameObject camera;
    public string nickname;
    public TextMeshPro nicknameText;

 [Header("Third-Person Components")]
    public Transform TPweaponHolder;
    public GameObject TPModel;
    public Animator tpAnimator; // Third-Person Animator
    public TwoBoneIKConstraint tpRightHandIKConstraint;
    public TwoBoneIKConstraint tpLeftHandIKConstraint;
    public RigBuilder tpRigBuilder; // Third-Person RigBuilder    public GameObject TPModel;

    public void IsLocalPlayer() // Fix method name here
    {
        if (TPModel != null)
        {
            TPModel.SetActive(false);
        }
        TPweaponHolder.gameObject.SetActive(false);
        movement.enabled = true;
        camera.SetActive(true);
    }
    [PunRPC]
    public void SetTPWeapon(int weaponIndex)
    {
        foreach (Transform weapon in TPweaponHolder)
        {
            weapon.gameObject.SetActive(false);
        }

        Transform selectedWeapon = TPweaponHolder.GetChild(weaponIndex);
        selectedWeapon.gameObject.SetActive(true);

        // Update IK targets
        TPWeaponIK weaponIK = selectedWeapon.GetComponent<TPWeaponIK>();
        if (weaponIK != null)
        {
            UpdateIKConstraints(weaponIK.rightHandIKTarget, weaponIK.leftHandIKTarget);
        }
    }

    private void UpdateIKConstraints(Transform rightHandTarget, Transform leftHandTarget)
    {
        if (tpRightHandIKConstraint != null)
            tpRightHandIKConstraint.data.target = rightHandTarget;
        if (tpLeftHandIKConstraint != null)
            tpLeftHandIKConstraint.data.target = leftHandTarget;

        if (tpRigBuilder != null)
            tpRigBuilder.Build();
    }


    [PunRPC]
    public void SetNickname(string _name)
    {
        nickname = _name;
        nicknameText.text = nickname;
    }

}
