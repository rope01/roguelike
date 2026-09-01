using UnityEngine;

public sealed class MoverSafetyGear : MonoBehaviour
{
    private Transform root;
    private Transform head;
    private Transform leftLens;
    private Transform rightLens;
    private Transform bridge;
    private Transform leftTemple;
    private Transform rightTemple;
    private MoverLoadResponse loadResponse;

    private void Awake()
    {
        loadResponse = GetComponent<MoverLoadResponse>();
        Build();
    }

    private void Build()
    {
        root = new GameObject("Head and safety glasses").transform;
        root.SetParent(transform, false);

        Material skin = CreateMaterial(new Color(0.72f, 0.49f, 0.32f), 0.16f);
        Material glasses = CreateMaterial(new Color(0.64f, 0.82f, 0.88f), 0.42f);
        Material frame = CreateMaterial(new Color(0.10f, 0.12f, 0.13f), 0.28f);

        head = Part("Rounded head", PrimitiveType.Sphere, skin);
        leftLens = Part("Left safety lens", PrimitiveType.Cube, glasses);
        rightLens = Part("Right safety lens", PrimitiveType.Cube, glasses);
        bridge = Part("Safety glasses bridge", PrimitiveType.Cube, frame);
        leftTemple = Part("Left glasses arm", PrimitiveType.Cube, frame);
        rightTemple = Part("Right glasses arm", PrimitiveType.Cube, frame);
    }

    private Transform Part(string objectName, PrimitiveType primitive, Material material)
    {
        GameObject part = GameObject.CreatePrimitive(primitive);
        part.name = objectName;
        part.transform.SetParent(root, false);
        part.GetComponent<Renderer>().sharedMaterial = material;
        Collider collider = part.GetComponent<Collider>();
        if (collider != null) Destroy(collider);
        return part.transform;
    }

    private static Material CreateMaterial(Color color, float smoothness)
    {
        Shader shader = Shader.Find("Standard");
        if (shader == null) shader = Shader.Find("Universal Render Pipeline/Lit");
        Material material = new Material(shader);
        material.color = color;
        material.SetFloat("_Smoothness", smoothness);
        material.SetFloat("_Glossiness", smoothness);
        return material;
    }

    private void LateUpdate()
    {
        if (root == null) return;
        if (loadResponse == null) loadResponse = GetComponent<MoverLoadResponse>();

        float sag = loadResponse != null ? loadResponse.BodySag * 0.45f : 0f;
        Vector3 compensation = loadResponse != null ? transform.TransformVector(loadResponse.CompensationLocal) * 0.72f : Vector3.zero;
        float sway = loadResponse != null ? Mathf.Sin(Time.time * 3f) * loadResponse.SwayAmount * 0.25f : 0f;

        // The local camera sits at roughly (0, 1.68, 0.12). The face is intentionally behind it,
        // so the head and glasses remain visible from outside without enclosing the first-person camera.
        Vector3 headPosition = transform.TransformPoint(new Vector3(0f, 1.59f - sag, -0.14f)) + compensation + transform.right * sway;
        Quaternion rotation = transform.rotation * Quaternion.Euler(loadResponse != null ? loadResponse.LoadFactor * 4f : 0f, 0f, -sway * 18f);

        head.position = headPosition;
        head.rotation = rotation;
        head.localScale = new Vector3(0.43f, 0.46f, 0.42f);

        Vector3 glassesCenter = headPosition + rotation * new Vector3(0f, 0.035f, 0.205f);
        Pose(leftLens, glassesCenter + rotation * new Vector3(-0.105f, 0f, 0f), rotation, new Vector3(0.17f, 0.085f, 0.018f));
        Pose(rightLens, glassesCenter + rotation * new Vector3(0.105f, 0f, 0f), rotation, new Vector3(0.17f, 0.085f, 0.018f));
        Pose(bridge, glassesCenter, rotation, new Vector3(0.055f, 0.018f, 0.022f));
        Pose(leftTemple, glassesCenter + rotation * new Vector3(-0.205f, 0f, -0.085f), rotation * Quaternion.Euler(0f, -10f, 0f), new Vector3(0.018f, 0.018f, 0.18f));
        Pose(rightTemple, glassesCenter + rotation * new Vector3(0.205f, 0f, -0.085f), rotation * Quaternion.Euler(0f, 10f, 0f), new Vector3(0.018f, 0.018f, 0.18f));
    }

    private static void Pose(Transform part, Vector3 position, Quaternion rotation, Vector3 scale)
    {
        part.position = position;
        part.rotation = rotation;
        part.localScale = scale;
    }
}
