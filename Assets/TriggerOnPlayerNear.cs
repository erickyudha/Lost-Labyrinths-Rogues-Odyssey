using UnityEngine;
using UnityEngine.Events;

public class TriggerOnPlayerNear : MonoBehaviour
{
    private Player player;
    public float detectionRange = 3f; // Detection range to trigger action
    public UnityEvent onPlayerEnterRange; // Event when player enters the detection range
    public UnityEvent onPlayerExitRange; // Event when player exits the detection range

    private bool playerNear = false;

    void Awake()
    {
        player = SessionManager.player;
    }

    void Update()
    {
        if (player != null)
        {
            float distance = Vector3.Distance(transform.position, player.transform.position);

            if (distance <= detectionRange && !playerNear)
            {
                // Player has come within the detection range
                playerNear = true;
                // Trigger UnityEvent when player enters range
                onPlayerEnterRange.Invoke();
            }
            else if (distance > detectionRange && playerNear)
            {
                // Player has moved out of the detection range
                playerNear = false;
                // Trigger UnityEvent when player exits range
                onPlayerExitRange.Invoke();
            }
        }
    }
}
