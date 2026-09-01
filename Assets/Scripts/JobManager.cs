using System.Collections.Generic;
using UnityEngine;

public sealed class JobManager : MonoBehaviour
{
    private readonly HashSet<CarryableItem> cargo = new();
    private readonly HashSet<CarryableItem> delivered = new();

    public static JobManager Instance { get; private set; }
    public int Fines { get; private set; }
    public int Earned { get; private set; }
    public int Total => cargo.Count;
    public int Delivered => delivered.Count;
    public int FinalPay => Mathf.Max(0, Earned - Fines);
    public bool Complete => Total > 0 && Delivered == Total;
    public string LastEvent { get; private set; } = "Load the apartment into the van";

    private void Awake() => Instance = this;

    public void Register(CarryableItem item) => cargo.Add(item);

    public void AddFine(int amount, string reason)
    {
        Fines += Mathf.Max(0, amount);
        LastEvent = "FINE -$" + amount + " — " + reason;
    }

    public void MarkDelivered(CarryableItem item)
    {
        if (!cargo.Contains(item) || !delivered.Add(item)) return;
        int payout = Mathf.RoundToInt(item.BaseValue * item.Condition);
        Earned += payout;
        LastEvent = item.DisplayName + " delivered: +$" + payout;
        if (Complete) LastEvent = "JOB COMPLETE — final pay $" + FinalPay;
    }
}
