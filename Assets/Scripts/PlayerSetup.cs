using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using TMPro;
using ExitGames.Client.Photon;

public class PlayerSetup : MonoBehaviour
{
    public Movement movement;
    public GameObject camera;
    public string nickname;
    public TextMeshPro nicknameText;

    public Transform TPweaponHolder;

    public void IsLocalPlayer() // Fix method name here
    {
        TPweaponHolder.gameObject.SetActive(false);
        movement.enabled = true;
        camera.SetActive(true);
    }
    [PunRPC]
    public void SetTPWeapon(int _weaponIndex)
    {
        foreach (Transform _weaon in TPweaponHolder)
        {
            _weaon.gameObject.SetActive(false);
        }

        TPweaponHolder.GetChild(_weaponIndex).gameObject.SetActive(true);
    }

    [PunRPC]
    public void SetNickname(string _name)
    {
        nickname = _name;
        nicknameText.text = nickname;
    }

}
