using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public sealed class VanController : MonoBehaviour
{
    private Rigidbody body;
    private PlayerMover driver;
    private float throttle;
    private float steering;

    public Transform Seat { get; private set; }
    public Transform ExitPoint { get; private set; }

    private void Awake()
    {
        body = GetComponent<Rigidbody>();
        Seat = new GameObject("Driver seat camera").transform;
        Seat.SetParent(transform, false);
        Seat.localPosition = new Vector3(2.0f, 1.45f, 0f);
        Seat.localRotation = Quaternion.Euler(0f, 90f, 0f);
        ExitPoint = new GameObject("Driver exit").transform;
        ExitPoint.SetParent(transform, false);
        ExitPoint.localPosition = new Vector3(1.7f, 0.3f, -2.4f);
    }

    private void Update()
    {
        throttle = driver == null ? 0f : Input.GetAxisRaw("Vertical");
        steering = driver == null ? 0f : Input.GetAxisRaw("Horizontal");
    }

    private void FixedUpdate()
    {
        if (driver == null) return;
        body.AddForce(transform.right * (throttle * 5200f), ForceMode.Force);
        float moving = Mathf.Clamp01(body.linearVelocity.magnitude / 2f);
        float direction = Mathf.Sign(Mathf.Approximately(throttle, 0f) ? 1f : throttle);
        body.AddTorque(Vector3.up * (steering * 1850f * moving * direction), ForceMode.Force);
        body.linearVelocity = Vector3.ClampMagnitude(body.linearVelocity, 18f);
    }

    public bool TrySetDriver(PlayerMover player)
    {
        if (driver != null) return false;
        driver = player;
        return true;
    }

    public void ClearDriver(PlayerMover player)
    {
        if (driver == player) driver = null;
    }
}
