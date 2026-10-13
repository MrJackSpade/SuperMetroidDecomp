using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;
using B = SuperMetroid.Core.Game.MotherBrainBodyProgram;
using H = SuperMetroid.Core.Game.MotherBrainHeadProgram;

namespace SuperMetroid.Core.Game;

/// <summary>
/// Mother Brain's body programs, declared in the order the cartridge authors them in bank $A9. The body enemy slot
/// runs them, and so does the head slot, which keeps <see cref="InitialDummy"/> (its frame being its hitbox) for life.
/// </summary>
public enum MotherBrainBodyProgram
{
    /// <summary><c>InstList_MotherBrainBody_WalkingForwards_ReallyFast</c> (<c>$A9:9730</c>).</summary>
    WalkForwardReallyFast,
    /// <summary><c>InstList_MotherBrainBody_WalkingForwards_Fast</c> (<c>$A9:976A</c>).</summary>
    WalkForwardFast,
    /// <summary><c>InstList_MotherBrainBody_WalkingForwards_Medium</c> (<c>$A9:97A4</c>).</summary>
    WalkForwardMedium,
    /// <summary><c>InstList_MotherBrainBody_WalkingForwards_Slow</c> (<c>$A9:97DE</c>).</summary>
    WalkForwardSlow,
    /// <summary><c>InstList_MotherBrainBody_WalkingForwards_ReallySlow</c> (<c>$A9:9818</c>).</summary>
    WalkForwardReallySlow,
    /// <summary><c>InstList_MotherBrainBody_WalkingBackwards_Slow</c> (<c>$A9:9852</c>).</summary>
    WalkBackwardSlow,
    /// <summary><c>InstList_MotherBrainBody_WalkingBackwards_ReallyFast</c> (<c>$A9:988C</c>).</summary>
    WalkBackwardReallyFast,
    /// <summary><c>InstList_MotherBrainBody_WalkingBackwards_Fast</c> (<c>$A9:98C6</c>).</summary>
    WalkBackwardFast,
    /// <summary><c>InstList_MotherBrainBody_WalkingBackwards_Medium</c> (<c>$A9:9900</c>).</summary>
    WalkBackwardMedium,
    /// <summary><c>InstList_MotherBrainBody_WalkingBackwards_ReallySlow</c> (<c>$A9:993A</c>).</summary>
    WalkBackwardReallySlow,
    /// <summary><c>InstList_MotherBrainBody_CrouchAndThenStandUp</c> (<c>$A9:9974</c>).</summary>
    CrouchAndThenStandUp,
    /// <summary><c>InstList_MotherBrainBody_StandingUpAfterCrouching_Slow</c> (<c>$A9:99AA</c>).</summary>
    StandAfterCrouchSlow,
    /// <summary><c>InstList_MotherBrainBody_StandingUpAfterCrouching_Fast</c> (<c>$A9:99C6</c>).</summary>
    StandAfterCrouchFast,
    /// <summary><c>InstList_MotherBrainBody_StandingUpAfterLeaningDown</c> (<c>$A9:99E2</c>).</summary>
    StandAfterLeaning,
    /// <summary><c>InstList_MotherBrainBody_LeaningDown</c> (<c>$A9:99F2</c>).</summary>
    LeanDown,
    /// <summary><c>InstList_MotherBrainBody_Crouched</c> (<c>$A9:9A02</c>).</summary>
    Crouched,
    /// <summary><c>InstList_MotherBrainBody_Crouch_Slow</c> (<c>$A9:9A0A</c>).</summary>
    CrouchSlow,
    /// <summary><c>InstList_MotherBrainBody_Crouch_Fast</c> (<c>$A9:9A26</c>).</summary>
    CrouchFast,
    /// <summary>The red hand-beam body program (<c>$A9:9A42</c>).</summary>
    HandBeam,
    /// <summary><c>InstList_MotherBrainHead_InitialDummy</c> (<c>$A9:9C13</c>), shared by the body and head slots.</summary>
    InitialDummy,
}

/// <summary>One of Mother Brain's body poses, an index into <see cref="MotherBrainBodyVisualDefinitions"/>.</summary>
/// <param name="Index">The pose's position in the body-frame catalog.</param>
public readonly record struct MotherBrainBodyFrame(int Index)
{
    /// <summary>Standing upright.</summary>
    internal static MotherBrainBodyFrame Standing => new(0);
    /// <summary>The crouched endpoint.</summary>
    internal static MotherBrainBodyFrame Crouched => new(9);
    /// <summary>Halfway between crouched and leaning down.</summary>
    internal static MotherBrainBodyFrame Uncrouching => new(10);
    /// <summary>Leaning down.</summary>
    internal static MotherBrainBodyFrame LeaningDown => new(11);
    /// <summary>The initial dummy's hitbox frame.</summary>
    internal static MotherBrainBodyFrame InitialDummy => new(12);

    /// <summary>Walking pose <paramref name="step"/> of eight.</summary>
    internal static MotherBrainBodyFrame Walking(int step) => new(1 + Ensure.BetweenInclusive(0, 7, step));

    /// <summary>Hand-beam pose <paramref name="step"/> of four.</summary>
    internal static MotherBrainBodyFrame HandBeam(int step) => new(13 + Ensure.BetweenInclusive(0, 3, step));

    /// <summary>The installed extended spritemap the pose shows.</summary>
    internal ushort Spritemap => MotherBrainBodyVisualDefinitions.Frame(Index).Pointer;
}

