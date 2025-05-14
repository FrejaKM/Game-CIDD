using UnityEngine;

public class NewMonoBehaviourScript : MonoBehaviour
{

    public Transform target;

    private float offset = 0.5f;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 newPosition = target.position;
        newPosition.y -= offset;
        
        transform.position = newPosition;
        
    }
}
