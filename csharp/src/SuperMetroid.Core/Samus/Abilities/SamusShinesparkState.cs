using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Samus.Drawing;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.Core.Samus.Palettes;
using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.Core.Samus.Artwork;

namespace SuperMetroid.Core.Samus.Abilities;

/// <summary>
/// State for stored Speed Booster shine and Samus's shinespark handler.
/// </summary>
/// <remarks>
/// This is the game's state machine, not a desktop “dash”
/// controller. Timers wrap at 65536, movement retains split 16.16 values, and every
/// palette is selected through the user's installed artwork. Block collision uses the shared dispatcher; solid
/// enemies stop a sideways spark through it, and an upward spark tests them before its block move.
/// </remarks>
public sealed class SamusShinesparkState
{
    /// <summary>
    /// The update's audio requests the spark's sounds join as they are made: Samus's, which her runtime points at its own.
    /// </summary>
    internal GameplayAudioRequests AudioRequests { get; private set; } = new();

    /// <summary>
    /// The upward spark's 16.16 acceleration in pixels per frame per frame: 7.0 when windup begins, raised by Samus's
    /// vertical acceleration on every vertical or diagonal spark call, and cleared when the crash finishes. A suit pickup
    /// during a suspended windup replaces it with the light beam's progress.
    /// </summary>
    private readonly SuitPickupSparkCarryover _acceleration;

    /// <summary>
    /// How far the crash's orbiting copies have turned, 0 through <see cref="CrashOrbitTravelCarryover.LargestTravel"/>:
    /// the crash's entry keeps what the last orbit or the first echo it released left, and the first echo overwrites it
    /// with its height while it flies.
    /// </summary>
    private readonly CrashOrbitTravelCarryover _orbitTravel;

    /// <summary>Creates a spark owner whose upward acceleration and orbit travel live in Samus's compatibility services.</summary>
    /// <param name="acceleration">Samus's suit pickup carryover, which holds the spark's upward acceleration.</param>
    /// <param name="orbitTravel">Samus's crash orbit carryover, which holds how far the crash's orbit has turned.</param>
    internal SamusShinesparkState(SuitPickupSparkCarryover acceleration, CrashOrbitTravelCarryover orbitTravel)
    {
        _acceleration = Ensure.NotNull(acceleration);
        _orbitTravel = Ensure.NotNull(orbitTravel);
    }

    /// <summary>Points the spark's sounds at <paramref name="requests"/>, Samus's audio requests.</summary>
    internal void BindAudioRequests(GameplayAudioRequests requests) => AudioRequests = Ensure.NotNull(requests);

    /// <summary>The special suit colouring the shine runs: none, the stored shine's, or the shinespark's.</summary>
    public SamusSpecialPaletteType PaletteType { get; private set; }

    /// <summary>The special-palette timer: 180 stored, 60 windup, and 15 during active motion.</summary>
    public int ShineTimer { get; private set; }

    /// <summary>
    /// The next step of the shine colour cycle, below the stored-shine list's six steps for the stored shine and below the
    /// active list's four while sparking: each palette call advances it by one and returns it to zero at that limit.
    /// </summary>
    public int PaletteStep { get; private set; }

    /// <summary>The shared 30-frame windup/crash countdown.</summary>
    public int StartStopTimer { get; internal set; }

    /// <summary>The acceleration a shinespark windup starts its upward motion with.</summary>
    private static readonly SignedSixteenSixteen InitialVerticalAcceleration = SignedSixteenSixteen.FromParts(7, 0);

    /// <summary>The hurt flash count a moving spark holds, past the flash's colours and its damage sound.</summary>
    private const int MovingSparkHurtFlashCount = 8;

    /// <summary>The shinespark movement handler currently installed.</summary>
    public ShinesparkPhase Phase { get; internal set; }

    /// <summary>
    /// Relinquishes the movement handler when normal movement is installed.
    /// It does not clear the independently owned palette timer, boost counter, or echoes;
    /// Draygon's unconditional release relies on that distinction.
    /// </summary>
    internal void RelinquishMovementHandler() => Phase = ShinesparkPhase.Inactive;

    /// <summary>
    /// Whether the spark's handler is the single installed movement handler: the windup setup
    /// installs windup, and launch and crash replace it in turn until the crash finishes.
    /// </summary>
    public bool OwnsMovementHandler => Phase is
        ShinesparkPhase.Windup or ShinesparkPhase.Horizontal or ShinesparkPhase.Vertical or
        ShinesparkPhase.Diagonal or ShinesparkPhase.Crash or ShinesparkPhase.CrashEchoCircle or
        ShinesparkPhase.CrashFinish;

    /// <summary>
    /// Crystal Flash replaces both the movement handler and shared special
    /// palette timer. The new owner holds them; do not resume this old owner
    /// after Flash finishes. Extra run speed and the boost counter are not cleared.
    /// </summary>
    internal void RelinquishToCrystalFlash()
    {
        Phase = ShinesparkPhase.Inactive;
        ShineTimer = 0;
        PaletteType = SamusSpecialPaletteType.None;
        PaletteStep = 0;
    }


    /// <summary>
    /// The stage of the crash's orbit: expanding at crash entry, orbiting once the radius reaches sixteen,
    /// and contracting once the orbit's travel reaches 128.
    /// </summary>
    public ShinesparkCrashOrbitStage CrashOrbitStage { get; private set; }

    /// <summary>
    /// Distance of the two orbiting copies from Samus in pixels, 0 through 16: zero at crash entry, raised by four per
    /// expanding call until it reaches sixteen and lowered by four per contracting call until it reaches zero.
    /// </summary>
    public int CrashRadius { get; private set; }

    /// <summary>The first orbiting copy's angle around Samus.</summary>
    public RotationAngle FirstCrashEchoAngle { get; private set; }

    /// <summary>The second orbiting copy's angle, half a turn from the first.</summary>
    public RotationAngle SecondCrashEchoAngle { get; private set; }

    /// <summary>
    /// The steps of a 256-step turn both orbiting copies turn by on each orbiting call: four while Samus faces left and
    /// minus four otherwise, set at crash entry.
    /// </summary>
    private int _crashOrbitTurn;

    /// <summary>Where the first orbiting copy is shown while the crash runs.</summary>
    public Position16<RoomPixels> FirstOrbitingCopy { get; private set; }

    /// <summary>Where the second orbiting copy is shown while the crash runs; it is drawn before the first.</summary>
    public Position16<RoomPixels> SecondOrbitingCopy { get; private set; }