/// <summary>What a Mother Brain body cue does.</summary>
internal enum MotherBrainBodyCueKind
{
    /// <summary>Publishes a body pose to the AI.</summary>
    SetPose,
    /// <summary>Moves the body, keeping BG2 aligned with it.</summary>
    Move,
    /// <summary>Plants a foot (shaking the room), then moves the body.</summary>
    StepAndMove,
    /// <summary>Moves the body vertically, counter-scrolling BG2 with a horizontal bias.</summary>
    MoveWithScrollBias,
    /// <summary>Spawns a charge particle at the hand.</summary>
    SpawnHandDust,
    /// <summary>Fires the red hand beam at Samus.</summary>
    FireHandBeam,
    /// <summary>Advances the hand-beam attack phase.</summary>
    AdvanceHandBeamPhase,
}

/// <summary>A gameplay cue of Mother Brain's body programs.</summary>
/// <param name="Kind">What the cue does.</param>
/// <param name="Pose">The pose a <see cref="MotherBrainBodyCueKind.SetPose"/> cue publishes.</param>
/// <param name="Offset">The body displacement, hand offset, or vertical move with its scroll bias as X.</param>
/// <param name="Dust">The particle a hand-dust cue spawns.</param>
internal readonly record struct MotherBrainBodyCue(
    MotherBrainBodyCueKind Kind, MotherBrainBodyPose Pose, Offset16<RoomPixels> Offset, MiscDustAnimation Dust)
{
    /// <summary>Publishes <paramref name="pose"/>.</summary>
    internal static MotherBrainBodyCue SetPose(MotherBrainBodyPose pose) => new(MotherBrainBodyCueKind.SetPose, pose, default, default);
    /// <summary>Moves the body by <paramref name="offset"/>.</summary>
    internal static MotherBrainBodyCue Move(Offset16<RoomPixels> offset) => new(MotherBrainBodyCueKind.Move, default, offset, default);
    /// <summary>Plants a foot, then moves the body by <paramref name="offset"/>.</summary>
    internal static MotherBrainBodyCue StepAndMove(Offset16<RoomPixels> offset) => new(MotherBrainBodyCueKind.StepAndMove, default, offset, default);
    /// <summary>Moves the body down by <paramref name="y"/>, scrolling BG2 with <paramref name="scrollBias"/>.</summary>
    internal static MotherBrainBodyCue MoveWithScrollBias(short y, short scrollBias) =>
        new(MotherBrainBodyCueKind.MoveWithScrollBias, default, new(scrollBias, y), default);
    /// <summary>Spawns <paramref name="dust"/> at <paramref name="offset"/> from the body.</summary>
    internal static MotherBrainBodyCue SpawnHandDust(Offset16<RoomPixels> offset, MiscDustAnimation dust) =>
        new(MotherBrainBodyCueKind.SpawnHandDust, default, offset, dust);
    /// <summary>Fires the red hand beam.</summary>
    internal static MotherBrainBodyCue FireHandBeam { get; } = new(MotherBrainBodyCueKind.FireHandBeam, default, default, default);
    /// <summary>Advances the hand-beam attack phase.</summary>
    internal static MotherBrainBodyCue AdvanceHandBeamPhase { get; } = new(MotherBrainBodyCueKind.AdvanceHandBeamPhase, default, default, default);
}

