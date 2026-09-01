using UnityEngine;

[DefaultExecutionOrder(1000)]
public sealed class StylizedWorkerAvatar : MonoBehaviour
{
    private PlayerMover mover;
    private CharacterController controller;
    private Camera view;

    private Transform modelRoot;
    private Transform bodyRoot;
    private Transform headRoot;

    private Transform belly;
    private Transform chest;
    private Transform pelvis;
    private Transform bib;
    private Transform bibPocket;
    private Transform leftStrap;
    private Transform rightStrap;
    private Transform chestReflectiveFront;
    private Transform chestReflectiveBack;

    private Transform leftUpperArm;
    private Transform leftLowerArm;
    private Transform rightUpperArm;
    private Transform rightLowerArm;
    private Transform leftGlove;
    private Transform rightGlove;
    private Transform leftUpperLeg;
    private Transform leftLowerLeg;
    private Transform rightUpperLeg;
    private Transform rightLowerLeg;
    private Transform leftBoot;
    private Transform rightBoot;

    private Transform legacyFirstPerson;
    private Transform legacyExternal;

    private Material skinMaterial;
    private Material orangeMaterial;
    private Material overallsMaterial;
    private Material reflectiveMaterial;
    private Material gloveMaterial;
    private Material bootMaterial;
    private Material hairMaterial;
    private Material frameMaterial;
    private Material lensMaterial;

    private float gait;
    private float crouch;
    private float movement;
    private float strafe;
    private float airborne;
    private float verticalSpeed;
    private float stun;
    private bool sprinting;
    private bool carrying;
    private bool built;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void InstallOnRuntimePlayer()
    {
        PlayerMover[] movers = Object.FindObjectsByType<PlayerMover>(FindObjectsSortMode.None);
        foreach (PlayerMover player in movers)
        {
            if (player != null && player.GetComponent<StylizedWorkerAvatar>() == null)
                player.gameObject.AddComponent<StylizedWorkerAvatar>();
        }
    }

    private void Awake()
    {
        mover = GetComponent<PlayerMover>();
        controller = GetComponent<CharacterController>();
        view = GetComponentInChildren<Camera>(true);
        BuildModel();
        FindLegacyVisuals();
        SuppressLegacyVisuals();
    }

    private void Start()
    {
        if (mover == null) mover = GetComponent<PlayerMover>();
        if (controller == null) controller = GetComponent<CharacterController>();
        if (view == null) view = GetComponentInChildren<Camera>(true);
    }

