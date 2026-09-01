using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public sealed class CarryableItem : MonoBehaviour
{
    private sealed class PlayerGrip
    {
        public Transform Left;
        public Transform Right;
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
        if (delivered || player == null || anchor == null) return false;
        if (!grips.TryGetValue(player, out PlayerGrip grip))
        {
            grip = new PlayerGrip();
            grips.Add(player, grip);
        }
        if (left)
        {
            if (grip.Left != null) return false;
            grip.Left = anchor;
        }
        else
        {
            if (grip.Right != null) return false;
            grip.Right = anchor;
        }
        body.WakeUp();
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
            if (grip.Left != null) { averageTarget += grip.Left.position; handCount++; }
            if (grip.Right != null) { averageTarget += grip.Right.position; handCount++; }
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
            body.AddForce(Vector3.down * body.linearVelocity.y * 18f, ForceMode.Acceleration);

        float badCoordination = Mathf.InverseLerp(1.35f, 2.8f, widestGrip) + Mathf.InverseLerp(5f, 11f, body.angularVelocity.magnitude);
        coordinationStress = Mathf.MoveTowards(coordinationStress, badCoordination, Time.fixedDeltaTime * (badCoordination > coordinationStress ? 1.8f : 1.2f));
        if (coordinationStress > 0.92f && grips.Count >= 2)
        {
            ApplyDamage(0.035f, "movers lost coordination");
            ReleaseAll();
            coordinationStress = 0f;
        }
    }

    private void ApplyHandForce(Transform hand, Vector3 averageTarget, bool enoughMovers, ref float widestGrip)
    {
        if (hand == null) return;
        widestGrip = Mathf.Max(widestGrip, Vector3.Distance(hand.position, averageTarget));
        Vector3 closest = itemCollider.ClosestPoint(hand.position);
        Vector3 error = hand.position - closest;
        float spring = enoughMovers ? 42f : 9f;
        float damping = enoughMovers ? 7.5f : 4f;
        Vector3 force = error * spring - body.GetPointVelocity(closest) * damping;
        force = Vector3.ClampMagnitude(force, enoughMovers ? 85f : 15f);
        if (!enoughMovers) force.y = Mathf.Min(force.y, 1.5f);
        body.AddForceAtPosition(force, closest, ForceMode.Acceleration);
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
