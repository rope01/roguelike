using UnityEngine;

/// <summary>
/// Runtime polish layer for the procedural worker avatar.
/// Keeps only one character visual active, moves the first-person body away
/// from the camera, and seats the overalls/PPE directly on the body surface.
/// </summary>
[DefaultExecutionOrder(2000)]
public sealed class AvatarVisualFix : MonoBehaviour
{
    private const float ThirdPersonDistance = 2.35f;
    private static readonly Vector3 FirstPersonBodyOffset = new Vector3(0f, 0f, -0.22f);

    private Camera view;
    private Transform modelRoot;
    private Transform bodyRoot;
    private Transform headRoot;
    private bool fitted;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void InstallOnPlayers()
    {
        PlayerMover[] players = Object.FindObjectsByType<PlayerMover>(FindObjectsSortMode.None);
        foreach (PlayerMover player in players)
        {
            if (player != null && player.GetComponent<AvatarVisualFix>() == null)
                player.gameObject.AddComponent<AvatarVisualFix>();
        }
    }

    private void Awake()
    {
        view = GetComponentInChildren<Camera>(true);
    }

    private void LateUpdate()
    {
        // FirstPersonBody and StylizedWorkerAvatar currently coexist in the project.
        // Both build a full procedural character, which caused doubled/floating PPE
        // and a second torso to fill the first-person camera. Keep the newer avatar only.
        DisableChild("First-person arms and legs only");
        DisableChild("Rigged mover character");
        DisableChild("First-person worker body");
        DisableChild("Rounded procedural mover");

        if (!BindStylizedAvatar()) return;

        if (!fitted)
        {
            FitOverallsToBody();
            FitHeadAccessories();
            fitted = true;
        }

        if (view == null) view = GetComponentInChildren<Camera>(true);
        if (view == null) return;

        bool thirdPerson = Vector3.Distance(view.transform.position, transform.position) > ThirdPersonDistance;

        // The camera sits at z ~= 0.08 while the old chest reached through it.
        // Pull only the first-person torso backwards so looking down shows the body
        // instead of putting the camera inside the overalls. Limb world targets are
        // still driven by StylizedWorkerAvatar, so hands/feet keep their gameplay pose.
        modelRoot.localPosition = thirdPerson ? Vector3.zero : FirstPersonBodyOffset;
    }

    private bool BindStylizedAvatar()
    {
        if (modelRoot == null)
            modelRoot = transform.Find("Reference stylized mover avatar");
        if (modelRoot == null) return false;

        if (bodyRoot == null)
            bodyRoot = modelRoot.Find("Body");
        if (headRoot == null)
            headRoot = modelRoot.Find("Head and accessories");

        return bodyRoot != null && headRoot != null;
    }

    private void DisableChild(string childName)
    {
        Transform child = transform.Find(childName);
        if (child != null && child.gameObject.activeSelf)
            child.gameObject.SetActive(false);
    }

    private void FitOverallsToBody()
    {
        // Slightly slimmer depth prevents the torso from swallowing the camera,
        // while retaining the rounded/chunky silhouette from third person.
        Fit(bodyRoot, "Overalls pelvis", new Vector3(0f, 0.89f, 0f), new Vector3(0.70f, 0.56f, 0.50f));
        Fit(bodyRoot, "Rounded belly", new Vector3(0f, 1.17f, 0.005f), new Vector3(0.74f, 0.70f, 0.54f));
        Fit(bodyRoot, "Upper shirt", new Vector3(0f, 1.39f, 0.005f), new Vector3(0.65f, 0.47f, 0.47f));

        // Bib and suspenders are intentionally sunk a few millimetres into the shirt.
        // That removes the visible air gap and makes them read as clothing, not plates.
        Fit(bodyRoot, "Overalls bib", new Vector3(0f, 1.285f, 0.270f), new Vector3(0.46f, 0.43f, 0.060f));
        Fit(bodyRoot, "Bib pocket", new Vector3(0f, 1.285f, 0.307f), new Vector3(0.23f, 0.15f, 0.022f));
        Fit(bodyRoot, "Left suspender", new Vector3(-0.205f, 1.475f, 0.215f), new Vector3(0.068f, 0.38f, 0.045f), new Vector3(-7f, 0f, -8f));
        Fit(bodyRoot, "Right suspender", new Vector3(0.205f, 1.475f, 0.215f), new Vector3(0.068f, 0.38f, 0.045f), new Vector3(-7f, 0f, 8f));

        Fit(bodyRoot, "Front reflective band", new Vector3(0f, 1.425f, 0.252f), new Vector3(0.61f, 0.058f, 0.028f));
        Fit(bodyRoot, "Back reflective band", new Vector3(0f, 1.425f, -0.242f), new Vector3(0.61f, 0.058f, 0.028f));
        Fit(bodyRoot, "Left shoulder reflective", new Vector3(-0.278f, 1.515f, 0.075f), new Vector3(0.050f, 0.29f, 0.055f), new Vector3(10f, 0f, -14f));
        Fit(bodyRoot, "Right shoulder reflective", new Vector3(0.278f, 1.515f, 0.075f), new Vector3(0.050f, 0.29f, 0.055f), new Vector3(10f, 0f, 14f));
    }

