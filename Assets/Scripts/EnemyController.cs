using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.AI;

public class EnemyController : MonoBehaviourPunCallbacks, IPunObservable
{
    public float health = 100f;
    public float damage = 10f;
    public float attackRange = 2f;
    public float detectionRange = 15f;
    public float movementSpeed = 3.5f;

    private Transform target;
    private NavMeshAgent agent;

    private bool isDead = false;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.speed = movementSpeed;

        if (PhotonNetwork.IsMasterClient)
        {
            FindClosestPlayer();
        }
    }

    void Update()
    {
        if (!PhotonNetwork.IsMasterClient || isDead) return;

        if (target != null)
        {
            float distanceToTarget = Vector3.Distance(transform.position, target.position);

            if (distanceToTarget <= attackRange)
            {
                AttackPlayer();
            }
            else if (distanceToTarget <= detectionRange)
            {
                agent.SetDestination(target.position);
            }
        }
    }

    private void FindClosestPlayer()
    {
        // Find the closest player
        Player[] players = PhotonNetwork.PlayerList;
        if (players.Length > 0)
        {
            target = GameObject.FindWithTag("Player").transform;
        }
    }

     [PunRPC]
    public void TakeDamage(float damage)
    {
        if (!photonView.IsMine) return; // Apply damage only on the owner client

        health -= damage;
        if (health <= 0)
        {
            Die();
        }
    }

    private void AttackPlayer()
    {
        if (target != null)
        {
            Health playerHealth = target.GetComponent<Health>();
            if (playerHealth != null && PhotonNetwork.IsMasterClient)
            {
                // Send RPC to apply damage
                playerHealth.photonView.RPC("TakeDamage", RpcTarget.All, (int)damage, "Zombie", PhotonNetwork.LocalPlayer);
            }
        }
    }

    private void Die()
    {
        isDead = true;
        agent.isStopped = true;
        PhotonNetwork.Destroy(gameObject);
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(isDead);
            stream.SendNext(transform.position);
        }
        else
        {
            isDead = (bool)stream.ReceiveNext();
            Vector3 position = (Vector3)stream.ReceiveNext();
            transform.position = position;
        }
    }
}
