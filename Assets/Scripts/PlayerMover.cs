using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(CharacterController))]
public sealed class PlayerMover : MonoBehaviour
{
    private CharacterController controller;
    private Camera view;
    private FirstPersonBody bodyRig;
    private PlayerLoadResponse loadResponse;
    private ProceduralHandIK handIK;
    private CarryableItem leftItem;
    private CarryableItem rightItem;
    private VanController drivingVan;
    private Transform originalCameraParent;
    private Vector3 originalCameraPosition;
    private Vector3 planarVelocity;
    private float verticalVelocity;
    private float pitch;
    private float stamina = 1f;
    private float gripReach = 1.20f;
    private float enteredVanAt;
    private float stunRemaining;
    private float cameraRoll;
    private float stepCycle;
    private bool wasGrounded;

    public static PlayerMover Local { get; private set; }
    public Transform GrabPoint => LeftGripPoint;
    public Transform LeftGripPoint => handIK != null ? handIK.LeftAnchor : null;
    public Transform RightGripPoint => handIK != null ? handIK.RightAnchor : null;
    public Transform ViewTransform => view != null ? view.transform : null;
    public float Stamina => stamina;
    public float GripReach => gripReach;
    public bool IsStunned => stunRemaining > 0f;
    public bool IsCarrying => leftItem != null || rightItem != null;
    public float LoadFactor => loadResponse != null ? loadResponse.LoadFactor : 0f;
    public float EffectiveCarryMass => loadResponse != null ? loadResponse.EffectiveMass : 0f;