    /// <summary>The echo the crash releases first, only when Samus's shot count leaves room for both.</summary>
    public ShinesparkCrashEcho FirstReleasedEcho { get; } = new();

    /// <summary>The echo the crash releases second, whenever the shot count leaves room for one; it is drawn before the first.</summary>
    public ShinesparkCrashEcho SecondReleasedEcho { get; } = new();

    /// <summary>
    /// Starting the X-ray scope clears the special suit-colouring timer and X-ray's colouring
    /// takes over Samus's suit. Release the shine's colouring so it cannot keep ticking or
    /// repaint over the visor's new colours.
    /// </summary>
    internal void RelinquishPaletteToXray()
    {
        ShineTimer = 0;
        PaletteType = SamusSpecialPaletteType.None;
        PaletteStep = 0;
        if (Phase == ShinesparkPhase.Stored)
            Phase = ShinesparkPhase.Inactive;
    }

    /// <summary>
    /// Clearing Samus's shots (a room load, the elevator, the opening cinematic's flashbacks) forgets the released echoes
    /// and restarts the crash's orbit: its stage, radius, angles, turn and copies, and its travel, which the next crash
    /// starts from. The movement-handler phase and its unrelated timers are unchanged.
    /// </summary>
    internal void ResetProjectileEchoStorage()
    {
        FirstReleasedEcho.Forget();
        SecondReleasedEcho.Forget();
        _orbitTravel.Clear();
        FirstCrashEchoAngle = SecondCrashEchoAngle = RotationAngle.Zero;
        _crashOrbitTurn = 0;
        FirstOrbitingCopy = SecondOrbitingCopy = Position16<RoomPixels>.Zero;
        CrashOrbitStage = ShinesparkCrashOrbitStage.Expanding;
        CrashRadius = 0;
    }

    /// <summary>
    /// A crouch at the full boost stage stores a shinespark.
    /// </summary>
    public bool TryStoreFromSpeedBooster(SamusSpeedBoostCounter speedBoostCounter)
    {
        if (!speedBoostCounter.IsAtFullStage)
            return false;

        ShineTimer = 180;
        PaletteType = SamusSpecialPaletteType.SpeedBoosterShine;
        PaletteStep = 0;
        Phase = ShinesparkPhase.Stored;
        return true;
    }

    /// <summary>
    /// Starts the shinespark windup after Samus's pose handling has
    /// selected windup <see cref="SamusPoseId.ShinesparkWindupRightPose"/> and <see cref="SamusPoseId.ShinesparkWindupLeftPose"/>.
    /// </summary>
    public void BeginWindup(SamusState samus)
    {
        Ensure.NotNull(samus);
        if (!SamusInputSnapshot.HoldsShine(samus))
            throw new InvalidOperationException("Shinespark windup requires a live stored shine.");

        InitializeWindup(samus);
    }

    /// <summary>The two shinespark demos start the windup without a stored shine.</summary>
    internal void BeginDemoLaunch(SamusState samus, SamusPoseId targetPose)
    {
        InitializeWindup(samus);
        BeginDirectionalLaunch(samus, targetPose);
    }

    /// <summary>Cancels beam charge and installs the 30-frame spark windup, palette and boost state.</summary>
    private void InitializeWindup(SamusState samus)
    {
        // The windup checks the charge counter before clearing it. A charge that has
        // reached the draw-time sound threshold first queues the charge-cancellation sound; shorter
        // charges are still torn down without a sound call.
        BeamChargeLevel cancelledCharge = samus.BeamCharge.Level;
        if (cancelledCharge.IsCharging)
        {
            samus.BeamCharge.ClearChargeAndFlare();
            if (cancelledCharge.HasCharged(BeamChargeDefinitions.ChargeSoundStartCounter))
                AudioRequests.QueueSound(SamusSound.Silence, SoundRequestPriority.High);
        }

        // The windup takes over Samus's movement and suit colouring. Flash's live
        // palette timer can admit this launch even after its movement was interrupted.
        samus.CrystalFlash.RelinquishMovementHandler();
        samus.CrystalFlash.RelinquishPaletteHandler();
        // A Reserve-Mode X-Ray cone effect survives, but directional spark setup replaces
        // X-ray's suit colouring with the shinespark's.
        samus.Xray.RelinquishPaletteHandler();

        // These are literal writes. Stage four remains published during windup even
        // though base speed is zero and the extra component becomes exactly 8.0000.
        samus.Kinematics.YDirection = SamusVerticalDirection.Up;
        samus.Kinematics.YSpeed = default;
        samus.KnockbackDirection = Movement.SamusKnockbackDirection.None;
        samus.KnockbackActive = false;
        samus.HorizontalSpeed.SpeedBoostCounter = SamusSpeedBoostCounter.AtFullStage;
        samus.HorizontalSpeed.ExtraRunSpeed = new UnsignedSixteenSixteen(8, 0);
        samus.HorizontalSpeed.BaseSpeed = default;
        samus.BombJumpRequest = SamusBombJumpRequest.None;

        _acceleration.StartSpark(InitialVerticalAcceleration);
        StartStopTimer = 30;
        ShineTimer = 60;
        PaletteType = SamusSpecialPaletteType.Shinesparking;
        PaletteStep = 0;
        Phase = ShinesparkPhase.Windup;
        samus.HurtFlash.Clear();
    }