    private void BuildModel()
    {
        if (built) return;
        built = true;

        skinMaterial = MakeMaterial("Worker skin", new Color(0.82f, 0.49f, 0.31f), 0.46f);
        orangeMaterial = MakeMaterial("Safety orange shirt", new Color(0.96f, 0.27f, 0.035f), 0.40f);
        overallsMaterial = MakeMaterial("Navy overalls", new Color(0.035f, 0.10f, 0.19f), 0.32f);
        reflectiveMaterial = MakeMaterial("Reflective tape", new Color(0.72f, 0.70f, 0.64f), 0.58f);
        gloveMaterial = MakeMaterial("Work gloves", new Color(0.095f, 0.073f, 0.056f), 0.27f);
        bootMaterial = MakeMaterial("Work boots", new Color(0.065f, 0.052f, 0.043f), 0.22f);
        hairMaterial = MakeMaterial("Hair and moustache", new Color(0.17f, 0.105f, 0.064f), 0.30f);
        frameMaterial = MakeMaterial("Goggle frame", new Color(0.22f, 0.25f, 0.27f), 0.60f);
        lensMaterial = MakeMaterial("Goggle lens", new Color(0.56f, 0.66f, 0.68f), 0.88f);

        modelRoot = new GameObject("Reference stylized mover avatar").transform;
        modelRoot.SetParent(transform, false);

        bodyRoot = new GameObject("Body").transform;
        bodyRoot.SetParent(modelRoot, false);

        pelvis = RoundedPart(bodyRoot, "Overalls pelvis", PrimitiveType.Sphere, overallsMaterial,
            new Vector3(0f, 0.89f, 0f), new Vector3(0.74f, 0.58f, 0.54f));
        belly = RoundedPart(bodyRoot, "Rounded belly", PrimitiveType.Sphere, orangeMaterial,
            new Vector3(0f, 1.17f, 0.01f), new Vector3(0.78f, 0.72f, 0.58f));
        chest = RoundedPart(bodyRoot, "Upper shirt", PrimitiveType.Sphere, orangeMaterial,
            new Vector3(0f, 1.39f, 0.015f), new Vector3(0.68f, 0.48f, 0.51f));

        bib = RoundedPart(bodyRoot, "Overalls bib", PrimitiveType.Cube, overallsMaterial,
            new Vector3(0f, 1.29f, 0.305f), new Vector3(0.48f, 0.46f, 0.075f));
        bibPocket = RoundedPart(bodyRoot, "Bib pocket", PrimitiveType.Cube, overallsMaterial,
            new Vector3(0f, 1.30f, 0.352f), new Vector3(0.24f, 0.16f, 0.025f));
        leftStrap = RoundedPart(bodyRoot, "Left suspender", PrimitiveType.Cube, overallsMaterial,
            new Vector3(-0.215f, 1.48f, 0.275f), new Vector3(0.075f, 0.40f, 0.055f));
        rightStrap = RoundedPart(bodyRoot, "Right suspender", PrimitiveType.Cube, overallsMaterial,
            new Vector3(0.215f, 1.48f, 0.275f), new Vector3(0.075f, 0.40f, 0.055f));

        chestReflectiveFront = RoundedPart(bodyRoot, "Front reflective band", PrimitiveType.Cube, reflectiveMaterial,
            new Vector3(0f, 1.43f, 0.296f), new Vector3(0.65f, 0.065f, 0.035f));
        chestReflectiveBack = RoundedPart(bodyRoot, "Back reflective band", PrimitiveType.Cube, reflectiveMaterial,
            new Vector3(0f, 1.43f, -0.296f), new Vector3(0.65f, 0.065f, 0.035f));
        RoundedPart(bodyRoot, "Left shoulder reflective", PrimitiveType.Cube, reflectiveMaterial,
            new Vector3(-0.285f, 1.52f, 0.09f), new Vector3(0.060f, 0.33f, 0.075f), new Vector3(11f, 0f, -13f));
        RoundedPart(bodyRoot, "Right shoulder reflective", PrimitiveType.Cube, reflectiveMaterial,
            new Vector3(0.285f, 1.52f, 0.09f), new Vector3(0.060f, 0.33f, 0.075f), new Vector3(11f, 0f, 13f));

        leftUpperArm = FloatingPart("Left orange sleeve", PrimitiveType.Capsule, orangeMaterial);
        leftLowerArm = FloatingPart("Left orange forearm", PrimitiveType.Capsule, orangeMaterial);
        rightUpperArm = FloatingPart("Right orange sleeve", PrimitiveType.Capsule, orangeMaterial);
        rightLowerArm = FloatingPart("Right orange forearm", PrimitiveType.Capsule, orangeMaterial);
        leftGlove = FloatingPart("Left work glove", PrimitiveType.Sphere, gloveMaterial);
        rightGlove = FloatingPart("Right work glove", PrimitiveType.Sphere, gloveMaterial);

        leftUpperLeg = FloatingPart("Left overalls thigh", PrimitiveType.Capsule, overallsMaterial);
        leftLowerLeg = FloatingPart("Left overalls shin", PrimitiveType.Capsule, overallsMaterial);
        rightUpperLeg = FloatingPart("Right overalls thigh", PrimitiveType.Capsule, overallsMaterial);
        rightLowerLeg = FloatingPart("Right overalls shin", PrimitiveType.Capsule, overallsMaterial);
        leftBoot = CreateBoot("Left chunky work boot");
        rightBoot = CreateBoot("Right chunky work boot");

        BuildHead();
        ApplyPose(0f, 0f, true, false, false, 0f, false, false);
    }

