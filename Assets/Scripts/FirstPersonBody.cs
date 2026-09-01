using UnityEngine;
using UnityEngine.Rendering;

public sealed class FirstPersonBody : MonoBehaviour
{
    private Transform firstPersonRoot;
    private Transform firstPersonTorsoRoot;
    private Transform externalRoot;
    private Transform externalTorsoRoot;
    private Transform externalHeadRoot;

    private Transform fpLeftUpperArm;
    private Transform fpLeftLowerArm;
    private Transform fpRightUpperArm;
    private Transform fpRightLowerArm;
    private Transform fpLeftHand;
    private Transform fpRightHand;
    private Transform fpLeftUpperLeg;
    private Transform fpLeftLowerLeg;
    private Transform fpRightUpperLeg;
    private Transform fpRightLowerLeg;
    private Transform fpLeftBoot;
    private Transform fpRightBoot;

    private Transform extLeftUpperArm;
    private Transform extLeftLowerArm;
    private Transform extRightUpperArm;
    private Transform extRightLowerArm;
    private Transform extLeftHand;
    private Transform extRightHand;
    private Transform extLeftUpperLeg;
    private Transform extLeftLowerLeg;
    private Transform extRightUpperLeg;
    private Transform extRightLowerLeg;
    private Transform extLeftBoot;
    private Transform extRightBoot;

    private Material skinMaterial;
    private Material orangeMaterial;
    private Material overallsMaterial;
    private Material gloveMaterial;
    private Material bootMaterial;
    private Material hairMaterial;
    private Material moustacheMaterial;
    private Material goggleFrameMaterial;
    private Material goggleLensMaterial;
    private Material reflectiveMaterial;
    private Material eyeWhiteMaterial;
    private Material pupilMaterial;

