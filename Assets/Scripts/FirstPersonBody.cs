using UnityEngine;

public sealed class FirstPersonBody : MonoBehaviour
{
    private Transform visualRoot;
    private Transform torso;
    private Transform belly;
    private Transform overallBib;
    private Transform leftStrap;
    private Transform rightStrap;
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
    private MoverLoadResponse loadResponse;
    private CharacterController controller;
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
    private Vector3 leftHandVisual;
    private Vector3 rightHandVisual;
    private bool leftHandInitialized;
    private bool rightHandInitialized;

    private void Awake()
    {
        mover = GetComponent<PlayerMover>();
        loadResponse = GetComponent<MoverLoadResponse>();
        controller = GetComponent<CharacterController>();
        BuildRig();
    }

    private void Start()
    {
        mover = GetComponent<PlayerMover>();
        loadResponse = GetComponent<MoverLoadResponse>();
        controller = GetComponent<CharacterController>();
    }

    private void BuildRig()
    {
        visualRoot = new GameObject("Visible first-person mover body").transform;
        visualRoot.SetParent(transform, false);

        Material vestOrange = CreateMaterial(new Color(0.95f, 0.31f, 0.025f), 0.12f);
        Material workBlue = CreateMaterial(new Color(0.055f, 0.18f, 0.34f), 0.16f);
        Material shirt = CreateMaterial(new Color(0.11f, 0.13f, 0.15f), 0.12f);
        Material gloves = CreateMaterial(new Color(0.18f, 0.20f, 0.19f), 0.22f);
        Material boots = CreateMaterial(new Color(0.045f, 0.043f, 0.040f), 0.18f);

        torso = Part("Rounded orange safety vest", PrimitiveType.Capsule, vestOrange);
        belly = Part("Soft belly", PrimitiveType.Sphere, vestOrange);
        overallBib = Part("Blue overall bib", PrimitiveType.Cube, workBlue);
        leftStrap = Part("Left overall strap", PrimitiveType.Cube, workBlue);
        rightStrap = Part("Right overall strap", PrimitiveType.Cube, workBlue);
        hips = Part("Blue overall hips", PrimitiveType.Capsule, workBlue);

        leftUpperArm = Part("Left upper arm", PrimitiveType.Capsule, shirt);
        leftLowerArm = Part("Left forearm", PrimitiveType.Capsule, shirt);
        rightUpperArm = Part("Right upper arm", PrimitiveType.Capsule, shirt);
        rightLowerArm = Part("Right forearm", PrimitiveType.Capsule, shirt);
        leftHand = Part("Left protective glove", PrimitiveType.Sphere, gloves);
        rightHand = Part("Right protective glove", PrimitiveType.Sphere, gloves);

        leftUpperLeg = Part("Left overall thigh", PrimitiveType.Capsule, workBlue);
        leftLowerLeg = Part("Left overall shin", PrimitiveType.Capsule, workBlue);
        rightUpperLeg = Part("Right overall thigh", PrimitiveType.Capsule, workBlue);
        rightLowerLeg = Part("Right overall shin", PrimitiveType.Capsule, workBlue);
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
        if (loadResponse == null) loadResponse = GetComponent<MoverLoadResponse>();
        if (controller == null) controller = GetComponent<CharacterController>();
        if (mover == null || visualRoot == null || !visualRoot.gameObject.activeSelf) return;

        float load = loadResponse != null ? loadResponse.LoadFactor : 0f;
        float extreme = loadResponse != null ? loadResponse.ExtremeLoadFactor : 0f;
        float kneeBend = loadResponse != null ? loadResponse.KneeBend : 0f;
        float bodySag = loadResponse != null ? loadResponse.BodySag : 0f;
        float swayAmount = loadResponse != null ? loadResponse.SwayAmount : 0f;
        Vector3 compensationWorld = loadResponse != null ? transform.TransformVector(loadResponse.CompensationLocal) : Vector3.zero;

        gait += Time.deltaTime * Mathf.Lerp(5f, sprinting ? 13f : 9f, movement) * Mathf.Lerp(1f, 0.68f, load);
        float step = grounded ? Mathf.Sin(gait) * movement : 0f;
        float liftLeft = grounded ? Mathf.Max(0f, Mathf.Sin(gait)) * 0.12f * movement : 0.05f;
        float liftRight = grounded ? Mathf.Max(0f, -Mathf.Sin(gait)) * 0.12f * movement : 0.05f;
        float crouchDrop = Mathf.Lerp(0f, -0.46f, crouch);
        float loadDrop = -bodySag;
        float flop = stun * Mathf.Sin(Time.time * 10f) * 0.22f;
        float loadSway = carrying ? Mathf.Sin(Time.time * Mathf.Lerp(4.2f, 2.7f, load)) * swayAmount : 0f;

        Vector3 hipCenter = transform.TransformPoint(new Vector3(strafe * 0.025f, 0.82f + crouchDrop + loadDrop, -0.045f));
        hipCenter += compensationWorld * 0.40f;

        Vector3 chestCenter = transform.TransformPoint(new Vector3(-strafe * 0.02f, 1.19f + crouchDrop + loadDrop * 0.72f, -0.075f));
        chestCenter += compensationWorld;
        chestCenter += transform.right * loadSway * 0.45f;

        float forwardLean = carrying ? Mathf.Lerp(0f, 8f, load) : step * 1.5f;
        float compensationPitch = loadResponse != null ? -loadResponse.CompensationLocal.z * 55f : 0f;
        float compensationRoll = loadResponse != null ? -loadResponse.CompensationLocal.x * 42f : 0f;

        PoseCapsule(hips, hipCenter,
            transform.rotation * Quaternion.Euler(0f, 0f, -strafe * 4f + flop * 15f + loadSway * 18f),
            new Vector3(0.53f, 0.20f, 0.40f));

        PoseCapsule(torso, chestCenter,
            transform.rotation * Quaternion.Euler(forwardLean + compensationPitch, 0f, -strafe * 2f + compensationRoll + flop * 10f),
            new Vector3(0.66f, 0.39f, 0.48f));

        Vector3 bellyCenter = chestCenter + transform.TransformVector(new Vector3(0f, -0.20f, 0.105f));
        PoseBlock(belly, bellyCenter, torso.rotation, new Vector3(0.63f, 0.48f, 0.48f));

        Vector3 bibCenter = chestCenter + transform.TransformVector(new Vector3(0f, -0.06f, 0.245f));
        PoseBlock(overallBib, bibCenter, torso.rotation, new Vector3(0.40f, 0.34f, 0.045f));
        PoseBlock(leftStrap, chestCenter + transform.TransformVector(new Vector3(-0.18f, 0.17f, 0.235f)), torso.rotation * Quaternion.Euler(0f, 0f, -8f), new Vector3(0.055f, 0.30f, 0.035f));
        PoseBlock(rightStrap, chestCenter + transform.TransformVector(new Vector3(0.18f, 0.17f, 0.235f)), torso.rotation * Quaternion.Euler(0f, 0f, 8f), new Vector3(0.055f, 0.30f, 0.035f));

        Vector3 leftHip = hipCenter + transform.right * -0.18f;
        Vector3 rightHip = hipCenter + transform.right * 0.18f;

        float plantedY = 0.08f;
        Vector3 leftFoot = transform.TransformPoint(new Vector3(-0.20f, plantedY, step * 0.21f + strafe * -0.05f));
        Vector3 rightFoot = transform.TransformPoint(new Vector3(0.20f, plantedY, -step * 0.21f + strafe * 0.05f));
        leftFoot.y += liftLeft;
        rightFoot.y += liftRight;

        if (!grounded)
        {
            leftFoot += transform.forward * 0.10f;
            rightFoot -= transform.forward * 0.04f;
        }
        if (stun > 0.01f)
        {
            leftFoot += transform.right * flop;
            rightFoot -= transform.right * flop;
        }

        float tremble = extreme * Mathf.Sin(Time.time * 18f) * 0.025f;
        leftFoot += transform.right * tremble;
        rightFoot -= transform.right * tremble;

        PoseLeg(leftHip, leftFoot, leftUpperLeg, leftLowerLeg, leftBoot, -1f, kneeBend, loadSway);
        PoseLeg(rightHip, rightFoot, rightUpperLeg, rightLowerLeg, rightBoot, 1f, kneeBend, -loadSway);

        Vector3 leftShoulder = chestCenter + transform.TransformVector(new Vector3(-0.37f, 0.20f, 0.01f));
        Vector3 rightShoulder = chestCenter + transform.TransformVector(new Vector3(0.37f, 0.20f, 0.01f));

        GetHandTarget(true, leftShoulder, step, flop, load, out Vector3 leftRaw, out Vector3 leftNormal);
        GetHandTarget(false, rightShoulder, -step, -flop, load, out Vector3 rightRaw, out Vector3 rightNormal);

        float handFollow = Mathf.Lerp(20f, 8f, load);
        leftHandVisual = SmoothHand(leftHandVisual, ref leftHandInitialized, leftRaw, handFollow);
        rightHandVisual = SmoothHand(rightHandVisual, ref rightHandInitialized, rightRaw, handFollow);

        PoseArm(leftShoulder, leftHandVisual, leftUpperArm, leftLowerArm, leftHand, -1f, leftGripping, leftNormal, load);
        PoseArm(rightShoulder, rightHandVisual, rightUpperArm, rightLowerArm, rightHand, 1f, rightGripping, rightNormal, load);
    }