/// <summary>
/// Mother Brain's brain (head) programs in bank $A9 order, run by her dedicated brain list rather than an enemy
/// slot. The unreachable frames after the initial dummy and the unused flash at <c>$A9:9C7F</c> and
/// <c>$A9:9CE7</c> are not authored.
/// </summary>
public enum MotherBrainHeadProgram
{
    /// <summary><c>InstList_MotherBrainHead_Stretching_Phase2_0</c> (<c>$A9:9B7F</c>).</summary>
    StretchingPhase2,
    /// <summary><c>InstList_MotherBrainHead_Stretching_Phase2_1</c> (<c>$A9:9BAB</c>).</summary>
    StretchingPhase2Loop,
    /// <summary><c>InstList_MotherBrainHead_Stretching_Phase3_0</c> (<c>$A9:9BB3</c>).</summary>
    StretchingPhase3,
    /// <summary><c>InstList_MotherBrainHead_Stretching_Phase3_1</c> (<c>$A9:9BDF</c>).</summary>
    StretchingPhase3Loop,
    /// <summary><c>InstList_MotherBrainHead_HyperBeamRecoil_0</c> (<c>$A9:9BE7</c>).</summary>
    HyperBeamRecoil,
    /// <summary><c>InstList_MotherBrainHead_HyperBeamRecoil_1</c> (<c>$A9:9C0B</c>).</summary>
    HyperBeamRecoilLoop,
    /// <summary><c>InstList_MotherBrainHead_Initial</c> (<c>$A9:9C21</c>).</summary>
    Initial,
    /// <summary><c>InstList_MotherBrainHead_Decapitated_0</c> (<c>$A9:9C29</c>).</summary>
    Decapitated,
    /// <summary><c>InstList_MotherBrainHead_Decapitated_1</c> (<c>$A9:9C31</c>).</summary>
    DecapitatedLoop,
    /// <summary><c>InstList_MotherBrainHead_DyingDrool_0</c> (<c>$A9:9C39</c>).</summary>
    DyingDrool,
    /// <summary><c>InstList_MotherBrainHead_DyingDrool_1</c> (<c>$A9:9C47</c>).</summary>
    DyingDroolBurst,
    /// <summary><c>InstList_MotherBrainHead_DyingDrool_2</c> (<c>$A9:9C5F</c>).</summary>
    DyingDroolLoop,
    /// <summary><c>InstList_MotherBrainHead_FiringRainbowBeam</c> (<c>$A9:9C77</c>).</summary>
    FiringRainbowBeam,
    /// <summary><c>InstList_MotherBrainHead_Neutral_Phase2_0</c> (<c>$A9:9C87</c>).</summary>
    NeutralPhase2,
    /// <summary><c>InstList_MotherBrainHead_Neutral_Phase2_1</c> (<c>$A9:9C9F</c>).</summary>
    NeutralPhase2Hold,
    /// <summary><c>InstList_MotherBrainHead_Neutral_Phase3_0</c> (<c>$A9:9CB9</c>).</summary>
    NeutralPhase3,
    /// <summary><c>InstList_MotherBrainHead_Neutral_Phase3_1</c> (<c>$A9:9CD1</c>).</summary>
    NeutralPhase3Hold,
    /// <summary><c>InstList_MotherBrainHead_Corpse_0</c> (<c>$A9:9D25</c>).</summary>
    Corpse,
    /// <summary><c>InstList_MotherBrainHead_Corpse_1</c> (<c>$A9:9D35</c>).</summary>
    CorpseLoop,
    /// <summary><c>InstList_MotherBrainHead_Attacking_4OnionRings_Phase2</c> (<c>$A9:9D3D</c>).</summary>
    FourOnionRingsPhase2,
    /// <summary><c>InstList_MotherBrainHead_Attacking_2OnionRings_Phase2</c> (<c>$A9:9D7F</c>).</summary>
    TwoOnionRingsPhase2,
    /// <summary><c>InstList_MotherBrainHead_Attacking_BabyMetroid</c> (<c>$A9:9DB1</c>).</summary>
    AttackingBabyMetroid,
    /// <summary><c>InstList_MotherBrainHead_AttackingSamus_4OnionRings_Phase3</c> (<c>$A9:9DBB</c>).</summary>
    AttackingSamusFourOnionRingsPhase3,
    /// <summary><c>InstList_MotherBrainHead_Attacking_4OnionRings_Phase3</c> (<c>$A9:9DC1</c>).</summary>
    FourOnionRingsPhase3,
    /// <summary><c>InstList_MotherBrainHead_Attacking_Bomb_Phase2</c> (<c>$A9:9ECC</c>).</summary>
    BombPhase2,
    /// <summary><c>InstList_MotherBrainHead_Attacking_Bomb_Phase3</c> (<c>$A9:9F00</c>).</summary>
    BombPhase3,
    /// <summary><c>InstList_MotherBrainHead_Attacking_Laser</c> (<c>$A9:9F34</c>).</summary>
    Laser,
    /// <summary><c>InstList_MotherBrainHead_ChargingRainbowBeam_0</c> (<c>$A9:9F6C</c>).</summary>
    ChargingRainbowBeam,
    /// <summary><c>InstList_MotherBrainHead_ChargingRainbowBeam_1</c> (<c>$A9:9F7A</c>).</summary>
    ChargingRainbowBeamLoop,
}

/// <summary>One brain frame, an index into <see cref="MotherBrainVisualDefinitions"/>.</summary>
/// <param name="Index">The frame's position in the Mother Brain composition catalog.</param>
public readonly record struct MotherBrainHeadFrame(int Index)
{
    /// <summary>The installed spritemap the frame shows.</summary>
    internal ushort Spritemap => MotherBrainVisualDefinitions.Frame(Index).Pointer;
}

/// <summary>What a Mother Brain brain cue does.</summary>
internal enum MotherBrainHeadCueKind
{
    /// <summary>Shakes the brain for fifty updates.</summary>
    ShakeBrain,
    /// <summary>Spawns a drool drop.</summary>
    SpawnDrool,
    /// <summary>Spawns a big purple breath cloud.</summary>
    SpawnBigPurpleBreath,
    /// <summary>Queues a library-two sound.</summary>
    QueueSound,
    /// <summary>Queues a library-three sound.</summary>
    QueueLibrary3Sound,
    /// <summary>Freezes the neck.</summary>
    DisableNeckMovement,
    /// <summary>Lets the neck move again.</summary>
    EnableNeckMovement,
    /// <summary>Aims the onion rings at Samus.</summary>
    AimOnionRingsAtSamus,
    /// <summary>Aims the onion rings at the Baby Metroid.</summary>
    AimOnionRingsAtBabyMetroid,
    /// <summary>Fires an onion ring along the aimed angle.</summary>
    SpawnOnionRing,
    /// <summary>Lobs a bomb with its afterburn count.</summary>
    SpawnBomb,
    /// <summary>Fires the mouth laser.</summary>
    SpawnLaser,
    /// <summary>Counts an attack on the Baby Metroid.</summary>
    CountBabyMetroidAttack,
    /// <summary>Resets the count of attacks on the Baby Metroid.</summary>
    ResetBabyMetroidAttacks,
    /// <summary>Queues the attack cry unless the Baby Metroid attack has run its course.</summary>
    QueueBabyMetroidAttackSound,
    /// <summary>Sets up the palette, breath and sound for the rainbow-beam charge.</summary>
    PrepareRainbowBeamCharge,
    /// <summary>Spawns a rainbow-beam charge particle.</summary>
    SpawnRainbowBeamChargeParticle,
    /// <summary>Usually holds the phase-two neutral pose another round.</summary>
    MaybeHoldNeutralPhase2,
    /// <summary>Usually holds the phase-three neutral pose another round.</summary>
    MaybeHoldNeutralPhase3,
    /// <summary>Rarely bursts into another round of dying drool.</summary>
    MaybeDroolAgain,
}

