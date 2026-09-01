using UnityEngine;

public sealed class PlayerLoadResponse : MonoBehaviour
{
    [SerializeField] private float heavyMassThreshold = 55f;
    [SerializeField] private float veryHeavyMassThreshold = 105f;
    [SerializeField] private float maxCompensationDistance = 2.4f;

    private CarryableItem leftItem;
    private CarryableItem rightItem;

    public float EffectiveMass { get; private set; }
    public float LoadFactor { get; private set; }
    public float ExtremeLoadFactor { get; private set; }
    public float ForwardLoad { get; private set; }
    public float SideLoad { get; private set; }
    public float CarrierDeficit { get; private set; }

    public float MovementMultiplier => Mathf.Clamp(
        Mathf.Lerp(1f, 0.62f, LoadFactor) * Mathf.Lerp(1f, 0.62f, ExtremeLoadFactor) * Mathf.Lerp(1f, 0.55f, CarrierDeficit),
        0.24f,
        1f);

    public float AccelerationMultiplier => Mathf.Clamp(
        Mathf.Lerp(1f, 0.52f, LoadFactor) * Mathf.Lerp(1f, 0.65f, ExtremeLoadFactor),
        0.28f,
        1f);

    public float CameraSink => Mathf.Lerp(0f, 0.11f, LoadFactor) + Mathf.Lerp(0f, 0.055f, ExtremeLoadFactor);
    public float BodySink => Mathf.Lerp(0f, 0.16f, LoadFactor) + Mathf.Lerp(0f, 0.09f, ExtremeLoadFactor);
    public float KneeBend => Mathf.Lerp(0f, 0.16f, LoadFactor) + Mathf.Lerp(0f, 0.13f, ExtremeLoadFactor);
    public float ShoulderDrop => Mathf.Lerp(0f, 0.055f, LoadFactor) + Mathf.Lerp(0f, 0.045f, ExtremeLoadFactor);
    public float GripDrop => Mathf.Lerp(0f, 0.12f, LoadFactor) + Mathf.Lerp(0f, 0.10f, ExtremeLoadFactor);
    public float SwayAmount => Mathf.Lerp(0.004f, 0.035f, LoadFactor) + Mathf.Lerp(0f, 0.025f, ExtremeLoadFactor);
    public float ColliderCompression => Mathf.Lerp(0f, 0.055f, LoadFactor) + Mathf.Lerp(0f, 0.045f, ExtremeLoadFactor);

    public void SetHeldItems(CarryableItem left, CarryableItem right)
    {
        leftItem = left;
        rightItem = right;
        Recalculate();
    }

    private void Update() => Recalculate();

    private void Recalculate()
    {
        EffectiveMass = 0f;
        CarrierDeficit = 0f;
        Vector3 weightedCenter = Vector3.zero;
        float centerWeight = 0f;

        Accumulate(leftItem, ref EffectiveMass, ref weightedCenter, ref centerWeight, ref CarrierDeficit);
        if (rightItem != null && rightItem != leftItem)
            Accumulate(rightItem, ref EffectiveMass, ref weightedCenter, ref centerWeight, ref CarrierDeficit);

        LoadFactor = Mathf.Clamp01(EffectiveMass / Mathf.Max(1f, heavyMassThreshold));
        ExtremeLoadFactor = Mathf.InverseLerp(heavyMassThreshold, Mathf.Max(heavyMassThreshold + 1f, veryHeavyMassThreshold), EffectiveMass);

        if (centerWeight <= 0.001f)
        {
            ForwardLoad = 0f;
            SideLoad = 0f;
            return;
        }

        Vector3 center = weightedCenter / centerWeight;
        Vector3 delta = center - transform.position;
        delta.y = 0f;
        Vector3 local = transform.InverseTransformDirection(delta);
        float divisor = Mathf.Max(0.5f, maxCompensationDistance);
        ForwardLoad = Mathf.Clamp(local.z / divisor, -1f, 1f);
        SideLoad = Mathf.Clamp(local.x / divisor, -1f, 1f);
    }

    private static void Accumulate(
        CarryableItem item,
        ref float totalMass,
        ref Vector3 weightedCenter,
        ref float centerWeight,
        ref float carrierDeficit)
    {
        if (item == null) return;

        int holders = Mathf.Max(1, item.HolderCount);
        float sharedMass = item.Mass / holders;
        totalMass += sharedMass;
        weightedCenter += item.WorldCenterOfMass * sharedMass;
        centerWeight += sharedMass;

        if (holders < item.MinimumCarriers)
        {
            float deficit = 1f - (float)holders / Mathf.Max(1, item.MinimumCarriers);
            carrierDeficit = Mathf.Max(carrierDeficit, deficit);
        }
    }
}