    private void BuildHead()
    {
        headRoot = new GameObject("Head and accessories").transform;
        headRoot.SetParent(modelRoot, false);

        RoundedPart(headRoot, "Head", PrimitiveType.Sphere, skinMaterial,
            new Vector3(0f, 1.69f, 0.025f), new Vector3(0.48f, 0.45f, 0.43f));
        RoundedPart(headRoot, "Left ear", PrimitiveType.Sphere, skinMaterial,
            new Vector3(-0.265f, 1.67f, 0.005f), new Vector3(0.105f, 0.14f, 0.095f));
        RoundedPart(headRoot, "Right ear", PrimitiveType.Sphere, skinMaterial,
            new Vector3(0.265f, 1.67f, 0.005f), new Vector3(0.105f, 0.14f, 0.095f));
        RoundedPart(headRoot, "Nose", PrimitiveType.Sphere, skinMaterial,
            new Vector3(0f, 1.665f, 0.405f), new Vector3(0.16f, 0.14f, 0.145f));

        RoundedPart(headRoot, "Left moustache", PrimitiveType.Sphere, hairMaterial,
            new Vector3(-0.075f, 1.585f, 0.405f), new Vector3(0.17f, 0.075f, 0.060f), new Vector3(0f, 0f, -17f));
        RoundedPart(headRoot, "Right moustache", PrimitiveType.Sphere, hairMaterial,
            new Vector3(0.075f, 1.585f, 0.405f), new Vector3(0.17f, 0.075f, 0.060f), new Vector3(0f, 0f, 17f));

        CreateGoggleLens("Left safety lens", -0.125f);
        CreateGoggleLens("Right safety lens", 0.125f);
        RoundedPart(headRoot, "Goggle bridge", PrimitiveType.Cube, frameMaterial,
            new Vector3(0f, 1.735f, 0.392f), new Vector3(0.075f, 0.035f, 0.035f));
        RoundedPart(headRoot, "Left goggle strap", PrimitiveType.Cube, frameMaterial,
            new Vector3(-0.255f, 1.735f, 0.05f), new Vector3(0.035f, 0.055f, 0.58f));
        RoundedPart(headRoot, "Right goggle strap", PrimitiveType.Cube, frameMaterial,
            new Vector3(0.255f, 1.735f, 0.05f), new Vector3(0.035f, 0.055f, 0.58f));
        RoundedPart(headRoot, "Back goggle strap", PrimitiveType.Cube, frameMaterial,
            new Vector3(0f, 1.735f, -0.205f), new Vector3(0.48f, 0.055f, 0.035f));

        RoundedPart(headRoot, "Hair cap", PrimitiveType.Sphere, hairMaterial,
            new Vector3(0f, 1.875f, -0.01f), new Vector3(0.41f, 0.18f, 0.37f));
        RoundedPart(headRoot, "Hair tuft 1", PrimitiveType.Sphere, hairMaterial,
            new Vector3(-0.10f, 1.965f, 0.01f), new Vector3(0.13f, 0.10f, 0.11f), new Vector3(0f, 0f, -18f));
        RoundedPart(headRoot, "Hair tuft 2", PrimitiveType.Sphere, hairMaterial,
            new Vector3(0.03f, 1.985f, 0.02f), new Vector3(0.14f, 0.105f, 0.12f), new Vector3(0f, 0f, 8f));
        RoundedPart(headRoot, "Hair tuft 3", PrimitiveType.Sphere, hairMaterial,
            new Vector3(0.14f, 1.955f, 0.005f), new Vector3(0.11f, 0.09f, 0.10f), new Vector3(0f, 0f, 24f));
    }

    private void CreateGoggleLens(string name, float x)
    {
        RoundedPart(headRoot, name, PrimitiveType.Cube, lensMaterial,
            new Vector3(x, 1.735f, 0.405f), new Vector3(0.205f, 0.125f, 0.028f));
        RoundedPart(headRoot, name + " top frame", PrimitiveType.Cube, frameMaterial,
            new Vector3(x, 1.805f, 0.418f), new Vector3(0.235f, 0.022f, 0.028f));
        RoundedPart(headRoot, name + " bottom frame", PrimitiveType.Cube, frameMaterial,
            new Vector3(x, 1.665f, 0.418f), new Vector3(0.235f, 0.022f, 0.028f));
        RoundedPart(headRoot, name + " inner frame", PrimitiveType.Cube, frameMaterial,
            new Vector3(x - 0.112f, 1.735f, 0.418f), new Vector3(0.022f, 0.14f, 0.028f));
        RoundedPart(headRoot, name + " outer frame", PrimitiveType.Cube, frameMaterial,
            new Vector3(x + 0.112f, 1.735f, 0.418f), new Vector3(0.022f, 0.14f, 0.028f));
    }

