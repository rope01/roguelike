using UnityEngine;

public sealed class FirstPersonBody : MonoBehaviour
{
    private const string CharacterResource = "Characters/Smooth_Male_Casual";

    private Transform firstPersonRoot;
    private Transform externalRoot;
    private Transform externalModel;
    private Transform externalTorso;

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
    private CharacterAnimationDriver animationDriver;
    private Vector3 torsoBaseScale;
    private float movement;
    private float strafe;
    private float gait;
    private float crouch;
    private float airborne;
    private float verticalSpeed;
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
        BuildExternalMover();
        ApplyVisibility();
    }

    private void Start() => mover = GetComponent<PlayerMover>();

    private void BuildFirstPersonLimbs()
    {
        firstPersonRoot = new GameObject("First-person arms and legs only").transform;
        firstPersonRoot.SetParent(transform, false);

        Material orange = MoverMaterial(new Color(0.96f, 0.25f, 0.025f), 0.42f);
        Material blue = MoverMaterial(new Color(0.035f, 0.16f, 0.34f), 0.34f);
        Material gloves = MoverMaterial(new Color(0.075f, 0.055f, 0.040f), 0.30f);
        Material boots = MoverMaterial(new Color(0.025f, 0.024f, 0.023f), 0.25f);

        leftUpperArm = Part(firstPersonRoot, "Left sleeve", PrimitiveType.Capsule, orange);
        leftLowerArm = Part(firstPersonRoot, "Left forearm", PrimitiveType.Capsule, gloves);
        rightUpperArm = Part(firstPersonRoot, "Right sleeve", PrimitiveType.Capsule, orange);
        rightLowerArm = Part(firstPersonRoot, "Right forearm", PrimitiveType.Capsule, gloves);
        leftHand = Part(firstPersonRoot, "Left glove", PrimitiveType.Sphere, gloves);
        rightHand = Part(firstPersonRoot, "Right glove", PrimitiveType.Sphere, gloves);
        leftUpperLeg = Part(firstPersonRoot, "Left thigh", PrimitiveType.Capsule, blue);
        leftLowerLeg = Part(firstPersonRoot, "Left shin", PrimitiveType.Capsule, blue);
        rightUpperLeg = Part(firstPersonRoot, "Right thigh", PrimitiveType.Capsule, blue);
        rightLowerLeg = Part(firstPersonRoot, "Right shin", PrimitiveType.Capsule, blue);
        leftBoot = Part(firstPersonRoot, "Left boot", PrimitiveType.Capsule, boots);
        rightBoot = Part(firstPersonRoot, "Right boot", PrimitiveType.Capsule, boots);
    }

    private void BuildExternalMover()
    {
        externalRoot = new GameObject("Rigged mover character").transform;
        externalRoot.SetParent(transform, false);

        GameObject source = Resources.Load<GameObject>(CharacterResource);
        if (source == null)
        {
            Debug.LogError("Rigged mover model was not imported. Expected Resources/" + CharacterResource + ".fbx");
            return;
        }

        externalModel = Instantiate(source, externalRoot).transform;
        externalModel.name = "Smooth rigged mover (CC0)";
        externalModel.localPosition = Vector3.zero;
        externalModel.localRotation = Quaternion.identity;
        externalModel.localScale = Vector3.one;

        NormalizeModelHeight(1.84f);
        RecolorWorkClothes();
        externalTorso = FindDeepChild(externalModel, "Torso");
        if (externalTorso != null)
        {
            torsoBaseScale = externalTorso.localScale;
            ApplyBeerBelly();
        }

        Animator animator = externalModel.GetComponentInChildren<Animator>();
        if (animator == null) animator = externalModel.gameObject.AddComponent<Animator>();
        animationDriver = externalRoot.gameObject.AddComponent<CharacterAnimationDriver>();
        animationDriver.Initialize(animator, Resources.LoadAll<AnimationClip>(CharacterResource));
    }

    private void NormalizeModelHeight(float targetHeight)
    {
        if (!TryGetBounds(externalModel, out Bounds initial) || initial.size.y < 0.01f) return;
        externalModel.localScale = Vector3.one * (targetHeight / initial.size.y);
        if (!TryGetBounds(externalModel, out Bounds scaled)) return;

        Vector3 playerOrigin = transform.position;
        externalModel.position += new Vector3(
            playerOrigin.x - scaled.center.x,
            playerOrigin.y - scaled.min.y,
            playerOrigin.z - scaled.center.z);
    }

    private void RecolorWorkClothes()
    {
        Material orange = MoverMaterial(new Color(0.96f, 0.25f, 0.025f), 0.42f);
        Material blue = MoverMaterial(new Color(0.035f, 0.16f, 0.34f), 0.34f);
        Material boots = MoverMaterial(new Color(0.025f, 0.024f, 0.023f), 0.25f);

        foreach (Renderer renderer in externalModel.GetComponentsInChildren<Renderer>(true))
        {
            string part = renderer.name.ToLowerInvariant();
            if (part.Contains("shirt")) ReplaceMaterials(renderer, orange);
            else if (part.Contains("pants")) ReplaceMaterials(renderer, blue);
            else if (part.Contains("shoe")) ReplaceMaterials(renderer, boots);
            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
            renderer.receiveShadows = true;
        }
    }

    private static void ReplaceMaterials(Renderer renderer, Material material)
    {
        Material[] materials = renderer.sharedMaterials;
        for (int i = 0; i < materials.Length; i++) materials[i] = material;
        renderer.sharedMaterials = materials;
    }

    private void ApplyBeerBelly()
    {
        if (externalTorso == null) return;
        externalTorso.localScale = new Vector3(
            torsoBaseScale.x * 1.18f,
            torsoBaseScale.y * 0.98f,
            torsoBaseScale.z * 1.24f);
    }

    public void SetPose(float moveAmount, float sideways, bool isGrounded, bool isCrouched, bool isSprinting, float ySpeed, float stunTime, bool isCarrying)
    {
        movement = Mathf.MoveTowards(movement, moveAmount, Time.deltaTime * 7f);
        strafe = Mathf.MoveTowards(strafe, sideways, Time.deltaTime * 7f);
        grounded = isGrounded;
        sprinting = isSprinting;
        carrying = isCarrying;
        verticalSpeed = ySpeed;
        crouch = Mathf.MoveTowards(crouch, isCrouched ? 1f : 0f, Time.deltaTime * 7.5f);
        airborne = Mathf.MoveTowards(airborne, isGrounded ? 0f : 1f, Time.deltaTime * 9f);
        stun = Mathf.MoveTowards(stun, stunTime > 0f ? 1f : 0f, Time.deltaTime * 5f);
        animationDriver?.SetState(movement, grounded, crouch > 0.45f, sprinting, verticalSpeed, carrying);
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
        externalRoot.localPosition = Vector3.zero;
        externalRoot.localRotation = Quaternion.Euler(
            stun * Mathf.Sin(Time.time * 9f) * 6f,
            0f,
            -strafe * 2.2f);
        ApplyBeerBelly();
    }

    private void AnimateFirstPersonLimbs()
    {
        float step = grounded ? Mathf.Sin(gait) * movement : 0f;
        float jumpBend = airborne * Mathf.Clamp01(0.75f - verticalSpeed * 0.04f);
        float liftLeft = grounded ? Mathf.Max(0f, Mathf.Sin(gait)) * 0.14f * movement : 0.16f + jumpBend * 0.18f;
        float liftRight = grounded ? Mathf.Max(0f, -Mathf.Sin(gait)) * 0.14f * movement : 0.12f + jumpBend * 0.22f;
        float bodyHeight = Mathf.Lerp(0f, -0.46f, crouch);
        float flop = stun * Mathf.Sin(Time.time * 10f) * 0.22f;

        Vector3 leftHip = transform.TransformPoint(new Vector3(-0.18f, 0.78f + bodyHeight, 0.03f));
        Vector3 rightHip = transform.TransformPoint(new Vector3(0.18f, 0.78f + bodyHeight, 0.03f));
        Vector3 leftFoot = transform.TransformPoint(new Vector3(-0.18f, 0.08f, step * 0.23f + strafe * -0.05f));
        Vector3 rightFoot = transform.TransformPoint(new Vector3(0.18f, 0.08f, -step * 0.23f + strafe * 0.05f));
        leftFoot.y += liftLeft;
        rightFoot.y += liftRight;
        if (!grounded)
        {
            leftFoot += transform.forward * (0.10f - jumpBend * 0.08f);
            rightFoot += transform.forward * (-0.02f + jumpBend * 0.10f);
        }
        if (stun > 0.01f)
        {
            leftFoot += transform.right * flop;
            rightFoot -= transform.right * flop;
        }
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
        if (airborne > 0.01f) local += new Vector3(0f, -0.03f, 0.10f);
        if (carrying) local += new Vector3(0f, 0.10f, 0.30f);
        return transform.TransformPoint(local) + transform.right * flop;
    }

    private void PoseArm(Vector3 shoulder, Vector3 handPosition, Transform upper, Transform lower, Transform hand, float side, bool gripping)
    {
        float reach = Vector3.Distance(shoulder, handPosition);
        if (reach > 1.15f) handPosition = shoulder + (handPosition - shoulder).normalized * 1.15f;
        Vector3 elbow = Vector3.Lerp(shoulder, handPosition, 0.48f) + transform.right * side * 0.22f + Vector3.down * 0.18f;
        PoseLimb(upper, shoulder, elbow, 0.16f);
        PoseLimb(lower, elbow, handPosition, 0.145f);
        PoseBlock(hand, handPosition, transform.rotation * Quaternion.Euler(gripping ? 70f : 10f, 0f, side * 12f), new Vector3(0.20f, 0.17f, 0.22f));
    }

    private void PoseLeg(Vector3 hip, Vector3 foot, Transform upper, Transform lower, Transform boot, float side)
    {
        float crouchKnee = crouch * 0.18f + airborne * 0.08f;
        Vector3 knee = Vector3.Lerp(hip, foot, 0.48f) + transform.forward * (0.13f + crouchKnee) + transform.right * side * 0.025f;
        PoseLimb(upper, hip, knee, 0.20f);
        PoseLimb(lower, knee, foot + Vector3.up * 0.08f, 0.175f);
        PoseBlock(boot, foot + transform.forward * 0.09f, transform.rotation * Quaternion.Euler(90f, 0f, 0f), new Vector3(0.21f, 0.31f, 0.19f));
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

    private static Material MoverMaterial(Color color, float smoothness)
    {
        Shader shader = Shader.Find("Standard");
        if (shader == null) shader = Shader.Find("Universal Render Pipeline/Lit");
        Material material = new Material(shader);
        material.color = color;
        if (material.HasProperty("_BaseColor")) material.SetColor("_BaseColor", color);
        if (material.HasProperty("_Smoothness")) material.SetFloat("_Smoothness", smoothness);
        if (material.HasProperty("_Glossiness")) material.SetFloat("_Glossiness", smoothness);
        if (material.HasProperty("_Metallic")) material.SetFloat("_Metallic", 0f);
        return material;
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

    private static Transform FindDeepChild(Transform root, string objectName)
    {
        foreach (Transform child in root.GetComponentsInChildren<Transform>(true))
            if (child.name.Equals(objectName, System.StringComparison.OrdinalIgnoreCase)) return child;
        return null;
    }

    private static bool TryGetBounds(Transform root, out Bounds bounds)
    {
        Renderer[] renderers = root.GetComponentsInChildren<Renderer>(true);
        if (renderers.Length == 0)
        {
            bounds = default;
            return false;
        }

        bounds = renderers[0].bounds;
        for (int i = 1; i < renderers.Length; i++) bounds.Encapsulate(renderers[i].bounds);
        return true;
    }
}
