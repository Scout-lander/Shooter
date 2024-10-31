using Photon.Pun;
using UnityEngine;

public class PlayerPhotonSound : MonoBehaviour
{
    public AudioSource footStepSource;
    public AudioClip footstepSFX;

    public AudioSource gunShootSource;
    public AudioClip[] allGunShootSFX;


    public void PlayFootstepSFX()
    {
        GetComponent<PhotonView>().RPC("PlayFoorstep_RPC", RpcTarget.All);
    }

    [PunRPC]
    public void PlayFoorstep_RPC()
    {
        footStepSource.clip = footstepSFX;

        footStepSource.pitch = UnityEngine.Random.Range(0.7f, 1.2f);
        footStepSource.volume = UnityEngine.Random.Range(0.2f, 0.35f);

        footStepSource.Play();
    }

    public void PlayShootSFX(int index)
    {
        GetComponent<PhotonView>().RPC("PlayShootSFX_RPC", RpcTarget.All, index);
    }

    [PunRPC]
    public void PlayShootSFX_RPC(int index)
    {
        gunShootSource.clip = allGunShootSFX[index];

        gunShootSource.pitch = UnityEngine.Random.Range(0.7f, 1.2f);
        gunShootSource.volume = UnityEngine.Random.Range(0.2f, 0.35f);

        gunShootSource.Play();
    }
}