/// <summary>A gameplay cue of Mother Brain's brain programs.</summary>
/// <param name="Kind">What the cue does.</param>
/// <param name="Sound">The sound a sound cue queues.</param>
/// <param name="Afterburns">The afterburn count a bomb cue lobs.</param>
internal readonly record struct MotherBrainHeadCue(MotherBrainHeadCueKind Kind, SoundEffectId? Sound, int Afterburns)
{
    /// <summary>A cue without operands.</summary>
    internal static MotherBrainHeadCue Of(MotherBrainHeadCueKind kind) => new(kind, null, 0);
    /// <summary>Queues library-two <paramref name="sound"/>.</summary>
    internal static MotherBrainHeadCue Sound2(SoundEffectId sound) => new(MotherBrainHeadCueKind.QueueSound, sound, 0);
    /// <summary>Queues library-three <paramref name="sound"/>.</summary>
    internal static MotherBrainHeadCue Sound3(SoundEffectId sound) => new(MotherBrainHeadCueKind.QueueLibrary3Sound, sound, 0);
    /// <summary>Lobs a bomb that afterburns <paramref name="afterburns"/> times.</summary>
    internal static MotherBrainHeadCue Bomb(int afterburns) => new(MotherBrainHeadCueKind.SpawnBomb, null, afterburns);
}

/// <summary>Mother Brain's typed body and brain programs (#641), in place of their bank-$A9 instruction lists.</summary>
internal static class MotherBrainAnimations
{
    /// <summary>Every body program.</summary>
    internal static AnimationProgramSet<B, MotherBrainBodyFrame, MotherBrainBodyCue> Body { get; } = new(
        new Dictionary<B, AnimationProgramStep<B, MotherBrainBodyFrame, MotherBrainBodyCue>[]>
    {
        [B.WalkForwardReallyFast] = WalkForward(2),
        [B.WalkForwardFast] = WalkForward(4),
        [B.WalkForwardMedium] = WalkForward(6),
        [B.WalkForwardSlow] = WalkForward(8),
        [B.WalkForwardReallySlow] = WalkForward(10),
        [B.WalkBackwardSlow] = WalkBackward(8),
        [B.WalkBackwardReallyFast] = WalkBackward(2),
        [B.WalkBackwardFast] = WalkBackward(4),
        [B.WalkBackwardMedium] = WalkBackward(6),
        [B.WalkBackwardReallySlow] = WalkBackward(10),
        [B.CrouchAndThenStandUp] =
        [
            .. Crouch(8, 8),
            BodyShow(MotherBrainBodyFrame.Crouched, 8),
            BodyCue(MotherBrainBodyCue.MoveWithScrollBias(-10, 4)),
            Pose(MotherBrainBodyPose.CrouchingTransition),
            BodyShow(MotherBrainBodyFrame.Uncrouching, 8),
            BodyCue(MotherBrainBodyCue.MoveWithScrollBias(-16, 4)),
            BodyShow(MotherBrainBodyFrame.LeaningDown, 8),
            BodyCue(MotherBrainBodyCue.MoveWithScrollBias(-12, -2)),
            BodyShow(MotherBrainBodyFrame.Standing, 8),
            Pose(MotherBrainBodyPose.Standing),
            BodyHold,
        ],
        [B.StandAfterCrouchSlow] = [.. StandUp(16), BodyHold],
        [B.StandAfterCrouchFast] = [.. StandUp(8), BodyHold],
        [B.StandAfterLeaning] =
        [
            Pose(MotherBrainBodyPose.CrouchingTransition),
            BodyShow(MotherBrainBodyFrame.LeaningDown, 8),
            BodyCue(MotherBrainBodyCue.MoveWithScrollBias(-12, -2)),
            BodyShow(MotherBrainBodyFrame.Standing, 8),
            Pose(MotherBrainBodyPose.Standing),
            BodyHold,
        ],
        [B.LeanDown] =
        [
            Pose(MotherBrainBodyPose.CrouchingTransition),
            BodyShow(MotherBrainBodyFrame.Standing, 8),
            BodyCue(MotherBrainBodyCue.MoveWithScrollBias(12, 4)),
            Pose(MotherBrainBodyPose.LeaningDown),
            BodyShow(MotherBrainBodyFrame.LeaningDown, 8),
            BodyHold,
        ],
        [B.Crouched] =
        [
            Pose(MotherBrainBodyPose.Crouched),
            BodyShow(MotherBrainBodyFrame.Crouched, 8),
            BodyHold,
        ],
        [B.CrouchSlow] = [.. Crouch(8, 8), BodyHold],
        [B.CrouchFast] = [.. Crouch(2, 2), BodyHold],
        [B.HandBeam] = HandBeam(),
        [B.InitialDummy] =
        [
            // A zero duration is the cartridge's: the timer wraps and the dummy frame stays for good.
            BodyShow(MotherBrainBodyFrame.InitialDummy, 0),
            BodyHold,
        ],
    });

    /// <summary>A forward walk whose frames last <paramref name="duration"/> updates.</summary>
    private static AnimationProgramStep<B, MotherBrainBodyFrame, MotherBrainBodyCue>[] WalkForward(int duration) =>
    [
        Pose(MotherBrainBodyPose.Walking),
        BodyShow(MotherBrainBodyFrame.Standing, duration),
        BodyCue(MotherBrainBodyCue.Move(new(1, -2))),
        BodyShow(MotherBrainBodyFrame.Walking(0), duration),
        BodyCue(MotherBrainBodyCue.Move(new(2, 0))),
        BodyShow(MotherBrainBodyFrame.Walking(1), duration),
        BodyCue(MotherBrainBodyCue.Move(new(0, 1))),
        BodyShow(MotherBrainBodyFrame.Walking(2), duration),
        BodyCue(MotherBrainBodyCue.StepAndMove(new(3, 1))),
        BodyShow(MotherBrainBodyFrame.Walking(3), duration),
        BodyCue(MotherBrainBodyCue.Move(new(15, -2))),
        BodyShow(MotherBrainBodyFrame.Walking(4), duration),
        BodyCue(MotherBrainBodyCue.Move(new(6, -4))),
        BodyShow(MotherBrainBodyFrame.Walking(5), duration),
        BodyCue(MotherBrainBodyCue.Move(new(-2, 4))),
        BodyShow(MotherBrainBodyFrame.Walking(6), duration),
        BodyCue(MotherBrainBodyCue.StepAndMove(new(-1, 2))),
        Pose(MotherBrainBodyPose.Standing),
        BodyShow(MotherBrainBodyFrame.Walking(7), duration),
        BodyHold,
    ];