    private Transform CreateBoot(string objectName)
    {
        Transform root = new GameObject(objectName).transform;
        root.SetParent(modelRoot, false);
        RoundedPart(root, "Boot upper", PrimitiveType.Sphere, bootMaterial,
            new Vector3(0f, 0.06f, 0f), new Vector3(0.29f, 0.22f, 0.34f));
        RoundedPart(root, "Boot toe", PrimitiveType.Sphere, bootMaterial,
            new Vector3(0f, 0.01f, 0.15f), new Vector3(0.31f, 0.18f, 0.42f));
        RoundedPart(root, "Boot sole", PrimitiveType.Cube, bootMaterial,
            new Vector3(0f, -0.09f, 0.10f), new Vector3(0.34f, 0.07f, 0.50f));
        return root;
    }

    private Transform FloatingPart(string objectName, PrimitiveType primitive, Material material)
    {
        GameObject part = GameObject.CreatePrimitive(primitive);
        part.name = objectName;
        part.transform.SetParent(modelRoot, false);
        Renderer renderer = part.GetComponent<Renderer>();
        renderer.sharedMaterial = material;
        renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
        renderer.receiveShadows = true;
        Collider collider = part.GetComponent<Collider>();
        if (collider != null) Destroy(collider);
        return part.transform;
    }

    private Transform RoundedPart(Transform parent, string objectName, PrimitiveType primitive, Material material, Vector3 localPosition, Vector3 localScale)
    {
        return RoundedPart(parent, objectName, primitive, material, localPosition, localScale, Vector3.zero);
    }

    private Transform RoundedPart(Transform parent, string objectName, PrimitiveType primitive, Material material, Vector3 localPosition, Vector3 localScale, Vector3 localEuler)
    {
        GameObject part = GameObject.CreatePrimitive(primitive);
        part.name = objectName;
        part.transform.SetParent(parent, false);
        part.transform.localPosition = localPosition;
        part.transform.localRotation = Quaternion.Euler(localEuler);
        part.transform.localScale = localScale;
        Renderer renderer = part.GetComponent<Renderer>();
        renderer.sharedMaterial = material;
        renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
        renderer.receiveShadows = true;
        Collider collider = part.GetComponent<Collider>();
        if (collider != null) Destroy(collider);
        return part.transform;
    }

    private void LateUpdate()
    {
        FindLegacyVisuals();
        SuppressLegacyVisuals();

        if (!built || modelRoot == null) return;
        if (mover == null) mover = GetComponent<PlayerMover>();
        if (controller == null) controller = GetComponent<CharacterController>();
        if (view == null) view = GetComponentInChildren<Camera>(true);
        if (controller == null || view == null) return;

        bool cameraBelongsToPlayer = view.transform.IsChildOf(transform);
        modelRoot.gameObject.SetActive(cameraBelongsToPlayer);
        if (!cameraBelongsToPlayer) return;

        bool thirdPerson = Vector3.Distance(view.transform.position, transform.position) > 2.35f;
        if (headRoot != null) headRoot.gameObject.SetActive(thirdPerson);

        Vector3 localVelocity = transform.InverseTransformDirection(controller.velocity);
        float planarSpeed = new Vector2(localVelocity.x, localVelocity.z).magnitude;
        float targetMovement = Mathf.Clamp01(planarSpeed / 4.2f);
        float targetStrafe = planarSpeed > 0.05f ? Mathf.Clamp(localVelocity.x / Mathf.Max(2.5f, planarSpeed), -1f, 1f) : 0f;
        bool grounded = controller.isGrounded;
        bool isCrouched = controller.height < 1.55f;
        bool isSprinting = planarSpeed > 5.25f && !isCrouched;
        bool isCarrying = mover != null && mover.IsCarrying;
        bool isStunned = mover != null && mover.IsStunned;

        movement = Mathf.MoveTowards(movement, targetMovement, Time.deltaTime * 8f);
        strafe = Mathf.MoveTowards(strafe, targetStrafe, Time.deltaTime * 8f);
        crouch = Mathf.MoveTowards(crouch, isCrouched ? 1f : 0f, Time.deltaTime * 7f);
        airborne = Mathf.MoveTowards(airborne, grounded ? 0f : 1f, Time.deltaTime * 9f);
        stun = Mathf.MoveTowards(stun, isStunned ? 1f : 0f, Time.deltaTime * 7f);
        verticalSpeed = controller.velocity.y;
        sprinting = isSprinting;
        carrying = isCarrying;

        gait += Time.deltaTime * Mathf.Lerp(5.5f, sprinting ? 12.8f : 8.8f, movement);
        ApplyPose(movement, strafe, grounded, isCrouched, isSprinting, verticalSpeed, isCarrying, isStunned);
    }

