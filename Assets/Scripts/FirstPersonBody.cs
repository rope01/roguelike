using UnityEngine;

public sealed class FirstPersonBody : MonoBehaviour
{
    private Transform firstPersonRoot;
    private Transform externalRoot;

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

    private Transform clayBelly;
    private Transform clayHead;
    private Transform clayLeftArm;
    private Transform clayRightArm;
    private Transform clayLeftLeg;
    private Transform clayRightLeg;

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
    private bool thirdPersonPreview;
    private bool modelVisible = true;

    private void Awake()
    {
        mover = GetComponent<PlayerMover>();
        BuildFirstPersonLimbs();
        BuildExternalClayMover();
        ApplyVisibility();
    }

    private void Start() => mover = GetComponent<PlayerMover>();

    private void BuildFirstPersonLimbs()
    {
        firstPersonRoot = new GameObject("First-person arms and legs only").transform;
        firstPersonRoot.SetParent(transform, false);

        Material orange = ClayMaterial(new Color(1.0f, 0.31f, 0.035f));
        Material blue = ClayMaterial(new Color(0.055f, 0.22f, 0.42f));
        Material gloves = ClayMaterial(new Color(0.095f, 0.075f, 0.055f));
        Material boots = ClayMaterial(new Color(0.045f, 0.040f, 0.038f));

        leftUpperArm = Part(firstPersonRoot, "Left soft sleeve", PrimitiveType.Capsule, orange);
        leftLowerArm = Part(firstPersonRoot, "Left rounded forearm", PrimitiveType.Capsule, gloves);
        rightUpperArm = Part(firstPersonRoot, "Right soft sleeve", PrimitiveType.Capsule, orange);
        rightLowerArm = Part(firstPersonRoot, "Right rounded forearm", PrimitiveType.Capsule, gloves);
        leftHand = Part(firstPersonRoot, "Left mitten", PrimitiveType.Sphere, gloves);
        rightHand = Part(firstPersonRoot, "Right mitten", PrimitiveType.Sphere, gloves);
        leftUpperLeg = Part(firstPersonRoot, "Left rounded thigh", PrimitiveType.Capsule, blue);
        leftLowerLeg = Part(firstPersonRoot, "Left rounded shin", PrimitiveType.Capsule, blue);
        rightUpperLeg = Part(firstPersonRoot, "Right rounded thigh", PrimitiveType.Capsule, blue);
        rightLowerLeg = Part(firstPersonRoot, "Right rounded shin", PrimitiveType.Capsule, blue);
        leftBoot = Part(firstPersonRoot, "Left round boot", PrimitiveType.Capsule, boots);
        rightBoot = Part(firstPersonRoot, "Right round boot", PrimitiveType.Capsule, boots);
    }