    /// <summary>A backward walk whose frames last <paramref name="duration"/> updates.</summary>
    private static AnimationProgramStep<B, MotherBrainBodyFrame, MotherBrainBodyCue>[] WalkBackward(int duration) =>
    [
        Pose(MotherBrainBodyPose.Walking),
        BodyShow(MotherBrainBodyFrame.Walking(7), duration),
        BodyCue(MotherBrainBodyCue.Move(new(1, -2))),
        BodyShow(MotherBrainBodyFrame.Walking(6), duration),
        BodyCue(MotherBrainBodyCue.Move(new(2, -4))),
        BodyShow(MotherBrainBodyFrame.Walking(5), duration),
        BodyCue(MotherBrainBodyCue.Move(new(-6, 4))),
        BodyShow(MotherBrainBodyFrame.Walking(4), duration),
        BodyCue(MotherBrainBodyCue.StepAndMove(new(-15, 2))),
        BodyShow(MotherBrainBodyFrame.Walking(3), duration),
        BodyCue(MotherBrainBodyCue.Move(new(-3, -1))),
        BodyShow(MotherBrainBodyFrame.Walking(2), duration),
        BodyCue(MotherBrainBodyCue.Move(new(0, -1))),
        BodyShow(MotherBrainBodyFrame.Walking(1), duration),
        BodyCue(MotherBrainBodyCue.Move(new(-2, 0))),
        BodyShow(MotherBrainBodyFrame.Walking(0), duration),
        BodyCue(MotherBrainBodyCue.StepAndMove(new(-1, 2))),
        Pose(MotherBrainBodyPose.Standing),
        BodyShow(MotherBrainBodyFrame.Standing, duration),
        BodyHold,
    ];

    /// <summary>Lowers the body into the crouch; the lean and uncrouch frames last the given updates.</summary>
    private static AnimationProgramStep<B, MotherBrainBodyFrame, MotherBrainBodyCue>[] Crouch(int lean, int uncrouch) =>
    [
        Pose(MotherBrainBodyPose.CrouchingTransition),
        BodyShow(MotherBrainBodyFrame.Standing, 8),
        BodyCue(MotherBrainBodyCue.MoveWithScrollBias(12, 4)),
        BodyShow(MotherBrainBodyFrame.LeaningDown, lean),
        BodyCue(MotherBrainBodyCue.MoveWithScrollBias(16, -2)),
        BodyShow(MotherBrainBodyFrame.Uncrouching, uncrouch),
        BodyCue(MotherBrainBodyCue.MoveWithScrollBias(10, -2)),
        Pose(MotherBrainBodyPose.Crouched),
        BodyShow(MotherBrainBodyFrame.Crouched, 8),
    ];

    /// <summary>Raises the body from the crouch to standing, each frame lasting <paramref name="duration"/> updates.</summary>
    private static AnimationProgramStep<B, MotherBrainBodyFrame, MotherBrainBodyCue>[] StandUp(int duration) =>
    [
        Pose(MotherBrainBodyPose.CrouchingTransition),
        BodyShow(MotherBrainBodyFrame.Crouched, duration),
        BodyCue(MotherBrainBodyCue.MoveWithScrollBias(-10, 4)),
        BodyShow(MotherBrainBodyFrame.Uncrouching, duration),
        BodyCue(MotherBrainBodyCue.MoveWithScrollBias(-16, 4)),
        BodyShow(MotherBrainBodyFrame.LeaningDown, duration),
        BodyCue(MotherBrainBodyCue.MoveWithScrollBias(-12, -2)),
        BodyShow(MotherBrainBodyFrame.Standing, duration),
        Pose(MotherBrainBodyPose.Standing),
    ];

    /// <summary>
    /// The red hand beam: the arm rises, eight charge particles alternate between the particle and the energy
    /// spark at the hand, the beam fires, and the body holds the standing pose before the next phase.
    /// </summary>
    private static AnimationProgramStep<B, MotherBrainBodyFrame, MotherBrainBodyCue>[] HandBeam()
    {
        var steps = new List<AnimationProgramStep<B, MotherBrainBodyFrame, MotherBrainBodyCue>>
        {
            Pose(MotherBrainBodyPose.DeathBeam),
            BodyShow(MotherBrainBodyFrame.Standing, 1),
            BodyShow(MotherBrainBodyFrame.HandBeam(0), 1),
            BodyShow(MotherBrainBodyFrame.HandBeam(1), 1),
        };
        for (int particle = 0; particle < 8; particle++)
        {
            steps.Add(BodyCue(particle % 2 == 0
                ? MotherBrainBodyCue.SpawnHandDust(new(36, -40), MiscDustAnimation.MotherBrainElbowChargeParticle)
                : MotherBrainBodyCue.SpawnHandDust(new(34, -42), MiscDustAnimation.MotherBrainElbowChargeEnergy)));
            steps.Add(BodyShow(MotherBrainBodyFrame.HandBeam(1), 1));
        }
        steps.AddRange(
        [
            BodyShow(MotherBrainBodyFrame.HandBeam(1), 1),
            BodyCue(MotherBrainBodyCue.FireHandBeam),
            BodyShow(MotherBrainBodyFrame.HandBeam(2), 1),
            BodyShow(MotherBrainBodyFrame.HandBeam(3), 1),
            BodyShow(MotherBrainBodyFrame.Standing, 240),
            BodyCue(MotherBrainBodyCue.AdvanceHandBeamPhase),
            Pose(MotherBrainBodyPose.Standing),
            BodyHold,
        ]);
        return [.. steps];
    }

