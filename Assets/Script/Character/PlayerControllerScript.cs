using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(Rigidbody))]
public class PlayerControllerScript : NetworkBehaviour
{
    [SerializeField] private LayerMask groundMask;
    public float speed = 5.0f;
    public float rotationSpeed = 10.0f;
    public float dashForce = 100f;
    public float dashDuration = 1.5f;
    public bool canDash = true;
    public Transform orientation;
    private Camera mainCamera;
    private Animator animator;
    private Rigidbody rb;
    private bool IsWalk;
    private Vector3 movementDirection;
    void Start()
    {
        if (!IsOwner) return;
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody>();
        IsWalk = false;
        mainCamera = Camera.main;

    }

    void moveForward()
    {
        float verticalInput = Input.GetAxis("Vertical");
        float horizontalInput = Input.GetAxis("Horizontal");
        movementDirection = transform.forward * verticalInput + transform.right * horizontalInput;

        if (movementDirection.magnitude > 1f)
        {
            movementDirection.Normalize();
        }
        rb.MovePosition(transform.position + movementDirection * speed * Time.fixedDeltaTime);
        if (Mathf.Abs(verticalInput) > 0.01f)
        {
            if (verticalInput > 0.01f)
            {
                if (!IsWalk)
                {

                    IsWalk = true;
                    animator.SetBool("IsWalk", true);

                }
            }
        }
        else if (IsWalk)
        {
            IsWalk = false;
            animator.SetBool("IsWalk", false);
        }
    }

    void FaceMousePosition()
    {
        Vector3 mousePosition = Input.mousePosition;
        mousePosition.z = mainCamera.transform.position.y - transform.position.y;
        Vector3 worldMousePosition = mainCamera.ScreenToWorldPoint(mousePosition);
        Vector3 direction = worldMousePosition - transform.position;
        direction.y = 0f; // Keep the direction only in the x and z axes

        if (direction != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);

        }

    }
    void Dash()
    {
        if (!IsOwner) return;
        if (Input.GetKeyDown(KeyCode.Space) && canDash)
        {
            Vector3 dashDirection = transform.forward; // Change this to the direction you want to dash in
            rb.AddForce(dashDirection * dashForce);
            canDash = false;
            StartCoroutine(DashCooldown());
        }
    }
    private IEnumerator DashCooldown()
    {
        yield return new WaitForSeconds(dashDuration);
        canDash = true;
    }

    private void FixedUpdate()
    {
        if (!IsOwner) return;
        moveForward();
        FaceMousePosition();
        Dash();
    }
}

