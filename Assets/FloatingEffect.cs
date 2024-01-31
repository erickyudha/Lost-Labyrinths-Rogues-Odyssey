using UnityEngine;

public class FloatingEffect : MonoBehaviour
{
    public float floatSpeed = 1.0f; // Speed of the floating effect
    public float floatAmount = 0.5f; // Amount of float movement
    private Vector3 startPosition; // Initial position of the GameObject

    void Start()
    {
        // Store the initial position of the GameObject
        startPosition = transform.position;
    }

    void Update()
    {
        // Calculate the floating movement using Mathf.Sin and Time
        float newPositionY = startPosition.y + Mathf.Sin(Time.time * floatSpeed) * floatAmount;

        // Update the GameObject's position to create the floating effect
        transform.position = new Vector3(transform.position.x, newPositionY, transform.position.z);
    }
}
