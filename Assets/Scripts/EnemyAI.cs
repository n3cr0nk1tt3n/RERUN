using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    [Header("Patrol Settings")]
    public Transform[] waypoints;
    public float moveSpeed = 2f;
    private int currentWaypointIndex = 0;
    private bool movingForward = true;

    [Header("Health Settings")]
    public int maxHealth = 3;
    private int currentHealth;

    [Header("Player Damage Settings")]
    public int damageToPlayer = 1;

    [Header("Sprite Flipping")]
    public bool flipSprite = true;
    public bool spriteFacesRight = true;
    private SpriteRenderer spriteRenderer;

    [Header("Audio")]
    public AudioSource audioSource; // Optional if not set via code
    public AudioClip hitSoundClip;

    private void Start()
    {
        currentHealth = maxHealth;
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (waypoints.Length < 2)
        {
            Debug.LogWarning("EnemyAI requires at least 2 waypoints to patrol.");
        }

        // Try to auto-get AudioSource if not manually set
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }
    }

    private void Update()
    {
        Patrol();
    }

    private void Patrol()
    {
        if (waypoints.Length < 2) return;

        Transform target = waypoints[currentWaypointIndex];
        Vector2 direction = target.position - transform.position;
        Vector2 moveDir = direction.normalized;

        transform.position = Vector2.MoveTowards(transform.position, target.position, moveSpeed * Time.deltaTime);

        // Flip sprite logic
        if (flipSprite && moveDir.x != 0)
        {
            spriteRenderer.flipX = spriteFacesRight ? (moveDir.x < 0) : (moveDir.x > 0);
        }

        // Switch waypoint
        if (Vector2.Distance(transform.position, target.position) < 0.1f)
        {
            if (movingForward)
            {
                currentWaypointIndex++;
                if (currentWaypointIndex >= waypoints.Length)
                {
                    currentWaypointIndex = waypoints.Length - 2;
                    movingForward = false;
                }
            }
            else
            {
                currentWaypointIndex--;
                if (currentWaypointIndex < 0)
                {
                    currentWaypointIndex = 1;
                    movingForward = true;
                }
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            PlayerRespawn player = collision.GetComponent<PlayerRespawn>();
            if (player != null)
            {
                // ✅ Play the damage sound if available
                if (audioSource != null && hitSoundClip != null)
                {
                    audioSource.PlayOneShot(hitSoundClip);
                }

                // Apply damage to player
                player.TakeDamage(damageToPlayer);
            }
        }
    }

    public void TakeDamage(int amount)
    {
        currentHealth -= amount;

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        Destroy(gameObject);
    }
}
