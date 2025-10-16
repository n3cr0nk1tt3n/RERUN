using UnityEngine;

public class HazardZone : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            PlayerRespawn respawn = collision.GetComponent<PlayerRespawn>();
            if (respawn != null)
            {
                Debug.Log("Player entered lava hazard. Respawning...");
                respawn.DieAndRespawn();
            }
        }
    }
}
