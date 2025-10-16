using UnityEngine;

public class CheckpointManager : MonoBehaviour
{
    public static CheckpointManager Instance;

    private Checkpoint currentCheckpoint;

    private void Awake()
    {
        // Singleton pattern
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    public void ActivateCheckpoint(Checkpoint newCheckpoint)
    {
        // Reactivate the previous checkpoint if one was active
        if (currentCheckpoint != null && currentCheckpoint != newCheckpoint)
        {
            currentCheckpoint.Reactivate();
        }

        // Set the new checkpoint as current and hide it
        currentCheckpoint = newCheckpoint;
        newCheckpoint.Hide();
    }
}