    /// <summary>
    /// Installs the launch handler the shinespark's direction selects
    /// for <see cref="SamusPoseId.ShinesparkHorizontalRightPose"/> through <see cref="SamusPoseId.ShinesparkDiagonalLeftPose"/>.
    /// </summary>
    public void BeginDirectionalLaunch(SamusState samus, SamusPoseId targetPose,
        bool deferPoseChange = false)
    {
        Ensure.NotNull(samus);
        // The launch installs its handler over whatever handler is current. A windup
        // whose handler normal movement replaced (a knockback ending in a windup pose) keeps its
        // pose and the windup's speeds, and a direction still launches from it.
        Phase = targetPose switch
        {
            SamusPoseId.ShinesparkHorizontalRightPose or
            SamusPoseId.ShinesparkHorizontalLeftPose => ShinesparkPhase.Horizontal,
            SamusPoseId.ShinesparkVerticalRightPose or
            SamusPoseId.ShinesparkVerticalLeftPose => ShinesparkPhase.Vertical,
            SamusPoseId.ShinesparkDiagonalRightPose or
            SamusPoseId.ShinesparkDiagonalLeftPose => ShinesparkPhase.Diagonal,
            // The launch is reached only from the six table-selected shinespark poses.
            // Any other pose is a caller contract violation, not another launch the game has.
            SamusPoseId.ForwardFacingPowerSuitPose or
                SamusPoseId.ForwardFacingSuitedPose or
                SamusPoseId.FacingRightNormalPose or
                SamusPoseId.FacingLeftNormalPose or
                SamusPoseId.MovingRightNormalPose or
                SamusPoseId.MovingLeftNormalPose or
                SamusPoseId.MovingRightGunExtendedPose or
                SamusPoseId.MovingLeftGunExtendedPose or
                SamusPoseId.CompactAirborneFallingRightPose or
                SamusPoseId.CompactAirborneFallingLeftPose or
                SamusPoseId.MoonwalkFacingLeftPose or
                SamusPoseId.MoonwalkFacingRightPose or
                SamusPoseId.MoonwalkAimUpLeftPose or
                SamusPoseId.MoonwalkAimUpRightPose or
                SamusPoseId.MoonwalkAimDownLeftPose or
                SamusPoseId.MoonwalkAimDownRightPose or
                SamusPoseId.MoonwalkTurnJumpLeftPose or
                SamusPoseId.MoonwalkTurnJumpRightPose or
                SamusPoseId.MoonwalkTurnJumpAimUpLeftPose or
                SamusPoseId.MoonwalkTurnJumpAimUpRightPose or
                SamusPoseId.MoonwalkTurnJumpAimDownLeftPose or
                SamusPoseId.MoonwalkTurnJumpAimDownRightPose or
                SamusPoseId.ShinesparkWindupRightPose or
                SamusPoseId.ShinesparkWindupLeftPose or
                SamusPoseId.CrystalFlashRightPose or
                SamusPoseId.CrystalFlashLeftPose or
                SamusPoseId.XrayingStandingRightPose or
                SamusPoseId.XrayingStandingLeftPose or
                SamusPoseId.DeathSequenceRightPose or
                SamusPoseId.DeathSequenceLeftPose or
                SamusPoseId.XrayingCrouchingRightPose or
                SamusPoseId.XrayingCrouchingLeftPose or
                SamusPoseId.CompactMorphingRightPose or
                SamusPoseId.DraygonHeldMorphingLeftPose or
                SamusPoseId.CompactStandingUpRightPose or
                SamusPoseId.DraygonHeldStandingUpLeftPose or
                SamusPoseId.DraygonHeldBallLeftPose or
                SamusPoseId.DrainedCrouchingRightPose or
                SamusPoseId.DrainedCrouchingLeftPose or
                SamusPoseId.DrainedStandingRightPose or
                SamusPoseId.DrainedStandingLeftPose or
                SamusPoseId.RanIntoWallRightPose or
                SamusPoseId.RanIntoWallLeftPose or
                SamusPoseId.RanIntoWallAimUpRightPose or
                SamusPoseId.RanIntoWallAimUpLeftPose or
                SamusPoseId.RanIntoWallAimDownRightPose or
                SamusPoseId.RanIntoWallAimDownLeftPose or
                SamusPoseId.TurningRightToLeftPose or
                SamusPoseId.TurningLeftToRightPose or
                SamusPoseId.SpinJumpRightPose or
                SamusPoseId.SpinJumpLeftPose or
                SamusPoseId.SpaceJumpRightPose or
                SamusPoseId.SpaceJumpLeftPose or
                SamusPoseId.ScrewAttackRightPose or
                SamusPoseId.ScrewAttackLeftPose or
                SamusPoseId.TurningRightToLeftJumpPose or
                SamusPoseId.TurningLeftToRightJumpPose or
                SamusPoseId.WallJumpRightPose or
                SamusPoseId.WallJumpLeftPose or
                SamusPoseId.GrappleSwingRightPose or
                SamusPoseId.GrappleSwingLeftPose or
                SamusPoseId.GrappleStandingRightPose or
                SamusPoseId.GrappleStandingLeftPose or
                SamusPoseId.GrappleStandingDownRightPose or
                SamusPoseId.GrappleStandingDownLeftPose or
                SamusPoseId.GrappleCrouchingRightPose or
                SamusPoseId.GrappleCrouchingLeftPose or
                SamusPoseId.GrappleCrouchingDownRightPose or
                SamusPoseId.GrappleCrouchingDownLeftPose or
                SamusPoseId.GrappleWallContactLeftPose or
                SamusPoseId.GrappleWallContactRightPose or
                SamusPoseId.DraygonGrabbedNeutralLeftPose or
                SamusPoseId.DraygonGrabbedAimUpLeftPose or
                SamusPoseId.DraygonGrabbedFiringLeftPose or
                SamusPoseId.DraygonGrabbedAimDownLeftPose or
                SamusPoseId.DraygonGrabbedMovingLeftPose or
                SamusPoseId.DraygonGrabbedNeutralRightPose or
                SamusPoseId.DraygonGrabbedAimUpRightPose or
                SamusPoseId.DraygonGrabbedFiringRightPose or
                SamusPoseId.DraygonGrabbedAimDownRightPose or
                SamusPoseId.DraygonGrabbedMovingRightPose or
                SamusPoseId.DamageBoostLeftPose or
                SamusPoseId.DamageBoostRightPose or
                SamusPoseId.KnockbackRightPose or
                SamusPoseId.KnockbackLeftPose or
                SamusPoseId.TurningRightToLeftFallingPose or
                SamusPoseId.TurningLeftToRightFallingPose or
                SamusPoseId.TurningRightToLeftJumpAimUpPose or
                SamusPoseId.TurningLeftToRightJumpAimUpPose or
                SamusPoseId.TurningRightToLeftJumpAimDownPose or
                SamusPoseId.TurningLeftToRightJumpAimDownPose or
                SamusPoseId.TurningRightToLeftFallingAimUpPose or
                SamusPoseId.TurningLeftToRightFallingAimUpPose or
                SamusPoseId.TurningRightToLeftFallingAimDownPose or
                SamusPoseId.TurningLeftToRightFallingAimDownPose or
                SamusPoseId.TurningRightToLeftJumpAimDiagonalUpPose or
                SamusPoseId.TurningLeftToRightJumpAimDiagonalUpPose or
                SamusPoseId.TurningRightToLeftFallingAimDiagonalUpPose or
                SamusPoseId.TurningLeftToRightFallingAimDiagonalUpPose or
                SamusPoseId.MorphBallGroundRightPose or
                SamusPoseId.MorphBallMovingRightPose or
                SamusPoseId.MorphBallMovingLeftPose or
                SamusPoseId.MorphBallFallingRightPose or
                SamusPoseId.MorphBallFallingLeftPose or
                SamusPoseId.UnusedKnockbackRightPose or
                SamusPoseId.UnusedKnockbackLeftPose or
                SamusPoseId.MorphingTransitionRightPose or
                SamusPoseId.MorphingTransitionLeftPose or
                SamusPoseId.UnmorphingTransitionRightPose or
                SamusPoseId.UnmorphingTransitionLeftPose or
                SamusPoseId.MorphBallGroundLeftPose or
                SamusPoseId.SpringBallGroundRightPose or
                SamusPoseId.SpringBallGroundLeftPose or
                SamusPoseId.SpringBallMovingRightPose or
                SamusPoseId.SpringBallMovingLeftPose or
                SamusPoseId.SpringBallFallingRightPose or
                SamusPoseId.SpringBallFallingLeftPose or
                SamusPoseId.SpringBallJumpRightPose or
                SamusPoseId.SpringBallJumpLeftPose or
                SamusPoseId.FallingRightPose or
                SamusPoseId.FallingLeftPose or
                SamusPoseId.FallingGunExtendedRightPose or
                SamusPoseId.FallingGunExtendedLeftPose or
                SamusPoseId.StandingAimUpRightPose or
                SamusPoseId.StandingAimUpLeftPose or
                SamusPoseId.StandingAimDiagonalUpRightPose or
                SamusPoseId.StandingAimDiagonalUpLeftPose or
                SamusPoseId.StandingAimDiagonalDownRightPose or
                SamusPoseId.StandingAimDiagonalDownLeftPose or
                SamusPoseId.RunningAimUpRightPose or
                SamusPoseId.RunningAimUpLeftPose or
                SamusPoseId.RunningAimDiagonalUpRightPose or
                SamusPoseId.RunningAimDiagonalUpLeftPose or
                SamusPoseId.RunningAimDiagonalDownRightPose or
                SamusPoseId.RunningAimDiagonalDownLeftPose or
                SamusPoseId.NormalJumpGunExtendedRightPose or
                SamusPoseId.NormalJumpGunExtendedLeftPose or
                SamusPoseId.NormalJumpAimUpRightPose or
                SamusPoseId.NormalJumpAimUpLeftPose or
                SamusPoseId.NormalJumpAimDownRightPose or
                SamusPoseId.NormalJumpAimDownLeftPose or
                SamusPoseId.NormalJumpForwardRightPose or
                SamusPoseId.NormalJumpForwardLeftPose or
                SamusPoseId.NormalJumpTransitionAimUpRightPose or
                SamusPoseId.NormalJumpTransitionAimUpLeftPose or
                SamusPoseId.NormalJumpTransitionAimDiagonalUpRightPose or
                SamusPoseId.NormalJumpTransitionAimDiagonalUpLeftPose or
                SamusPoseId.NormalJumpTransitionAimDiagonalDownRightPose or
                SamusPoseId.NormalJumpTransitionAimDiagonalDownLeftPose or
                SamusPoseId.NormalJumpAimDiagonalUpRightPose or
                SamusPoseId.NormalJumpAimDiagonalUpLeftPose or
                SamusPoseId.NormalJumpAimDiagonalDownRightPose or
                SamusPoseId.NormalJumpAimDiagonalDownLeftPose or
                SamusPoseId.FallingAimUpRightPose or
                SamusPoseId.FallingAimUpLeftPose or
                SamusPoseId.FallingAimDownRightPose or
                SamusPoseId.FallingAimDownLeftPose or
                SamusPoseId.FallingAimDiagonalUpRightPose or
                SamusPoseId.FallingAimDiagonalUpLeftPose or
                SamusPoseId.FallingAimDiagonalDownRightPose or
                SamusPoseId.FallingAimDiagonalDownLeftPose or
                SamusPoseId.CrouchingAimDiagonalUpRightPose or
                SamusPoseId.CrouchingAimDiagonalUpLeftPose or
                SamusPoseId.CrouchingAimDiagonalDownRightPose or
                SamusPoseId.CrouchingAimDiagonalDownLeftPose or
                SamusPoseId.CrouchingAimUpRightPose or
                SamusPoseId.CrouchingAimUpLeftPose or
                SamusPoseId.TurningRightToLeftCrouchingPose or
                SamusPoseId.TurningLeftToRightCrouchingPose or
                SamusPoseId.TurningRightToLeftAimUpPose or
                SamusPoseId.TurningLeftToRightAimUpPose or
                SamusPoseId.TurningRightToLeftAimDiagonalDownPose or
                SamusPoseId.TurningLeftToRightAimDiagonalDownPose or
                SamusPoseId.TurningRightToLeftAimDiagonalUpPose or
                SamusPoseId.TurningLeftToRightAimDiagonalUpPose or
                SamusPoseId.TurningRightToLeftCrouchingAimUpPose or
                SamusPoseId.TurningLeftToRightCrouchingAimUpPose or
                SamusPoseId.TurningRightToLeftCrouchingAimDiagonalDownPose or
                SamusPoseId.TurningLeftToRightCrouchingAimDiagonalDownPose or
                SamusPoseId.TurningRightToLeftCrouchingAimDiagonalUpPose or
                SamusPoseId.TurningLeftToRightCrouchingAimDiagonalUpPose or
                SamusPoseId.LandingAimUpRightPose or
                SamusPoseId.LandingAimUpLeftPose or
                SamusPoseId.LandingAimDiagonalUpRightPose or
                SamusPoseId.LandingAimDiagonalUpLeftPose or
                SamusPoseId.LandingAimDiagonalDownRightPose or
                SamusPoseId.LandingAimDiagonalDownLeftPose or
                SamusPoseId.FiringLandingRightPose or
                SamusPoseId.FiringLandingLeftPose or
                SamusPoseId.CrouchingTransitionAimUpRightPose or
                SamusPoseId.CrouchingTransitionAimUpLeftPose or
                SamusPoseId.CrouchingTransitionAimDiagonalUpRightPose or
                SamusPoseId.CrouchingTransitionAimDiagonalUpLeftPose or
                SamusPoseId.CrouchingTransitionAimDiagonalDownRightPose or
                SamusPoseId.CrouchingTransitionAimDiagonalDownLeftPose or
                SamusPoseId.StandingTransitionAimUpRightPose or
                SamusPoseId.StandingTransitionAimUpLeftPose or
                SamusPoseId.StandingTransitionAimDiagonalUpRightPose or
                SamusPoseId.StandingTransitionAimDiagonalUpLeftPose or
                SamusPoseId.StandingTransitionAimDiagonalDownRightPose or
                SamusPoseId.StandingTransitionAimDiagonalDownLeftPose or
                SamusPoseId.CrouchingRightPose or
                SamusPoseId.CrouchingLeftPose or
                SamusPoseId.CrouchingTransitionRightPose or
                SamusPoseId.CrouchingTransitionLeftPose or
                SamusPoseId.StandingTransitionRightPose or
                SamusPoseId.StandingTransitionLeftPose or
                SamusPoseId.NeutralJumpTransitionRightPose or
                SamusPoseId.NeutralJumpTransitionLeftPose or
                SamusPoseId.NeutralJumpRightPose or
                SamusPoseId.NeutralJumpLeftPose or
                SamusPoseId.NormalLandingRightPose or
                SamusPoseId.NormalLandingLeftPose or
                SamusPoseId.SpinLandingRightPose or
                SamusPoseId.SpinLandingLeftPose or
                SamusPoseId.GlitchBallRightPose or
                SamusPoseId.GlitchBallMorphingRightPose or
                SamusPoseId.GlitchBallMorphingLeftPose or
                SamusPoseId.InstantMorphingRightPose or
                SamusPoseId.InstantMorphingLeftPose or
                SamusPoseId.GlitchBallLeftPose or
                SamusPoseId.CompactAirborneRightPose or
                SamusPoseId.CompactAirborneLeftPose or
                SamusPoseId.GrappleAirborneGunExtendedRightPose or
                SamusPoseId.GrappleAirborneAimDownRightPose or
                SamusPoseId.GrappleAirborneAimDiagonalDownRightPose or
                SamusPoseId.DraygonGrabbingLeftPose => throw new ArgumentOutOfRangeException(
                nameof(targetPose),
                $"Pose {targetPose} is not one of the six directional shinespark poses."),
            SamusPoseId.FirstUnauthoredPose or
                SamusPoseId.SecondUnauthoredPose or
                SamusPoseId.LastUnauthoredPose => throw new ArgumentOutOfRangeException(
                nameof(targetPose),
                $"Pose {targetPose} is not one of the six directional shinespark poses."),
            var undefined => throw new InvalidOperationException($"Undefined {nameof(SamusPoseId)} {undefined}."),
        };

        samus.InputSnapshot.ShinesparkPoseInputLocked = true;
        samus.InputSnapshot.AutoJumpInputPending = false;
        if (!deferPoseChange)
            ApplyDirectionalLaunchPose(samus, targetPose);
        samus.HorizontalSpeed.ClearSpeedEchoes();
        AudioRequests.QueueSound(RoomSound.Shinespark, SoundRequestPriority.High);
    }

