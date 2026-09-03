namespace SuperMetroid.Core.Game;

/// <summary>
/// Bank-$A6 function pointers used by both Ridley encounters. Keeping the original
/// addresses in debugger-visible state makes every transition directly comparable with
/// a WRAM trace and prevents host-only ordinals from obscuring shared cartridge code.
/// </summary>
public enum RidleyAiFunction : ushort
{
    ClearVelocity = 0xa354,
    WaitForDoorTransition = 0xa35b,
    InitialDelay = 0xa377,
    FadeInEyes = 0xa389,
    FadeInBody = 0xa3df,
    WaitBeforeRoar = 0xa455,
    WaitBeforeLiftoff = 0xa478,
    CeresLiftoffAccelerating = 0xa6af,
    CeresLiftoffDecelerating = 0xa6c8,
    CeresHovering = 0xa6e8,
    CeresFireballMoveToPosition = 0xa782,
    CeresFireballShooting = 0xa7f9,
    CeresLungeSetup = 0xa83c,
    CeresLungeMain = 0xa84e,
    CeresSwoopSetup = 0xa88d,
    CeresSwoopMoveToPosition = 0xa8a4,
    CeresSwoopDescendingAimingDown = 0xa8d4,
    CeresSwoopDescendingAimingLeft = 0xa8f8,
    CeresSwoopAscendingAimingUp = 0xa923,
    CeresSwoopAscendingAimingUpFaster = 0xa947,
    CeresRealRetreatRising = 0xa971,
    CeresRetreatDelay = 0xa9a0,
    CeresPublishEscapeHandoff = 0xaa11,
    CeresInactive = 0xaa1b,
    CeresActivateSelfDestruct = 0xc04e,
    CeresSelfDestructPaletteOnly = 0xaa50,

    // Lower Norfair Ridley begins at $B2F3 after the shared reveal sequence. These names
    // describe the native movement/attack phases; their values are not host inventions.
    NorfairEnterArena = 0xb2f3,
    NorfairSelectAttack = 0xb321,
    NorfairHoverSetup = 0xb3ec,
    NorfairHover = 0xb3f8,
    NorfairSwoopSetup = 0xb441,
    NorfairSwoopMoveToStart = 0xb455,
    NorfairSwoopAimDown = 0xb493,
    NorfairSwoopAimSideways = 0xb4d1,
    NorfairSwoopAimUp = 0xb516,
    NorfairSwoopClimb = 0xb554,
    NorfairSwoopRecover = 0xb594,
    NorfairPogoSetup = 0xb5c4,
    NorfairPogoDescending = 0xb5e5,
    NorfairPogoAscending = 0xb613,
    NorfairFireballSetup = 0xb68b,
    NorfairFireballMoveToSide = 0xb6a7,
    NorfairFireballMoveToHeight = 0xb6dd,
    NorfairFireballAttack = 0xb70e,
    NorfairFireballRecover = 0xb7b9,
    NorfairGrabApproach = 0xbab7,
    NorfairCarrySetup = 0xbb8f,
    NorfairCarryMoveToAnchor = 0xbbc4,
    NorfairCarryRise = 0xbbf1,
    NorfairCarryRelease = 0xbc2e,
    NorfairReturnToArena = 0xbd4e,

    // The following functions are shared with Ceres's Baby retrieval route but are only
    // selected during the real fight when Ridley has grabbed Samus.
    CeresFakeRetreatMoveToPosition = 0xbd9a,
    CeresFakeRetreatRising = 0xbdbc,
    CeresWaitBeforeRetrievingBaby = 0xbdf2,
    CeresRetrieveBaby = 0xbe03,

    NorfairGrabbedSamus = 0xc04e,
    NorfairReleaseSamus = 0xc538,
    NorfairDeathStart = 0xc53e,
    NorfairDeathExplosions = 0xc551,
    NorfairDeathFall = 0xc588,
    NorfairDeathImpact = 0xc5a8,
    NorfairDeathWait = 0xc5c8,
    NorfairDeathFinish = 0xc5da,
}

