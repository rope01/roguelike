using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(CharacterController))]
public sealed class PlayerMover : MonoBehaviour
{
    private CharacterController controller;
    private Camera view;
    private CarryableItem heldItem;
    private VanController drivingVan;
    private float verticalVelocity;
    private float pitch;
    private float enteredVanAt;
    private Transform originalCameraParent;
    private Vector3 originalCameraPosition;
    private Renderer[] bodyRenderers;

    public Transform GrabPoint { get; private set; }

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        view = GetComponentInChildren<Camera>();
        bodyRenderers = GetComponentsInChildren<Renderer>();
        originalCameraParent = view.transform.parent;
        originalCameraPosition = view.transform.localPosition;

        GrabPoint = new GameObject("Physical grab target").transform;
        GrabPoint.SetParent(view.transform, false);
        GrabPoint.localPosition = new Vector3(0f, -0.28f, 2.15f);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Cursor.lockState = Cursor.lockState == CursorLockMode.Locked ? CursorLockMode.None : CursorLockMode.Locked;
            Cursor.visible = Cursor.lockState != CursorLockMode.Locked;
        }
        if (Input.GetKeyDown(KeyCode.R)) SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);

        if (drivingVan != null)
        {
            Look();
            if (Input.GetKeyDown(KeyCode.F) && Time.time - enteredVanAt > 0.4f) EndDriving();
            return;
        }

        Move();
        Look();
        if (Input.GetKeyDown(KeyCode.E))
        {
            if (heldItem == null) TryGrab(); else ReleaseHeld();
        }
        if (Input.GetKeyDown(KeyCode.F)) TryEnterVan();
    }

    private void Move()
    {
        float speed = 5.3f;
        if (heldItem != null) speed *= heldItem.HolderCount < heldItem.MinimumCarriers ? 0.42f : 0.72f;
        Vector3 input = transform.right * Input.GetAxisRaw("Horizontal") + transform.forward * Input.GetAxisRaw("Vertical");
        input = Vector3.ClampMagnitude(input, 1f);
        if (controller.isGrounded)
        {
            verticalVelocity = -1.5f;
            if (Input.GetKeyDown(KeyCode.Space) && heldItem == null) verticalVelocity = 5.2f;
        }
        else verticalVelocity += Physics.gravity.y * Time.deltaTime;
        Vector3 velocity = input * speed;
        velocity.y = verticalVelocity;
        controller.Move(velocity * Time.deltaTime);
    }

    private void Look()
    {
        if (Cursor.lockState != CursorLockMode.Locked) return;
        float x = Input.GetAxis("Mouse X") * 2.1f;
        float y = Input.GetAxis("Mouse Y") * 2.1f;
        pitch = Mathf.Clamp(pitch - y, -78f, 78f);
        view.transform.localRotation = Quaternion.Euler(pitch, 0f, 0f);
        if (drivingVan == null) transform.Rotate(Vector3.up * x);
    }

    private void TryGrab()
    {
        if (!Physics.Raycast(view.transform.position, view.transform.forward, out RaycastHit hit, 3.6f, ~0, QueryTriggerInteraction.Ignore)) return;
        CarryableItem item = hit.collider.GetComponentInParent<CarryableItem>();
        if (item != null && item.TryGrab(this)) heldItem = item;
    }

    private void ReleaseHeld()
    {
        if (heldItem == null) return;
        CarryableItem item = heldItem;
        heldItem = null;
        item.Release(this);
    }

    public void ForceRelease(CarryableItem item)
    {
        if (heldItem == item) heldItem = null;
    }

    private void TryEnterVan()
    {
        foreach (Collider nearby in Physics.OverlapSphere(transform.position, 4.5f))
        {
            VanController van = nearby.GetComponentInParent<VanController>();
            if (van == null || !van.TrySetDriver(this)) continue;
            BeginDriving(van);
            return;
        }
    }

    private void BeginDriving(VanController van)
    {
        ReleaseHeld();
        drivingVan = van;
        enteredVanAt = Time.time;
        controller.enabled = false;
        foreach (Renderer renderer in bodyRenderers) renderer.enabled = false;
        view.transform.SetParent(van.Seat, false);
        view.transform.localPosition = Vector3.zero;
        view.transform.localRotation = Quaternion.identity;
        pitch = 0f;
    }

    public void EndDriving()
    {
        if (drivingVan == null) return;
        VanController van = drivingVan;
        drivingVan = null;
        van.ClearDriver(this);
        transform.position = van.ExitPoint.position;
        transform.rotation = Quaternion.Euler(0f, van.transform.eulerAngles.y, 0f);
        view.transform.SetParent(originalCameraParent, false);
        view.transform.localPosition = originalCameraPosition;
        view.transform.localRotation = Quaternion.identity;
        foreach (Renderer renderer in bodyRenderers) renderer.enabled = true;
        controller.enabled = true;
    }
}