    /// <summary>Commits the interrupted launch pose after the current frame's animation.</summary>
    internal static void ApplyDirectionalLaunchPose(SamusState samus, SamusPoseId pose)
    {
        samus.Pose = pose;
        SamusPoseMetadata.RefreshCollisionRadii(samus);
        SamusPresentation.InitializeAnimation(samus, initialFrame: 0);
    }

    /// <summary>Runs one installed special-handler frame.</summary>
    /// <param name="terrain">The room terrain Samus's block collision probes: the level data and the room objects that react to touched blocks (#633).</param>
    /// <param name="samus">Live player state; movement, energy, contact damage, pose state, and echo coordinates may change during the call.</param>
    /// <param name="frame">What one shinespark step reads from the frame: clocks, projectiles, invincibility, timeout policy and the previous checkpoint (#633).</param>
    public ShinesparkMovementResult Step(SamusTerrain terrain, SamusState samus, ShinesparkFrame frame)
    {

        Ensure.NotNull(terrain.Level);
        Ensure.NotNull(samus);

        if (Phase == ShinesparkPhase.Windup)
        {
            // The windup countdown: exact zero and signed underflow both launch up.
            bool timedOut = --StartStopTimer <= 0;
            SamusPoseId? pendingLaunchPose = null;
            if (timedOut)
            {
                SamusPoseId verticalPose = SamusPoseMetadata.IsFacing(samus, SamusFacingDirection.Left)
                    ? SamusPoseId.ShinesparkVerticalLeftPose
                    : SamusPoseId.ShinesparkVerticalRightPose;
                BeginDirectionalLaunch(samus, verticalPose, frame.DeferTimeoutPoseChange);
                if (frame.DeferTimeoutPoseChange)
                    pendingLaunchPose = verticalPose;
            }

            return new ShinesparkMovementResult(
timedOut,                 PendingLaunchPose: pendingLaunchPose);
        }

        if (Phase is not (ShinesparkPhase.Horizontal or
            ShinesparkPhase.Vertical or ShinesparkPhase.Diagonal))
        {
            if (Phase == ShinesparkPhase.Crash)
                return StepCrashOrbit(samus);
            if (Phase == ShinesparkPhase.CrashEchoCircle)
                return StepCrashEchoCircle();
            if (Phase == ShinesparkPhase.CrashFinish)
                return FinishCrash(samus, frame.Projectiles);


            // Stored/inactive states are not installed special movement handlers. Returning
            // an empty snapshot is defensive; runtime normally never dispatches them here.
            return new ShinesparkMovementResult(
false);
        }

        samus.HorizontalSpeed.ContactDamageIndex = SamusContactDamageIndex.Shinespark;
        samus.HurtFlash.HoldAt(MovingSparkHurtFlashCount);

        // Each active handler samples echoes before motion. Runtime suppresses its ordinary
        // post-motion producer while this special handler owns the frame.
        samus.HorizontalSpeed.CaptureSpeedEchoPosition(frame.GameTimeFrames, samus.Position.Whole);

        BlockMoveResult? horizontal = null;
        BlockMoveResult? vertical = null;
        // Each axis ends by capping how far the camera's previous position trails
        // Samus. Only a camera reads it; fixtures without one pass no checkpoint.
        if (Phase is ShinesparkPhase.Horizontal or ShinesparkPhase.Diagonal)
        {
            horizontal = MoveX(terrain.Level, samus, terrain.RoomObjects);
            if (frame.PreviousCheckpoint is { } checkpoint)
                CapPreviousX(samus, checkpoint);
        }
        if (Phase is ShinesparkPhase.Vertical or ShinesparkPhase.Diagonal)
        {
            vertical = MoveY(terrain.Level, samus, frame.FrameCounter, terrain.RoomObjects);
            if (frame.PreviousCheckpoint is { } checkpoint)
                CapPreviousY(samus, checkpoint);
        }

        // The game's X and Y movement share one collision flag. Diagonal movement runs Y
        // last, so a clear vertical probe replaces an earlier wall hit and allows the
        // spark to slide upward. Combining both results would terminate a retail spark
        // early; use the last executed axis, including when its result is clear.
        bool collided = vertical?.Collided ?? horizontal?.Collided ?? false;
        // The cannot-die assist bypasses only the energy exit. Terrain/enemy collision still
        // terminates all three launch directions through the ordinary crash handler.
        bool hasSustainingEnergy = samus.Health >= ShinesparkDefinitions.MinimumSustainingEnergy;
        bool lowEnergy = !frame.SamusCannotDie && !hasSustainingEnergy;
        if (collided || lowEnergy)
            BeginCrash(samus);

        // At exactly 30 energy, this frame still moves and drains to 29. The following
        // frame moves once more before the shinespark's end check observes that 29 is below the threshold.
        // With the cannot-die assist, energy loss continues without requiring refills: below
        // the cutoff, continue draining to one, never decrementing zero or one.
        bool drained = frame.SamusCannotDie
            ? samus.Health > 1
            : hasSustainingEnergy;
        if (drained)
            samus.Health = SamusQuantity.DecrementPositive(samus.Health);

        return new ShinesparkMovementResult(
false);
    }

