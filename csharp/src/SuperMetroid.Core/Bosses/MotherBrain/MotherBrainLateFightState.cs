namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>The state every late-fight phase reads and writes: the body function and its timer, the neck and head programs, the brain's flags and the earthquake, with the walking and posture requests they share.</summary>
internal sealed class MotherBrainLateFightState
{

    /// <summary>Builds the component from the collaborators it uses.</summary>
    internal MotherBrainLateFightState()
    {
    }

    /// <summary>
    /// Body enemy-slot animation state. The caller advances its enemy-instruction stage
    /// after <see cref="MotherBrainRainbowBeamAttackSequence.Step"/>, matching the native AI-then-instruction order.
    /// </summary>
    public MotherBrainBodyAnimationState Body { get; } = new();

    /// <summary>Current body-function equivalent.</summary>
    public MotherBrainBodyFunction? Phase { get; internal set; }

    /// <summary>Mother Brain brain-slot room position used by `$A9:BB82/$BB8F`'s aim vector.</summary>
    public Position16<RoomPixels> BrainPosition { get; set; }

    /// <summary>Current brain program installed through `$A9:C447`, or null before the first request.</summary>
    public MotherBrainHeadProgram? HeadProgram { get; private set; }

    /// <summary>Neck angular speed in 8.8 angle units (Mother Brain body extra word `$0FBC`); every producer writes a positive speed of at most $0900.</summary>
    public int NeckAngleDelta { get; internal set; }

    /// <summary>Whether articulated neck angle dispatch runs on the brain's turn.</summary>
    public bool NeckMovementEnabled { get; internal set; }

    /// <summary>Lower neck movement index selected by the current actor phase.</summary>
    public MotherBrainNeckMovement LowerNeckMovementIndex { get; internal set; }

    /// <summary>Upper neck movement index selected by the current actor phase.</summary>
    public MotherBrainNeckMovement UpperNeckMovementIndex { get; internal set; }

    /// <summary>Native body function timer at enemy word <c>$0FB0</c>.</summary>
    public NativeCountdown FunctionTimer { get; internal set; }

    /// <summary>Native rainbow-beam SFX-playing flag.</summary>
    public bool RainbowBeamSoundPlaying { get; internal set; }

    /// <summary>Main brain-shake timer seeded to fifty by <c>$A9:BE96</c> when clear.</summary>
    public NativeCountdown BrainMainShakeTimer { get; internal set; }

    /// <summary>Native brain-palette handling flag cleared when the rainbow beam expires.</summary>
    public bool BrainPaletteHandlingEnabled { get; internal set; } = true;

    /// <summary>Drool generation flag cleared as Mother Brain enters low-power mode.</summary>
    public bool DroolGenerationEnabled { get; internal set; } = true;

    /// <summary>Small-purple-breath flag cleared when the grey corpse is published.</summary>
    public bool SmallPurpleBreathGenerationEnabled { get; internal set; } = true;

    /// <summary>
    /// Count of body-AI writes to <see cref="SmallPurpleBreathGenerationEnabled"/>. The live
    /// flag has a second native writer, the brain-list opcode $A9:9F8E, so the live state
    /// publishes this flag only when the body actually wrote it.
    /// </summary>
    public uint SmallPurpleBreathGenerationWriteCount { get; internal set; }

    /// <summary>Stops the small purple breath, counting the native write as <see cref="MotherBrainRevivalPhases.EnableSmallPurpleBreathGeneration"/> does.</summary>
    internal void DisableSmallPurpleBreathGeneration()
    {
        SmallPurpleBreathGenerationEnabled = false;
        SmallPurpleBreathGenerationWriteCount++;
    }

    /// <summary>
    /// Native Mother Brain health-based body-palette flag. Revival writes one only after
    /// the walk to X `$50` has really completed; it is not synonymous with the separate
    /// brain-slot palette handler above.
    /// </summary>
    public bool HealthBasedPaletteHandlingEnabled { get; internal set; }

    /// <summary>Palette-transition record index at WRAM <c>$7E:802E</c>.</summary>
    public int GreyTransitionCounter { get; internal set; }

    /// <summary>
    /// Cross-actor corpse handshake polled by the Baby: <see cref="MotherBrainPhase2CorpseState.Corpse"/> once
    /// Mother Brain has completed the ninth grey-table probe, then <see cref="MotherBrainPhase2CorpseState.Revived"/>.
    /// </summary>
    public MotherBrainPhase2CorpseState Phase2CorpseState { get; internal set; }

    /// <summary>Brain-slot health rewritten to 36,000 when corpse state one is published.</summary>
    public int BrainHealth { get; internal set; } = 3000;

    /// <summary>
    /// Index of the room shake this actor last requested (0, 2, 5 or 8), written by the one-frame
    /// delay, the drain initializer and the death sequence; the room earthquake owns the shake.
    /// </summary>
    public int EarthquakeType { get; internal set; }

    /// <summary>
    /// Earthquake timer as last written by this actor. The global earthquake updater owns
    /// independent per-frame decrements, so this class does not fabricate them.
    /// </summary>
    public NativeCountdown EarthquakeTimer { get; internal set; }

