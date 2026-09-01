using UnityEngine;

public sealed class JobHUD : MonoBehaviour
{
    private GUIStyle titleStyle;
    private GUIStyle helpStyle;
    private GUIStyle completeStyle;

    private void Awake()
    {
        titleStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 21,
            fontStyle = FontStyle.Bold,
            normal = { textColor = Color.white },
            wordWrap = true
        };
        helpStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 16,
            normal = { textColor = new Color(1f, 1f, 1f, 0.82f) },
            wordWrap = true
        };
        completeStyle = new GUIStyle(titleStyle)
        {
            fontSize = 34,
            alignment = TextAnchor.MiddleCenter
        };
    }

    private void OnGUI()
    {
        JobManager job = JobManager.Instance;
        if (job == null) return;

        if (job.Complete)
        {
            GUI.Label(new Rect(0f, 0f, Screen.width, Screen.height),
                "JOB COMPLETE\nFinal pay: $" + job.FinalPay + "\nPress R to restart", completeStyle);
            return;
        }

        string status = "CHEAP APARTMENT — FIRST JOB\n" +
                        "Delivered: " + job.Delivered + "/" + job.Total +
                        "    Earned: $" + job.Earned + "    Damage: -$" + job.Fines +
                        "    Current pay: $" + job.FinalPay + "\n" + job.LastEvent;
        GUI.Label(new Rect(24f, 22f, 850f, 140f), status, titleStyle);
        PlayerMover player = PlayerMover.Local;
        string physicalState = player == null ? "" :
            "   Stamina: " + Mathf.RoundToInt(player.Stamina * 100f) + "%   Hand reach: " + player.GripReach.ToString("0.0") + "m";
        GUI.Label(new Rect(24f, Screen.height - 92f, 1000f, 74f),
            "LMB left hand   RMB right hand   Mouse wheel reach   E release both" + physicalState +
            "\nWASD move   Shift sprint   Ctrl crouch   Space jump   F drive   V inspect character   R restart\nSOFA + FRIDGE require TWO DIFFERENT MOVERS", helpStyle);
        GUI.Label(new Rect(Screen.width * 0.5f - 8f, Screen.height * 0.5f - 14f, 20f, 28f), "+", titleStyle);
    }
}