    /// <summary>
    /// Runs the stored shine's or the shinespark's suit colouring, restoring Samus's
    /// normal colours when its signed countdown expires.
    /// </summary>
    public bool UpdatePalette(SamusColours colours, SuperMetroid.Core.Progression.SamusEquipmentFlags equippedItems)
    {
        Ensure.NotNull(colours);
        if (PaletteType is not (SamusSpecialPaletteType.SpeedBoosterShine or SamusSpecialPaletteType.Shinesparking))
            return false;

        if (PaletteType == SamusSpecialPaletteType.SpeedBoosterShine && ShineTimer == 170)
            AudioRequests.QueueSound(RoomSound.StoredShinespark, SoundRequestPriority.High);

        bool expired = --ShineTimer <= 0;
        if (expired)
        {
            PaletteType = SamusSpecialPaletteType.None;
            PaletteStep = 0;
            if (Phase == ShinesparkPhase.Stored)
                Phase = ShinesparkPhase.Inactive;
            SamusNormalSuitPalette.Load(colours, equippedItems);
            return true;
        }

        SamusSuit suit = equippedItems.ShownSuit();
        bool catalogued = PaletteType == SamusSpecialPaletteType.SpeedBoosterShine
            ? SamusFullBodyCyclePalettes.TryStoredShinePalette(
                suit, PaletteStep, out SamusFullBodyCyclePalette palette)
            : SamusFullBodyCyclePalettes.TryActiveShinesparkPalette(
                suit, PaletteStep, out palette);
        if (!catalogued)
            throw new InvalidDataException(
                $"Shinespark palette type {PaletteType} step {PaletteStep} is not compiled.");
        colours.ShowSuit(SamusShades.FullBodyCycle(palette));

        int steps = PaletteType == SamusSpecialPaletteType.SpeedBoosterShine
            ? SamusPaletteDefinitions.FullBodyCycles.StoredShineSteps
            : SamusPaletteDefinitions.FullBodyCycles.ActiveShinesparkSteps;
        // The step is always below its limit before this advance, so it never approaches a wrap.
        PaletteStep++;
        if (PaletteStep >= steps)
            PaletteStep = 0;
        return true;
    }