    /// <summary>Shows a body frame.</summary>
    private static AnimationProgramStep<B, MotherBrainBodyFrame, MotherBrainBodyCue> BodyShow(MotherBrainBodyFrame frame, int duration) =>
        AnimationProgramStep<B, MotherBrainBodyFrame, MotherBrainBodyCue>.Show(frame, duration);

    /// <summary>Publishes a body cue.</summary>
    private static AnimationProgramStep<B, MotherBrainBodyFrame, MotherBrainBodyCue> BodyCue(MotherBrainBodyCue cue) =>
        AnimationProgramStep<B, MotherBrainBodyFrame, MotherBrainBodyCue>.Publish(cue);

    /// <summary>Publishes a body pose.</summary>
    private static AnimationProgramStep<B, MotherBrainBodyFrame, MotherBrainBodyCue> Pose(MotherBrainBodyPose pose) =>
        BodyCue(MotherBrainBodyCue.SetPose(pose));

    /// <summary>The native sleep that ends a body program.</summary>
    private static AnimationProgramStep<B, MotherBrainBodyFrame, MotherBrainBodyCue> BodyHold =>
        AnimationProgramStep<B, MotherBrainBodyFrame, MotherBrainBodyCue>.Hold;

    /// <summary>Every brain program.</summary>
    internal static AnimationProgramSet<H, MotherBrainHeadFrame, MotherBrainHeadCue> Head { get; } = new(
        new Dictionary<H, AnimationProgramStep<H, MotherBrainHeadFrame, MotherBrainHeadCue>[]>
    {
        [H.StretchingPhase2] = [.. Stretch(2, 3, 4, drool: true), FallInto(H.StretchingPhase2Loop)],
        [H.StretchingPhase2Loop] = [Show(2, 1), GoTo(H.StretchingPhase2Loop)],
        [H.StretchingPhase3] = [.. Stretch(8, 9, 10, drool: true), FallInto(H.StretchingPhase3Loop)],
        [H.StretchingPhase3Loop] = [Show(8, 1), GoTo(H.StretchingPhase3Loop)],
        [H.HyperBeamRecoil] = [.. Stretch(8, 9, 10, drool: false), FallInto(H.HyperBeamRecoilLoop)],
        [H.HyperBeamRecoilLoop] = [Show(8, 1), GoTo(H.HyperBeamRecoilLoop)],
        [H.Initial] = [Show(0, 4), GoTo(H.Initial)],
        [H.Decapitated] = [Show(8, 8), Show(7, 4), FallInto(H.DecapitatedLoop)],
        [H.DecapitatedLoop] = [Show(6, 4), GoTo(H.DecapitatedLoop)],
        [H.DyingDrool] =
        [
            Cue(MotherBrainHeadCueKind.ShakeBrain),
            Show(8, 4),
            Show(9, 4),
            HeadCue(MotherBrainHeadCue.Sound2(MotherBrainSoundDefinitions.Roar)),
            FallInto(H.DyingDroolBurst),
        ],
        [H.DyingDroolBurst] =
        [
            Show(10, 2),
            Cue(MotherBrainHeadCueKind.SpawnDrool),
            Show(10, 2),
            Cue(MotherBrainHeadCueKind.SpawnDrool),
            Show(10, 2),
            Cue(MotherBrainHeadCueKind.SpawnDrool),
            Show(10, 2),
            Cue(MotherBrainHeadCueKind.SpawnDrool),
            FallInto(H.DyingDroolLoop),
        ],
        [H.DyingDroolLoop] =
        [
            Show(10, 2),
            Cue(MotherBrainHeadCueKind.MaybeDroolAgain),
            GoTo(H.DyingDroolLoop),
        ],
        [H.FiringRainbowBeam] = [Show(2, 1), GoTo(H.FiringRainbowBeam)],
        [H.NeutralPhase2] = [Show(0, 4), Show(1, 4), Show(2, 8), Show(1, 4), Show(0, 4), Show(1, 4), FallInto(H.NeutralPhase2Hold)],
        [H.NeutralPhase2Hold] =
        [
            Show(2, 8),
            Cue(MotherBrainHeadCueKind.MaybeHoldNeutralPhase2),
            Show(1, 4),
            GoTo(H.NeutralPhase2),
        ],
        [H.NeutralPhase3] = [Show(6, 4), Show(7, 4), Show(8, 8), Show(7, 4), Show(6, 4), Show(7, 4), FallInto(H.NeutralPhase3Hold)],
        [H.NeutralPhase3Hold] =
        [
            Show(8, 8),
            Show(7, 8),
            Cue(MotherBrainHeadCueKind.MaybeHoldNeutralPhase3),
            Show(6, 4),
            GoTo(H.NeutralPhase3),
        ],
        [H.Corpse] = [Show(6, 2), Show(7, 2), Show(8, 64), Show(11, 64), FallInto(H.CorpseLoop)],
        [H.CorpseLoop] = [Show(12, 2), GoTo(H.CorpseLoop)],
        [H.FourOnionRingsPhase2] = OnionRingsPhase2(rings: 4),
        [H.TwoOnionRingsPhase2] = OnionRingsPhase2(rings: 2),
        [H.AttackingBabyMetroid] =
        [
            Cue(MotherBrainHeadCueKind.CountBabyMetroidAttack),
            Cue(MotherBrainHeadCueKind.DisableNeckMovement),
            Cue(MotherBrainHeadCueKind.AimOnionRingsAtBabyMetroid),
            GoTo(H.FourOnionRingsPhase3),
        ],
        [H.AttackingSamusFourOnionRingsPhase3] =
        [
            Cue(MotherBrainHeadCueKind.ResetBabyMetroidAttacks),
            Cue(MotherBrainHeadCueKind.DisableNeckMovement),
            Cue(MotherBrainHeadCueKind.AimOnionRingsAtSamus),
            FallInto(H.FourOnionRingsPhase3),
        ],
        [H.FourOnionRingsPhase3] =
        [
            Show(8, 4),
            Show(9, 4),
            Cue(MotherBrainHeadCueKind.QueueBabyMetroidAttackSound),
            Show(10, 8),
            Cue(MotherBrainHeadCueKind.SpawnOnionRing),
            HeadCue(MotherBrainHeadCue.Sound3(MotherBrainSoundDefinitions.OnionRings)),
            Show(10, 3),
            Cue(MotherBrainHeadCueKind.SpawnOnionRing),
            Show(10, 3),
            Cue(MotherBrainHeadCueKind.SpawnOnionRing),
            Show(10, 3),
            Cue(MotherBrainHeadCueKind.SpawnOnionRing),
            Show(10, 16),
            Show(9, 4),
            Show(8, 16),
            Cue(MotherBrainHeadCueKind.EnableNeckMovement),
            GoTo(H.NeutralPhase3),
        ],
        [H.BombPhase2] = Bomb(phaseThree: false),
        [H.BombPhase3] = Bomb(phaseThree: true),
        [H.Laser] =
        [
            Show(1, 16),
            Show(2, 4),
            Cue(MotherBrainHeadCueKind.SpawnLaser),
            Show(2, 32),
            Cue(MotherBrainHeadCueKind.EnableNeckMovement),
            GoTo(H.NeutralPhase2),
        ],
        [H.ChargingRainbowBeam] =
        [
            Cue(MotherBrainHeadCueKind.PrepareRainbowBeamCharge),
            Show(2, 4),
            Show(1, 4),
            Show(0, 2),
            FallInto(H.ChargingRainbowBeamLoop),
        ],
        [H.ChargingRainbowBeamLoop] =
        [
            Cue(MotherBrainHeadCueKind.SpawnRainbowBeamChargeParticle),
            Show(0, 30),
            GoTo(H.ChargingRainbowBeamLoop),
        ],
    });

