namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>The state every late-fight phase reads and writes: the body function and its timer, the neck and head programs and the brain's flags, with the walking and posture requests they share.</summary>
internal sealed class MotherBrainLateFightState
{

    /// <summary>Builds the component from the collaborators it uses.</summary>
    internal MotherBrainLateFightState()
    {
    }

    /// <summary>
    /// Body enemy-slot animation state. The caller advances its enemy-instruction stage
    /// after <see cref="MotherBrainRainbowBeamAttackSequence.Step"/>, matching the AI-then-instruction order.
    /// </summary>
    public MotherBrainBodyAnimationState Body { get; } = new();

    /// <summary>Current body-function equivalent.</summary>
    public MotherBrainBodyFunction? Phase { get; internal set; }

    /// <summary>Mother Brain brain-slot room position used by the final beam's aim vector.</summary>
    public Position16<RoomPixels> BrainPosition { get; set; }

    /// <summary>Current brain program installed through the brain-program request, or null before the first request.</summary>
    public MotherBrainHeadProgram? HeadProgram { get; private set; }

    /// <summary>Neck angular speed in 8.8 angle units; every producer writes a positive speed of at most 2304.</summary>
    public int NeckAngleDelta { get; internal set; }

    /// <summary>Whether articulated neck angle dispatch runs on the brain's turn.</summary>
    public bool NeckMovementEnabled { get; internal set; }

    /// <summary>The lower neck movement selected by the current actor phase.</summary>
    public MotherBrainNeckMovement LowerNeckMovement { get; internal set; }

    /// <summary>The upper neck movement selected by the current actor phase.</summary>
    public MotherBrainNeckMovement UpperNeckMovement { get; internal set; }

    /// <summary>Body function timer.</summary>
    public int FunctionTimer { get; internal set; }

    /// <summary>Rainbow-beam SFX-playing flag.</summary>
    public bool RainbowBeamSoundPlaying { get; internal set; }

    /// <summary>Main brain-shake timer seeded to fifty by the rainbow step when clear.</summary>
    public int BrainMainShakeTimer { get; internal set; }

    /// <summary>Drool generation flag cleared as Mother Brain enters low-power mode.</summary>
    public bool DroolGenerationEnabled { get; internal set; } = true;

    /// <summary>Small-purple-breath flag cleared when the grey corpse is published.</summary>
    public bool SmallPurpleBreathGenerationEnabled { get; internal set; } = true;

    /// <summary>
    /// Count of body-AI writes to <see cref="SmallPurpleBreathGenerationEnabled"/>. The live
    /// flag has a second writer, a brain-list step, so the live state
    /// publishes this flag only when the body actually wrote it.
    /// </summary>
    public uint SmallPurpleBreathGenerationWriteCount { get; internal set; }

    /// <summary>Stops the small purple breath, counting the write as <see cref="MotherBrainRevivalPhases.EnableSmallPurpleBreathGeneration"/> does.</summary>
    internal void DisableSmallPurpleBreathGeneration()
    {
        SmallPurpleBreathGenerationEnabled = false;
        SmallPurpleBreathGenerationWriteCount++;
    }

    /// <summary>
    /// Whether Mother Brain's whole body shows her health band's colours. Revival enables it only after
    /// the walk to X 80 has really completed, and disabling her brain's effects as she dies ends it.
    /// </summary>
    public bool HealthColoursEnabled { get; internal set; }

    /// <summary>Palette-transition record index.</summary>
    public int GreyTransitionCounter { get; internal set; }

    /// <summary>
    /// Cross-actor corpse handshake polled by the Baby: <see cref="MotherBrainPhase2CorpseState.Corpse"/> once
    /// Mother Brain has completed the ninth grey-table probe, then <see cref="MotherBrainPhase2CorpseState.Revived"/>.
    /// </summary>
    public MotherBrainPhase2CorpseState Phase2CorpseState { get; internal set; }

    /// <summary>Brain-slot health rewritten to 36,000 when corpse state one is published.</summary>
    public int BrainHealth { get; internal set; } = 3000;

    /// <summary>
    /// The updates the finish-off has spent readying the Baby Metroid's arrival so far.
    /// <see cref="MotherBrainReadyingDefinitions.BabyArrivalDuration"/> means it has arrived and the spawn handoff has run.
    /// </summary>
    public int BabyArrivalReadyingElapsed { get; internal set; }

    /// <summary>True once the final beam's Baby spawn has requested the cutscene Baby enemy population entry.</summary>
    public bool BabyMetroidSpawned { get; internal set; }

    /// <summary>Restarts the separate brain instruction stream and publishes a new head-list request serial.</summary>
    internal void SetHeadProgram(MotherBrainHeadProgram program)
    {
        // The brain-program request restarts the separate brain program and its timer. Unlike ordinary
        // enemy programs it has no loop-counter write here.
        HeadProgram = program;
        HeadAnimationScriptRequestSerial++;
    }

    /// <summary>Sets the NTSC neck angle and opposed raise/lower indices for retreat after the attack.</summary>
    internal void RetractHead()
    {
        // NTSC takes 80 from the regional constant pair of 80 and 99.
        NeckAngleDelta = 80;
        NeckMovementEnabled = true;
        LowerNeckMovement = MotherBrainNeckMovement.Raise;
        UpperNeckMovement = MotherBrainNeckMovement.Lower;
    }

