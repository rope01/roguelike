using UnityEngine;

public sealed class FirstPersonBody : MonoBehaviour
{
    private Transform visualRoot;
    private Transform torso;
    private Transform hips;
    private Transform leftUpperArm;
    private Transform leftLowerArm;
    private Transform rightUpperArm;
    private Transform rightLowerArm;
    private Transform leftHand;
    private Transform rightHand;
    private Transform leftUpperLeg;
    private Transform leftLowerLeg;
    private Transform rightUpperLeg;
    private Transform rightLowerLeg;
    private Transform leftBoot;
    private Transform rightBoot;
    private PlayerMover mover;
    private float movement;
    private float strafe;
    private float gait;
    private float crouch;
    private float stun;
    private bool grounded;
    private bool sprinting;
    private bool carrying;
    private bool leftGripping;
    private bool rightGripping;

    private void Awake()
    {
        mover = GetComponent<PlayerMover>();
        BuildRig();
    }

    private void Start() => mover = GetComponent<PlayerMover>();

    private void BuildRig()
    {
        visualRoot = new GameObject("Visible first-person body").transform;
        visualRoot.SetParent(transform, false);

        Material uniform = CreateMaterial(new Color(0.92f, 0.34f, 0.035f));
        Material darkUniform = CreateMaterial(new Color(0.14f, 0.12f, 0.11f));
        Material skin = CreateMaterial(new Color(0.78f, 0.54f, 0.36f));
        Material boots = CreateMaterial(new Color(0.055f, 0.050f, 0.045f));

        torso = Part("Torso", PrimitiveType.Cube, uniform);
        hips = Part("Hips", PrimitiveType.Cube, darkUniform);
        leftUpperArm = Part("Left upper arm", PrimitiveType.Capsule, uniform);
        leftLowerArm = Part("Left forearm", PrimitiveType.Capsule, skin);
        rightUpperArm = Part("Right upper arm", PrimitiveType.Capsule, uniform);
        rightLowerArm = Part("Right forearm", PrimitiveType.Capsule, skin);
        leftHand = Part("Left hand", PrimitiveType.Sphere, skin);
        rightHand = Part("Right hand", PrimitiveType.Sphere, skin);
        leftUpperLeg = Part("Left thigh", PrimitiveType.Capsule, darkUniform);
        leftLowerLeg = Part("Left shin", PrimitiveType.Capsule, darkUniform);
        rightUpperLeg = Part("Right thigh", PrimitiveType.Capsule, darkUniform);
        rightLowerLeg = Part("Right shin", PrimitiveType.Capsule, darkUniform);
        leftBoot = Part("Left boot", PrimitiveType.Cube, boots);
        rightBoot = Part("Right boot", PrimitiveType.Cube, boots);
    }

    private Transform Part(string objectName, PrimitiveType primitive, Material material)
    {
        GameObject part = GameObject.CreatePrimitive(primitive);
        part.name = objectName;
        part.transform.SetParent(visualRoot, false);
        part.GetComponent<Renderer>().sharedMaterial = material;
        Collider collider = part.GetComponent<Collider>();
        if (collider != null) Destroy(collider);
        return part.transform;
    }

    private static Material CreateMaterial(Color color)
    {
        Shader shader = Shader.Find("Standard");
        if (shader == null) shader = Shader.Find("Universal Render Pipeline/Lit");
        Material material = new Material(shader);
        material.color = color;
        material.SetFloat("_Smoothness", 0.18f);
        material.SetFloat("_Glossiness", 0.18f);
        return material;
    }

    public void SetPose(float moveAmount, float sideways, bool isGrounded, bool isCrouched, bool isSprinting, float stunTime, bool isCarrying)
    {
        movement = Mathf.MoveTowards(movement, moveAmount, Time.deltaTime * 7f);
        strafe = Mathf.MoveTowards(strafe, sideways, Time.deltaTime * 7f);
        grounded = isGrounded;
        sprinting = isSprinting;
        carrying = isCarrying;
        crouch = Mathf.MoveTowards(crouch, isCrouched ? 1f : 0f, Time.deltaTime * 6f);
        stun = Mathf.MoveTowards(stun, stunTime > 0f ? 1f : 0f, Time.deltaTime * 5f);
    }

    public void SetHandGrip(bool left, bool gripping)
    {
        if (left) leftGripping = gripping;
        else rightGripping = gripping;
    }

    public void SetVisible(bool visible)
    {
        if (visualRoot != null) visualRoot.gameObject.SetActive(visible);
    }