    /// <summary>
    /// The neck stretch and the Hyper Beam recoil: the brain shakes, the mouth opens through
    /// <paramref name="open"/>, <paramref name="wide"/> and <paramref name="widest"/>, breathing a big purple cloud
    /// with the stretch cry, and drools while stretching.
    /// </summary>
    private static AnimationProgramStep<H, MotherBrainHeadFrame, MotherBrainHeadCue>[] Stretch(int open, int wide, int widest, bool drool)
    {
        var steps = new List<AnimationProgramStep<H, MotherBrainHeadFrame, MotherBrainHeadCue>>
        {
            Cue(MotherBrainHeadCueKind.ShakeBrain),
            Show(open, 2),
            Show(wide, 2),
        };
        if (drool) steps.Add(Cue(MotherBrainHeadCueKind.SpawnDrool));
        steps.Add(Show(wide, 2));
        steps.Add(Cue(MotherBrainHeadCueKind.SpawnBigPurpleBreath));
        steps.Add(HeadCue(MotherBrainHeadCue.Sound2(MotherBrainSoundDefinitions.Roar)));
        foreach (int duration in new[] { 16, 16, 32 })
        {
            if (drool) steps.Add(Cue(MotherBrainHeadCueKind.SpawnDrool));
            steps.Add(Show(widest, duration));
        }
        steps.Add(Show(wide, 4));
        return [.. steps];
    }

    /// <summary>A phase-two onion-ring volley of <paramref name="rings"/> rings aimed at Samus.</summary>
    private static AnimationProgramStep<H, MotherBrainHeadFrame, MotherBrainHeadCue>[] OnionRingsPhase2(int rings)
    {
        var steps = new List<AnimationProgramStep<H, MotherBrainHeadFrame, MotherBrainHeadCue>>
        {
            Cue(MotherBrainHeadCueKind.DisableNeckMovement),
            Show(2, 4),
            Show(3, 4),
            HeadCue(MotherBrainHeadCue.Sound2(MotherBrainSoundDefinitions.AttackCry)),
            Show(4, 8),
        };
        for (int ring = 0; ring < rings; ring++)
        {
            steps.Add(Cue(MotherBrainHeadCueKind.AimOnionRingsAtSamus));
            steps.Add(Cue(MotherBrainHeadCueKind.SpawnOnionRing));
            if (ring == 0) steps.Add(HeadCue(MotherBrainHeadCue.Sound3(MotherBrainSoundDefinitions.OnionRings)));
            steps.Add(Show(4, ring == rings - 1 ? 16 : 3));
        }
        steps.AddRange(
        [
            Show(3, 4),
            Show(2, 16),
            Cue(MotherBrainHeadCueKind.EnableNeckMovement),
            GoTo(H.NeutralPhase2),
        ]);
        return [.. steps];
    }