    private Vector3 SmoothHand(Vector3 current, ref bool initialized, Vector3 target, float follow)
    {
        if (!initialized)
        {
            initialized = true;
            return target;
        }
        return Vector3.Lerp(current, target, 1f - Mathf.Exp(-follow * Time.deltaTime));
    }

    private void GetHandTarget(bool left, Vector3 shoulder, float step, float flop, float load, out Vector3 target, out Vector3 contactNormal)
    {
        bool gripping = left ? leftGripping : rightGripping;
        if (gripping && mover.TryGetVisualHandContact(left, out target, out contactNormal))
        {
            // Heavy objects pull the hands slightly downward while preserving actual collider contact.
            target += Vector3.down * (loadResponse != null ? loadResponse.HandDrop * 0.12f : 0f);
            return;
        }

        float side = left ? -1f : 1f;
        Vector3 local = new Vector3(side * 0.39f, 0.95f - crouch * 0.34f, 0.03f - step * 0.16f);
        if (carrying) local += new Vector3(0f, 0.05f - load * 0.12f, 0.24f);
        target = transform.TransformPoint(local) + transform.right * flop;
        contactNormal = transform.forward;
    }

    private void PoseArm(Vector3 shoulder, Vector3 handPosition, Transform upper, Transform lower, Transform hand, float side, bool gripping, Vector3 contactNormal, float load)
    {
        float maxReach = Mathf.Lerp(1.43f, 1.52f, load);
        Vector3 shoulderToHand = handPosition - shoulder;
        if (shoulderToHand.magnitude > maxReach)
            handPosition = shoulder + shoulderToHand.normalized * maxReach;

        Vector3 elbowHint = transform.right * side * Mathf.Lerp(0.25f, 0.16f, load)
            + Vector3.down * Mathf.Lerp(0.15f, 0.26f, load)
            + transform.forward * 0.06f;
        Vector3 midpoint = Vector3.Lerp(shoulder, handPosition, 0.50f) + elbowHint;

        PoseLimb(upper, shoulder, midpoint, Mathf.Lerp(0.155f, 0.17f, load));
        PoseLimb(lower, midpoint, handPosition, 0.14f);

        Quaternion handRotation;
        if (gripping)
        {
            Vector3 forward = -contactNormal;
            if (forward.sqrMagnitude < 0.001f) forward = transform.forward;
            handRotation = Quaternion.LookRotation(forward.normalized, transform.up) * Quaternion.Euler(0f, side * 16f, side * 8f);
        }
        else handRotation = transform.rotation * Quaternion.Euler(15f, 0f, side * 10f);

        PoseBlock(hand, handPosition, handRotation, new Vector3(0.22f, 0.16f, 0.25f));
    }