    private void BuildExternalClayMover()
    {
        externalRoot = new GameObject("Full clay mover with beer belly").transform;
        externalRoot.SetParent(transform, false);

        Material orange = ClayMaterial(new Color(1.0f, 0.31f, 0.035f));
        Material orangeLight = ClayMaterial(new Color(1.0f, 0.47f, 0.08f));
        Material blue = ClayMaterial(new Color(0.055f, 0.22f, 0.42f));
        Material skin = ClayMaterial(new Color(0.86f, 0.61f, 0.41f));
        Material gloves = ClayMaterial(new Color(0.095f, 0.075f, 0.055f));
        Material boots = ClayMaterial(new Color(0.045f, 0.040f, 0.038f));
        Material dark = ClayMaterial(new Color(0.035f, 0.032f, 0.03f));
        Material glass = ClayMaterial(new Color(0.16f, 0.28f, 0.32f));

        Transform hips = Part(externalRoot, "Soft overalls hips", PrimitiveType.Sphere, blue);
        SetLocal(hips, new Vector3(0f, 0.83f, 0f), Quaternion.identity, new Vector3(0.56f, 0.38f, 0.44f));

        clayBelly = Part(externalRoot, "Big soft beer belly", PrimitiveType.Sphere, orange);
        SetLocal(clayBelly, new Vector3(0f, 1.24f, 0.14f), Quaternion.Euler(7f, 0f, 0f), new Vector3(0.76f, 0.69f, 0.66f));

        Transform chest = Part(externalRoot, "Rounded upper body", PrimitiveType.Capsule, orangeLight);
        SetLocal(chest, new Vector3(0f, 1.48f, -0.02f), Quaternion.identity, new Vector3(0.49f, 0.34f, 0.45f));

        Transform bib = Part(externalRoot, "Blue overalls bib", PrimitiveType.Sphere, blue);
        SetLocal(bib, new Vector3(0f, 1.33f, 0.68f), Quaternion.Euler(8f, 0f, 0f), new Vector3(0.42f, 0.40f, 0.08f));
        Transform leftStrap = Part(externalRoot, "Left suspender", PrimitiveType.Capsule, blue);
        Transform rightStrap = Part(externalRoot, "Right suspender", PrimitiveType.Capsule, blue);
        SetLocal(leftStrap, new Vector3(-0.27f, 1.57f, 0.43f), Quaternion.Euler(-8f, 0f, -6f), new Vector3(0.06f, 0.25f, 0.055f));
        SetLocal(rightStrap, new Vector3(0.27f, 1.57f, 0.43f), Quaternion.Euler(-8f, 0f, 6f), new Vector3(0.06f, 0.25f, 0.055f));

        clayHead = Part(externalRoot, "Squishy clay head", PrimitiveType.Sphere, skin);
        SetLocal(clayHead, new Vector3(0f, 1.93f, 0.01f), Quaternion.identity, new Vector3(0.47f, 0.52f, 0.45f));
        Transform leftEar = Part(clayHead, "Left ear", PrimitiveType.Sphere, skin);
        Transform rightEar = Part(clayHead, "Right ear", PrimitiveType.Sphere, skin);
        SetLocal(leftEar, new Vector3(-0.94f, 0f, 0f), Quaternion.identity, new Vector3(0.18f, 0.24f, 0.15f));
        SetLocal(rightEar, new Vector3(0.94f, 0f, 0f), Quaternion.identity, new Vector3(0.18f, 0.24f, 0.15f));
        Transform nose = Part(clayHead, "Round nose", PrimitiveType.Sphere, skin);
        SetLocal(nose, new Vector3(0f, -0.04f, 0.93f), Quaternion.identity, new Vector3(0.20f, 0.18f, 0.20f));
        Transform leftLens = Part(clayHead, "Left glasses lens", PrimitiveType.Sphere, glass);
        Transform rightLens = Part(clayHead, "Right glasses lens", PrimitiveType.Sphere, glass);
        SetLocal(leftLens, new Vector3(-0.34f, 0.18f, 0.91f), Quaternion.identity, new Vector3(0.28f, 0.22f, 0.07f));
        SetLocal(rightLens, new Vector3(0.34f, 0.18f, 0.91f), Quaternion.identity, new Vector3(0.28f, 0.22f, 0.07f));
        Transform bridge = Part(clayHead, "Glasses bridge", PrimitiveType.Capsule, dark);
        SetLocal(bridge, new Vector3(0f, 0.18f, 0.94f), Quaternion.Euler(0f, 0f, 90f), new Vector3(0.045f, 0.13f, 0.045f));
        Transform mouth = Part(clayHead, "Small tired mouth", PrimitiveType.Capsule, dark);
        SetLocal(mouth, new Vector3(0f, -0.38f, 0.91f), Quaternion.Euler(0f, 0f, 90f), new Vector3(0.035f, 0.13f, 0.035f));

        clayLeftArm = LimbChain(externalRoot, "Left clay arm", new Vector3(-0.55f, 1.52f, 0f), orangeLight, gloves, gloves, -1f);
        clayRightArm = LimbChain(externalRoot, "Right clay arm", new Vector3(0.55f, 1.52f, 0f), orangeLight, gloves, gloves, 1f);
        clayLeftLeg = LegChain(externalRoot, "Left clay leg", new Vector3(-0.25f, 0.82f, 0f), blue, boots);
        clayRightLeg = LegChain(externalRoot, "Right clay leg", new Vector3(0.25f, 0.82f, 0f), blue, boots);
    }

    private Transform LimbChain(Transform parent, string name, Vector3 position, Material upperMaterial, Material lowerMaterial, Material handMaterial, float side)
    {
        Transform pivot = new GameObject(name).transform;
        pivot.SetParent(parent, false);
        pivot.localPosition = position;
        Transform upper = Part(pivot, "Rounded upper arm", PrimitiveType.Capsule, upperMaterial);
        Transform lower = Part(pivot, "Rounded forearm", PrimitiveType.Capsule, lowerMaterial);
        Transform hand = Part(pivot, "Soft mitten", PrimitiveType.Sphere, handMaterial);
        SetLocal(upper, new Vector3(side * 0.03f, -0.27f, 0f), Quaternion.Euler(0f, 0f, side * 4f), new Vector3(0.16f, 0.31f, 0.16f));
        SetLocal(lower, new Vector3(side * 0.05f, -0.75f, 0f), Quaternion.Euler(0f, 0f, side * 4f), new Vector3(0.14f, 0.25f, 0.14f));
        SetLocal(hand, new Vector3(side * 0.07f, -1.08f, 0f), Quaternion.identity, new Vector3(0.21f, 0.19f, 0.18f));
        return pivot;
    }

