using UnityEngine;

public sealed class MoverLoadResponse : MonoBehaviour
{
    [SerializeField] private float heavyMassThreshold = 85f;
    [SerializeField] private float extremeMassThreshold = 150f;

    private float targetLoad;
    private float targetExtreme;
    private Vector3 targetCompensationLocal;

    public float LoadFactor { get; private set; }
    public float ExtremeLoadFactor { get; private set; }
    public float MoveSpeedMultiplier => Mathf.Lerp(1f, 0.38f, LoadFactor) * Mathf.Lerp(1f, 0.72f, ExtremeLoadFactor);
    public float AccelerationMultiplier => Mathf.Lerp(1f, 0.34f, LoadFactor);
    public float HandDrop => Mathf.Lerp(0f, 0.34f, LoadFactor) + Mathf.Lerp(0f, 0.16f, ExtremeLoadFactor);
    public float KneeBend => Mathf.Lerp(0f, 0.28f, LoadFactor) + Mathf.Lerp(0f, 0.20f, ExtremeLoadFactor);
    public float BodySag => Mathf.Lerp(0f, 0.14f, LoadFactor) + Mathf.Lerp(0f, 0.11f, ExtremeLoadFactor);
    public float SwayAmount => Mathf.Lerp(0.012f, 0.075f, LoadFactor) + Mathf.Lerp(0f, 0.045f, ExtremeLoadFactor);
    public Vector3 CompensationLocal { get; private set; }

    public void SetLoad(float mass, Vector3 worldCenter, bool carrying)
    {
        if (!carrying || mass <= 0.01f)
        {
            targetLoad = 0f;
            targetExtreme = 0f;
            targetCompensationLocal = Vector3.zero;
            return;
        }

        targetLoad = Mathf.Clamp01(mass / Mathf.Max(1f, heavyMassThreshold));
        targetExtreme = Mathf.InverseLerp(heavyMassThreshold, Mathf.Max(heavyMassThreshold + 1f, extremeMassThreshold), mass);

        Vector3 localOffset = transform.InverseTransformPoint(worldCenter);
        Vector3 horizontal = new Vector3(localOffset.x, 0f, localOffset.z);
        if (horizontal.sqrMagnitude > 0.0001f)
        {
            Vector3 opposite = -horizontal.normalized;
            float leverage = Mathf.Clamp01(horizontal.magnitude / 1.8f);
            targetCompensationLocal = opposite * (0.08f + 0.15f * leverage) * targetLoad;
        }
        else targetCompensationLocal = Vector3.zero;
    }

    private void Update()
    {
        float rise = targetLoad > LoadFactor ? 3.2f : 4.8f;
        LoadFactor = Mathf.MoveTowards(LoadFactor, targetLoad, Time.deltaTime * rise);
        ExtremeLoadFactor = Mathf.MoveTowards(ExtremeLoadFactor, targetExtreme, Time.deltaTime * 3f);
        CompensationLocal = Vector3.Lerp(CompensationLocal, targetCompensationLocal, 1f - Mathf.Exp(-6f * Time.deltaTime));
    }
}
