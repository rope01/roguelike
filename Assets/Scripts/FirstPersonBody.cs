using UnityEngine;
using UnityEngine.Rendering;

[DefaultExecutionOrder(40)]
public sealed class FirstPersonBody : MonoBehaviour
{
    private Transform visualRoot;
    private Transform torso;
    private Transform belly;
    private Transform hips;
    private Transform bib;
    private Transform leftSuspender;
    private Transform rightSuspender;
    private Transform reflectiveBar;
    private Transform head;
    private Transform leftLens;
    private Transform rightLens;
    private Transform leftUpperArm;
    private Transform leftLowerArm;
    private Transform rightUpperArm;
    private Transform rightLowerArm;
    private Transform leftHand;
    private Transform rightHand;
    private Transform leftThumb;
    private Transform rightThumb;
    private Transform leftUpperLeg;
    private Transform leftLowerLeg;
    private Transform rightUpperLeg;
    private Transform rightLowerLeg;
    private Transform leftBoot;
    private Transform rightBoot;

    private PlayerMover mover;
    private PlayerLoadResponse loadResponse;
    private ProceduralHandIK handIK;
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
        loadResponse = GetComponent<PlayerLoadResponse>();
        handIK = GetComponent<ProceduralHandIK>();
        BuildRig();
    }

    private void Start()
    {
        mover = GetComponent<PlayerMover>();
        loadResponse = GetComponent<PlayerLoadResponse>();
        handIK = GetComponent<ProceduralHandIK>();
    }

    private void BuildRig()
    {
        visualRoot = new GameObject("Visible mover body").transform;
        visualRoot.SetParent(transform, false);

        Material vest = CreateMaterial(new Color(0.96f, 0.30f, 0.025f));
        Material overalls = CreateMaterial(new Color(0.055f, 0.20f, 0.46f));
        Material darkBlue = CreateMaterial(new Color(0.035f, 0.09f, 0.20f));
        Material reflective = CreateMaterial(new Color(0.88f, 0.92f, 0.72f), 0.48f);
        Material gloves = CreateMaterial(new Color(0.16f, 0.18f, 0.17f));
        Material skin = CreateMaterial(new Color(0.76f, 0.52f, 0.34f));
        Material boots = CreateMaterial(new Color(0.045f, 0.045f, 0.040f));
        Material lenses = CreateTransparentMaterial(new Color(0.72f, 0.90f, 0.95f, 0.36f));

        torso = Part("Rounded orange vest", PrimitiveType.Sphere, vest);
        belly = Part("Rounded overall belly", PrimitiveType.Sphere, overalls);
        hips = Part("Overall hips", PrimitiveType.Sphere, overalls);
        bib = Part("Overall bib", PrimitiveType.Cube, overalls);
        leftSuspender = Part("Left suspender", PrimitiveType.Cube, darkBlue);
        rightSuspender = Part("Right suspender", PrimitiveType.Cube, darkBlue);
        reflectiveBar = Part("Reflective vest stripe", PrimitiveType.Cube, reflective);

        head = Part("Mover head", PrimitiveType.Sphere, skin);
        leftLens = Part("Left safety lens", PrimitiveType.Cube, lenses);
        rightLens = Part("Right safety lens", PrimitiveType.Cube, lenses);

        leftUpperArm = Part("Left upper arm", PrimitiveType.Capsule, darkBlue);
        leftLowerArm = Part("Left forearm", PrimitiveType.Capsule, darkBlue);
        rightUpperArm = Part("Right upper arm", PrimitiveType.Capsule, darkBlue);
        rightLowerArm = Part("Right forearm", PrimitiveType.Capsule, darkBlue);
        leftHand = Part("Left work glove", PrimitiveType.Sphere, gloves);
        rightHand = Part("Right work glove", PrimitiveType.Sphere, gloves);
        leftThumb = Part("Left glove thumb", PrimitiveType.Sphere, gloves);
        rightThumb = Part("Right glove thumb", PrimitiveType.Sphere, gloves);

        leftUpperLeg = Part("Left overall thigh", PrimitiveType.Capsule, overalls);
        leftLowerLeg = Part("Left overall shin", PrimitiveType.Capsule, overalls);
        rightUpperLeg = Part("Right overall thigh", PrimitiveType.Capsule, overalls);
        rightLowerLeg = Part("Right overall shin", PrimitiveType.Capsule, overalls);
        leftBoot = Part("Left work boot", PrimitiveType.Cube, boots);
        rightBoot = Part("Right work boot", PrimitiveType.Cube, boots);
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

    private static Material CreateMaterial(Color color, float smoothness = 0.18f)
    {
        Shader shader = Shader.Find("Standard");
        if (shader == null) shader = Shader.Find("Universal Render Pipeline/Lit");
        Material material = new Material(shader);
        material.color = color;
        material.SetFloat("_Smoothness", smoothness);
        material.SetFloat("_Glossiness", smoothness);
        return material;
    }

    private static Material CreateTransparentMaterial(Color color)
    {
        Material material = CreateMaterial(color, 0.62f);
        material.color = color;
        if (material.HasProperty("_Mode")) material.SetFloat("_Mode", 3f);
        if (material.HasProperty("_Surface")) material.SetFloat("_Surface", 1f);
        material.SetInt("_SrcBlend", (int)BlendMode.SrcAlpha);
        material.SetInt("_DstBlend", (int)BlendMode.OneMinusSrcAlpha);
        material.SetInt("_ZWrite", 0);
        material.DisableKeyword("_ALPHATEST_ON");
        material.EnableKeyword("_ALPHABLEND_ON");
        material.renderQueue = 3000;
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
        if (loadResponse == null) loadResponse = GetComponent<PlayerLoadResponse>();
        if (handIK == null) handIK = GetComponent<ProceduralHandIK>();
        if (mover == null || visualRoot == null || !visualRoot.gameObject.activeSelf) return;

        float load = loadResponse != null ? loadResponse.LoadFactor : 0f;
        float extreme = loadResponse != null ? loadResponse.ExtremeLoadFactor : 0f;
        float bodySink = loadResponse != null && carrying ? loadResponse.BodySink : 0f;
        float kneeBend = loadResponse != null && carrying ? loadResponse.KneeBend : 0f;
        float shoulderDrop = loadResponse != null && carrying ? loadResponse.ShoulderDrop : 0f;
        float forwardLoad = loadResponse != null && carrying ? loadResponse.ForwardLoad : 0f;
        float sideLoad = loadResponse != null && carrying ? loadResponse.SideLoad : 0f;

        gait += Time.deltaTime * Mathf.Lerp(5f, sprinting ? 13f : 9f, movement);
        float step = grounded ? Mathf.Sin(gait) * movement : 0f;
        float liftLeft = grounded ? Mathf.Max(0f, Mathf.Sin(gait)) * 0.13f * movement : 0.05f;
        float liftRight = grounded ? Mathf.Max(0f, -Mathf.Sin(gait)) * 0.13f * movement : 0.05f;
        float crouchDrop = Mathf.Lerp(0f, -0.47f, crouch);
        float bodyHeight = crouchDrop - bodySink;
        float flop = stun * Mathf.Sin(Time.time * 10f) * 0.22f;
        float tremble = extreme * Mathf.Sin(Time.time * 16f) * 0.018f;

        Vector3 counterLocal = new Vector3(-sideLoad * 0.10f * load, 0f, -forwardLoad * 0.14f * load);
        Vector3 hipCenter = transform.TransformPoint(new Vector3(counterLocal.x * 0.45f, 0.84f + bodyHeight, -0.055f + counterLocal.z * 0.45f));
        Vector3 bellyCenter = transform.TransformPoint(new Vector3(counterLocal.x * 0.70f, 1.03f + bodyHeight, -0.105f + counterLocal.z * 0.70f));
        Vector3 chestCenter = transform.TransformPoint(new Vector3(counterLocal.x, 1.29f + bodyHeight, -0.15f + counterLocal.z));

        float carryBend = carrying ? 5f * load + 3f * extreme : 0f;
        float counterPitch = -forwardLoad * 12f * load;
        float counterRoll = -sideLoad * 9f * load;
        Quaternion hipsRotation = transform.rotation * Quaternion.Euler(0f, 0f, -strafe * 3f + counterRoll * 0.35f + flop * 13f);
        Quaternion chestRotation = transform.rotation * Quaternion.Euler(carryBend + counterPitch + step * 1.5f, 0f, -strafe * 2f + counterRoll + flop * 9f);

        PoseBlock(hips, hipCenter, hipsRotation, new Vector3(0.62f, 0.34f, 0.43f));
        PoseBlock(belly, bellyCenter, chestRotation, new Vector3(0.64f, 0.48f, 0.45f));
        PoseBlock(torso, chestCenter, chestRotation, new Vector3(0.68f, 0.62f, 0.43f));

        Vector3 bibPosition = chestCenter + chestRotation * new Vector3(0f, -0.10f, 0.205f);
        PoseBlock(bib, bibPosition, chestRotation, new Vector3(0.35f, 0.31f, 0.028f));
        PoseBlock(leftSuspender, chestCenter + chestRotation * new Vector3(-0.18f, 0.08f, 0.216f), chestRotation * Quaternion.Euler(0f, 0f, -7f), new Vector3(0.055f, 0.42f, 0.025f));
        PoseBlock(rightSuspender, chestCenter + chestRotation * new Vector3(0.18f, 0.08f, 0.216f), chestRotation * Quaternion.Euler(0f, 0f, 7f), new Vector3(0.055f, 0.42f, 0.025f));
        PoseBlock(reflectiveBar, chestCenter + chestRotation * new Vector3(0f, 0.10f, 0.222f), chestRotation, new Vector3(0.54f, 0.055f, 0.022f));

        PoseHead(bodyHeight, load, extreme);

        float stance = 0.19f + load * 0.035f + extreme * 0.025f;
        Vector3 leftHip = transform.TransformPoint(new Vector3(-0.18f, 0.79f + bodyHeight, -0.03f));
        Vector3 rightHip = transform.TransformPoint(new Vector3(0.18f, 0.79f + bodyHeight, -0.03f));
        Vector3 leftFoot = transform.TransformPoint(new Vector3(-stance, 0.09f, step * 0.22f + strafe * -0.045f + tremble));
        Vector3 rightFoot = transform.TransformPoint(new Vector3(stance, 0.09f, -step * 0.22f + strafe * 0.045f - tremble));
        leftFoot.y += liftLeft;
        rightFoot.y += liftRight;

        if (!grounded)
        {
            leftFoot += transform.forward * 0.10f;
            rightFoot -= transform.forward * 0.045f;
        }
        if (stun > 0.01f)
        {
            leftFoot += transform.right * flop;
            rightFoot -= transform.right * flop;
        }

        PoseLeg(leftHip, leftFoot, leftUpperLeg, leftLowerLeg, leftBoot, -1f, kneeBend, tremble);
        PoseLeg(rightHip, rightFoot, rightUpperLeg, rightLowerLeg, rightBoot, 1f, kneeBend, -tremble);

        Vector3 leftShoulder = chestCenter + chestRotation * new Vector3(-0.36f, 0.13f - shoulderDrop, 0.01f);
        Vector3 rightShoulder = chestCenter + chestRotation * new Vector3(0.36f, 0.13f - shoulderDrop, 0.01f);

        ResolveHandPose(true, leftShoulder, step, flop, out Vector3 leftTarget, out Quaternion leftRotation);
        ResolveHandPose(false, rightShoulder, -step, -flop, out Vector3 rightTarget, out Quaternion rightRotation);
        PoseArm(leftShoulder, leftTarget, leftRotation, leftUpperArm, leftLowerArm, leftHand, leftThumb, -1f, leftGripping, load);
        PoseArm(rightShoulder, rightTarget, rightRotation, rightUpperArm, rightLowerArm, rightHand, rightThumb, 1f, rightGripping, load);
    }

    private void PoseHead(float bodyHeight, float load, float extreme)
    {
        float cameraSink = loadResponse != null && carrying ? loadResponse.CameraSink : 0f;
        float crouchHead = crouch * 0.47f;
        Vector3 headCenter = transform.TransformPoint(new Vector3(0f, 1.67f - crouchHead - cameraSink, -0.11f));
        Quaternion headRotation = transform.rotation * Quaternion.Euler(carrying ? 2f * load : 0f, 0f, 0f);
        PoseBlock(head, headCenter, headRotation, new Vector3(0.43f, 0.46f, 0.39f));

        Vector3 lensBase = headCenter + headRotation * new Vector3(0f, 0.035f, 0.205f);
        PoseBlock(leftLens, lensBase + headRotation * new Vector3(-0.105f, 0f, 0f), headRotation, new Vector3(0.17f, 0.09f, 0.028f));
        PoseBlock(rightLens, lensBase + headRotation * new Vector3(0.105f, 0f, 0f), headRotation, new Vector3(0.17f, 0.09f, 0.028f));
    }

    private void ResolveHandPose(bool left, Vector3 shoulder, float step, float flop, out Vector3 target, out Quaternion rotation)
    {
        if (handIK != null && handIK.TryGetVisualPose(left, out target, out rotation)) return;

        float side = left ? -1f : 1f;
        Vector3 local = new Vector3(side * 0.35f, 0.92f - crouch * 0.34f, 0.06f - step * 0.16f);
        if (carrying) local += new Vector3(0f, 0.08f, 0.28f);
        target = transform.TransformPoint(local) + transform.right * flop;
        rotation = transform.rotation * Quaternion.Euler(leftGripping || rightGripping ? 55f : 8f, 0f, side * 10f);
    }

    private void PoseArm(
        Vector3 shoulder,
        Vector3 handPosition,
        Quaternion handRotation,
        Transform upper,
        Transform lower,
        Transform hand,
        Transform thumb,
        float side,
        bool gripping,
        float load)
    {
        Vector3 shoulderToHand = handPosition - shoulder;
        float reach = shoulderToHand.magnitude;
        float bend = Mathf.Lerp(0.24f, 0.15f, Mathf.InverseLerp(0.55f, 1.45f, reach));
        bend += gripping ? 0.04f : 0f;
        bend += load * 0.035f;

        Vector3 elbowHint = transform.right * side * bend + Vector3.down * (0.16f + load * 0.07f);
        Vector3 midpoint = Vector3.Lerp(shoulder, handPosition, 0.50f) + elbowHint;
        PoseLimb(upper, shoulder, midpoint, Mathf.Lerp(0.16f, 0.18f, load));
        PoseLimb(lower, midpoint, handPosition, Mathf.Lerp(0.14f, 0.16f, load));
        PoseBlock(hand, handPosition, handRotation, new Vector3(0.20f, 0.145f, 0.23f));

        Vector3 thumbOffset = handRotation * new Vector3(side * 0.10f, -0.02f, 0.035f);
        Quaternion thumbRotation = handRotation * Quaternion.Euler(20f, side * 35f, side * 25f);
        PoseBlock(thumb, handPosition + thumbOffset, thumbRotation, new Vector3(0.09f, 0.08f, 0.13f));
    }

    private void PoseLeg(Vector3 hip, Vector3 foot, Transform upper, Transform lower, Transform boot, float side, float loadBend, float tremble)
    {
        Vector3 knee = Vector3.Lerp(hip, foot, 0.47f)
                       + transform.forward * (0.14f + loadBend)
                       + transform.right * (side * 0.03f + tremble);
        PoseLimb(upper, hip, knee, 0.19f);
        PoseLimb(lower, knee, foot + Vector3.up * 0.09f, 0.155f);
        Quaternion bootRotation = transform.rotation * Quaternion.Euler(0f, side * 2f, -side * tremble * 110f);
        PoseBlock(boot, foot + transform.forward * 0.075f, bootRotation, new Vector3(0.28f, 0.18f, 0.43f));
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