/// <summary>
/// One of the seven twenty-byte logical tail entries initialized by $A6:D2D6. The tail
/// is shared by Ceres Ridley and Lower Norfair Ridley; encounter-specific controllers only
/// change these common targets and activation words.
/// </summary>
public sealed class RidleyTailSegment
{
    public bool Active { get; internal set; }
    public ushort StaggerAngle { get; internal set; }
    public ushort MovementDirection { get; internal set; }
    public ushort Distance { get; internal set; }

    /// <summary>
    /// Nonzero while this segment is extending for a whip. Native stores this separately
    /// from the live radius and clears it after the live distance passes the request.
    /// </summary>
    public ushort TargetDistance { get; internal set; }

    public ushort Angle { get; internal set; }
    public ushort XOffset { get; internal set; }
    public ushort YOffset { get; internal set; }
    public ushort XPosition { get; internal set; }
    public ushort YPosition { get; internal set; }
}

/// <summary>
/// Named projection of Ridley's common enemy slot and three extended WRAM workspaces.
/// Ceres-only Baby/Mode-7 words and Norfair-only combat words intentionally coexist here:
/// the original shared initializer owns one layout and only one Ridley can exist at a time.
/// </summary>
public sealed class RidleyEnemyState
{
    public RidleyAiFunction Function { get; internal set; }
    public ushort FightMode { get; internal set; }
    public ushort HitCounter { get; internal set; }
    public ushort SpritemapPaletteIndex { get; internal set; }
    public ushort CommonDrawPaletteIndex { get; internal set; }
    public ushort MovementAnimationEnabled { get; internal set; }
    public ushort FacingDirection { get; internal set; }
    public ushort IdleTailWhipEnabled { get; internal set; }
    public ushort TailDamage { get; internal set; }
    public ushort FunctionTimer { get; internal set; }
    public ushort FadePaletteOffset { get; internal set; }
    public ushort MinimumY { get; internal set; }
    public ushort MaximumY { get; internal set; }
    public ushort MinimumX { get; internal set; }
    public ushort MaximumX { get; internal set; }
    public ushort HorizontalVelocity { get; internal set; }
    public ushort VerticalVelocity { get; internal set; }
    public ushort WingFrame { get; internal set; }
    public ushort WingAnimationTimer { get; internal set; }
    public ushort WingAnimationTimerDelta { get; internal set; }
    public ushort TailFunctionIndex { get; internal set; }
    public ushort TailAngleDelta { get; internal set; }
    public ushort TailMinimumClockwiseAngle { get; internal set; }
    public ushort TailMaximumCounterClockwiseAngle { get; internal set; }
    public ushort TailWhipTargetClockwiseAngle { get; internal set; }
    public ushort TailWhipTargetCounterClockwiseAngle { get; internal set; }
    public ushort TailWhipRequest { get; internal set; }
    public ushort TailExtensionSpeed { get; internal set; }
    public ushort IdealInterSegmentTailAngle { get; internal set; }
    public RidleyTailSegment[] TailSegments { get; internal set; } = [];
    public bool Roaring { get; internal set; }
    public ushort HoverCounter { get; internal set; }
    public ushort FeetDistanceIndex { get; internal set; }
    public ushort FireballBaseXPosition { get; internal set; }
    public ushort FireballBaseYPosition { get; internal set; }
    public ushort FireballXVelocity { get; internal set; }
    public ushort FireballYVelocity { get; internal set; }
    public ushort SwoopAngleAccumulator { get; internal set; }
    public ushort SwoopSpeedMagnitude { get; internal set; }