    /// <summary>A bomb lob: phase two lobs a seven-afterburn bomb, phase three a one-afterburn bomb.</summary>
    private static AnimationProgramStep<H, MotherBrainHeadFrame, MotherBrainHeadCue>[] Bomb(bool phaseThree)
    {
        // Phase three uses the damaged head (frames 6 to 10) where phase two uses the whole one (0 to 4).
        int head = phaseThree ? 6 : 0;
        return
        [
            Show(head, 4),
            Show(head + 1, 4),
            Show(head + 2, 8),
            Cue(MotherBrainHeadCueKind.DisableNeckMovement),
            Show(head + 2, 4),
            Show(head + 3, 4),
            HeadCue(MotherBrainHeadCue.Sound2(MotherBrainSoundDefinitions.AttackCry)),
            Show(head + 4, 8),
            HeadCue(MotherBrainHeadCue.Bomb(phaseThree ? 1 : 7)),
            Cue(MotherBrainHeadCueKind.SpawnBigPurpleBreath),
            Show(head + 4, 32),
            Show(head + 3, 4),
            Show(head + 2, 16),
            Cue(MotherBrainHeadCueKind.EnableNeckMovement),
            GoTo(phaseThree ? H.NeutralPhase3 : H.NeutralPhase2),
        ];
    }

    /// <summary>Shows brain frame <paramref name="index"/> of the composition catalog.</summary>
    private static AnimationProgramStep<H, MotherBrainHeadFrame, MotherBrainHeadCue> Show(int index, int duration) =>
        AnimationProgramStep<H, MotherBrainHeadFrame, MotherBrainHeadCue>.Show(new MotherBrainHeadFrame(index), duration);

    /// <summary>Publishes a brain cue without operands.</summary>
    private static AnimationProgramStep<H, MotherBrainHeadFrame, MotherBrainHeadCue> Cue(MotherBrainHeadCueKind kind) =>
        HeadCue(MotherBrainHeadCue.Of(kind));

    /// <summary>Publishes a brain cue.</summary>
    private static AnimationProgramStep<H, MotherBrainHeadFrame, MotherBrainHeadCue> HeadCue(MotherBrainHeadCue cue) =>
        AnimationProgramStep<H, MotherBrainHeadFrame, MotherBrainHeadCue>.Publish(cue);

    /// <summary>Continues at <paramref name="program"/>.</summary>
    private static AnimationProgramStep<H, MotherBrainHeadFrame, MotherBrainHeadCue> GoTo(H program) =>
        AnimationProgramStep<H, MotherBrainHeadFrame, MotherBrainHeadCue>.GoTo(program);

    /// <summary>Runs on into <paramref name="program"/>.</summary>
    private static AnimationProgramStep<H, MotherBrainHeadFrame, MotherBrainHeadCue> FallInto(H program) =>
        AnimationProgramStep<H, MotherBrainHeadFrame, MotherBrainHeadCue>.FallInto(program);
}

/// <summary>The cutscene Baby Metroid's programs, in bank $A9 order.</summary>
public enum MotherBrainBabyProgram
{
    /// <summary><c>InstList_BabyMetroid_Initial</c> (<c>$A9:CFA2</c>): the slow idle pulse.</summary>
    Initial,
    /// <summary><c>InstList_BabyMetroid_DrainingMotherBrain</c> (<c>$A9:CFB8</c>): the quick draining pulse.</summary>
    DrainingMotherBrain,
    /// <summary><c>InstList_BabyMetroid_TakingFatalBlow</c> (<c>$A9:CFCE</c>).</summary>
    TakingFatalBlow,
}

/// <summary>One of the Baby Metroid's three pulse poses, an index into <see cref="BabyMetroidVisualDefinitions"/>.</summary>
/// <param name="Index">The pose's position in the catalog: closed, half open, wide open.</param>
public readonly record struct BabyMetroidFrame(int Index)
{
    /// <summary>The installed spritemap the pose shows.</summary>
    internal ushort Spritemap => BabyMetroidVisualDefinitions.Spritemap(Index);
}

/// <summary>The cutscene Baby's typed programs (#641); they publish no cues.</summary>
internal static class MotherBrainBabyAnimations
{
    /// <summary>Every Baby program.</summary>
    internal static AnimationProgramSet<MotherBrainBabyProgram, BabyMetroidFrame, NoAnimationCue> Programs { get; } = new(
        new Dictionary<MotherBrainBabyProgram, AnimationProgramStep<MotherBrainBabyProgram, BabyMetroidFrame, NoAnimationCue>[]>
    {
        [MotherBrainBabyProgram.Initial] = [Show(0, 16), Show(1, 16), Show(2, 16), Show(1, 16), GoTo(MotherBrainBabyProgram.Initial)],
        [MotherBrainBabyProgram.DrainingMotherBrain] =
            [Show(0, 8), Show(1, 8), Show(2, 5), Show(1, 2), GoTo(MotherBrainBabyProgram.DrainingMotherBrain)],
        [MotherBrainBabyProgram.TakingFatalBlow] =
            [Show(2, 128), AnimationProgramStep<MotherBrainBabyProgram, BabyMetroidFrame, NoAnimationCue>.Hold],
    });

    /// <summary>Shows pose <paramref name="index"/>.</summary>
    private static AnimationProgramStep<MotherBrainBabyProgram, BabyMetroidFrame, NoAnimationCue> Show(int index, int duration) =>
        AnimationProgramStep<MotherBrainBabyProgram, BabyMetroidFrame, NoAnimationCue>.Show(new BabyMetroidFrame(index), duration);

    /// <summary>Continues at <paramref name="program"/>.</summary>
    private static AnimationProgramStep<MotherBrainBabyProgram, BabyMetroidFrame, NoAnimationCue> GoTo(MotherBrainBabyProgram program) =>
        AnimationProgramStep<MotherBrainBabyProgram, BabyMetroidFrame, NoAnimationCue>.GoTo(program);
}

