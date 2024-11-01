using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Recoil : MonoBehaviour
{
    public Animator animator;
    private string recoilTrigger = "recoil";

    public void PerformRecoil()
    {
        animator.SetTrigger(recoilTrigger);
        // Additional recoil logic
    }
}
