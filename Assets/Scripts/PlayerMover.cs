using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(CharacterController))]
public sealed class PlayerMover : MonoBehaviour
{
    private CharacterController controller;
    private Camera view;
    private FirstPersonBody bodyRig;
    private CarryableItem leftItem;
    private CarryableItem rightItem;
    private VanController drivingVan;
    private Transform leftGripPoint;
    private Transform rightGripPoint;
    private Transform originalCameraParent;
    private Vector3 originalCameraPosition;
    private float verticalVelocity;
    private float pitch;
    private float stamina = 1f;
    private float gripReach = 1.55f;
    private float enteredVanAt;
    private float stunRemaining;
    private float cameraRoll;
    private float stepCycle;
    private bool wasGrounded;
    private bool thirdPersonPreview;
    private bool crouched;
    private float previewYaw;
    private float previewElevation = 18f;

    public static PlayerMover Local { get; private set; }
    public Transform GrabPoint => leftGripPoint;
    public Transform LeftGripPoint => leftGripPoint;
    public Transform RightGripPoint => rightGripPoint;
    public float Stamina => stamina;
    public float GripReach => gripReach;
    public bool IsStunned => stunRemaining > 0f;
    public bool IsCarrying => leftItem != null || rightItem != null;

    private void Awake()
    {
        Local = this;
        controller = GetComponent<CharacterController>();
        view = GetComponentInChildren<Camera>();
        bodyRig = GetComponent<FirstPersonBody>();
        originalCameraParent = view.transform.parent;
        originalCameraPosition = view.transform.localPosition;
        leftGripPoint = CreateGripPoint("Left physical hand", -0.32f);
        rightGripPoint = CreateGripPoint("Right physical hand", 0.32f);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private Transform CreateGripPoint(string objectName, float horizontal)
    {
        Transform point = new GameObject(objectName).transform;
        point.SetParent(view.transform, false);
        point.localPosition = new Vector3(horizontal, -0.30f, gripReach);
        return point;
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

        if (Input.GetKeyDown(KeyCode.V)) TogglePerspectivePreview();
        Look();
        UpdateGripReach();
        if (stunRemaining > 0f)
        {
            stunRemaining -= Time.deltaTime;
            MoveStunned();
            UpdateCamera(Vector3.zero, false, false);
            return;
        }

        bool wantsCrouch = Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.C);
        crouched = wantsCrouch || (crouched && !CanStand());
        Vector3 moveInput = ReadMovement();
        bool sprinting = Input.GetKey(KeyCode.LeftShift) && !crouched && moveInput.sqrMagnitude > 0.1f && stamina > 0.04f && !IsCarrying;
        Move(moveInput, sprinting, crouched);
        if (!thirdPersonPreview) HandleHands();
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
            if (!wasGrounded && verticalVelocity < -10.5f) Stumble(Mathf.InverseLerp(-10.5f, -22f, verticalVelocity), -transform.forward);
            verticalVelocity = -1.7f;
            if (Input.GetKeyDown(KeyCode.Space) && !crouched && !IsCarrying) verticalVelocity = 5.2f;
        }
        else verticalVelocity += Physics.gravity.y * Time.deltaTime;
        wasGrounded = grounded;

        float speed = crouched ? 2.25f : sprinting ? 6.6f : 4.35f;
        if (IsCarrying)
        {
            int required = Mathf.Max(leftItem != null ? leftItem.MinimumCarriers : 1, rightItem != null ? rightItem.MinimumCarriers : 1);
            int carriers = Mathf.Max(leftItem != null ? leftItem.HolderCount : 0, rightItem != null ? rightItem.HolderCount : 0);
            speed *= carriers < required ? 0.28f : 0.67f;
        }
        if (sprinting) stamina = Mathf.Max(0f, stamina - Time.deltaTime * 0.24f);
        else stamina = Mathf.Min(1f, stamina + Time.deltaTime * (IsCarrying ? 0.10f : 0.18f));

