using UnityEngine;

public class BallController : MonoBehaviour
{
    Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            Vector3 dir =
                (transform.position - collision.transform.position).normalized;

            rb.AddForce(dir * 2f, ForceMode.Impulse);
            rb.AddTorque(Vector3.Cross(Vector3.up, dir) * 10f, ForceMode.Impulse);
        }
    }
}