using UnityEngine;
using UnityEngine.UI;

public sealed class JobHUD : MonoBehaviour
{
    private Text status;

    private void Start()
    {
        Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        GameObject root = new GameObject("HUD");
        Canvas canvas = root.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        root.AddComponent<CanvasScaler>();
        root.AddComponent<GraphicRaycaster>();

        status = MakeText(canvas.transform, font, 22, TextAnchor.UpperLeft);
        status.rectTransform.anchorMin = status.rectTransform.anchorMax = new Vector2(0f, 1f);
        status.rectTransform.pivot = new Vector2(0f, 1f);
        status.rectTransform.anchoredPosition = new Vector2(24f, -22f);
        status.rectTransform.sizeDelta = new Vector2(760f, 160f);

        Text controls = MakeText(canvas.transform, font, 17, TextAnchor.LowerLeft);
        controls.text = "WASD move   E grab/drop   F drive/exit   Space jump   R restart\nSOFA + FRIDGE: two movers required";
        controls.color = new Color(1f, 1f, 1f, 0.78f);
        controls.rectTransform.anchorMin = controls.rectTransform.anchorMax = Vector2.zero;
        controls.rectTransform.pivot = Vector2.zero;
        controls.rectTransform.anchoredPosition = new Vector2(24f, 20f);
        controls.rectTransform.sizeDelta = new Vector2(800f, 70f);

        Text crosshair = MakeText(canvas.transform, font, 24, TextAnchor.MiddleCenter);
        crosshair.text = "+";
        crosshair.rectTransform.anchorMin = crosshair.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        crosshair.rectTransform.pivot = new Vector2(0.5f, 0.5f);
        crosshair.rectTransform.sizeDelta = new Vector2(30f, 30f);
    }

    private void Update()
    {
        JobManager job = JobManager.Instance;
        if (job == null || status == null) return;
        status.text = "CHEAP APARTMENT — FIRST JOB\nDelivered: " + job.Delivered + "/" + job.Total +
                      "    Earned: $" + job.Earned + "    Damage: -$" + job.Fines +
                      "    Current pay: $" + job.FinalPay + "\n" + job.LastEvent;
        if (!job.Complete) return;
        status.alignment = TextAnchor.MiddleCenter;
        status.rectTransform.anchorMin = status.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        status.rectTransform.pivot = new Vector2(0.5f, 0.5f);
        status.rectTransform.anchoredPosition = Vector2.zero;
        status.rectTransform.sizeDelta = new Vector2(900f, 220f);
        status.fontSize = 34;
        status.text = "JOB COMPLETE\nFinal pay: $" + job.FinalPay + "\nPress R to restart";
    }

    private static Text MakeText(Transform parent, Font font, int size, TextAnchor alignment)
    {
        GameObject obj = new GameObject("Text");
        obj.transform.SetParent(parent, false);
        Text text = obj.AddComponent<Text>();
        text.font = font;
        text.fontSize = size;
        text.alignment = alignment;
        text.color = Color.white;
        text.horizontalOverflow = HorizontalWrapMode.Wrap;
        text.verticalOverflow = VerticalWrapMode.Overflow;
        return text;
    }
}
