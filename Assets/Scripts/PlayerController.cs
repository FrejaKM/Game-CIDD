using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class PlayerController : MonoBehaviour
{
    // Rigidbody of the player.
    private Rigidbody rb;

    // Variable to keep track of collected "PickUp" objects.
    private int count;

    // Movement along X and Y axes.
    private float movementX;
    private float movementY;

    // Speed at which the player moves.
    public float speed = 0;
    public float normalSpeed;  // Store the original speed value
    
    // Oil spill effect
    public float oilSpillSpeedMultiplier = 10.4f;  // How much to reduce speed (30% of normal)
    public float oilSpillExtraSlipperiness = 0.7f;  // Extra slipperiness on oil (lower value = more slippery)
    private bool isOnOilSpill = false;
    
    // Drag settings to reduce unwanted rolling
    public float regularDrag = 0.5f;      // Normal drag when moving
    public float stoppingDrag = 3.0f;     // Higher drag when not providing input
    public float rotationalDrag = 0.5f;   // Controls how quickly rotation slows down
    
    // Jump force for controlling jump height
    public float jumpForce = 5.0f;
    
    // Ground check variables
    private bool isGrounded = true;

    // UI text component to display count of "PickUp" objects collected.
    public TextMeshProUGUI countText;

    // UI object to display winning text.
    public GameObject winTextObject;
    
    // Sound effects
    
    // Pickup
    [SerializeField] public AudioClip pickupSoundClip;
    public float pickupSoundClipVolume = 1f;
    
    // Jump
    [SerializeField] public AudioClip jumpSoundClip;
    public float jumpSoundClipVolume = 1f;

    // Start is called before the first frame update.
    void Start()
    {
        // Get and store the Rigidbody component attached to the player.
        rb = GetComponent<Rigidbody>();
        
        // Store the original speed for later reference
        normalSpeed = speed;
        
        // Apply initial drag settings
        rb.linearDamping = regularDrag;
        rb.angularDamping = rotationalDrag;

        // Initialize count to zero.
        count = 0;

        // Update the count display.
        SetCountText();

        // Initially set the win text to be inactive.
        winTextObject.SetActive(false);
    }
 
    // This function is called when a move input is detected.
    void OnMove(InputValue movementValue)
    {
        // Convert the input value into a Vector2 for movement.
        Vector2 movementVector = movementValue.Get<Vector2>();

        // Store the X and Y components of the movement.
        movementX = movementVector.x; 
        movementY = movementVector.y; 
    }

    // Update is called once per frame
    void Update()
    {
        // Check for jump input with space bar
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            SoundFXManager.instance.PlaySoundFXClip(jumpSoundClip, transform, jumpSoundClipVolume);
            isGrounded = false;
        }
    }

    // FixedUpdate is called once per fixed frame-rate frame.
    private void FixedUpdate() 
    {
        // Check if player is providing movement input
        bool hasInput = (Mathf.Abs(movementX) > 0.1f || Mathf.Abs(movementY) > 0.1f);
        
        // Adjust drag based on whether the player is trying to move and if on oil
        float currentRegularDrag = isOnOilSpill ? regularDrag * oilSpillExtraSlipperiness : regularDrag;
        float currentStoppingDrag = isOnOilSpill ? stoppingDrag * oilSpillExtraSlipperiness : stoppingDrag;
        
        rb.linearDamping = hasInput ? currentRegularDrag : currentStoppingDrag;
        
        // Only apply force if there's input
        if (hasInput)
        {
            // Create a 3D movement vector using the X and Y inputs.
            Vector3 movement = new Vector3(movementX, 0.0f, movementY);
            
            // Apply force to the Rigidbody to move the player.
            rb.AddForce(movement * speed);
        }
        else if (isGrounded)
        {
            // Counter rolling - reduced effect when on oil
            float counterForceMagnitude = isOnOilSpill ? 0.2f : 0.5f;
            
            // Counter rolling by applying a counter force to the XZ velocity
            Vector3 counterForce = -rb.linearVelocity;
            counterForce.y = 0; // Don't affect vertical movement
            
            // Apply a mild counter force to slow down without feeling unnatural
            rb.AddForce(counterForce * currentStoppingDrag * counterForceMagnitude, ForceMode.Acceleration);
            
            // Counter rotation as well - less effective on oil
            float rotationDamping = isOnOilSpill ? 0.95f : 0.9f; // Higher value means less damping
            if (rb.angularVelocity.magnitude > 0.1f)
            {
                rb.angularVelocity *= rotationDamping;
            }
        }
    }

    void OnTriggerEnter(Collider other) 
    {
        // Check if the object the player collided with has the "PickUp" tag.
        if (other.gameObject.CompareTag("PickUp")) 
        {
            // Deactivate the collided object (making it disappear).
            other.gameObject.SetActive(false);

            // Increment the count of "PickUp" objects collected.
            count = count + 1;
            
            SoundFXManager.instance.PlaySoundFXClip(pickupSoundClip, transform, pickupSoundClipVolume);

            // Update the count display.
            SetCountText();
        }
        
        // Check if the player entered an oil spill
        if (other.gameObject.CompareTag("OilSpill"))
        {
            EnterOilSpill();
        }
    }
    
    void OnTriggerStay(Collider other)
    {
        // Ensure oil effect stays active as long as player is on the oil
        if (other.gameObject.CompareTag("OilSpill"))
        {
            isOnOilSpill = true;
        }
    }
    
    void OnTriggerExit(Collider other)
    {
        // When the player exits an oil spill
        if (other.gameObject.CompareTag("OilSpill"))
        {
            ExitOilSpill();
        }
    }
    
    // Function to apply oil spill effects
    void EnterOilSpill()
    {
        isOnOilSpill = true;
        speed = normalSpeed * oilSpillSpeedMultiplier;
        
        // Optional: Add visual feedback
        Debug.Log("Entered oil spill - movement impaired!");
    }
    
    // Function to remove oil spill effects
    void ExitOilSpill()
    {
        isOnOilSpill = false;
        speed = normalSpeed;
        
        // Optional: Add visual feedback
        Debug.Log("Exited oil spill - movement restored!");
    }

    // Function to update the displayed count of "PickUp" objects collected.
    void SetCountText() 
    {
        // Update the count text with the current count.
        countText.text = count.ToString() + " / 10";

        // Check if the count has reached or exceeded the win condition.
        if (count >= 10)
        {
            // Display the win text.
            winTextObject.SetActive(true);
        }
    }
    
    private void OnCollisionEnter(Collision collision)
    {
        // Reset isGrounded when the player lands on any surface
        if (collision.contacts[0].normal.y > 0.5f)
        {
            isGrounded = true;
        }
    }
}