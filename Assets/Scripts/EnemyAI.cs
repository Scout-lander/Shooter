using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class EnemyAI : MonoBehaviour
{
    public float wanderRadius = 10f;           // Radius within which the AI wanders randomly
    public float detectionRange = 15f;         // Range within which the AI detects potential targets
    public float attackRange = 2f;             // Range within which the AI can attack a target
    public float wanderWaitTime = 3f;          // Time between random wander points
    public float attackCooldown = 1.5f;        // Time between attacks

    [Header("Movement Speeds")]
    public float walkSpeed = 3.5f;             // Walking speed
    public float runSpeed = 7f;                // Running speed when a player is detected
    public float turnSpeed = 720f;             // How quickly the AI turns toward the target

    private NavMeshAgent agent;
    private Health targetHealth;               // Current target's Health component
    private bool isWandering = false;
    private bool canAttack = true;             // Controls attack cooldown

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.speed = walkSpeed;               // Set initial speed to walking speed
        agent.angularSpeed = turnSpeed;        // Set turning speed for quicker response
        StartCoroutine(Wander());
    }

    void Update()
    {
        // Check for nearby targets with Health component if not currently attacking or chasing
        if (canAttack && targetHealth == null)
        {
            DetectTarget();
        }

        if (targetHealth != null)
        {
            float distanceToTarget = Vector3.Distance(transform.position, targetHealth.transform.position);

            if (distanceToTarget <= attackRange && canAttack)
            {
                // Attack the target if within attack range
                StartCoroutine(AttackTarget());
            }
            else if (distanceToTarget <= detectionRange)
            {
                // Continuously move towards the target while within detection range
                agent.speed = runSpeed;        // Set speed to running speed
                agent.SetDestination(targetHealth.transform.position);
            }
            else
            {
                // Clear the target if it moves out of detection range or is dead
                if (targetHealth.health <= 0 || distanceToTarget > detectionRange)
                {
                    targetHealth = null;
                    agent.speed = walkSpeed;   // Revert to walking speed
                    if (!isWandering) StartCoroutine(Wander());
                }
            }
        }
        else if (!isWandering)
        {
            // Resume wandering if no target is detected
            StartCoroutine(Wander());
        }
    }

    private void DetectTarget()
    {
        // Find all colliders within the detection range
        Collider[] colliders = Physics.OverlapSphere(transform.position, detectionRange);

        targetHealth = null;
        foreach (var collider in colliders)
        {
            Health health = collider.GetComponent<Health>();
            if (health != null && health.health > 0)
            {
                targetHealth = health;
                break; // Exit the loop as soon as a valid target is found
            }
        }
    }

    private IEnumerator AttackTarget()
    {
        canAttack = false;  // Disable attack until cooldown is over

        if (targetHealth != null && targetHealth.health > 0)
        {
            // Perform attack logic here (e.g., deal damage to the target's Health component)
            Debug.Log("Attacking target with Health component!");
            targetHealth.TakeDamage(10, "EnemyAI", false); // Provide "EnemyAI" as the attacker name and assume no headshot
        }

        yield return new WaitForSeconds(attackCooldown);  // Wait for the attack cooldown
        canAttack = true;  // Re-enable attack
    }

    private IEnumerator Wander()
    {
        isWandering = true;

        while (targetHealth == null)  // Continue wandering if no target is detected
        {
            // Pick a random point within the wander radius
            Vector3 wanderPoint = GetRandomWanderPoint();

            // Move the agent to the new wander point
            agent.speed = walkSpeed;           // Ensure the agent is walking while wandering
            agent.SetDestination(wanderPoint);

            // Wait for a few seconds before picking the next point
            yield return new WaitForSeconds(wanderWaitTime);
        }

        isWandering = false;
    }

    private Vector3 GetRandomWanderPoint()
    {
        // Generate a random point within the wander radius
        Vector3 randomDirection = Random.insideUnitSphere * wanderRadius;
        randomDirection += transform.position;

        NavMeshHit navHit;
        NavMesh.SamplePosition(randomDirection, out navHit, wanderRadius, -1);

        return navHit.position;
    }

    private void OnDrawGizmosSelected()
    {
        // Draw detection and wander ranges in the editor
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, wanderRadius);
    }
}
