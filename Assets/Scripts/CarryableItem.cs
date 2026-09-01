using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public sealed class CarryableItem : MonoBehaviour
{
    private readonly List<PlayerMover> holders = new();
    private Rigidbody body;
    private float condition = 1f;
    private float lastDamageTime;
    private float fragility = 1f;
    private bool delivered;

    public string DisplayName { get; private set; } = "Cargo";
    public int MinimumCarriers { get; private set; } = 1;
    public int BaseValue { get; private set; } = 100;
    public int HolderCount => holders.Count;
    public float Condition => condition;

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
        JobManager.Instance?.Register(this);
    }

    private void Start() => JobManager.Instance?.Register(this);

    public bool TryGrab(PlayerMover player)
    {
        if (delivered || holders.Contains(player) || holders.Count >= 4) return false;
        holders.Add(player);
        body.WakeUp();
        return true;
    }

    public void Release(PlayerMover player) => holders.Remove(player);

    public void ReleaseAll()
    {
        PlayerMover[] copy = holders.ToArray();
        holders.Clear();
        foreach (PlayerMover holder in copy) holder.ForceRelease(this);
    }

    private void FixedUpdate()
    {
        holders.RemoveAll(holder => holder == null);
        if (holders.Count == 0 || delivered) return;

        for (int i = holders.Count - 1; i >= 0; i--)
        {
            if (Vector3.Distance(holders[i].GrabPoint.position, body.worldCenterOfMass) <= 4.4f) continue;
            PlayerMover lost = holders[i];
            holders.RemoveAt(i);
            lost.ForceRelease(this);
        }

        if (holders.Count >= 2 && Vector3.Distance(holders[0].transform.position, holders[1].transform.position) > 5.8f)
        {
            ReleaseAll();
            ApplyDamage(0.05f, "team lost grip");
            return;
        }
        if (holders.Count == 0) return;

        Vector3 target = Vector3.zero;
        foreach (PlayerMover holder in holders) target += holder.GrabPoint.position;
        target /= holders.Count;

        bool enoughMovers = holders.Count >= MinimumCarriers;
        if (!enoughMovers) target.y = Mathf.Min(body.worldCenterOfMass.y, target.y - 0.65f);

        float spring = enoughMovers ? 34f : 8f;
        float damping = enoughMovers ? 7f : 4f;
        Vector3 acceleration = (target - body.worldCenterOfMass) * spring - body.linearVelocity * damping;
        acceleration = Vector3.ClampMagnitude(acceleration, enoughMovers ? 75f : 16f);
        if (!enoughMovers) acceleration.y = Mathf.Min(0f, acceleration.y);
        body.AddForce(acceleration, ForceMode.Acceleration);

        if (enoughMovers && holders.Count >= 2)
        {
            Vector3 line = holders[1].transform.position - holders[0].transform.position;
            line.y = 0f;
            if (line.sqrMagnitude > 0.2f)
            {
                Quaternion desired = Quaternion.LookRotation(Vector3.Cross(Vector3.up, line.normalized), Vector3.up);
                body.MoveRotation(Quaternion.Slerp(body.rotation, desired, Time.fixedDeltaTime * 2.2f));
            }
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        float impact = collision.relativeVelocity.magnitude;
        if (impact > 3.4f) ApplyDamage((impact - 3.4f) * 0.018f * fragility, "impact");
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
