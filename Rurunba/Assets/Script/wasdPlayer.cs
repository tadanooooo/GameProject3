using UnityEngine;
using UnityEngine.InputSystem;

public class WASDPlayer : MonoBehaviour
{
    [Header("ëñçsê›íË")]
    public float moveSpeed = 15f;

    [Header("âÒì]ÇÃë¨Ç≥")]
    public float turnSmoothSpeed = 10f;

    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
    }

    void Update()
    {
        Vector2 input = Vector2.zero;

        // WASDì¸óÕéÊìæ
        if (Keyboard.current != null)
        {
            if (Keyboard.current.wKey.isPressed) input.y += 1;
            if (Keyboard.current.sKey.isPressed) input.y -= 1;
            if (Keyboard.current.aKey.isPressed) input.x -= 1;
            if (Keyboard.current.dKey.isPressed) input.x += 1;
        }

        Debug.Log(input);

        Vector3 moveInput = new Vector3(input.x, 0, input.y);

        if (moveInput.magnitude > 0.1f)
        {
            rb.linearVelocity = new Vector3(
                moveInput.x * moveSpeed,
                rb.linearVelocity.y,
                moveInput.z * moveSpeed
            );

            Quaternion targetRotation = Quaternion.LookRotation(moveInput);

            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRotation,
                turnSmoothSpeed * Time.deltaTime
            );
        }
        else
        {
            rb.linearVelocity = new Vector3(
                0,
                rb.linearVelocity.y,
                0
            );
        }
    }
}