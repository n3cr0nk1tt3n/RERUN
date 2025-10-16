using UnityEngine;

public class CameraZoneController : MonoBehaviour
{
    [Header("Player Tracking")]
    public Transform player;          // Reference to player transform

    [Header("Zone Settings")]
    public float zoneWidth = 18f;     // Width of the camera zone (should match camera view width)
    public float zoneHeight = 10f;    // Height of the camera zone (should match camera view height)

    [Header("Camera Movement")]
    public float panSpeed = 5f;       // Speed of camera panning

    private Vector2 currentZoneCenter; // The center of the current camera zone
    private Vector2 initialZoneOrigin; // The bottom-left corner of the initial zone grid

    void Start()
    {
        // Use the current camera position as starting center
        currentZoneCenter = new Vector2(transform.position.x, transform.position.y);

        // Calculate the bottom-left corner of the grid based on the starting camera position.
        // This aligns zones so that your starting camera view is exactly your initial zone.
        float originX = currentZoneCenter.x - (zoneWidth / 2);
        float originY = currentZoneCenter.y - (zoneHeight / 2);
        initialZoneOrigin = new Vector2(originX, originY);
    }

    void Update()
    {
        Vector2 playerPos = player.position;

        // Calculate player offset relative to initial zone origin
        float offsetX = playerPos.x - initialZoneOrigin.x;
        float offsetY = playerPos.y - initialZoneOrigin.y;

        // Determine which zone the player is in, relative to initial origin
        int zoneX = Mathf.FloorToInt(offsetX / zoneWidth);
        int zoneY = Mathf.FloorToInt(offsetY / zoneHeight);

        // Calculate the center of the zone the player is in, relative to initial origin
        Vector2 targetZoneCenter = new Vector2(
            initialZoneOrigin.x + (zoneX * zoneWidth) + (zoneWidth / 2),
            initialZoneOrigin.y + (zoneY * zoneHeight) + (zoneHeight / 2)
        );

        // Only update current zone center if it changed (player moved to a new zone)
        if (targetZoneCenter != currentZoneCenter)
        {
            currentZoneCenter = targetZoneCenter;
        }

        // Smoothly move the camera toward the current zone center
        Vector3 targetPosition = new Vector3(currentZoneCenter.x, currentZoneCenter.y, transform.position.z);
        transform.position = Vector3.Lerp(transform.position, targetPosition, panSpeed * Time.deltaTime);
    }

    // Optional: visualize current zone in editor
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireCube(new Vector3(currentZoneCenter.x, currentZoneCenter.y, 0f), new Vector3(zoneWidth, zoneHeight, 0f));
    }
}