    private PlayerMover mover;
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
        BuildMaterials();
        BuildFirstPersonBody();
        BuildExternalMover();
        ApplyVisibility();
    }

    private void Start() => mover = GetComponent<PlayerMover>();

    private void BuildMaterials()
    {
        skinMaterial = CreateMaterial(new Color(0.83f, 0.49f, 0.29f), 0.28f);
        orangeMaterial = CreateMaterial(new Color(0.96f, 0.24f, 0.025f), 0.36f);
        overallsMaterial = CreateMaterial(new Color(0.035f, 0.115f, 0.23f), 0.28f);
        gloveMaterial = CreateMaterial(new Color(0.075f, 0.055f, 0.04f), 0.26f);
        bootMaterial = CreateMaterial(new Color(0.055f, 0.045f, 0.038f), 0.22f);
        hairMaterial = CreateMaterial(new Color(0.17f, 0.095f, 0.05f), 0.25f);
        moustacheMaterial = CreateMaterial(new Color(0.13f, 0.065f, 0.035f), 0.22f);
        goggleFrameMaterial = CreateMaterial(new Color(0.12f, 0.14f, 0.15f), 0.46f);
        reflectiveMaterial = CreateMaterial(new Color(0.78f, 0.79f, 0.73f), 0.58f);
        eyeWhiteMaterial = CreateMaterial(new Color(0.92f, 0.90f, 0.82f), 0.34f);
        pupilMaterial = CreateMaterial(new Color(0.055f, 0.07f, 0.065f), 0.30f);
        goggleLensMaterial = CreateTransparentMaterial(new Color(0.65f, 0.78f, 0.80f, 0.38f), 0.82f);
    }

    private void BuildFirstPersonBody()
    {
        firstPersonRoot = new GameObject("First-person worker body").transform;
        firstPersonRoot.SetParent(transform, false);

        firstPersonTorsoRoot = new GameObject("Visible torso").transform;
        firstPersonTorsoRoot.SetParent(firstPersonRoot, false);

        Shape(firstPersonTorsoRoot, "Orange shirt", PrimitiveType.Sphere, orangeMaterial,
            new Vector3(0f, 1.24f, 0.02f), new Vector3(0.74f, 0.54f, 0.48f));
        Shape(firstPersonTorsoRoot, "Rounded overalls belly", PrimitiveType.Sphere, overallsMaterial,
            new Vector3(0f, 0.98f, 0.03f), new Vector3(0.84f, 0.70f, 0.58f));
        Shape(firstPersonTorsoRoot, "Overalls bib", PrimitiveType.Cube, overallsMaterial,
            new Vector3(0f, 1.24f, 0.30f), new Vector3(0.48f, 0.34f, 0.055f));
        Shape(firstPersonTorsoRoot, "Left suspender", PrimitiveType.Cube, overallsMaterial,
            new Vector3(-0.23f, 1.39f, 0.27f), new Vector3(0.075f, 0.38f, 0.045f), Quaternion.Euler(-7f, 0f, -7f));
        Shape(firstPersonTorsoRoot, "Right suspender", PrimitiveType.Cube, overallsMaterial,
            new Vector3(0.23f, 1.39f, 0.27f), new Vector3(0.075f, 0.38f, 0.045f), Quaternion.Euler(-7f, 0f, 7f));
        Shape(firstPersonTorsoRoot, "Reflective chest stripe", PrimitiveType.Cube, reflectiveMaterial,
            new Vector3(0f, 1.34f, 0.333f), new Vector3(0.54f, 0.075f, 0.024f));

        fpLeftUpperArm = Part(firstPersonRoot, "Left orange upper arm", PrimitiveType.Capsule, orangeMaterial);
        fpLeftLowerArm = Part(firstPersonRoot, "Left orange forearm", PrimitiveType.Capsule, orangeMaterial);
        fpRightUpperArm = Part(firstPersonRoot, "Right orange upper arm", PrimitiveType.Capsule, orangeMaterial);
        fpRightLowerArm = Part(firstPersonRoot, "Right orange forearm", PrimitiveType.Capsule, orangeMaterial);
        fpLeftHand = Part(firstPersonRoot, "Left work glove", PrimitiveType.Sphere, gloveMaterial);
        fpRightHand = Part(firstPersonRoot, "Right work glove", PrimitiveType.Sphere, gloveMaterial);
        fpLeftUpperLeg = Part(firstPersonRoot, "Left overalls thigh", PrimitiveType.Capsule, overallsMaterial);
        fpLeftLowerLeg = Part(firstPersonRoot, "Left overalls shin", PrimitiveType.Capsule, overallsMaterial);
        fpRightUpperLeg = Part(firstPersonRoot, "Right overalls thigh", PrimitiveType.Capsule, overallsMaterial);
        fpRightLowerLeg = Part(firstPersonRoot, "Right overalls shin", PrimitiveType.Capsule, overallsMaterial);
        fpLeftBoot = Part(firstPersonRoot, "Left safety boot", PrimitiveType.Capsule, bootMaterial);
        fpRightBoot = Part(firstPersonRoot, "Right safety boot", PrimitiveType.Capsule, bootMaterial);
    }

    private void BuildExternalMover()
    {
        externalRoot = new GameObject("Rounded procedural mover").transform;
        externalRoot.SetParent(transform, false);

        externalTorsoRoot = new GameObject("Body mass").transform;
        externalTorsoRoot.SetParent(externalRoot, false);

        Shape(externalTorsoRoot, "Orange work shirt", PrimitiveType.Sphere, orangeMaterial,
            new Vector3(0f, 1.25f, 0f), new Vector3(0.82f, 0.62f, 0.60f));
        Shape(externalTorsoRoot, "Large overalls belly", PrimitiveType.Sphere, overallsMaterial,
            new Vector3(0f, 0.98f, 0f), new Vector3(0.92f, 0.76f, 0.68f));
        Shape(externalTorsoRoot, "Overalls bib", PrimitiveType.Cube, overallsMaterial,
            new Vector3(0f, 1.24f, 0.345f), new Vector3(0.54f, 0.38f, 0.055f));
        Shape(externalTorsoRoot, "Left front suspender", PrimitiveType.Cube, overallsMaterial,
            new Vector3(-0.25f, 1.40f, 0.31f), new Vector3(0.08f, 0.40f, 0.05f), Quaternion.Euler(-7f, 0f, -8f));
        Shape(externalTorsoRoot, "Right front suspender", PrimitiveType.Cube, overallsMaterial,
            new Vector3(0.25f, 1.40f, 0.31f), new Vector3(0.08f, 0.40f, 0.05f), Quaternion.Euler(-7f, 0f, 8f));
        Shape(externalTorsoRoot, "Front reflective stripe", PrimitiveType.Cube, reflectiveMaterial,
            new Vector3(0f, 1.34f, 0.378f), new Vector3(0.62f, 0.08f, 0.026f));
        Shape(externalTorsoRoot, "Back reflective stripe", PrimitiveType.Cube, reflectiveMaterial,
            new Vector3(0f, 1.34f, -0.378f), new Vector3(0.62f, 0.08f, 0.026f));
        Shape(externalTorsoRoot, "Left reflective shoulder", PrimitiveType.Cube, reflectiveMaterial,
            new Vector3(-0.31f, 1.46f, 0.16f), new Vector3(0.055f, 0.30f, 0.035f), Quaternion.Euler(-10f, 0f, -18f));
        Shape(externalTorsoRoot, "Right reflective shoulder", PrimitiveType.Cube, reflectiveMaterial,
            new Vector3(0.31f, 1.46f, 0.16f), new Vector3(0.055f, 0.30f, 0.035f), Quaternion.Euler(-10f, 0f, 18f));

        Shape(externalTorsoRoot, "Neck", PrimitiveType.Capsule, skinMaterial,
            new Vector3(0f, 1.46f, 0f), new Vector3(0.25f, 0.20f, 0.25f));

        externalHeadRoot = new GameObject("Head and PPE").transform;
        externalHeadRoot.SetParent(externalTorsoRoot, false);
        externalHeadRoot.localPosition = new Vector3(0f, 1.53f, 0f);

        Shape(externalHeadRoot, "Head", PrimitiveType.Sphere, skinMaterial,
            new Vector3(0f, 0.02f, 0.015f), new Vector3(0.58f, 0.50f, 0.52f));
        Shape(externalHeadRoot, "Left ear", PrimitiveType.Sphere, skinMaterial,
            new Vector3(-0.31f, 0.00f, 0.01f), new Vector3(0.15f, 0.19f, 0.12f));
        Shape(externalHeadRoot, "Right ear", PrimitiveType.Sphere, skinMaterial,
            new Vector3(0.31f, 0.00f, 0.01f), new Vector3(0.15f, 0.19f, 0.12f));
        Shape(externalHeadRoot, "Large rounded nose", PrimitiveType.Sphere, skinMaterial,
            new Vector3(0f, -0.01f, 0.286f), new Vector3(0.19f, 0.16f, 0.19f));
        Shape(externalHeadRoot, "Left moustache", PrimitiveType.Sphere, moustacheMaterial,
            new Vector3(-0.085f, -0.085f, 0.278f), new Vector3(0.21f, 0.085f, 0.085f), Quaternion.Euler(0f, 0f, -12f));
        Shape(externalHeadRoot, "Right moustache", PrimitiveType.Sphere, moustacheMaterial,
            new Vector3(0.085f, -0.085f, 0.278f), new Vector3(0.21f, 0.085f, 0.085f), Quaternion.Euler(0f, 0f, 12f));

        Shape(externalHeadRoot, "Hair cap", PrimitiveType.Sphere, hairMaterial,
            new Vector3(0f, 0.21f, -0.015f), new Vector3(0.50f, 0.16f, 0.47f));
        Shape(externalHeadRoot, "Hair tuft left", PrimitiveType.Sphere, hairMaterial,
            new Vector3(-0.12f, 0.29f, 0.02f), new Vector3(0.14f, 0.10f, 0.15f), Quaternion.Euler(0f, 0f, -18f));
        Shape(externalHeadRoot, "Hair tuft center", PrimitiveType.Sphere, hairMaterial,
            new Vector3(0.00f, 0.31f, 0.00f), new Vector3(0.14f, 0.10f, 0.15f));
        Shape(externalHeadRoot, "Hair tuft right", PrimitiveType.Sphere, hairMaterial,
            new Vector3(0.12f, 0.29f, -0.01f), new Vector3(0.14f, 0.10f, 0.15f), Quaternion.Euler(0f, 0f, 18f));
        Shape(externalHeadRoot, "Back hair", PrimitiveType.Sphere, hairMaterial,
            new Vector3(0f, 0.06f, -0.24f), new Vector3(0.49f, 0.24f, 0.16f));

        Shape(externalHeadRoot, "Left eye white", PrimitiveType.Sphere, eyeWhiteMaterial,
            new Vector3(-0.13f, 0.055f, 0.266f), new Vector3(0.12f, 0.082f, 0.032f));
        Shape(externalHeadRoot, "Right eye white", PrimitiveType.Sphere, eyeWhiteMaterial,
            new Vector3(0.13f, 0.055f, 0.266f), new Vector3(0.12f, 0.082f, 0.032f));
        Shape(externalHeadRoot, "Left pupil", PrimitiveType.Sphere, pupilMaterial,
            new Vector3(-0.13f, 0.047f, 0.285f), new Vector3(0.043f, 0.043f, 0.016f));
        Shape(externalHeadRoot, "Right pupil", PrimitiveType.Sphere, pupilMaterial,
            new Vector3(0.13f, 0.047f, 0.285f), new Vector3(0.043f, 0.043f, 0.016f));

        Shape(externalHeadRoot, "Left safety lens", PrimitiveType.Sphere, goggleLensMaterial,
            new Vector3(-0.145f, 0.055f, 0.302f), new Vector3(0.27f, 0.18f, 0.026f));
        Shape(externalHeadRoot, "Right safety lens", PrimitiveType.Sphere, goggleLensMaterial,
            new Vector3(0.145f, 0.055f, 0.302f), new Vector3(0.27f, 0.18f, 0.026f));
        Shape(externalHeadRoot, "Goggle bridge", PrimitiveType.Cube, goggleFrameMaterial,
            new Vector3(0f, 0.055f, 0.321f), new Vector3(0.075f, 0.03f, 0.026f));

        Shape(externalHeadRoot, "Left goggle top", PrimitiveType.Cube, goggleFrameMaterial,
            new Vector3(-0.145f, 0.137f, 0.320f), new Vector3(0.29f, 0.022f, 0.022f));
        Shape(externalHeadRoot, "Left goggle bottom", PrimitiveType.Cube, goggleFrameMaterial,
            new Vector3(-0.145f, -0.027f, 0.320f), new Vector3(0.29f, 0.022f, 0.022f));
        Shape(externalHeadRoot, "Left goggle outer", PrimitiveType.Cube, goggleFrameMaterial,
            new Vector3(-0.285f, 0.055f, 0.320f), new Vector3(0.022f, 0.18f, 0.022f));
        Shape(externalHeadRoot, "Left goggle inner", PrimitiveType.Cube, goggleFrameMaterial,
            new Vector3(-0.005f, 0.055f, 0.320f), new Vector3(0.022f, 0.18f, 0.022f));

        Shape(externalHeadRoot, "Right goggle top", PrimitiveType.Cube, goggleFrameMaterial,
            new Vector3(0.145f, 0.137f, 0.320f), new Vector3(0.29f, 0.022f, 0.022f));
        Shape(externalHeadRoot, "Right goggle bottom", PrimitiveType.Cube, goggleFrameMaterial,
            new Vector3(0.145f, -0.027f, 0.320f), new Vector3(0.29f, 0.022f, 0.022f));
        Shape(externalHeadRoot, "Right goggle outer", PrimitiveType.Cube, goggleFrameMaterial,
            new Vector3(0.285f, 0.055f, 0.320f), new Vector3(0.022f, 0.18f, 0.022f));
        Shape(externalHeadRoot, "Right goggle inner", PrimitiveType.Cube, goggleFrameMaterial,
            new Vector3(0.005f, 0.055f, 0.320f), new Vector3(0.022f, 0.18f, 0.022f));

        Shape(externalHeadRoot, "Left goggle arm", PrimitiveType.Cube, goggleFrameMaterial,
            new Vector3(-0.305f, 0.065f, 0.09f), new Vector3(0.045f, 0.055f, 0.42f));
        Shape(externalHeadRoot, "Right goggle arm", PrimitiveType.Cube, goggleFrameMaterial,
            new Vector3(0.305f, 0.065f, 0.09f), new Vector3(0.045f, 0.055f, 0.42f));

        extLeftUpperArm = Part(externalRoot, "Left orange upper arm", PrimitiveType.Capsule, orangeMaterial);
        extLeftLowerArm = Part(externalRoot, "Left orange forearm", PrimitiveType.Capsule, orangeMaterial);
        extRightUpperArm = Part(externalRoot, "Right orange upper arm", PrimitiveType.Capsule, orangeMaterial);
        extRightLowerArm = Part(externalRoot, "Right orange forearm", PrimitiveType.Capsule, orangeMaterial);
        extLeftHand = Part(externalRoot, "Left oversized work glove", PrimitiveType.Sphere, gloveMaterial);
        extRightHand = Part(externalRoot, "Right oversized work glove", PrimitiveType.Sphere, gloveMaterial);
        extLeftUpperLeg = Part(externalRoot, "Left overalls thigh", PrimitiveType.Capsule, overallsMaterial);
        extLeftLowerLeg = Part(externalRoot, "Left overalls shin", PrimitiveType.Capsule, overallsMaterial);
        extRightUpperLeg = Part(externalRoot, "Right overalls thigh", PrimitiveType.Capsule, overallsMaterial);
        extRightLowerLeg = Part(externalRoot, "Right overalls shin", PrimitiveType.Capsule, overallsMaterial);
        extLeftBoot = Part(externalRoot, "Left safety boot", PrimitiveType.Capsule, bootMaterial);
        extRightBoot = Part(externalRoot, "Right safety boot", PrimitiveType.Capsule, bootMaterial);
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
        if (thirdPersonPreview) AnimateExternalMover();
        else AnimateFirstPersonBody();
    }

    private void AnimateFirstPersonBody()
    {
        float carrySag = carrying ? 0.075f : 0f;
        float hipDrop = crouch * 0.34f + carrySag;
        float shoulderDrop = crouch * 0.32f + carrySag;
        float bodyDrop = crouch * 0.32f + carrySag;
        float bodyLean = carrying ? 7f : 0f;
        float stunRoll = stun * Mathf.Sin(Time.time * 10f) * 7f;

        firstPersonTorsoRoot.localPosition = new Vector3(0f, -bodyDrop, 0f);
        firstPersonTorsoRoot.localRotation = Quaternion.Euler(bodyLean + crouch * 5f, 0f, -strafe * 1.6f + stunRoll);

        float step = grounded ? Mathf.Sin(gait) * movement : 0f;
        float jumpBend = airborne * Mathf.Clamp01(0.75f - verticalSpeed * 0.04f);
        float liftLeft = grounded ? Mathf.Max(0f, Mathf.Sin(gait)) * 0.14f * movement : 0.15f + jumpBend * 0.19f;
        float liftRight = grounded ? Mathf.Max(0f, -Mathf.Sin(gait)) * 0.14f * movement : 0.11f + jumpBend * 0.23f;
        float flop = stun * Mathf.Sin(Time.time * 10f) * 0.22f;

        Vector3 leftHip = transform.TransformPoint(new Vector3(-0.19f, 0.79f - hipDrop, 0.03f));
        Vector3 rightHip = transform.TransformPoint(new Vector3(0.19f, 0.79f - hipDrop, 0.03f));
        Vector3 leftFoot = transform.TransformPoint(new Vector3(-0.19f, 0.09f, step * 0.24f + strafe * -0.05f));
        Vector3 rightFoot = transform.TransformPoint(new Vector3(0.19f, 0.09f, -step * 0.24f + strafe * 0.05f));
        leftFoot.y += liftLeft;
        rightFoot.y += liftRight;

        if (!grounded)
        {
            leftFoot += transform.forward * (0.09f - jumpBend * 0.08f);
            rightFoot += transform.forward * (-0.03f + jumpBend * 0.11f);
        }
        if (stun > 0.01f)
        {
            leftFoot += transform.right * flop;
            rightFoot -= transform.right * flop;
        }

        PoseLeg(leftHip, leftFoot, fpLeftUpperLeg, fpLeftLowerLeg, fpLeftBoot, -1f, 0.215f, 0.19f, 0.23f);
        PoseLeg(rightHip, rightFoot, fpRightUpperLeg, fpRightLowerLeg, fpRightBoot, 1f, 0.215f, 0.19f, 0.23f);

        Vector3 leftShoulder = transform.TransformPoint(new Vector3(-0.36f, 1.39f - shoulderDrop, 0.03f));
        Vector3 rightShoulder = transform.TransformPoint(new Vector3(0.36f, 1.39f - shoulderDrop, 0.03f));
        PoseArm(leftShoulder, HandTarget(true, step, flop), fpLeftUpperArm, fpLeftLowerArm, fpLeftHand, -1f, leftGripping, 0.19f, 0.175f, 0.23f);
        PoseArm(rightShoulder, HandTarget(false, -step, -flop), fpRightUpperArm, fpRightLowerArm, fpRightHand, 1f, rightGripping, 0.19f, 0.175f, 0.23f);
    }

    private void AnimateExternalMover()
    {
        float carrySag = carrying ? 0.08f : 0f;
        float drop = crouch * 0.32f + carrySag;
        float bodyLean = carrying ? 9f : 0f;
        float stunRoll = stun * Mathf.Sin(Time.time * 9f) * 8f;
        float breathing = Mathf.Sin(Time.time * 2.2f) * 0.006f;

        externalTorsoRoot.localPosition = new Vector3(0f, -drop + breathing, 0f);
        externalTorsoRoot.localRotation = Quaternion.Euler(bodyLean + crouch * 8f, 0f, -strafe * 2.4f + stunRoll);
        externalHeadRoot.localRotation = Quaternion.Euler(-bodyLean * 0.28f - crouch * 3f, strafe * 2f, strafe * -2f - stunRoll * 0.25f);

        float step = grounded ? Mathf.Sin(gait) * movement : 0f;
        float stride = sprinting ? 0.33f : 0.25f;
        float jumpBend = airborne * Mathf.Clamp01(0.75f - verticalSpeed * 0.04f);
        float liftLeft = grounded ? Mathf.Max(0f, Mathf.Sin(gait)) * 0.16f * movement : 0.13f + jumpBend * 0.20f;
        float liftRight = grounded ? Mathf.Max(0f, -Mathf.Sin(gait)) * 0.16f * movement : 0.10f + jumpBend * 0.24f;

        Vector3 leftHip = transform.TransformPoint(new Vector3(-0.20f, 0.79f - drop * 0.88f, 0f));
        Vector3 rightHip = transform.TransformPoint(new Vector3(0.20f, 0.79f - drop * 0.88f, 0f));
        Vector3 leftFoot = transform.TransformPoint(new Vector3(-0.20f, 0.10f, step * stride + strafe * -0.05f));
        Vector3 rightFoot = transform.TransformPoint(new Vector3(0.20f, 0.10f, -step * stride + strafe * 0.05f));
        leftFoot.y += liftLeft;
        rightFoot.y += liftRight;

        if (!grounded)
        {
            leftFoot += transform.forward * (0.08f - jumpBend * 0.08f);
            rightFoot += transform.forward * (-0.03f + jumpBend * 0.11f);
        }

        PoseLeg(leftHip, leftFoot, extLeftUpperLeg, extLeftLowerLeg, extLeftBoot, -1f, 0.24f, 0.205f, 0.25f);
        PoseLeg(rightHip, rightFoot, extRightUpperLeg, extRightLowerLeg, extRightBoot, 1f, 0.24f, 0.205f, 0.25f);

        Vector3 leftShoulder = transform.TransformPoint(new Vector3(-0.39f, 1.39f - drop, 0.02f));
        Vector3 rightShoulder = transform.TransformPoint(new Vector3(0.39f, 1.39f - drop, 0.02f));
        float armSwing = Mathf.Sin(gait) * movement * (sprinting ? 0.28f : 0.20f);

        Vector3 leftHandTarget;
        Vector3 rightHandTarget;
        if (carrying)
        {
            leftHandTarget = transform.TransformPoint(new Vector3(-0.28f, 1.00f - drop, 0.58f));
            rightHandTarget = transform.TransformPoint(new Vector3(0.28f, 1.00f - drop, 0.58f));
        }
        else
        {
            leftHandTarget = transform.TransformPoint(new Vector3(-0.43f, 0.90f - drop, -armSwing));
            rightHandTarget = transform.TransformPoint(new Vector3(0.43f, 0.90f - drop, armSwing));
        }

        PoseArm(leftShoulder, leftHandTarget, extLeftUpperArm, extLeftLowerArm, extLeftHand, -1f, carrying, 0.215f, 0.195f, 0.255f);
        PoseArm(rightShoulder, rightHandTarget, extRightUpperArm, extRightLowerArm, extRightHand, 1f, carrying, 0.215f, 0.195f, 0.255f);
    }

    private Vector3 HandTarget(bool left, float step, float flop)
    {
        Transform grip = left ? mover.LeftGripPoint : mover.RightGripPoint;
        bool gripping = left ? leftGripping : rightGripping;
        if (gripping && grip != null) return grip.position;

        float side = left ? -1f : 1f;
        Vector3 local = new Vector3(side * 0.38f, 0.94f - crouch * 0.31f - (carrying ? 0.06f : 0f), 0.08f - step * 0.18f);
        if (airborne > 0.01f) local += new Vector3(0f, -0.03f, 0.10f);
        if (carrying) local += new Vector3(side * -0.07f, 0.10f, 0.31f);
        return transform.TransformPoint(local) + transform.right * flop;
    }

    private void PoseArm(
        Vector3 shoulder,
        Vector3 handPosition,
        Transform upper,
        Transform lower,
        Transform hand,
        float side,
        bool gripping,
        float upperThickness,
        float lowerThickness,
        float handSize)
    {
        float reach = Vector3.Distance(shoulder, handPosition);
        if (reach > 1.18f) handPosition = shoulder + (handPosition - shoulder).normalized * 1.18f;
        Vector3 elbow = Vector3.Lerp(shoulder, handPosition, 0.49f)
            + transform.right * side * 0.23f
            + Vector3.down * 0.17f
            + transform.forward * (gripping ? 0.04f : 0f);

        PoseLimb(upper, shoulder, elbow, upperThickness);
        PoseLimb(lower, elbow, handPosition, lowerThickness);
        PoseBlock(hand, handPosition, transform.rotation * Quaternion.Euler(gripping ? 68f : 12f, 0f, side * 11f),
            new Vector3(handSize, handSize * 0.82f, handSize * 1.03f));
    }

    private void PoseLeg(
        Vector3 hip,
        Vector3 foot,
        Transform upper,
        Transform lower,
        Transform boot,
        float side,
        float upperThickness,
        float lowerThickness,
        float bootSize)
    {
        float crouchKnee = crouch * 0.20f + airborne * 0.08f + (carrying ? 0.055f : 0f);
        Vector3 knee = Vector3.Lerp(hip, foot, 0.48f)
            + transform.forward * (0.14f + crouchKnee)
            + transform.right * side * 0.025f;

        PoseLimb(upper, hip, knee, upperThickness);
        PoseLimb(lower, knee, foot + Vector3.up * 0.085f, lowerThickness);
        PoseBlock(boot, foot + transform.forward * 0.10f, transform.rotation * Quaternion.Euler(90f, 0f, 0f),
            new Vector3(bootSize, bootSize * 1.48f, bootSize * 0.86f));
    }

    private Transform Part(Transform parent, string objectName, PrimitiveType primitive, Material material)
    {
        GameObject part = GameObject.CreatePrimitive(primitive);
        part.name = objectName;
        part.transform.SetParent(parent, false);
        Renderer renderer = part.GetComponent<Renderer>();
        renderer.sharedMaterial = material;
        renderer.shadowCastingMode = ShadowCastingMode.On;
        renderer.receiveShadows = true;
        Collider collider = part.GetComponent<Collider>();
        if (collider != null) Destroy(collider);
        return part.transform;
    }

    private Transform Shape(
        Transform parent,
        string objectName,
        PrimitiveType primitive,
        Material material,
        Vector3 localPosition,
        Vector3 localScale,
        Quaternion? localRotation = null)
    {
        Transform part = Part(parent, objectName, primitive, material);
        part.localPosition = localPosition;
        part.localRotation = localRotation ?? Quaternion.identity;
        part.localScale = localScale;
        return part;
    }

    private static Material CreateMaterial(Color color, float smoothness)
    {
        Shader shader = Shader.Find("Standard");
        if (shader == null) shader = Shader.Find("Universal Render Pipeline/Lit");
        if (shader == null) shader = Shader.Find("Unlit/Color");

        Material material = new Material(shader);
        material.color = color;
        if (material.HasProperty("_BaseColor")) material.SetColor("_BaseColor", color);
        if (material.HasProperty("_Smoothness")) material.SetFloat("_Smoothness", smoothness);
        if (material.HasProperty("_Glossiness")) material.SetFloat("_Glossiness", smoothness);
        if (material.HasProperty("_Metallic")) material.SetFloat("_Metallic", 0f);
        return material;
    }

    private static Material CreateTransparentMaterial(Color color, float smoothness)
    {
        Material material = CreateMaterial(color, smoothness);
        material.color = color;
        if (material.HasProperty("_BaseColor")) material.SetColor("_BaseColor", color);

        if (material.HasProperty("_Mode"))
        {
            material.SetFloat("_Mode", 3f);
            material.SetInt("_SrcBlend", (int)BlendMode.SrcAlpha);
            material.SetInt("_DstBlend", (int)BlendMode.OneMinusSrcAlpha);
            material.SetInt("_ZWrite", 0);
            material.DisableKeyword("_ALPHATEST_ON");
            material.EnableKeyword("_ALPHABLEND_ON");
            material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            material.renderQueue = 3000;
        }
        if (material.HasProperty("_Surface"))
        {
            material.SetFloat("_Surface", 1f);
            material.SetFloat("_Blend", 0f);
            material.renderQueue = 3000;
        }
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
}