    /// <summary>
    /// Stops translational spark motion, sets the orbit's angles and turn, ends the boost's echoes, places both orbiting
    /// copies on Samus and installs the crash handler. The orbit's travel is not restarted: the orbit counts on from
    /// what the last orbit or the first echo it released left (<see cref="CrashOrbitTravelCarryover"/>).
    /// </summary>
    private void BeginCrash(SamusState samus)
    {
        // The crash entry kills translational velocity before installing the crash handler.
        samus.Kinematics.YSpeed = default;
        samus.Kinematics.YDirection = SamusVerticalDirection.None;
        samus.HorizontalSpeed.ExtraRunSpeed = default;
        samus.HorizontalSpeed.SpeedBoostCounter = default;
        // Ending the shinespark retains this frame's contact damage. The shared frame
        // prologue clears it before the newly installed crash handler runs next.
        samus.HurtFlash.Clear();
        bool facingLeft = SamusPoseMetadata.IsFacing(samus, SamusFacingDirection.Left);
        FirstCrashEchoAngle = RotationAngle.FromSteps(facingLeft ? 32 : 224);
        SecondCrashEchoAngle = RotationAngle.FromSteps(facingLeft ? 160 : 96);
        _crashOrbitTurn = facingLeft ? CrashOrbitTurnSteps : -CrashOrbitTurnSteps;
        CrashOrbitStage = ShinesparkCrashOrbitStage.Expanding;
        CrashRadius = 0;
        samus.HorizontalSpeed.ClearSpeedEchoes();
        FirstOrbitingCopy = SecondOrbitingCopy = samus.Position.Whole;
        Phase = ShinesparkPhase.Crash;
        // The crash entry queues its two sounds together.
        AudioRequests.QueueSound(SamusSound.SamusDamaged, SoundRequestPriority.Normal);
        AudioRequests.QueueSound(RoomSound.ShinesparkEnd, SoundRequestPriority.Normal);
    }

    /// <summary>The steps of a 256-step turn the orbiting copies turn by on each orbiting call.</summary>
    private const int CrashOrbitTurnSteps = 4;

    /// <summary>Runs the crash's orbit: its expanding, orbiting and contracting stages.</summary>
    private ShinesparkMovementResult StepCrashOrbit(
        SamusState samus)
    {
        ShineTimer = 15;
        switch (CrashOrbitStage)
        {
            case ShinesparkCrashOrbitStage.Expanding:
            {
                // The crash tests the old radius against twelve, after calculating +4.
                // The transition frame therefore publishes radius sixteen and the orbiting stage.
                int oldRadius = CrashRadius;
                CrashRadius += 4;
                if (oldRadius >= 12)
                    CrashOrbitStage = ShinesparkCrashOrbitStage.Orbiting;
                break;
            }

            case ShinesparkCrashOrbitStage.Orbiting:
                FirstCrashEchoAngle = FirstCrashEchoAngle.TurnedBySteps(_crashOrbitTurn);
                SecondCrashEchoAngle = SecondCrashEchoAngle.TurnedBySteps(_crashOrbitTurn);
                if (_orbitTravel.Advance())
                    CrashOrbitStage = ShinesparkCrashOrbitStage.Contracting;
                break;

            case ShinesparkCrashOrbitStage.Contracting:
                CrashRadius -= 4;
                if (CrashRadius == 0)
                {
                    Phase = ShinesparkPhase.CrashEchoCircle;
                    StartStopTimer = 30;
                    CrashOrbitStage = ShinesparkCrashOrbitStage.Expanding;
                    FirstCrashEchoAngle = RotationAngle.Zero;
                    SecondCrashEchoAngle = RotationAngle.Zero;
                }
                break;

            default:
                throw new InvalidOperationException(
                    $"Shinespark crash orbit stage {CrashOrbitStage} is undefined.");
        }

        // The copies stand the crash radius, 0 through 16, from Samus along their angles; placing them past the room's edge
        // continues from its other end, as every room position does.
        FirstOrbitingCopy = samus.Position.Whole + ProjectileSinLookup(FirstCrashEchoAngle, CrashRadius);
        SecondOrbitingCopy = samus.Position.Whole + ProjectileSinLookup(SecondCrashEchoAngle, CrashRadius);

        return new ShinesparkMovementResult(
false);
    }