    // Lower Norfair combat state ($7E:78xx/$80xx). These replace opaque numbered words
    // with the meanings established by the corresponding bank-$A6 consumers.
    public ushort HealthStage { get; internal set; }
    public ushort AttackTableIndex { get; internal set; }
    public ushort PreviousSamusX { get; internal set; }
    public ushort SamusMovementDirection { get; internal set; }
    public ushort GrabbedSamusMovementLagTimer { get; internal set; }
    public ushort GrabbedSamusMovementIndex { get; internal set; }
    public ushort GrabState { get; internal set; }
    public ushort IntangibilityTimer { get; internal set; }
    public ushort TargetX { get; internal set; }
    public ushort TargetY { get; internal set; }
    public ushort PogoTargetX { get; internal set; }
    public ushort PogoBounceCount { get; internal set; }
    public ushort PogoDownwardAcceleration { get; internal set; }
    public ushort PogoUpwardAcceleration { get; internal set; }
    public ushort GrabXOffset { get; internal set; }
    public ushort GrabYOffset { get; internal set; }
    public ushort FireballVolleyCounter { get; internal set; }
    public ushort FireballCooldown { get; internal set; }
    public ushort PowerBombReactionLatched { get; internal set; }
    public ushort HurtMovementClamp { get; internal set; }
    public ushort ZeroHealthLungeCount { get; internal set; }
    public ushort DeathExplosionTimer { get; internal set; }
    public ushort DeathExplosionCount { get; internal set; }
    public ushort FxTargetYPosition { get; internal set; }
    public ushort FxYSubVelocity { get; internal set; }
    public ushort FxTimer { get; internal set; }
    public bool DeathBreakupSpawned { get; internal set; }
    public bool BossDefeatPublished { get; internal set; }
    public ushort? LastDeathSoundEffect { get; internal set; }
    public MusicCommand? MusicRequest { get; internal set; }

    // Ceres's private Baby actor and Mode-7 getaway reuse Ridley's extended workspaces.
    public ushort BabyInstruction { get; internal set; }
    public ushort BabyInstructionTimer { get; internal set; }
    public ushort BabyFunction { get; internal set; }
    public ushort BabyCurrentSpritemap { get; internal set; }
    public ushort BabyXPosition { get; internal set; }
    public ushort BabyYPosition { get; internal set; }
    public ushort BabyYSubposition { get; internal set; }
    public ushort BabyVerticalVelocity { get; internal set; }
    public bool Mode7Active { get; internal set; }
    public bool Mode7Finished { get; internal set; }
    public ushort Mode7TableByteIndex { get; internal set; }
    public SnesAngle Mode7Angle { get; internal set; }
    public ushort Mode7HorizontalOffset { get; internal set; }
    public ushort Mode7VerticalOffset { get; internal set; }
    public ushort Mode7Zoom { get; internal set; }
    public ushort Mode7MatrixA { get; internal set; }
    public ushort Mode7MatrixB { get; internal set; }
    public ushort Mode7MatrixC { get; internal set; }
    public ushort Mode7MatrixD { get; internal set; }
    public ushort Mode7CenterX { get; internal set; }
    public ushort Mode7CenterY { get; internal set; }
    public ushort Mode7BabyFrame { get; internal set; }
    public ushort Mode7WingFrame { get; internal set; }

    // Ceres self-destruct presentation ($A6:C04E-$C135). FunctionTimer deliberately
    // retains the native phase values 0,2,4,6,8,10,12 while these words project the
    // extended enemy workspaces used by the DMA-list and optional Japanese typewriter.
    public ushort CeresEscapeTransferListPointer { get; internal set; }
    public ushort CeresEscapeTextPointer { get; internal set; }
    public ushort CeresEscapeTextDestination { get; internal set; }
    public ushort CeresEscapeTextDelayTimer { get; internal set; }
    public ushort CeresEscapeTextDelay { get; internal set; }
    public ushort CeresEscapeTextSoundCounter { get; internal set; }
    public ushort CeresEscapePaletteFrame { get; internal set; }
}
