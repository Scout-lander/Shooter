using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SendAnimEventToSFX : MonoBehaviour
{
    public PlayerPhotonSound playerPhotonSound;

    public void TriggerFootstep() 
    {
        playerPhotonSound.PlayFootstepSFX();
    }
}