    /// <summary>Runs the 30-frame center echo hold.</summary>
    private ShinesparkMovementResult StepCrashEchoCircle()
    {
        ShineTimer = 15;
        if (--StartStopTimer <= 0)
            Phase = ShinesparkPhase.CrashFinish;
        return new ShinesparkMovementResult(
false);
    }

    /// <summary>
    /// The crash's end: releases its echoes on Samus's shots as far as her shot count leaves room, restores standing,
    /// and expires the shinesparking palette. A released first echo leaves Samus's height as the next orbit's travel; a
    /// crash that releases no first echo leaves the travel its own orbit ended on.
    /// </summary>
    internal ShinesparkMovementResult FinishCrash(SamusState samus, SamusProjectileSystem projectiles)
    {
        Ensure.NotNull(projectiles);
        // Resolve the crash axis before publishing the standing pose.
        var departureAngles = ShinesparkEchoDepartures.DepartureAngles(samus.Pose, projectiles.DrainedCrashDepartures);
        samus.HorizontalSpeed.ClearSpeedEchoes();

        if (projectiles.Shinespark.ReleaseCrashEchoes(FirstReleasedEcho, SecondReleasedEcho, departureAngles,
                SamusBeamCombinations.Of(samus.EquippedBeams), samus.Position.Whole))
            _orbitTravel.LeaveEchoHeight(samus.Position.Whole.Y);

        ShineTimer = 1;
        _acceleration.StopSpark();
        Phase = ShinesparkPhase.Inactive;
        return new ShinesparkMovementResult(
false,             CrashSequenceFinished: true);
    }

    /// <summary>Commits the crash's transitional standing pose after this frame's animation.</summary>
    public static void ApplyCrashFinishPose(SamusState samus)
    {
        // Finishing the crash restores the normal input mode as well as the movement mode. That
        // replaces a drained pose's movement handler too: a Samus drained mid-crash is free.
        samus.InputSnapshot.ShinesparkPoseInputLocked = false;
        samus.Drained.RelinquishMovementHandler();
        SamusPoseId standingPose = SamusPoseMetadata.IsFacing(samus, SamusFacingDirection.Left)
            ? SamusPoseId.FacingLeftNormalPose
            : SamusPoseId.FacingRightNormalPose;
        int previousRadius = samus.Kinematics.Radius.Y;
        samus.Pose = standingPose;
        // The transitional-pose handler restores normal movement and
        // aligns the standing body's bottom with the old crash body. The input phase alone
        // publishes the new live radius on the following frame.
        samus.AlignBottomAfterPoseChange(previousRadius, SamusPoseMetadata.ReadPoseYRadius(standingPose));
        SamusPresentation.InitializeAnimation(samus, initialFrame: 0);
    }

    /// <summary>The largest radius an echo's shot holds, in pixels; one more expansion continues from zero.</summary>
    internal const int LargestEchoRadius = 65535;

    /// <summary>The radii the sine lookup places an echo at, in pixels; a radius past them places it from Samus again.</summary>
    private const int EchoPlacementRadii = 256;

    /// <summary>Pixels an echo's radius grows by on each update.</summary>
    private const int EchoRadiusStep = 8;

    /// <summary>How far right of or below the camera's corner an echo may lie, in pixels, before it leaves the screen.</summary>
    private const int EchoScreenReach = 256;

    /// <summary>
    /// Runs one update of the released echo <paramref name="shot"/> carries: its radius grows by eight, continuing past
    /// <see cref="LargestEchoRadius"/> from zero, and the echo is placed that far from Samus's current position along its
    /// angle, with the radius taken within the lookup's 256 pixels, so an echo still on the screen at 248 pixels returns
    /// to Samus. An echo placed left of or above the camera's corner, or <see cref="EchoScreenReach"/> or more right of
    /// or below it, has left the screen; the first echo leaves its height for the crash's orbit, or nothing once it has
    /// left (<see cref="CrashOrbitTravelCarryover"/>).
    /// </summary>
    /// <param name="samus">Samus, around whom the echo is placed.</param>
    /// <param name="cameraPosition">The camera's room position.</param>
    /// <param name="shot">The shot carrying one of the crash's released echoes.</param>
    /// <returns>Whether the echo is still on the screen; its shot is cleared otherwise.</returns>
    /// <exception cref="InvalidOperationException">The shot carries neither of the crash's released echoes.</exception>
    internal bool StepReleasedEcho(SamusState samus, Position16<RoomPixels> cameraPosition, SamusProjectile shot)
    {
        ShinesparkCrashEcho echo = FirstReleasedEcho.IsCarriedBy(shot) ? FirstReleasedEcho
            : SecondReleasedEcho.IsCarriedBy(shot) ? SecondReleasedEcho
            : throw new InvalidOperationException("The shot carries none of the crash's released echoes.");

        // The point is recalculated from Samus's current position on every update; these are expanding radial
        // copies, not ordinary projectiles with independent velocity.
        int expanded = shot.EchoRadius + EchoRadiusStep;
        shot.EchoRadius = expanded > LargestEchoRadius ? expanded - (LargestEchoRadius + 1) : expanded;
        Position16<RoomPixels> point = samus.Position.Whole +
            ProjectileSinLookup(shot.EchoAngle.ToAngle(), shot.EchoRadius % EchoPlacementRadii);
        bool onScreen = CoordinateAxis.Difference(point.X, cameraPosition.X) is >= 0 and < EchoScreenReach &&
            CoordinateAxis.Difference(point.Y, cameraPosition.Y) is >= 0 and < EchoScreenReach;

        if (echo == FirstReleasedEcho)
        {
            if (onScreen)
                _orbitTravel.LeaveEchoHeight(point.Y);
            else
                _orbitTravel.Clear();
        }
        if (!onScreen)
            return false;

        echo.MoveTo(point);
        shot.Position = shot.Position.WithWhole(point);
        return true;
    }