    private Transform LegChain(Transform parent, string name, Vector3 position, Material pants, Material bootMaterial)
    {
        Transform pivot = new GameObject(name).transform;
        pivot.SetParent(parent, false);
        pivot.localPosition = position;
        Transform thigh = Part(pivot, "Soft thigh", PrimitiveType.Capsule, pants);
        Transform shin = Part(pivot, "Soft shin", PrimitiveType.Capsule, pants);
        Transform boot = Part(pivot, "Rounded boot", PrimitiveType.Capsule, bootMaterial);
        SetLocal(thigh, new Vector3(0f, -0.28f, 0f), Quaternion.identity, new Vector3(0.22f, 0.34f, 0.22f));
        SetLocal(shin, new Vector3(0f, -0.75f, 0f), Quaternion.identity, new Vector3(0.18f, 0.27f, 0.18f));
        SetLocal(boot, new Vector3(0f, -1.08f, 0.10f), Quaternion.Euler(90f, 0f, 0f), new Vector3(0.22f, 0.29f, 0.20f));
        return pivot;
    }

    private Transform Part(Transform parent, string objectName, PrimitiveType primitive, Material material)
    {
        GameObject part = GameObject.CreatePrimitive(primitive);
        part.name = objectName;
        part.transform.SetParent(parent, false);
        part.GetComponent<Renderer>().sharedMaterial = material;
        Collider collider = part.GetComponent<Collider>();
        if (collider != null) Destroy(collider);
        return part.transform;
    }

