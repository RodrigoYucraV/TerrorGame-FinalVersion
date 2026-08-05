using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float speed = 5f;
    private Rigidbody rb;
    private Vector3 inputDirection;

    private void Awake() => rb = GetComponent<Rigidbody>();

    public void SetMovementInput(Vector3 direction) => inputDirection = direction;

    private void FixedUpdate()
    {
        if (inputDirection != Vector3.zero)
        {
            Vector3 move = transform.TransformDirection(inputDirection) * speed;
            rb.MovePosition(rb.position + move * Time.fixedDeltaTime);
        }
    }
}