    /// <summary>
    /// Zero-based index of the next Baby Metroid sprite-tile transfer. Four means the
    /// terminating zero entry has been observed and the spawn handoff has run.
    /// </summary>
    public int BabyMetroidTileTransferIndex { get; internal set; }

    /// <summary>True once `$A9:BE1B` has requested the cutscene Baby enemy population entry.</summary>
    public bool BabyMetroidSpawned { get; internal set; }

    /// <summary>Restarts the separate brain instruction stream and publishes a new head-list request serial.</summary>
    internal void SetHeadProgram(MotherBrainHeadProgram program)
    {
        // `$A9:C447` restarts the separate brain program and its timer. Unlike ordinary
        // enemy programs it has no loop-counter write here.
        HeadProgram = program;
        HeadInstructionListRequestSerial++;
    }

    /// <summary>Sets the NTSC neck angle and opposed raise/lower indices for retreat after the attack.</summary>
    internal void RetractHead()
    {
        // NTSC takes `$0050` from the regional `$0050/$0063` constant at `$A9:BB51`.
        NeckAngleDelta = 80;
        NeckMovementEnabled = true;
        LowerNeckMovementIndex = MotherBrainNeckMovement.Raise;
        UpperNeckMovementIndex = MotherBrainNeckMovement.Lower;
    }

    /// <summary>Selects the forward body list for the native animation delay and applies the forward target rules.</summary>
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
                $"Unsupported painful forward animation delay ${animationDelay:X4}."),
        };
        return MakeBodyWalkForwards(targetX, program);
    }

    /// <summary>Selects the backward body list for the native animation delay and applies the backward target rules.</summary>
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
                $"Unsupported painful backward animation delay ${animationDelay:X4}."),
        };
        return MakeBodyWalkBackwards(targetX, program);
    }

    /// <summary>Requests the slow backward body animation until its target is reached.</summary>
    internal MotherBrainWalkResult RequestWalkBackwardReallySlow(int targetX) =>
        MakeBodyWalkBackwards(targetX, MotherBrainBodyProgram.WalkBackwardReallySlow);

    /// <summary>
    /// Ports <c>$A9:C601</c>. A signed overshoot of the target reports carry first; at
    /// equality the walk is still requested. A body still mid-pose then reports no carry,
    /// ahead of the <c>$80</c> arena limit that also reports carry. Only a standing body
    /// inside both limits installs the walk.
    /// </summary>
    internal MotherBrainWalkResult MakeBodyWalkForwards(int targetX, MotherBrainBodyProgram program)
    {
        if (unchecked((short)(targetX - Body.Position.X)) < 0)
            return MotherBrainWalkResult.Reached;
        if (Body.Pose != MotherBrainBodyPose.Standing)
            return MotherBrainWalkResult.Waiting;
        if (NativeAtLeast(Body.Position.X, 128))
            return MotherBrainWalkResult.Reached;
        Body.SetProgram(program);
        return MotherBrainWalkResult.WalkInstalled;
    }

    /// <summary>
    /// Ports <c>$A9:C647</c>. Reaching or passing the target reports carry first, so a walk
    /// can complete the AI phase on the frame its movement opcode arrives. A body still
    /// mid-pose then reports no carry, ahead of the <c>$30</c> arena limit that also reports
    /// carry. Only a standing body inside both limits installs the walk.
    /// </summary>
    internal MotherBrainWalkResult MakeBodyWalkBackwards(int targetX, MotherBrainBodyProgram program)
    {
        if (unchecked((short)(targetX - Body.Position.X)) >= 0)
            return MotherBrainWalkResult.Reached;
        if (Body.Pose != MotherBrainBodyPose.Standing)
            return MotherBrainWalkResult.Waiting;
        if (unchecked((short)(Body.Position.X - 48)) < 0)
            return MotherBrainWalkResult.Reached;
        Body.SetProgram(program);
        return MotherBrainWalkResult.WalkInstalled;
    }

    /// <summary>Uses the random low byte at or above $C0 to toggle a standing or leaning body list.</summary>
    internal bool MaybeRequestStandUpOrLeanDown(MotherBrainRandomSample randomNumberSeed)
    {
        // `$A9:C1A7` ignores every pose except standing (zero) and leaning (six), and its
        // low-byte threshold is inclusive at `$C0`. A request still executes as body
        // instruction bytecode later in the enemy-processing stage.
        if (randomNumberSeed.LowByte < 192)
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
            _ => throw new InvalidOperationException($"Undefined {nameof(MotherBrainBodyPose)} {(int)Body.Pose}."),
        };
        if (program is { } standUp)
        {
            Body.SetProgram(standUp);
            animationRequested = true;
        }

        return false;
    }

    /// <summary>Performs the cartridge's signed 16-bit subtraction test for a lower bound.</summary>
    internal static bool NativeAtLeast(ushort value, ushort threshold) =>
        unchecked((short)(value - threshold)) >= 0;

    /// <summary>How many head instruction lists the phases have requested; a change marks a new request.</summary>
    internal uint HeadInstructionListRequestSerial { get; private set; }
}