    private static Material ClayMaterial(Color color)
    {
        Shader shader = Shader.Find("Standard");
        if (shader == null) shader = Shader.Find("Universal Render Pipeline/Lit");
        Material material = new Material(shader);
        material.color = color;
        if (material.HasProperty("_BaseColor")) material.SetColor("_BaseColor", color);
        if (material.HasProperty("_Smoothness")) material.SetFloat("_Smoothness", 0.38f);
        if (material.HasProperty("_Glossiness")) material.SetFloat("_Glossiness", 0.38f);
        if (material.HasProperty("_Metallic")) material.SetFloat("_Metallic", 0f);
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

    public void SetThirdPersonPreview(bool enabled)
    {
        thirdPersonPreview = enabled;
        ApplyVisibility();
    }

    public void SetVisible(bool visible)
    {
        modelVisible = visible;
        ApplyVisibility();
    }

    private void ApplyVisibility()
    {
        if (firstPersonRoot != null) firstPersonRoot.gameObject.SetActive(modelVisible && !thirdPersonPreview);
        if (externalRoot != null) externalRoot.gameObject.SetActive(modelVisible && thirdPersonPreview);
    }

    private void LateUpdate()
    {
        if (mover == null) mover = GetComponent<PlayerMover>();
        if (mover == null || !modelVisible) return;
        gait += Time.deltaTime * Mathf.Lerp(5f, sprinting ? 13f : 9f, movement);
        if (thirdPersonPreview) AnimateExternalModel();
        else AnimateFirstPersonLimbs();
    }

    private void AnimateExternalModel()
    {
        float step = grounded ? Mathf.Sin(gait) * movement : 0f;
        float wobble = Mathf.Sin(Time.time * 2.4f) * 0.012f + Mathf.Abs(step) * 0.035f;
        externalRoot.localPosition = new Vector3(0f, Mathf.Lerp(0f, -0.43f, crouch) + Mathf.Abs(step) * 0.025f, 0f);
        externalRoot.localRotation = Quaternion.Euler(stun * Mathf.Sin(Time.time * 9f) * 7f, 0f, -strafe * 3f + step * 1.5f);
        clayBelly.localScale = new Vector3(0.76f + wobble, 0.69f - wobble * 0.35f, 0.66f + wobble * 0.8f);
        clayHead.localRotation = Quaternion.Euler(step * -2f, strafe * 4f, step * -3f);

        float armSwing = carrying ? -42f : step * 24f;
        clayLeftArm.localRotation = Quaternion.Euler(armSwing, 0f, -4f);
        clayRightArm.localRotation = Quaternion.Euler(carrying ? -42f : -step * 24f, 0f, 4f);
        clayLeftLeg.localRotation = Quaternion.Euler(-step * 24f, 0f, 0f);
        clayRightLeg.localRotation = Quaternion.Euler(step * 24f, 0f, 0f);
    }

    private void AnimateFirstPersonLimbs()
    {
        float step = grounded ? Mathf.Sin(gait) * movement : 0f;
        float liftLeft = grounded ? Mathf.Max(0f, Mathf.Sin(gait)) * 0.14f * movement : 0.06f;
        float liftRight = grounded ? Mathf.Max(0f, -Mathf.Sin(gait)) * 0.14f * movement : 0.06f;
        float bodyHeight = Mathf.Lerp(0f, -0.48f, crouch);
        float flop = stun * Mathf.Sin(Time.time * 10f) * 0.22f;

        Vector3 leftHip = transform.TransformPoint(new Vector3(-0.18f, 0.78f + bodyHeight, 0.03f));
        Vector3 rightHip = transform.TransformPoint(new Vector3(0.18f, 0.78f + bodyHeight, 0.03f));
        Vector3 leftFoot = transform.TransformPoint(new Vector3(-0.18f, 0.08f, step * 0.23f + strafe * -0.05f));
        Vector3 rightFoot = transform.TransformPoint(new Vector3(0.18f, 0.08f, -step * 0.23f + strafe * 0.05f));
        leftFoot.y += liftLeft;
        rightFoot.y += liftRight;
        if (!grounded) { leftFoot += transform.forward * 0.12f; rightFoot -= transform.forward * 0.05f; }
        if (stun > 0.01f) { leftFoot += transform.right * flop; rightFoot -= transform.right * flop; }
        PoseLeg(leftHip, leftFoot, leftUpperLeg, leftLowerLeg, leftBoot, -1f);
        PoseLeg(rightHip, rightFoot, rightUpperLeg, rightLowerLeg, rightBoot, 1f);

        Vector3 leftShoulder = transform.TransformPoint(new Vector3(-0.34f, 1.39f + bodyHeight, 0.03f));
        Vector3 rightShoulder = transform.TransformPoint(new Vector3(0.34f, 1.39f + bodyHeight, 0.03f));
        PoseArm(leftShoulder, HandTarget(true, step, flop), leftUpperArm, leftLowerArm, leftHand, -1f, leftGripping);
        PoseArm(rightShoulder, HandTarget(false, -step, -flop), rightUpperArm, rightLowerArm, rightHand, 1f, rightGripping);
    }

    private Vector3 HandTarget(bool left, float step, float flop)
    {
        Transform grip = left ? mover.LeftGripPoint : mover.RightGripPoint;
        bool gripping = left ? leftGripping : rightGripping;
        if (gripping && grip != null) return grip.position;
        float side = left ? -1f : 1f;
        Vector3 local = new Vector3(side * 0.36f, 0.94f - crouch * 0.34f, 0.08f - step * 0.18f);
        if (carrying) local += new Vector3(0f, 0.10f, 0.30f);
        return transform.TransformPoint(local) + transform.right * flop;
    }

    private void PoseArm(Vector3 shoulder, Vector3 handPosition, Transform upper, Transform lower, Transform hand, float side, bool gripping)
    {
        float reach = Vector3.Distance(shoulder, handPosition);
        if (reach > 1.15f) handPosition = shoulder + (handPosition - shoulder).normalized * 1.15f;
        Vector3 elbow = Vector3.Lerp(shoulder, handPosition, 0.48f) + transform.right * side * 0.22f + Vector3.down * 0.18f;
        PoseLimb(upper, shoulder, elbow, 0.145f);
        PoseLimb(lower, elbow, handPosition, 0.13f);
        PoseBlock(hand, handPosition, transform.rotation * Quaternion.Euler(gripping ? 70f : 10f, 0f, side * 12f), new Vector3(0.19f, 0.16f, 0.21f));
    }

    private void PoseLeg(Vector3 hip, Vector3 foot, Transform upper, Transform lower, Transform boot, float side)
    {
        Vector3 knee = Vector3.Lerp(hip, foot, 0.48f) + transform.forward * 0.13f + transform.right * side * 0.025f;
        PoseLimb(upper, hip, knee, 0.18f);
        PoseLimb(lower, knee, foot + Vector3.up * 0.08f, 0.155f);
        PoseBlock(boot, foot + transform.forward * 0.08f, transform.rotation * Quaternion.Euler(90f, 0f, 0f), new Vector3(0.19f, 0.28f, 0.18f));
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

    private static void SetLocal(Transform part, Vector3 position, Quaternion rotation, Vector3 scale)
    {
        part.localPosition = position;
        part.localRotation = rotation;
        part.localScale = scale;
    }
}
