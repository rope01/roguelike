using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public sealed class CarryableItem : MonoBehaviour
{
    private sealed class HandHold
    {
        public Transform Anchor;
        public Vector3 LocalContact;
        public Vector3 LocalNormal;
    }

    private sealed class PlayerGrip
    {
        public HandHold Left;
        public HandHold Right;
        public int HandCount => (Left != null ? 1 : 0) + (Right != null ? 1 : 0);
    }

    private readonly Dictionary<PlayerMover, PlayerGrip> grips = new();
    private readonly List<PlayerMover> cleanup = new();
    private Rigidbody body;
    private Collider itemCollider;
    private float condition = 1f;
    private float lastDamageTime;
    private float fragility = 1f;
    private float coordinationStress;
    private bool delivered;

    public string DisplayName { get; private set; } = "Cargo";
    public int MinimumCarriers { get; private set; } = 1;
    public int BaseValue { get; private set; } = 100;
    public int HolderCount => grips.Count;
    public float Condition => condition;
    public float Mass => body != null ? body.mass : 0f;
    public Vector3 WorldCenterOfMass => body != null ? body.worldCenterOfMass : transform.position;

    public void Configure(string itemName, int minimumCarriers, int value, float itemFragility)
    {
        DisplayName = itemName;
        MinimumCarriers = Mathf.Clamp(minimumCarriers, 1, 4);
        BaseValue = Mathf.Max(1, value);
        fragility = Mathf.Max(0.1f, itemFragility);
    }

    private void Awake()
    {
        body = GetComponent<Rigidbody>();
        itemCollider = GetComponent<Collider>();
        body.maxAngularVelocity = 8f;
        JobManager.Instance?.Register(this);
    }

    private void Start() => JobManager.Instance?.Register(this);

    public bool TryGrabHand(PlayerMover player, bool left, Transform anchor)
    {
        Vector3 hint = anchor != null ? anchor.position : transform.position;
        return TryGrabHand(player, left, anchor, hint);
    }

    public bool TryGrabHand(PlayerMover player, bool left, Transform anchor, Vector3 hitPoint)
    {
        if (delivered || player == null || anchor == null || itemCollider == null) return false;
        if (!grips.TryGetValue(player, out PlayerGrip grip))
        {
            grip = new PlayerGrip();
            grips.Add(player, grip);
        }

        if (left && grip.Left != null) return false;
        if (!left && grip.Right != null) return false;

        HandHold hold = CreateSurfaceHold(player, left, anchor, hitPoint);
        if (left) grip.Left = hold;
        else grip.Right = hold;

        body.WakeUp();
        return true;
    }

    private HandHold CreateSurfaceHold(PlayerMover player, bool left, Transform anchor, Vector3 hitPoint)
    {
        Bounds bounds = itemCollider.bounds;
        Vector3 front = player.ViewTransform != null
            ? player.ViewTransform.position - bounds.center
            : player.transform.position - bounds.center;
        front.y = 0f;
        if (front.sqrMagnitude < 0.01f) front = -player.transform.forward;
        front.Normalize();

        float side = left ? -1f : 1f;
        float span = Mathf.Clamp(Mathf.Max(bounds.extents.x, bounds.extents.z) * 0.34f, 0.12f, 0.55f);
        float height = Mathf.Max(0.05f, bounds.size.y);
        float margin = Mathf.Min(0.14f, height * 0.20f);
        float desiredY = Mathf.Clamp(hitPoint.y, bounds.min.y + margin, bounds.max.y - margin);
        if (Mass > 45f) desiredY = Mathf.Lerp(desiredY, bounds.min.y + height * 0.42f, 0.28f);

        Vector3 candidate = bounds.center
                            + front * (bounds.extents.magnitude + 0.65f)
                            + player.transform.right * side * span;
        candidate.y = desiredY;

        Vector3 contact = itemCollider.ClosestPoint(candidate);
        Vector3 normal = candidate - contact;
        if (normal.sqrMagnitude < 0.0001f)
        {
            normal = contact - bounds.center;
            if (normal.sqrMagnitude < 0.0001f) normal = front;
        }
        normal.Normalize();

        return new HandHold
        {
            Anchor = anchor,
            LocalContact = transform.InverseTransformPoint(contact),
            LocalNormal = transform.InverseTransformDirection(normal).normalized
        };
    }

    public bool TryGetGripPose(PlayerMover player, bool left, out Vector3 contact, out Vector3 surfaceNormal)
    {
        contact = transform.position;
        surfaceNormal = -transform.forward;
        if (player == null || !grips.TryGetValue(player, out PlayerGrip grip)) return false;

        HandHold hold = left ? grip.Left : grip.Right;
        if (hold == null) return false;

        contact = transform.TransformPoint(hold.LocalContact);
        surfaceNormal = transform.TransformDirection(hold.LocalNormal).normalized;
        return true;
    }

    public void ReleaseHand(PlayerMover player, bool left)
    {
        if (!grips.TryGetValue(player, out PlayerGrip grip)) return;
        if (left) grip.Left = null;
        else grip.Right = null;
        if (grip.HandCount == 0) grips.Remove(player);
    }

    public void ReleaseAll()
    {
        cleanup.Clear();
        cleanup.AddRange(grips.Keys);
        foreach (PlayerMover player in cleanup)
        {
            if (player == null) continue;
            player.ForceRelease(this, true);
            player.ForceRelease(this, false);
        }
        grips.Clear();
    }

    private void FixedUpdate()
    {
        RemoveBrokenGrips();
        if (grips.Count == 0 || delivered) return;

        int handCount = 0;
        Vector3 averageTarget = Vector3.zero;
        foreach (PlayerGrip grip in grips.Values)
        {
            if (grip.Left?.Anchor != null) { averageTarget += grip.Left.Anchor.position; handCount++; }
            if (grip.Right?.Anchor != null) { averageTarget += grip.Right.Anchor.position; handCount++; }
        }
        if (handCount == 0) return;
        averageTarget /= handCount;

        bool enoughMovers = grips.Count >= MinimumCarriers;
        float widestGrip = 0f;
        foreach (PlayerGrip grip in grips.Values)
        {
            ApplyHandForce(grip.Left, averageTarget, enoughMovers, ref widestGrip);
            ApplyHandForce(grip.Right, averageTarget, enoughMovers, ref widestGrip);
        }

        if (!enoughMovers && body.linearVelocity.y > 0f)
            body.AddForce(Vector3.down * body.linearVelocity.y * Mathf.Max(12f, Mass * 0.2f), ForceMode.Force);

        float badCoordination = Mathf.InverseLerp(1.35f, 2.8f, widestGrip) + Mathf.InverseLerp(5f, 11f, body.angularVelocity.magnitude);
        coordinationStress = Mathf.MoveTowards(coordinationStress, badCoordination, Time.fixedDeltaTime * (badCoordination > coordinationStress ? 1.8f : 1.2f));
        if (coordinationStress > 0.92f && grips.Count >= 2)
        {
            ApplyDamage(0.035f, "movers lost coordination");
            ReleaseAll();
            coordinationStress = 0f;
        }
    }

    private void ApplyHandForce(HandHold hand, Vector3 averageTarget, bool enoughMovers, ref float widestGrip)
    {
        if (hand?.Anchor == null) return;

        Vector3 contact = transform.TransformPoint(hand.LocalContact);
        Vector3 target = hand.Anchor.position;
        widestGrip = Mathf.Max(widestGrip, Vector3.Distance(target, averageTarget));

        Vector3 error = target - contact;
        float spring = enoughMovers ? 1500f : 330f;
        float damping = enoughMovers ? 95f : 42f;
        float maxForce = enoughMovers ? 950f : 180f;
        Vector3 force = error * spring - body.GetPointVelocity(contact) * damping;
        force = Vector3.ClampMagnitude(force, maxForce);
        if (!enoughMovers) force.y = Mathf.Min(force.y, maxForce * 0.45f);

        body.AddForceAtPosition(force, contact, ForceMode.Force);
    }

    private void RemoveBrokenGrips()
    {
        cleanup.Clear();
        foreach (KeyValuePair<PlayerMover, PlayerGrip> pair in grips)
        {
            PlayerMover player = pair.Key;
            PlayerGrip grip = pair.Value;
            bool invalid = player == null || grip.HandCount == 0;
            if (!invalid) invalid = Vector3.Distance(player.transform.position, body.worldCenterOfMass) > 4.8f;
            if (invalid) cleanup.Add(player);
        }

        foreach (PlayerMover player in cleanup)
        {
            if (player != null)
            {
                player.ForceRelease(this, true);
                player.ForceRelease(this, false);
            }
            grips.Remove(player);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        float impact = collision.relativeVelocity.magnitude;
        if (impact > 3.2f) ApplyDamage((impact - 3.2f) * 0.018f * fragility, "impact");
    }

    private void ApplyDamage(float amount, string reason)
    {
        if (delivered || Time.time - lastDamageTime < 0.22f) return;
        lastDamageTime = Time.time;
        float oldCondition = condition;
        condition = Mathf.Clamp01(condition - amount);
        int fine = Mathf.CeilToInt((oldCondition - condition) * BaseValue);
        if (fine > 0) JobManager.Instance?.AddFine(fine, DisplayName + ": " + reason);
    }

    public void Deliver()
    {
        if (delivered) return;
        delivered = true;
        ReleaseAll();
        body.linearVelocity = Vector3.zero;
        body.angularVelocity = Vector3.zero;
        body.isKinematic = true;
        JobManager.Instance?.MarkDelivered(this);
        gameObject.SetActive(false);
    }
}