    private void LateUpdate()
    {
        if (mover == null) mover = GetComponent<PlayerMover>();
        if (mover == null || visualRoot == null || !visualRoot.gameObject.activeSelf) return;

        gait += Time.deltaTime * Mathf.Lerp(5f, sprinting ? 13f : 9f, movement);
        float step = grounded ? Mathf.Sin(gait) * movement : 0f;
        float liftLeft = grounded ? Mathf.Max(0f, Mathf.Sin(gait)) * 0.14f * movement : 0.06f;
        float liftRight = grounded ? Mathf.Max(0f, -Mathf.Sin(gait)) * 0.14f * movement : 0.06f;
        float bodyHeight = Mathf.Lerp(0f, -0.48f, crouch);
        float flop = stun * Mathf.Sin(Time.time * 10f) * 0.22f;

        Vector3 hipCenter = transform.TransformPoint(new Vector3(strafe * 0.025f, 0.83f + bodyHeight, 0f));
        Vector3 chestCenter = transform.TransformPoint(new Vector3(-strafe * 0.025f, 1.20f + bodyHeight, 0.01f));
        PoseBlock(hips, hipCenter, transform.rotation * Quaternion.Euler(0f, 0f, -strafe * 4f + flop * 15f), new Vector3(0.43f, 0.22f, 0.30f));
        PoseBlock(torso, chestCenter, transform.rotation * Quaternion.Euler(carrying ? -6f : step * 2f, 0f, strafe * -3f + flop * 10f), new Vector3(0.56f, 0.58f, 0.34f));

        Vector3 leftHip = transform.TransformPoint(new Vector3(-0.17f, 0.78f + bodyHeight, 0f));
        Vector3 rightHip = transform.TransformPoint(new Vector3(0.17f, 0.78f + bodyHeight, 0f));
        Vector3 leftFoot = transform.TransformPoint(new Vector3(-0.18f, 0.08f, step * 0.23f + strafe * -0.05f));
        Vector3 rightFoot = transform.TransformPoint(new Vector3(0.18f, 0.08f, -step * 0.23f + strafe * 0.05f));
        leftFoot.y += liftLeft;
        rightFoot.y += liftRight;
        if (!grounded) { leftFoot += transform.forward * 0.12f; rightFoot -= transform.forward * 0.05f; }
        if (stun > 0.01f) { leftFoot += transform.right * flop; rightFoot -= transform.right * flop; }
        PoseLeg(leftHip, leftFoot, leftUpperLeg, leftLowerLeg, leftBoot, -1f);
        PoseLeg(rightHip, rightFoot, rightUpperLeg, rightLowerLeg, rightBoot, 1f);

        Vector3 leftShoulder = transform.TransformPoint(new Vector3(-0.34f, 1.39f + bodyHeight, 0.02f));
        Vector3 rightShoulder = transform.TransformPoint(new Vector3(0.34f, 1.39f + bodyHeight, 0.02f));
        Vector3 leftTarget = HandTarget(true, leftShoulder, step, flop);
        Vector3 rightTarget = HandTarget(false, rightShoulder, -step, -flop);
        PoseArm(leftShoulder, leftTarget, leftUpperArm, leftLowerArm, leftHand, -1f, leftGripping);
        PoseArm(rightShoulder, rightTarget, rightUpperArm, rightLowerArm, rightHand, 1f, rightGripping);
    }

    private Vector3 HandTarget(bool left, Vector3 shoulder, float step, float flop)
    {
        Transform grip = left ? mover.LeftGripPoint : mover.RightGripPoint;
        bool gripping = left ? leftGripping : rightGripping;
        if (gripping && grip != null) return grip.position;
        float side = left ? -1f : 1f;
        Vector3 local = new Vector3(side * 0.35f, 0.91f - crouch * 0.34f, 0.04f - step * 0.18f);
        if (carrying) local += new Vector3(0f, 0.10f, 0.30f);
        return transform.TransformPoint(local) + transform.right * flop;
    }

    private void PoseArm(Vector3 shoulder, Vector3 handPosition, Transform upper, Transform lower, Transform hand, float side, bool gripping)
    {
        Vector3 elbowHint = transform.right * side * 0.22f + Vector3.down * 0.18f;
        Vector3 midpoint = Vector3.Lerp(shoulder, handPosition, 0.48f) + elbowHint;
        float reach = Vector3.Distance(shoulder, handPosition);
        if (reach > 1.15f) handPosition = shoulder + (handPosition - shoulder).normalized * 1.15f;
        PoseLimb(upper, shoulder, midpoint, 0.13f);
        PoseLimb(lower, midpoint, handPosition, 0.115f);
        PoseBlock(hand, handPosition, transform.rotation * Quaternion.Euler(gripping ? 70f : 10f, 0f, side * 12f), new Vector3(0.17f, 0.13f, 0.20f));
    }

    private void PoseLeg(Vector3 hip, Vector3 foot, Transform upper, Transform lower, Transform boot, float side)
    {
        Vector3 knee = Vector3.Lerp(hip, foot, 0.48f) + transform.forward * 0.13f + transform.right * side * 0.025f;
        PoseLimb(upper, hip, knee, 0.16f);
        PoseLimb(lower, knee, foot + Vector3.up * 0.08f, 0.135f);
        PoseBlock(boot, foot + transform.forward * 0.055f, transform.rotation, new Vector3(0.24f, 0.16f, 0.38f));
    }

    private static void PoseLimb(Transform limb, Vector3 start, Vector3 end, float thickness)
    {
        Vector3 direction = end - start;
        float length = Mathf.Max(0.02f, direction.magnitude);
        limb.position = (start + end) * 0.5f;
        limb.rotation = Quaternion.FromToRotation(Vector3.up, direction.normalized);
        limb.localScale = new Vector3(thickness, length * 0.5f, thickness);
    }

    private static void PoseBlock(Transform part, Vector3 position, Quaternion rotation, Vector3 scale)
    {
        part.position = position;
        part.rotation = rotation;
        part.localScale = scale;
    }
}