    private void PoseLeg(Vector3 hip, Vector3 foot, Transform upper, Transform lower, Transform boot, float side, float kneeBend, float sway)
    {
        Vector3 knee = Vector3.Lerp(hip, foot, 0.48f)
            + transform.forward * (0.14f + kneeBend)
            + transform.right * (side * 0.035f + sway * 0.22f);

        PoseLimb(upper, hip, knee, 0.19f);
        PoseLimb(lower, knee, foot + Vector3.up * 0.09f, 0.155f);
        PoseBlock(boot, foot + transform.forward * 0.075f, transform.rotation * Quaternion.Euler(0f, side * 2f, 0f), new Vector3(0.29f, 0.18f, 0.43f));
    }

    private static void PoseLimb(Transform limb, Vector3 start, Vector3 end, float thickness)
    {
        Vector3 direction = end - start;
        float length = Mathf.Max(0.02f, direction.magnitude);
        limb.position = (start + end) * 0.5f;
        limb.rotation = Quaternion.FromToRotation(Vector3.up, direction.normalized);
        limb.localScale = new Vector3(thickness, length * 0.5f, thickness);
    }

    private static void PoseCapsule(Transform part, Vector3 position, Quaternion rotation, Vector3 scale)
    {
        part.position = position;
        part.rotation = rotation;
        part.localScale = scale;
    }

    private static void PoseBlock(Transform part, Vector3 position, Quaternion rotation, Vector3 scale)
    {
        part.position = position;
        part.rotation = rotation;
        part.localScale = scale;
    }
}