    private void ApplyPose(float moveAmount, float sideways, bool grounded, bool isCrouched, bool isSprinting, float ySpeed, bool isCarrying, bool isStunned)
    {
        if (modelRoot == null) return;

        float crouchAmount = isCrouched ? Mathf.Max(crouch, 0.75f) : crouch;
        float bodyDrop = crouchAmount * 0.34f;
        float step = grounded ? Mathf.Sin(gait) * moveAmount : 0f;
        float sprintScale = isSprinting ? 1.24f : 1f;
        float bob = grounded ? Mathf.Abs(Mathf.Sin(gait * 2f)) * 0.018f * moveAmount : 0f;
        float jumpTuck = !grounded ? Mathf.Clamp01(0.55f - ySpeed * 0.045f) : 0f;
        float wobble = isStunned ? Mathf.Sin(Time.time * 10f) * 0.09f : 0f;

        bodyRoot.localPosition = new Vector3(wobble * 0.22f, -bodyDrop + bob, 0f);
        bodyRoot.localRotation = Quaternion.Euler(
            isStunned ? Mathf.Sin(Time.time * 8f) * 5f : (isCarrying ? 4.5f : 0f),
            0f,
            -sideways * 2.5f - wobble * 15f);

        if (headRoot != null)
        {
            headRoot.localPosition = new Vector3(wobble * 0.10f, -bodyDrop + bob, 0f);
            headRoot.localRotation = Quaternion.Euler(isCarrying ? 2f : 0f, 0f, -sideways * 1.5f);
        }

        float leftLift = grounded ? Mathf.Max(0f, Mathf.Sin(gait)) * 0.13f * moveAmount : 0.11f + jumpTuck * 0.15f;
        float rightLift = grounded ? Mathf.Max(0f, -Mathf.Sin(gait)) * 0.13f * moveAmount : 0.13f + jumpTuck * 0.18f;

        Vector3 leftHip = transform.TransformPoint(new Vector3(-0.205f, 0.83f - bodyDrop, 0f));
        Vector3 rightHip = transform.TransformPoint(new Vector3(0.205f, 0.83f - bodyDrop, 0f));
        Vector3 leftFoot = transform.TransformPoint(new Vector3(-0.205f, 0.13f, step * 0.25f * sprintScale - sideways * 0.035f));
        Vector3 rightFoot = transform.TransformPoint(new Vector3(0.205f, 0.13f, -step * 0.25f * sprintScale + sideways * 0.035f));
        leftFoot.y += leftLift;
        rightFoot.y += rightLift;
        if (!grounded)
        {
            leftFoot += transform.forward * (0.08f - jumpTuck * 0.05f);
            rightFoot += transform.forward * (-0.02f + jumpTuck * 0.08f);
        }

        PoseLeg(leftHip, leftFoot, leftUpperLeg, leftLowerLeg, leftBoot, -1f, crouchAmount, !grounded);
        PoseLeg(rightHip, rightFoot, rightUpperLeg, rightLowerLeg, rightBoot, 1f, crouchAmount, !grounded);

        Vector3 leftShoulder = transform.TransformPoint(new Vector3(-0.39f, 1.42f - bodyDrop + bob, 0.015f));
        Vector3 rightShoulder = transform.TransformPoint(new Vector3(0.39f, 1.42f - bodyDrop + bob, 0.015f));

        Vector3 leftHandTarget;
        Vector3 rightHandTarget;
        if (isCarrying && mover != null)
        {
            Transform leftGrip = mover.LeftGripPoint;
            Transform rightGrip = mover.RightGripPoint;
            leftHandTarget = leftGrip != null ? leftGrip.position : transform.TransformPoint(new Vector3(-0.31f, 1.04f - bodyDrop, 0.48f));
            rightHandTarget = rightGrip != null ? rightGrip.position : transform.TransformPoint(new Vector3(0.31f, 1.04f - bodyDrop, 0.48f));
        }
        else
        {
            float armSwing = step * 0.22f * sprintScale;
            leftHandTarget = transform.TransformPoint(new Vector3(-0.42f, 0.94f - bodyDrop, 0.02f - armSwing));
            rightHandTarget = transform.TransformPoint(new Vector3(0.42f, 0.94f - bodyDrop, 0.02f + armSwing));
            if (!grounded)
            {
                leftHandTarget += transform.forward * 0.10f;
                rightHandTarget += transform.forward * 0.10f;
            }
        }

        leftHandTarget += transform.right * wobble;
        rightHandTarget -= transform.right * wobble;
        PoseArm(leftShoulder, leftHandTarget, leftUpperArm, leftLowerArm, leftGlove, -1f, isCarrying);
        PoseArm(rightShoulder, rightHandTarget, rightUpperArm, rightLowerArm, rightGlove, 1f, isCarrying);
    }