        Vector3 velocity = input * speed;
        velocity.y = verticalVelocity;
        controller.height = Mathf.MoveTowards(controller.height, crouched ? 1.25f : 1.85f, Time.deltaTime * 5f);
        controller.center = new Vector3(0f, controller.height * 0.5f, 0f);
        controller.Move(velocity * Time.deltaTime);
        bodyRig?.SetPose(input.magnitude, Vector3.Dot(input, transform.right), grounded, crouched, sprinting, verticalVelocity, stunRemaining, IsCarrying);
    }

    private void MoveStunned()
    {
        verticalVelocity += Physics.gravity.y * Time.deltaTime;
        controller.Move((Vector3.down * 1.5f + transform.right * Mathf.Sin(Time.time * 9f) * 0.25f) * Time.deltaTime);
        bodyRig?.SetPose(0f, 0f, controller.isGrounded, true, false, verticalVelocity, stunRemaining, false);
    }

    private bool CanStand()
    {
        const float standingHeight = 1.85f;
        float radius = controller.radius * 0.94f;
        Vector3 feet = transform.position + Vector3.up * radius;
        Vector3 head = transform.position + Vector3.up * (standingHeight - radius);
        Collider[] overlaps = Physics.OverlapCapsule(feet, head, radius, ~0, QueryTriggerInteraction.Ignore);
        foreach (Collider overlap in overlaps)
        {
            if (overlap == controller || overlap.transform.root == transform.root) continue;
            return false;
        }
        return true;
    }

    private void Look()
    {
        if (Cursor.lockState != CursorLockMode.Locked) return;
        float x = Input.GetAxis("Mouse X") * 2.0f;
        float y = Input.GetAxis("Mouse Y") * 2.0f;
        if (thirdPersonPreview)
        {
            previewYaw += x;
            previewElevation = Mathf.Clamp(previewElevation - y, -8f, 42f);
            return;
        }
        pitch = Mathf.Clamp(pitch - y, -82f, 82f);
        if (drivingVan == null) transform.Rotate(Vector3.up * x);
        else view.transform.localRotation = Quaternion.Euler(pitch, 0f, 0f);
    }

    private void UpdateCamera(Vector3 input, bool sprinting, bool crouched)
    {
        float movement = new Vector3(input.x, 0f, input.z).magnitude;
        if (controller.isGrounded) stepCycle += movement * Time.deltaTime * (sprinting ? 13f : 9f);
        float bob = controller.isGrounded ? Mathf.Sin(stepCycle) * 0.025f * movement : 0f;
        float sideBob = controller.isGrounded ? Mathf.Cos(stepCycle * 0.5f) * 0.018f * movement : 0f;
        float targetHeight = crouched ? 1.08f : 1.62f;
        Vector3 target = new Vector3(sideBob, targetHeight + bob, originalCameraPosition.z);
        if (thirdPersonPreview)
        {
            Vector3 orbit = Quaternion.Euler(previewElevation, previewYaw, 0f) * new Vector3(0f, 0f, -4.4f);
            target = new Vector3(0f, 1.20f, 0f) + orbit;
        }
        view.transform.localPosition = Vector3.Lerp(view.transform.localPosition, target, Time.deltaTime * 12f);
        cameraRoll = Mathf.Lerp(cameraRoll, IsStunned ? Mathf.Sin(Time.time * 8f) * 18f : -Vector3.Dot(input, transform.right) * 1.8f, Time.deltaTime * 7f);
        if (thirdPersonPreview)
        {
            Quaternion lookAtCharacter = Quaternion.LookRotation(new Vector3(0f, 1.20f, 0f) - view.transform.localPosition, Vector3.up);
            view.transform.localRotation = Quaternion.Slerp(view.transform.localRotation, lookAtCharacter, Time.deltaTime * 14f);
        }
        else view.transform.localRotation = Quaternion.Euler(pitch, 0f, cameraRoll);
    }

    private void TogglePerspectivePreview()
    {
        if (IsCarrying) ReleaseAllHands();
        thirdPersonPreview = !thirdPersonPreview;
        if (thirdPersonPreview)
        {
            previewYaw = 0f;
            previewElevation = 18f;
        }
        bodyRig?.SetThirdPersonPreview(thirdPersonPreview);
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
        if (!Physics.Raycast(view.transform.position, view.transform.forward, out RaycastHit hit, gripReach + 1.2f, ~0, QueryTriggerInteraction.Ignore)) return;
        CarryableItem item = hit.collider.GetComponentInParent<CarryableItem>();
        Transform anchor = left ? leftGripPoint : rightGripPoint;
        if (item != null && item.TryGrabHand(this, left, anchor)) SetHeld(left, item);
    }

    private void SetHeld(bool left, CarryableItem item)
    {
        if (left) leftItem = item;
        else rightItem = item;
        bodyRig?.SetHandGrip(left, item != null);
    }

    private void UpdateGripReach()
    {
        float wheel = Input.mouseScrollDelta.y;
        if (!Mathf.Approximately(wheel, 0f)) gripReach = Mathf.Clamp(gripReach + wheel * 0.18f, 0.85f, 2.45f);
        leftGripPoint.localPosition = new Vector3(-0.32f, -0.30f, gripReach);
        rightGripPoint.localPosition = new Vector3(0.32f, -0.30f, gripReach);
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
        if (incoming > 6.5f && hitBody.mass > 20f) Stumble(Mathf.InverseLerp(6.5f, 15f, incoming), hitBody.linearVelocity.normalized);
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
        thirdPersonPreview = false;
        bodyRig?.SetThirdPersonPreview(false);
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
