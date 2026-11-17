using UnityEngine;
using static TMPro.SpriteAssetUtilities.TexturePacker_JsonArray;

public class BasicEnemy : MonoBehaviour
{
    private Rigidbody2D rb;

    [Header("Enemy Stats")]
    [Tooltip("Amount of hits the enemy can take")]
    [SerializeField] private int hP = 1;
    [Tooltip("How fast the enemy travels towards the player")]
    [SerializeField] private float speed = 1.0f;
    [Tooltip("How far from the enemy can the enemy see")]
    [SerializeField] private float visionRadius = 1.0f;
    [Tooltip("The amount of points rewarded on death")]
    [SerializeField] private float pointYield = 100.0f;

    [Tooltip("When enemy is hit, it is knocked back")]
    [SerializeField] private bool knockbackEnemyOnHit = true;
    [Tooltip("Amount the enemy is knocked back on hit")]
    [SerializeField] private float knockbackAmount = 1.0f;

    [Header("Player")]
    [Tooltip("Player object of the game. Can be left empty if the enemy doesn't target the player")]
    [SerializeField] private GameObject player;
    [Tooltip("Whether the enemy targets/runs towards the player")]
    [SerializeField] bool targetPlayer = true;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        // Code that runs if enemy targets the player
        if (targetPlayer)
        {
            // Make sure that a player was set
            if (player != null)
            {
                // Get x and y values of both player and enemy for checking vision
                float playerX = player.transform.position.x;
                float playerY = player.transform.position.y;
                float x = this.transform.position.x;
                float y = this.transform.position.y;

                float playerDist = Mathf.Sqrt(Mathf.Pow(playerX - x,2) + Mathf.Pow(playerY - y, 2));

                if (playerDist <= visionRadius)
                {
                    // Calculate new coordinates and move the enemy
                    float angle = Mathf.Atan2(playerY - y, playerX - x);

                    float newX = x + speed * Time.deltaTime * Mathf.Cos(angle);
                    float newY = y + speed * Time.deltaTime * Mathf.Sin(angle);

                    transform.position = new Vector3(newX, newY, transform.position.z);

                    transform.rotation = Quaternion.Euler(0, 0, angle * Mathf.Rad2Deg);
                }
            } 
            else
            {
                Debug.Log("Player not set!");
            }
        }
    }

    // When enemy is hit, remove health and destroy enemy if needed
    void Hit()
    {
        hP -= 1;

        if (hP <= 0)
        {
            Die();
        } 
        else if (knockbackEnemyOnHit)
        {
            Knockback();
        }
    }

    // When an enemy dies, update the score and create a paint splatter before destroying the enemy
    void Die()
    {
        UpdateScore();
        SplatterPaint();
        Destroy(gameObject);
    }

    // Update the score based on point yield
    void UpdateScore()
    {
        Debug.Log(pointYield + " points obtained!");
    }

    // Create paint splatter on the ground
    void SplatterPaint()
    {
        
    }

    // Knockback the enemy a distance
    void Knockback()
    {
        // Make sure that a player was set
        if (player != null)
        {
            // Get x and y values of both player and enemy for checking vision
            float playerX = player.transform.position.x;
            float playerY = player.transform.position.y;
            float x = this.transform.position.x;
            float y = this.transform.position.y;

            // Calculate new coordinates and move the enemy
            float angle = Mathf.Atan2(playerY - y, playerX - x);

            float newX = x - knockbackAmount * Mathf.Cos(angle);
            float newY = y - knockbackAmount * Mathf.Sin(angle);

            transform.position = new Vector3(newX, newY, transform.position.z);

            transform.rotation = Quaternion.Euler(0, 0, angle * Mathf.Rad2Deg);
        }
        else
        {
            Debug.Log("Player not set!");
        }
    }

    
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject == player)
        {
            // Stun the player for being hit and damage the enemy
            player.GetComponent<PlayerMovement>().HitByEnemy();
            Hit();

            // Reset velocity of enemy so it doesn't go flying off infinitely
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
        }
    }
}