    private void PoseArm(Vector3 shoulder, Vector3 handPosition, Transform upper, Transform lower, Transform glove, float side, bool gripping)
    {
        float maxReach = 1.08f;
        Vector3 shoulderToHand = handPosition - shoulder;
        if (shoulderToHand.magnitude > maxReach)
            handPosition = shoulder + shoulderToHand.normalized * maxReach;

        Vector3 elbow = Vector3.Lerp(shoulder, handPosition, 0.49f)
            + transform.right * side * 0.14f
            + Vector3.down * 0.12f
            + (gripping ? transform.forward * 0.08f : Vector3.zero);

        PoseLimb(upper, shoulder, elbow, 0.185f);
        PoseLimb(lower, elbow, handPosition, 0.165f);
        glove.position = handPosition;
        glove.rotation = transform.rotation * Quaternion.Euler(gripping ? 55f : 8f, 0f, side * 8f);
        glove.localScale = new Vector3(0.23f, 0.20f, 0.20f);
    }

    private void PoseLeg(Vector3 hip, Vector3 foot, Transform upper, Transform lower, Transform boot, float side, float crouchAmount, bool inAir)
    {
        Vector3 knee = Vector3.Lerp(hip, foot, 0.47f)
            + transform.forward * (0.14f + crouchAmount * 0.21f + (inAir ? 0.08f : 0f))
            + transform.right * side * 0.025f;

        PoseLimb(upper, hip, knee, 0.235f);
        PoseLimb(lower, knee, foot + Vector3.up * 0.10f, 0.205f);
        boot.position = foot + transform.forward * 0.08f;
        boot.rotation = transform.rotation;
    }

    private static void PoseLimb(Transform limb, Vector3 start, Vector3 end, float thickness)
    {
        if (limb == null) return;
        Vector3 direction = end - start;
        float length = Mathf.Max(0.025f, direction.magnitude);
        limb.position = (start + end) * 0.5f;
        limb.rotation = Quaternion.FromToRotation(Vector3.up, direction.normalized);
        limb.localScale = new Vector3(thickness, length * 0.5f, thickness);
    }

    private void FindLegacyVisuals()
    {
        if (legacyFirstPerson == null) legacyFirstPerson = transform.Find("First-person arms and legs only");
        if (legacyExternal == null) legacyExternal = transform.Find("Rigged mover character");
    }

    private void SuppressLegacyVisuals()
    {
        if (legacyFirstPerson != null && legacyFirstPerson.gameObject.activeSelf)
            legacyFirstPerson.gameObject.SetActive(false);
        if (legacyExternal != null && legacyExternal.gameObject.activeSelf)
            legacyExternal.gameObject.SetActive(false);
    }

    private static Material MakeMaterial(string materialName, Color color, float smoothness)
    {
        Shader shader = Shader.Find("Universal Render Pipeline/Lit");
        if (shader == null) shader = Shader.Find("Standard");
        Material material = new Material(shader);
        material.name = materialName;
        material.color = color;
        if (material.HasProperty("_BaseColor")) material.SetColor("_BaseColor", color);
        if (material.HasProperty("_Color")) material.SetColor("_Color", color);
        if (material.HasProperty("_Smoothness")) material.SetFloat("_Smoothness", smoothness);
        if (material.HasProperty("_Glossiness")) material.SetFloat("_Glossiness", smoothness);
        if (material.HasProperty("_Metallic")) material.SetFloat("_Metallic", 0f);
        return material;
    }
}
