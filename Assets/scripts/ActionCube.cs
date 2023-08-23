using UnityEngine;
using Panda;

public class ActionCube : MonoBehaviour
{
    // Array of enemy transforms
    [SerializeField] private Transform[] Enemies;

    // Current target enemy
    private Transform Target;

    // Movement speed
    private float Velocity = 1f;

    // Energy level
    [SerializeField] private int Energy = 5;

    // Minimum distance to check for enemy
    private float minDistance = 1.5f;

    // Reference to the Rigidbody component
    private Rigidbody rb;

    private Vector3[] patrolPoints; // Store patrol points

    private int currentPatrolIndex = 0; // Index of the current patrol point

    private Vector3 restPosition = new Vector3(0f, 1f, 0f); // Resting position

    private bool isResting = false; // Flag to indicate if cube is resting

    private void Start()
    {
        // Get the Rigidbody component on start
        rb = GetComponent<Rigidbody>();

        // Define the patrol points around the 20x20 plane
        patrolPoints = new Vector3[]
        {
            new Vector3(-8f, 1f, -8f),
            new Vector3(-8f, 1f, 8f),
            new Vector3(8f, 1f, 8f),
            new Vector3(8f, 1f, -8f)
        };
    }

    // Check if an enemy is close enough
    [Task]
    void EnemyClose()
    {
        Debug.Log("Performing EnemyClose task");

        foreach (Transform enemy in Enemies)
        {
            if (Vector3.Distance(transform.position, enemy.position) <= minDistance)
            {
                Target = enemy;
                Task.current.Succeed(); // Enemy is close, succeed the task
                return;
            }
        }
        Task.current.Fail(); // No enemy close, fail the task
    }

    // Move towards the target enemy
    [Task]
    void GoEnemy()
    {
        Debug.Log("Performing GoEnemy task");

        if (Target != null)
        {
            // Calculate direction and move
            Vector3 direction = Target.position - transform.position;
            transform.Translate(direction.normalized * Velocity * Time.deltaTime);
            Task.current.Succeed(); // Continue moving towards enemy
        }
        else
        {
            Task.current.Fail(); // No target set, fail the task
        }
    }

    // Perform an attack action (jump attack)
    [Task]
    void Attack()
    {
        if (Target != null && Energy >= 2)
        {
            // Calculate the direction towards the enemy
            Vector3 directionToEnemy = (Target.position - transform.position).normalized;

            // Apply a jump attack impulse towards the enemy using Rigidbody.AddForce
            float jumpForce = 10f; // Adjust the jump force as needed
            rb.AddForce(directionToEnemy * jumpForce, ForceMode.Impulse);

            Energy--;

            Task.current.Succeed();
        }
        else
        {
            Task.current.Fail();
        }
    }



    // Rest and recover energy at the specified position
    [Task]
    void Rest()
    {
        Debug.Log("Performing Rest task");

        // Check if energy is low and cube is not at rest position
        if (Energy < 5 && !isResting)
        {
            // Move towards the rest position
            Vector3 direction = restPosition - transform.position;
            transform.Translate(direction * Velocity * Time.deltaTime);

            // Check if close enough to the rest position
            if (Vector3.Distance(transform.position, restPosition) < 0.1f)
            {
                isResting = true; // Start resting
                Energy += 1; // Recover energy

                // Change the current patrol index after resting
                currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Length;
            }
        }
        else
        {
            isResting = false; // Stop resting
        }

        // If energy is fully restored, stop resting
        if (Energy == 5)
        {
            isResting = false;
            Task.current.Succeed();
        }
        else
        {
            Task.current.Fail();
        }
    }

    // Patrol around the 20x20 plane
    [Task]
    void Patrol()
    {
        Debug.Log("Performing Patrol task");

        // If resting, stay in place
        if (isResting)
        {
            Task.current.Fail();
            return;
        }

        // Move towards the next patrol point
        Vector3 direction = patrolPoints[currentPatrolIndex] - transform.position;
        transform.Translate(direction.normalized * Velocity * Time.deltaTime);

        // Check if close to the current patrol point
        if (Vector3.Distance(transform.position, patrolPoints[currentPatrolIndex]) < 0.5f)
        {
            // Move to the next patrol point
            currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Length;
        }

        Task.current.Succeed(); // Continue patrolling
    }
}
