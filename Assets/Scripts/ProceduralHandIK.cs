using UnityEngine;

[DefaultExecutionOrder(-40)]
public sealed class ProceduralHandIK : MonoBehaviour
{
    private sealed class HandState
    {
        public Transform Anchor;
        public CarryableItem Item;
        public Vector3 Velocity;
        public Vector3 VisualPosition;
        public Quaternion VisualRotation = Quaternion.identity;
        public bool Initialized;
    }

    private readonly HandState left = new();
    private readonly HandState right = new();

    private PlayerMover mover;
    private PlayerLoadResponse loadResponse;
    private Camera view;
    private float reach = 1.2f;
    private Vector3 lastCameraPosition;
    private Vector3 cameraVelocity;

    public Transform LeftAnchor => left.Anchor;
    public Transform RightAnchor => right.Anchor;

    private void Awake()
    {
        loadResponse = GetComponent<PlayerLoadResponse>();
        view = GetComponentInChildren<Camera>();
        EnsureAnchors();
        if (view != null) lastCameraPosition = view.transform.position;
    }

    private void Start()
    {
        mover = GetComponent<PlayerMover>();
        if (view == null) view = GetComponentInChildren<Camera>();
        if (loadResponse == null) loadResponse = GetComponent<PlayerLoadResponse>();
        EnsureAnchors();
        if (view != null) lastCameraPosition = view.transform.position;
    }

    public void Bind(PlayerMover player, Camera camera)
    {
        mover = player;
        view = camera;
        if (loadResponse == null) loadResponse = GetComponent<PlayerLoadResponse>();
        EnsureAnchors();
        if (view != null) lastCameraPosition = view.transform.position;
    }

    public void SetReach(float value) => reach = Mathf.Clamp(value, 0.75f, 1.65f);

    public void SetHeldItem(bool isLeft, CarryableItem item)
    {
        HandState hand = isLeft ? left : right;
        hand.Item = item;
    }

    public bool TryGetVisualPose(bool isLeft, out Vector3 position, out Quaternion rotation)
    {
        HandState hand = isLeft ? left : right;
        position = hand.VisualPosition;
        rotation = hand.VisualRotation;
        return hand.Initialized;
    }

    private void EnsureAnchors()
    {
        if (left.Anchor == null) left.Anchor = CreateAnchor("Left hand target");
        if (right.Anchor == null) right.Anchor = CreateAnchor("Right hand target");
    }

    private Transform CreateAnchor(string objectName)
    {
        GameObject target = new GameObject(objectName);
        target.transform.SetParent(transform, false);
        return target.transform;
    }

    private void LateUpdate()
    {
        if (view == null) view = GetComponentInChildren<Camera>();
        if (view == null) return;
        if (loadResponse == null) loadResponse = GetComponent<PlayerLoadResponse>();

        float dt = Mathf.Max(Time.deltaTime, 0.0001f);
        cameraVelocity = Vector3.Lerp(cameraVelocity, (view.transform.position - lastCameraPosition) / dt, 1f - Mathf.Exp(-dt * 12f));
        lastCameraPosition = view.transform.position;

        UpdateHand(left, true, dt);
        UpdateHand(right, false, dt);
    }

    private void UpdateHand(HandState hand, bool isLeft, float dt)
    {
        float side = isLeft ? -1f : 1f;
        float load = loadResponse != null ? loadResponse.LoadFactor : 0f;
        float extreme = loadResponse != null ? loadResponse.ExtremeLoadFactor : 0f;
        float gripDrop = loadResponse != null ? loadResponse.GripDrop : 0f;
        float sway = loadResponse != null ? loadResponse.SwayAmount : 0.004f;

        float usableReach = Mathf.Clamp(reach, 0.75f, 1.65f);
        Vector3 desired = view.transform.position
                          + view.transform.forward * usableReach
                          + view.transform.right * side * Mathf.Lerp(0.28f, 0.34f, load)
                          + view.transform.up * (-0.29f - gripDrop);

        Vector3 inertia = -Vector3.ClampMagnitude(cameraVelocity * Mathf.Lerp(0.006f, 0.015f, load), 0.14f);
        Vector3 proceduralSway = transform.right * Mathf.Sin(Time.time * Mathf.Lerp(5f, 8f, extreme) + (isLeft ? 0f : 1.7f)) * sway;
        proceduralSway += Vector3.up * Mathf.Sin(Time.time * 6.3f + (isLeft ? 0.8f : 2.1f)) * sway * 0.55f;
        desired += inertia + proceduralSway;

        float smoothTime = Mathf.Lerp(0.035f, 0.085f, load);
        hand.Anchor.position = Vector3.SmoothDamp(hand.Anchor.position, desired, ref hand.Velocity, smoothTime, 18f, dt);
        Quaternion anchorRotation = view.transform.rotation * Quaternion.Euler(8f + load * 7f, 0f, side * 10f);
        hand.Anchor.rotation = Quaternion.Slerp(hand.Anchor.rotation, anchorRotation, 1f - Mathf.Exp(-dt * 20f));

        Vector3 visualTarget = hand.Anchor.position;
        Quaternion visualRotation = hand.Anchor.rotation;

        if (hand.Item != null && mover != null && hand.Item.TryGetGripPose(mover, isLeft, out Vector3 contact, out Vector3 surfaceNormal))
        {
            visualTarget = contact + surfaceNormal * 0.018f;
            Vector3 up = Vector3.ProjectOnPlane(view.transform.up, surfaceNormal).normalized;
            if (up.sqrMagnitude < 0.01f) up = transform.up;
            visualRotation = Quaternion.LookRotation(-surfaceNormal, up) * Quaternion.Euler(0f, isLeft ? -8f : 8f, isLeft ? 6f : -6f);
        }

        if (!hand.Initialized)
        {
            hand.VisualPosition = visualTarget;
            hand.VisualRotation = visualRotation;
            hand.Initialized = true;
        }
        else
        {
            float positionTightness = hand.Item != null ? 36f : 18f;
            float rotationTightness = hand.Item != null ? 28f : 16f;
            hand.VisualPosition = Vector3.Lerp(hand.VisualPosition, visualTarget, 1f - Mathf.Exp(-dt * positionTightness));
            hand.VisualRotation = Quaternion.Slerp(hand.VisualRotation, visualRotation, 1f - Mathf.Exp(-dt * rotationTightness));
        }
    }
}
