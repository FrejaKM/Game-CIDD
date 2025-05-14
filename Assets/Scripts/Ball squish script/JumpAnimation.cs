using UnityEngine;

public class JumpAnimation : MonoBehaviour
{
    public Animator animator;
    public string trigger;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        animator = GetComponent<Animator>();
        
    }

    // Update is called once per frame
    
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            animator.SetTrigger(trigger);
        }
    }
}