    /// <summary>Selects the forward body list for the animation delay and applies the forward target rules.</summary>
    internal MotherBrainWalkResult RequestWalkForward(int targetX, int animationDelay)
    {
        MotherBrainBodyProgram program = animationDelay switch
        {
            2 => MotherBrainBodyProgram.WalkForwardReallyFast,
            4 => MotherBrainBodyProgram.WalkForwardFast,
            6 => MotherBrainBodyProgram.WalkForwardMedium,
            8 => MotherBrainBodyProgram.WalkForwardSlow,
            10 => MotherBrainBodyProgram.WalkForwardReallySlow,
            _ => throw new InvalidOperationException(
                $"Unsupported painful forward animation delay {animationDelay}."),
        };
        return MakeBodyWalkForwards(targetX, program);
    }

    /// <summary>Selects the backward body list for the animation delay and applies the backward target rules.</summary>
    internal MotherBrainWalkResult RequestWalkBackward(int targetX, int animationDelay)
    {
        MotherBrainBodyProgram program = animationDelay switch
        {
            2 => MotherBrainBodyProgram.WalkBackwardReallyFast,
            4 => MotherBrainBodyProgram.WalkBackwardFast,
            6 => MotherBrainBodyProgram.WalkBackwardMedium,
            8 => MotherBrainBodyProgram.WalkBackwardSlow,
            10 => MotherBrainBodyProgram.WalkBackwardReallySlow,
            _ => throw new InvalidOperationException(
                $"Unsupported painful backward animation delay {animationDelay}."),
        };
        return MakeBodyWalkBackwards(targetX, program);
    }

    /// <summary>Requests the slow backward body animation until its target is reached.</summary>
    internal MotherBrainWalkResult RequestWalkBackwardReallySlow(int targetX) =>
        MakeBodyWalkBackwards(targetX, MotherBrainBodyProgram.WalkBackwardReallySlow);

    /// <summary>
    /// Makes the body walk forwards. A signed overshoot of the target reports carry first; at
    /// equality the walk is still requested. A body still mid-pose then reports no carry,
    /// ahead of the 128 arena limit that also reports carry. Only a standing body
    /// inside both limits installs the walk.
    /// </summary>
    internal MotherBrainWalkResult MakeBodyWalkForwards(int targetX, MotherBrainBodyProgram program)
    {
        if (CoordinateAxis.Difference(targetX, Body.Position.X) < 0)
            return MotherBrainWalkResult.Reached;
        if (Body.Pose != MotherBrainBodyPose.Standing)
            return MotherBrainWalkResult.Waiting;
        if (AtLeastBySignedDifference(Body.Position.X, 128))
            return MotherBrainWalkResult.Reached;
        Body.SetProgram(program);
        return MotherBrainWalkResult.WalkInstalled;
    }

    /// <summary>
    /// Makes the body walk backwards. Reaching or passing the target reports carry first, so a walk
    /// can complete the AI phase on the frame its movement step arrives. A body still
    /// mid-pose then reports no carry, ahead of the 48 arena limit that also reports
    /// carry. Only a standing body inside both limits installs the walk.
    /// </summary>
    internal MotherBrainWalkResult MakeBodyWalkBackwards(int targetX, MotherBrainBodyProgram program)
    {
        if (CoordinateAxis.Difference(targetX, Body.Position.X) >= 0)
            return MotherBrainWalkResult.Reached;
        if (Body.Pose != MotherBrainBodyPose.Standing)
            return MotherBrainWalkResult.Waiting;
        if (CoordinateAxis.Difference(Body.Position.X, 48) < 0)
            return MotherBrainWalkResult.Reached;
        Body.SetProgram(program);
        return MotherBrainWalkResult.WalkInstalled;
    }

    /// <summary>Takes the posture chance (<see cref="MotherBrainRandomDefinitions.PostureChanges"/>) to toggle a standing or leaning body list.</summary>
    internal bool MaybeRequestStandUpOrLeanDown(MotherBrainRandomSample randomNumberSeed)
    {
        // The lean helper ignores every pose except standing (zero) and leaning (six). A request still executes as body
        // instruction bytecode later in the enemy-processing stage.
        if (!randomNumberSeed.Takes(MotherBrainRandomDefinitions.PostureChanges))
            return false;

        if (Body.Pose == MotherBrainBodyPose.Standing)
        {
            Body.SetProgram(MotherBrainBodyProgram.LeanDown);
            return true;
        }

        if (Body.Pose == MotherBrainBodyPose.LeaningDown)
        {
            MakeBodyStandUp(out bool requested);
            return requested;
        }

        return false;
    }

    /// <summary>Requests the pose-specific standing animation and reports whether the body is already standing.</summary>
    internal bool MakeBodyStandUp(out bool animationRequested)
    {
        animationRequested = false;
        if (Body.Pose == MotherBrainBodyPose.Standing)
            return true;

        MotherBrainBodyProgram? program = Body.Pose switch
        {
            MotherBrainBodyPose.Crouched => MotherBrainBodyProgram.StandAfterCrouchFast,
            MotherBrainBodyPose.LeaningDown => MotherBrainBodyProgram.StandAfterLeaning,
            MotherBrainBodyPose.Standing or MotherBrainBodyPose.Walking or
                MotherBrainBodyPose.CrouchingTransition or MotherBrainBodyPose.DeathBeam => null,
            _ => throw new InvalidOperationException($"Undefined {nameof(MotherBrainBodyPose)} {Body.Pose}."),
        };
        if (program is { } standUp)
        {
            Body.SetProgram(standUp);
            animationRequested = true;
        }

        return false;
    }

    /// <summary>Whether <paramref name="value"/> is at least <paramref name="threshold"/> by the sign of their wrapped difference, as the original game compares positions.</summary>
    internal static bool AtLeastBySignedDifference(int value, int threshold) =>
        CoordinateAxis.Difference(value, threshold) >= 0;

    /// <summary>How many head programs the phases have requested; a change marks a new request.</summary>
    internal uint HeadAnimationScriptRequestSerial { get; private set; }
}