    private void Awake()
    {
        Local = this;
        controller = GetComponent<CharacterController>();
        view = GetComponentInChildren<Camera>();
        bodyRig = GetComponent<FirstPersonBody>();
        loadResponse = GetComponent<PlayerLoadResponse>();
        if (loadResponse == null) loadResponse = gameObject.AddComponent<PlayerLoadResponse>();
        handIK = GetComponent<ProceduralHandIK>();
        if (handIK == null) handIK = gameObject.AddComponent<ProceduralHandIK>();

        originalCameraParent = view.transform.parent;
        originalCameraPosition = view.transform.localPosition;
        handIK.Bind(this, view);
        handIK.SetReach(gripReach);
        loadResponse.SetHeldItems(null, null);

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape)) ToggleCursor();
        if (Input.GetKeyDown(KeyCode.R)) SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);

        if (drivingVan != null)
        {
            Look();
            if (Input.GetKeyDown(KeyCode.F) && Time.time - enteredVanAt > 0.4f) EndDriving();
            return;
        }

        Look();
        UpdateGripReach();
        if (stunRemaining > 0f)
        {
            stunRemaining -= Time.deltaTime;
            MoveStunned();
            UpdateCamera(Vector3.zero, false, false);
            return;
        }

        bool crouched = Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.C);
        Vector3 moveInput = ReadMovement();
        bool sprinting = Input.GetKey(KeyCode.LeftShift) && !crouched && moveInput.sqrMagnitude > 0.1f && stamina > 0.04f && !IsCarrying;
        Move(moveInput, sprinting, crouched);
        HandleHands();
        if (Input.GetKeyDown(KeyCode.F)) TryEnterVan();
        UpdateCamera(moveInput, sprinting, crouched);
    }

    private void ToggleCursor()
    {
        Cursor.lockState = Cursor.lockState == CursorLockMode.Locked ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible = Cursor.lockState != CursorLockMode.Locked;
    }

    private Vector3 ReadMovement()
    {
        Vector3 input = transform.right * Input.GetAxisRaw("Horizontal") + transform.forward * Input.GetAxisRaw("Vertical");
        return Vector3.ClampMagnitude(input, 1f);
    }

    private void Move(Vector3 input, bool sprinting, bool crouched)
    {
        bool grounded = controller.isGrounded;
        if (grounded)
        {
            if (!wasGrounded && verticalVelocity < -10.5f)
                Stumble(Mathf.InverseLerp(-10.5f, -22f, verticalVelocity), -transform.forward);

            verticalVelocity = -1.7f;
            if (Input.GetKeyDown(KeyCode.Space) && !crouched && !IsCarrying) verticalVelocity = 5.2f;
        }
        else
        {
            verticalVelocity += Physics.gravity.y * Time.deltaTime;
        }
        wasGrounded = grounded;

        float speed = crouched ? 2.25f : sprinting ? 6.6f : 4.35f;
        if (IsCarrying && loadResponse != null) speed *= loadResponse.MovementMultiplier;

        if (sprinting) stamina = Mathf.Max(0f, stamina - Time.deltaTime * 0.24f);
        else stamina = Mathf.Min(1f, stamina + Time.deltaTime * (IsCarrying ? 0.10f : 0.18f));

        Vector3 targetPlanar = input * speed;
        if (IsCarrying && loadResponse != null)
        {
            float acceleration = 30f * loadResponse.AccelerationMultiplier;
            float braking = 37f * Mathf.Lerp(loadResponse.AccelerationMultiplier, 1f, 0.25f);
            float rate = input.sqrMagnitude > 0.01f ? acceleration : braking;
            planarVelocity = Vector3.MoveTowards(planarVelocity, targetPlanar, rate * Time.deltaTime);
        }
        else
        {
            planarVelocity = targetPlanar;
        }

        Vector3 velocity = planarVelocity;
        velocity.y = verticalVelocity;

        float compression = IsCarrying && loadResponse != null ? loadResponse.ColliderCompression : 0f;
        float targetHeight = crouched ? 1.25f : 1.85f - compression;
        controller.height = Mathf.MoveTowards(controller.height, targetHeight, Time.deltaTime * 5f);
        controller.radius = Mathf.MoveTowards(controller.radius, crouched ? 0.37f : 0.38f, Time.deltaTime * 4f);
        controller.center = new Vector3(0f, controller.height * 0.5f, 0f);
        controller.Move(velocity * Time.deltaTime);

        bodyRig?.SetPose(input.magnitude, Vector3.Dot(input, transform.right), grounded, crouched, sprinting, stunRemaining, IsCarrying);
    }

    private void MoveStunned()
    {
        verticalVelocity += Physics.gravity.y * Time.deltaTime;
        controller.Move((Vector3.down * 1.5f + transform.right * Mathf.Sin(Time.time * 9f) * 0.25f) * Time.deltaTime);
        bodyRig?.SetPose(0f, 0f, controller.isGrounded, true, false, stunRemaining, false);
    }

    private void Look()
    {
        if (Cursor.lockState != CursorLockMode.Locked) return;
        float x = Input.GetAxis("Mouse X") * 2.0f;
        float y = Input.GetAxis("Mouse Y") * 2.0f;
        pitch = Mathf.Clamp(pitch - y, -82f, 82f);
        if (drivingVan == null) transform.Rotate(Vector3.up * x);
        else view.transform.localRotation = Quaternion.Euler(pitch, 0f, 0f);
    }

    private void UpdateCamera(Vector3 input, bool sprinting, bool crouched)
    {
        float movement = new Vector3(input.x, 0f, input.z).magnitude;
        if (controller.isGrounded) stepCycle += movement * Time.deltaTime * (sprinting ? 13f : 9f);

        float load = loadResponse != null ? loadResponse.LoadFactor : 0f;
        float bobScale = Mathf.Lerp(1f, 0.55f, load);
        float bob = controller.isGrounded ? Mathf.Sin(stepCycle) * 0.025f * movement * bobScale : 0f;
        float sideBob = controller.isGrounded ? Mathf.Cos(stepCycle * 0.5f) * 0.018f * movement * bobScale : 0f;
        float loadSink = IsCarrying && loadResponse != null ? loadResponse.CameraSink * (crouched ? 0.55f : 1f) : 0f;
        float targetHeight = (crouched ? 1.10f : 1.67f) - loadSink;

        Vector3 target = new Vector3(sideBob, targetHeight + bob, originalCameraPosition.z);
        view.transform.localPosition = Vector3.Lerp(view.transform.localPosition, target, 1f - Mathf.Exp(-Time.deltaTime * 13f));

        float sideLoad = loadResponse != null ? loadResponse.SideLoad * load : 0f;
        float loadRoll = sideLoad * 2.4f;
        cameraRoll = Mathf.Lerp(
            cameraRoll,
            IsStunned ? Mathf.Sin(Time.time * 8f) * 18f : -Vector3.Dot(input, transform.right) * 1.8f + loadRoll,
            1f - Mathf.Exp(-Time.deltaTime * 7f));
        view.transform.localRotation = Quaternion.Euler(pitch, 0f, cameraRoll);
    }

    private void HandleHands()
    {
        if (Input.GetMouseButtonDown(0)) ToggleHand(true);
        if (Input.GetMouseButtonDown(1)) ToggleHand(false);
        if (Input.GetKeyDown(KeyCode.E)) ReleaseAllHands();
    }

    private void ToggleHand(bool left)
    {
        CarryableItem current = left ? leftItem : rightItem;
        if (current != null)
        {
            current.ReleaseHand(this, left);
            SetHeld(left, null);
            return;
        }

        if (!Physics.Raycast(view.transform.position, view.transform.forward, out RaycastHit hit, gripReach + 0.95f, ~0, QueryTriggerInteraction.Ignore)) return;
        CarryableItem item = hit.collider.GetComponentInParent<CarryableItem>();
        Transform anchor = left ? LeftGripPoint : RightGripPoint;
        if (item != null && anchor != null && item.TryGrabHand(this, left, anchor, hit.point)) SetHeld(left, item);
    }

    private void SetHeld(bool left, CarryableItem item)
    {
        if (left) leftItem = item;
        else rightItem = item;

        bodyRig?.SetHandGrip(left, item != null);
        handIK?.SetHeldItem(left, item);
        loadResponse?.SetHeldItems(leftItem, rightItem);
    }

    private void UpdateGripReach()
    {
        float wheel = Input.mouseScrollDelta.y;
        if (!Mathf.Approximately(wheel, 0f)) gripReach = Mathf.Clamp(gripReach + wheel * 0.14f, 0.78f, 1.65f);
        handIK?.SetReach(gripReach);
    }

    public void ForceRelease(CarryableItem item, bool left)
    {
        if (left && leftItem == item) SetHeld(true, null);
        if (!left && rightItem == item) SetHeld(false, null);
    }

    public void ReleaseAllHands()
    {
        CarryableItem left = leftItem;
        CarryableItem right = rightItem;
        if (left != null) left.ReleaseHand(this, true);
        if (right != null) right.ReleaseHand(this, false);
        SetHeld(true, null);
        SetHeld(false, null);
    }

    public void Stumble(float severity, Vector3 direction)
    {
        if (severity < 0.12f || drivingVan != null) return;
        stunRemaining = Mathf.Max(stunRemaining, Mathf.Lerp(0.35f, 1.6f, Mathf.Clamp01(severity)));
        ReleaseAllHands();
        cameraRoll = Mathf.Sign(Vector3.Dot(direction, transform.right)) * 22f;
    }

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        Rigidbody hitBody = hit.rigidbody;
        if (hitBody == null || hitBody.isKinematic) return;

        float incoming = hitBody.linearVelocity.magnitude;
        if (incoming > 6.5f && hitBody.mass > 20f)
            Stumble(Mathf.InverseLerp(6.5f, 15f, incoming), hitBody.linearVelocity.normalized);

        if (hit.moveDirection.y > -0.25f && hitBody.mass < 90f)
        {
            float push = Mathf.Lerp(55f, 12f, Mathf.InverseLerp(5f, 90f, hitBody.mass));
            hitBody.AddForceAtPosition(new Vector3(hit.moveDirection.x, 0f, hit.moveDirection.z) * push, hit.point, ForceMode.Force);
        }
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
        ReleaseAllHands();
        drivingVan = van;
        enteredVanAt = Time.time;
        controller.enabled = false;
        bodyRig?.SetVisible(false);
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
        bodyRig?.SetVisible(true);
        controller.enabled = true;
    }
}