    /// <summary>Exact split multiplication used by the crash's sine and cosine.</summary>
    internal static Offset16<RoomPixels> ProjectileSinLookup(
        RotationAngle angle,
        int radius)
    {
        int x = LookupSignedComponent(angle, radius);
        int y = LookupSignedComponent(
            angle - RotationAngle.QuarterTurn,
            radius);
        return new(x, y);
    }

    /// <summary>Multiplies a positive sine half-wave and restores its sign after truncation, matching the projectile table.</summary>
    private static int LookupSignedComponent(
        RotationAngle angle,
        int radius)
    {
        // The game multiplies the positive half-wave, discards the fraction, then
        // restores sign. Shifting a negative product would round differently.
        return Trigonometry.ScaledBySine(radius, angle.ToTrigonometryAngle());
    }

    /// <summary>
    /// After horizontal spark movement, a previous X more than
    /// 15 pixels behind Samus is pulled to 15 behind, in either direction.
    /// </summary>
    private static void CapPreviousX(SamusState samus, FixedPosition<RoomPixels> checkpoint)
    {
        int delta = CoordinateAxis.Difference(samus.Position.X.Whole, samus.PeekPreviousPosition(checkpoint).X);
        if (delta >= 0)
        {
            if (delta >= 16)
                samus.WritePreviousXPosition(CoordinateAxis.Wrap(samus.Position.X.Whole - 15));
        }
        else if (delta < -15)
        {
            samus.WritePreviousXPosition(CoordinateAxis.Wrap(samus.Position.X.Whole + 15));
        }
    }

    /// <summary>
    /// After vertical spark movement, a previous Y more than 14
    /// pixels below Samus is pulled to 14 below.
    /// </summary>
    private static void CapPreviousY(SamusState samus, FixedPosition<RoomPixels> checkpoint)
    {
        // Both positions lie in one room, far closer than half the coordinate range.
        if (CoordinateAxis.Difference(samus.Position.Y.Whole, samus.PeekPreviousPosition(checkpoint).Y) < -14)
            samus.WritePreviousYPosition(CoordinateAxis.Wrap(samus.Position.Y.Whole + 14));
    }

    /// <summary>Accelerates horizontal spark speed up to fifteen pixels per frame and probes breakable room blocks.</summary>
    private BlockMoveResult MoveX(

        RoomLevelData level,
        SamusState samus,
        ISamusRoomObjects? roomObjects)
    {
        ShineTimer = 15;
        samus.HorizontalSpeed.ExtraRunSpeed += samus.Kinematics.YAcceleration;
        if (samus.HorizontalSpeed.ExtraRunSpeed.Whole >= 15)
        {
            samus.HorizontalSpeed.ExtraRunSpeed = new UnsignedSixteenSixteen(15, 0);
        }

        var displacement = SamusHorizontalDisplacement.Toward(SamusPoseMetadata.IsFacing(samus, SamusFacingDirection.Left), samus, baseSpeed: default);
        return SamusBlockCollision.MoveHorizontal(new SamusTerrain { Level = level, RoomObjects = roomObjects }, samus.Kinematics, samus, new HorizontalBlockMove { Displacement = displacement.Displacement, CanBreakBombBlocks = true, CollisionMovementDirection = displacement.CollisionDirection });
    }

    /// <summary>Accumulates upward spark acceleration while capping only the temporary collision displacement and probing solid enemies first.</summary>
    private BlockMoveResult MoveY(

        RoomLevelData level,
        SamusState samus,
        int frameCounter,
        ISamusRoomObjects? roomObjects)
    {
        ShineTimer = 15;
        _acceleration.Accelerate(samus.Kinematics.YAcceleration.AsDisplacement());
        samus.Kinematics.YSpeed += _acceleration.SparkAcceleration;

        // The clamp operates on a temporary displacement, not stored velocity.
        // Keep accumulating the speed even after movement reaches its cap.
        UnsignedSixteenSixteen displacementSpeed = samus.Kinematics.YSpeed.Whole >= 14
            ? samus.Kinematics.YSpeed.WithWhole(14)
            : samus.Kinematics.YSpeed;
        SignedSixteenSixteen upwardDisplacement = SamusExtraDisplacement.AddToVerticalSpeedDisplacement(
            samus.Kinematics,
            -displacementSpeed.AsDisplacement());

        // The upward move temporarily negates the signed result, clamps only a positive
        // upward magnitude at 15 whole pixels while preserving its fraction, then negates
        // it back before the room move. If external Y is strong enough to reverse motion
        // downward, the temporary value is negative and deliberately bypasses this clamp.
        // The cap test is the sign of the whole pixels' difference from fifteen, which wraps for a magnitude more
        // than 32753 pixels upward or downward, as the game's comparison does.
        SignedSixteenSixteen collisionMagnitude = -upwardDisplacement;
        if (CoordinateAxis.Difference(collisionMagnitude.WholePixels, 15) >= 0)
        {
            collisionMagnitude = SignedSixteenSixteen.FromParts(15, collisionMagnitude.Fraction);
            upwardDisplacement = -collisionMagnitude;
        }
        // Shinespark's solid-enemy branch sets collision and returns without
        // adding the clipped distance to Y. The ordinary upward mover instead advances
        // to the enemy boundary; using it here shifts the entire crash/recovery anchor.
        UnsignedSixteenSixteen enemyProbeDistance = UnsignedSixteenSixteen.FromDisplacement(collisionMagnitude);
        SolidEnemyCollisionResult enemy = SamusSolidEnemyCollision.Probe(
            samus.Kinematics, samus.Kinematics.InteractiveEnemies, SamusCollisionDirection.Up,
            enemyProbeDistance.Whole, enemyProbeDistance.Fraction);
        samus.Kinematics.RecordSolidEnemyCollision(SamusCollisionDirection.Up, enemy.Enemy);
        if (enemy.Collided)
            return new BlockMoveResult(SignedSixteenSixteen.Zero, true, EnemyCollision: enemy);

        return SamusBlockCollision.MoveVertical(new SamusTerrain { Level = level, RoomObjects = roomObjects }, samus.Kinematics, samus, new VerticalBlockMove { Displacement = upwardDisplacement, ScanLeftToRight = SamusCadenceDefinitions.BlockScansLeftToRight.IsDue(frameCounter), CanBreakBombBlocks = true, IncludeSolidEnemies = false });
    }
}
