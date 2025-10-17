using UnityEngine;

public class HazardZone : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("Player")) return;

        PlayerRespawn respawn = collision.GetComponent<PlayerRespawn>();
        if (respawn != null)
        {
            // Kill immediately and respawn (spawns the death FX at the entry point)
            respawn.DieAndRespawn();
        }
    }
}