    private void FitHeadAccessories()
    {
        // The head sphere is only ~0.43 units deep. Previous face pieces were at
        // z ~= 0.40, leaving a large visible gap in front of the face.
        Fit(headRoot, "Nose", new Vector3(0f, 1.665f, 0.235f), new Vector3(0.16f, 0.14f, 0.15f));
        Fit(headRoot, "Left moustache", new Vector3(-0.073f, 1.595f, 0.252f), new Vector3(0.16f, 0.070f, 0.050f), new Vector3(0f, 0f, -17f));
        Fit(headRoot, "Right moustache", new Vector3(0.073f, 1.595f, 0.252f), new Vector3(0.16f, 0.070f, 0.050f), new Vector3(0f, 0f, 17f));

        Fit(headRoot, "Left safety lens", new Vector3(-0.120f, 1.735f, 0.218f), new Vector3(0.195f, 0.115f, 0.020f));
        Fit(headRoot, "Right safety lens", new Vector3(0.120f, 1.735f, 0.218f), new Vector3(0.195f, 0.115f, 0.020f));
        Fit(headRoot, "Goggle bridge", new Vector3(0f, 1.735f, 0.232f), new Vector3(0.060f, 0.025f, 0.022f));

        Fit(headRoot, "Left safety lens top frame", new Vector3(-0.120f, 1.800f, 0.232f), new Vector3(0.215f, 0.018f, 0.020f));
        Fit(headRoot, "Left safety lens bottom frame", new Vector3(-0.120f, 1.670f, 0.232f), new Vector3(0.215f, 0.018f, 0.020f));
        Fit(headRoot, "Left safety lens inner frame", new Vector3(-0.018f, 1.735f, 0.232f), new Vector3(0.018f, 0.128f, 0.020f));
        Fit(headRoot, "Left safety lens outer frame", new Vector3(-0.222f, 1.735f, 0.225f), new Vector3(0.018f, 0.128f, 0.020f));

        Fit(headRoot, "Right safety lens top frame", new Vector3(0.120f, 1.800f, 0.232f), new Vector3(0.215f, 0.018f, 0.020f));
        Fit(headRoot, "Right safety lens bottom frame", new Vector3(0.120f, 1.670f, 0.232f), new Vector3(0.215f, 0.018f, 0.020f));
        Fit(headRoot, "Right safety lens inner frame", new Vector3(0.018f, 1.735f, 0.232f), new Vector3(0.018f, 0.128f, 0.020f));
        Fit(headRoot, "Right safety lens outer frame", new Vector3(0.222f, 1.735f, 0.225f), new Vector3(0.018f, 0.128f, 0.020f));

        // Short temples now pass through the side of the head instead of floating
        // as two long rectangular rails beside it.
        Fit(headRoot, "Left goggle strap", new Vector3(-0.222f, 1.735f, 0.075f), new Vector3(0.028f, 0.038f, 0.275f));
        Fit(headRoot, "Right goggle strap", new Vector3(0.222f, 1.735f, 0.075f), new Vector3(0.028f, 0.038f, 0.275f));

        // Safety glasses already have temples; the old rear cube looked like a
        // disconnected bar behind the skull, so remove that redundant piece.
        Transform backStrap = headRoot.Find("Back goggle strap");
        if (backStrap != null) backStrap.gameObject.SetActive(false);
    }

    private static void Fit(Transform root, string objectName, Vector3 localPosition, Vector3 localScale)
    {
        Fit(root, objectName, localPosition, localScale, Vector3.zero);
    }

    private static void Fit(Transform root, string objectName, Vector3 localPosition, Vector3 localScale, Vector3 localEuler)
    {
        if (root == null) return;
        Transform part = root.Find(objectName);
        if (part == null) return;

        part.localPosition = localPosition;
        part.localRotation = Quaternion.Euler(localEuler);
        part.localScale = localScale;
    }
}
