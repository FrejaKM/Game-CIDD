using UnityEngine;

public class Car : MonoBehaviour
{
    public float speed = 5f;
    public bool movingRight = true;
    public float despawnThreshold = 15f; // Distance from origin where car should despawn
    
    void Update()
    {
        // Move the car
        float direction = movingRight ? 1f : -1f;
        transform.Translate(Vector3.right * direction * speed * Time.deltaTime);
        
        // Check if car should despawn based on its position
        if ((movingRight && transform.position.x > despawnThreshold) || 
            (!movingRight && transform.position.x < -despawnThreshold))
        {
            Destroy(gameObject);
        }
    }
    
    // Optional: Visualize the despawn threshold in the editor
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawLine(
            new Vector3(despawnThreshold, transform.position.y, transform.position.z), 
            new Vector3(despawnThreshold, transform.position.y + 2, transform.position.z)
        );
        Gizmos.DrawLine(
            new Vector3(-despawnThreshold, transform.position.y, transform.position.z), 
            new Vector3(-despawnThreshold, transform.position.y + 2, transform.position.z)
        );
    }
}