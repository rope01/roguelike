using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

public sealed class CharacterAnimationDriver : MonoBehaviour
{
    private enum Motion
    {
        Idle,
        Walk,
        Run,
        Jump,
        Crouch
    }

    private PlayableGraph graph;
    private AnimationMixerPlayable mixer;
    private AnimationClipPlayable[] playables;
    private AnimationClip[] clips;
    private float[] weights;
    private bool initialized;
    private Motion currentMotion;
    private float movement;
    private float verticalSpeed;
    private bool grounded = true;
    private bool crouched;
    private bool sprinting;
    private bool carrying;

    public void Initialize(Animator animator, AnimationClip[] importedClips)
    {
        if (animator == null || importedClips == null || importedClips.Length == 0)
        {
            Debug.LogError("Mover animation clips were not imported from the character FBX.");
            return;
        }

        clips = new[]
        {
            FindClip(importedClips, "Man_Idle"),
            FindClip(importedClips, "Man_Walk"),
            FindClip(importedClips, "Man_Run"),
            FindClip(importedClips, "Man_Jump"),
            FindClip(importedClips, "Man_Sitting")
        };

        AnimationClip fallback = clips[(int)Motion.Idle];
        if (fallback == null)
        {
            foreach (AnimationClip candidate in importedClips)
            {
                if (candidate == null || candidate.name.StartsWith("__preview__")) continue;
                fallback = candidate;
                break;
            }
        }
        if (fallback == null) return;
        for (int i = 0; i < clips.Length; i++) if (clips[i] == null) clips[i] = fallback;

        animator.runtimeAnimatorController = null;
        animator.applyRootMotion = false;
        animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;

        graph = PlayableGraph.Create("Mover animation graph");
        graph.SetTimeUpdateMode(DirectorUpdateMode.GameTime);
        mixer = AnimationMixerPlayable.Create(graph, clips.Length);
        playables = new AnimationClipPlayable[clips.Length];
        weights = new float[clips.Length];

        for (int i = 0; i < clips.Length; i++)
        {
            playables[i] = AnimationClipPlayable.Create(graph, clips[i]);
            playables[i].SetApplyFootIK(i != (int)Motion.Jump);
            graph.Connect(playables[i], 0, mixer, i);
            mixer.SetInputWeight(i, i == (int)Motion.Idle ? 1f : 0f);
            weights[i] = i == (int)Motion.Idle ? 1f : 0f;
        }

        AnimationPlayableOutput output = AnimationPlayableOutput.Create(graph, "Mover animation", animator);
        output.SetSourcePlayable(mixer);
        graph.Play();
        initialized = true;
    }

    public void SetState(float moveAmount, bool isGrounded, bool isCrouched, bool isSprinting, float ySpeed, bool isCarrying)
    {
        movement = moveAmount;
        grounded = isGrounded;
        crouched = isCrouched;
        sprinting = isSprinting;
        verticalSpeed = ySpeed;
        carrying = isCarrying;
    }

    private void Update()
    {
        if (!initialized) return;

        Motion target = ChooseMotion();
        if (target != currentMotion)
        {
            if (target == Motion.Jump || target == Motion.Crouch) playables[(int)target].SetTime(0d);
            currentMotion = target;
        }

        for (int i = 0; i < weights.Length; i++)
        {
            float targetWeight = i == (int)currentMotion ? 1f : 0f;
            weights[i] = Mathf.MoveTowards(weights[i], targetWeight, Time.deltaTime * 7.5f);
            mixer.SetInputWeight(i, weights[i]);
        }

        UpdatePlaybackSpeeds();
    }

    private Motion ChooseMotion()
    {
        if (!grounded) return Motion.Jump;
        if (crouched) return Motion.Crouch;
        if (movement < 0.08f) return Motion.Idle;
        if (sprinting && !carrying) return Motion.Run;
        return Motion.Walk;
    }

    private void UpdatePlaybackSpeeds()
    {
        playables[(int)Motion.Idle].SetSpeed(1d);
        playables[(int)Motion.Walk].SetSpeed(Mathf.Lerp(0.72f, 1.18f, movement));
        playables[(int)Motion.Run].SetSpeed(Mathf.Lerp(0.92f, 1.22f, movement));
        playables[(int)Motion.Jump].SetSpeed(verticalSpeed < -0.5f ? 0.72d : 1d);

        Loop(Motion.Idle);
        Loop(Motion.Walk);
        Loop(Motion.Run);

        AnimationClip crouchClip = clips[(int)Motion.Crouch];
        double crouchHold = Mathf.Min(0.52f, crouchClip.length * 0.45f);
        playables[(int)Motion.Crouch].SetSpeed(playables[(int)Motion.Crouch].GetTime() >= crouchHold ? 0d : 1d);
    }

    private void Loop(Motion motion)
    {
        int index = (int)motion;
        double length = clips[index].length;
        if (length > 0d && playables[index].GetTime() >= length)
            playables[index].SetTime(playables[index].GetTime() % length);
    }

    private static AnimationClip FindClip(AnimationClip[] candidates, string namePart)
    {
        foreach (AnimationClip clip in candidates)
            if (clip != null && clip.name.Contains(namePart)) return clip;
        return null;
    }

    private void OnDestroy()
    {
        if (graph.IsValid()) graph.Destroy();
    }
}
