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
/// This is a transcription of the state machine, not a desktop “dash”
/// controller. Timers wrap as 16-bit words, movement retains split 16.16 values, and every
/// palette is selected through the user's installed artwork. Solid-enemy collision remains an explicit
/// boundary; block collision uses the translated dispatcher.
/// </remarks>
public sealed class SamusShinesparkState
{
    /// <summary>Live Power Bomb state consulted when a spark sound request is produced.</summary>
    private SamusPowerBombExplosionState? _audioPowerBomb;
    /// <summary>The session's held beam charge and flare, canceled when spark windup begins.</summary>
    private BeamChargeState? _charge;
    /// <summary>One-frame library-one sound request from canceling a sufficiently charged flare.</summary>
    private bool _chargeCancellationSoundRequested;

    /// <summary>
    /// Binds the room's projectile and sound owners before movement/palette production.
    /// </summary>
    internal void BindProjectileOwners(
        BeamChargeState charge,
        SamusPowerBombExplosionState powerBomb)
    {
        _charge = charge;
        _audioPowerBomb = powerBomb;
    }

    /// <summary>Queue guard captured at the stored-shine warning call, not at publication.</summary>
    public bool StoredShineWarningSoundSuppressed { get; private set; }
    /// <summary>Queue guard captured at directional launch.</summary>
    public bool LaunchSoundSuppressed { get; private set; }
    /// <summary>Queue guard shared by the two adjacent crash-entry sound calls.</summary>
    public bool CrashSoundSuppressed { get; private set; }
    /// <summary>Special-palette handler this owner installs: none, stored shine, or shinesparking.</summary>
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

    /// <summary>
    /// The upward spark's 16.16 acceleration in pixels per frame per frame: 7.0 when windup begins, raised by
    /// Samus's vertical acceleration on every vertical or diagonal spark call, and cleared when the crash finishes.
    /// Its storage is shared with the suit pickup's scratch, which can replace it (see
    /// <see cref="ApplySuitPickupScratch"/>); the adds wrap at 32 bits exactly as the game's word pair does.
    /// </summary>
    public SignedSixteenSixteen VerticalAcceleration { get; private set; }

    /// <summary>
    /// Mirrors the suit pickup's stage number and light-beam position, which share their
    /// storage with the spark acceleration pair the spark later reads.
    /// A suspended windup must not retain its pre-pickup 7.0000 initializer.
    /// </summary>
    /// <param name="substate">The suit pickup's stage number, which becomes the acceleration's whole word.</param>
    /// <param name="lightBeamPosition">The suit pickup's light-beam word, which becomes the acceleration's fraction.</param>
    internal void ApplySuitPickupScratch(int substate, int lightBeamPosition) =>
        VerticalAcceleration = SignedSixteenSixteen.FromParts(substate, lightBeamPosition);

    /// <summary>The shinespark movement handler currently installed.</summary>
    public ShinesparkPhase Phase { get; internal set; }

    /// <summary>
    /// Relinquishes the movement-handler pointer when normal movement is installed.
    /// It does not clear the independently owned palette timer, boost counter, or echoes;
    /// Draygon's unconditional release relies on that distinction.
    /// </summary>
    internal void RelinquishMovementHandler() => Phase = ShinesparkPhase.Inactive;

    /// <summary>
    /// Whether the spark's routine is installed in the single movement-handler word: the windup setup
    /// installs windup, and launch and crash replace it in turn until the crash finishes.
    /// </summary>
    public bool OwnsMovementHandler => Phase is
        ShinesparkPhase.Windup or ShinesparkPhase.Horizontal or ShinesparkPhase.Vertical or
        ShinesparkPhase.Diagonal or ShinesparkPhase.Crash or ShinesparkPhase.CrashEchoCircle or
        ShinesparkPhase.CrashFinish;

    /// <summary>
    /// CrystalFlash replaces both the movement pointer and shared special
    /// palette words. The new owner holds those words; do not resume this old owner
    /// after Flash finishes. Extra run speed and the boost counter are not cleared.
    /// </summary>
    internal void RelinquishToCrystalFlash()
    {
        Phase = ShinesparkPhase.Inactive;
        ShineTimer = 0;
        PaletteType = SamusSpecialPaletteType.None;
        PaletteStep = 0;
    }

    /// <summary>Set at stored timer 170, requesting the stored-shine warning sound.</summary>
    public bool StoredShineWarningSoundRequested { get; set; }

    /// <summary>Set when direction selection requests the shinespark launch sound.</summary>
    public bool LaunchSoundRequested { get; set; }

    /// <summary>Set when energy or collision installs the crash handler.</summary>
    public bool CrashSoundRequested { get; set; }

    /// <summary>Consumes the stored-shine warning's high-priority sound request.</summary>
    public bool ConsumeStoredShineWarningSoundRequest()
    {
        bool requested = StoredShineWarningSoundRequested;
        StoredShineWarningSoundRequested = false;
        return requested;
    }

    /// <summary>Consumes directional launch's high-priority sound request.</summary>
    public bool ConsumeLaunchSoundRequest()
    {
        bool requested = LaunchSoundRequested;
        LaunchSoundRequested = false;
        return requested;
    }

    /// <summary>
    /// Consumes crash setup, which requests two sounds at normal priority: one in library one and one in
    /// library three.
    /// </summary>
    public bool ConsumeCrashSoundRequest()
    {
        bool requested = CrashSoundRequested;
        CrashSoundRequested = false;
        return requested;
    }

    /// <summary>
    /// Consumes the shinespark windup's charge-cancellation sound request,
    /// emitted only when windup cancels a charge whose counter reached sixteen.
    /// </summary>
    public bool ConsumeChargeCancellationSoundRequest()
    {
        bool requested = _chargeCancellationSoundRequested;
        _chargeCancellationSoundRequested = false;
        return requested;
    }

    /// <summary>Producer-time Power Bomb guard for the pending charge-cancellation call.</summary>
    public bool ChargeCancellationSoundSuppressed { get; private set; }

    /// <summary>
    /// The stage of the crash echoes' orbit: expanding at crash entry, orbiting once the radius reaches sixteen,
    /// and contracting once the angular travel reaches 128.
    /// </summary>
    public ShinesparkCrashOrbitStage CrashOrbitStage { get; private set; }

    /// <summary>
    /// Distance of the two crash echoes from Samus in pixels, 0 through 16: zero at crash entry, raised by four per
    /// expanding call until it reaches sixteen and lowered by four per contracting call until it reaches zero.
    /// </summary>
    public int CrashRadius { get; private set; }

    /// <summary>First byte-angle formerly stored in speed-echo X speed slot zero.</summary>
    public RotationAngle FirstCrashEchoAngle { get; private set; }

    /// <summary>Second byte-angle, exactly 128 degrees-of-256 opposite the first.</summary>
    public RotationAngle SecondCrashEchoAngle { get; private set; }

    /// <summary>
    /// The crash echoes' angular travel, 0 through <see cref="LargestCrashAngularTravel"/>. Its storage is the
    /// released slot-three echo's Y coordinate: crash entry does not reset it, so a preceding echo or crash leaves a
    /// value the orbit starts from. It advances only through <see cref="AdvanceCrashAngularTravel"/>, which wraps
    /// past the largest value to zero, and the orbit ends on the sign of its distance from 128.
    /// </summary>
    public int CrashAngularTravel
    {
        get => FirstReleasedCrashEchoSlot.Position.Y;
        private set => FirstReleasedCrashEchoSlot.Position = FirstReleasedCrashEchoSlot.Position.WithY(value);
    }

    /// <summary>The largest angular travel the shared echo coordinate holds; one more step wraps past it to zero.</summary>
    internal const int LargestCrashAngularTravel = ushort.MaxValue;

    /// <summary>Angular travel added on each orbiting call.</summary>
    private const int CrashAngularTravelStep = 4;

    /// <summary>
    /// Adds one orbiting call's travel, wrapping past <see cref="LargestCrashAngularTravel"/> to zero as the shared
    /// echo coordinate does. A seeded travel near the largest value therefore restarts from zero and orbits longer.
    /// </summary>
    private void AdvanceCrashAngularTravel()
    {
        int advanced = CrashAngularTravel + CrashAngularTravelStep;
        CrashAngularTravel = advanced > LargestCrashAngularTravel ? advanced - (LargestCrashAngularTravel + 1) : advanced;
    }

    /// <summary>
    /// Host-readable echo drawing words associated with projectile slot three.
    /// Active is the drawing enable, not ownership: a combo can overwrite the projectile
    /// without clearing these words or allowing the old echo handler to keep running.
    /// </summary>
    public ShinesparkReleasedEcho FirstReleasedCrashEcho => FirstReleasedCrashEchoSlot.Snapshot;

    /// <summary>
    /// Host-readable snapshot of projectile slot four. The game draws this slot before
    /// slot three, even though it is initialized second.
    /// </summary>
    public ShinesparkReleasedEcho SecondReleasedCrashEcho => SecondReleasedCrashEchoSlot.Snapshot;

    /// <summary>Fixed slot-three echo storage whose position aliases crash angular words.</summary>
    internal readonly ReleasedEchoSlot FirstReleasedCrashEchoSlot = new();
    /// <summary>Fixed slot-four echo storage drawn before the slot-three copy.</summary>
    internal readonly ReleasedEchoSlot SecondReleasedCrashEchoSlot = new();

    /// <summary>
    /// Starting the X-ray scope clears the special-palette timer and replaces the shared
    /// special-palette handler with X-Ray. Relinquish this owner's palette state so
    /// it cannot keep ticking or repaint over the newly installed visor palette.
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
    /// ResetProjectileData clears the complete speed-echo arrays and index.
    /// Keep the movement-handler phase and its unrelated timers unchanged.
    /// </summary>
    internal void ResetProjectileEchoStorage()
    {
        FirstReleasedCrashEchoSlot.Clear();
        SecondReleasedCrashEchoSlot.Clear();
        FirstCrashEchoAngle = SecondCrashEchoAngle = RotationAngle.Zero;
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
        StoredShineWarningSoundRequested = false;
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
        // charges are still torn down without a sound call. The live projectile owner
        // contains the flare layers and previous-counter word, while Samus mirrors the charge counter
        // for movement decisions. Keep both views synchronized at this seam.
        int cancelledCharge = samus.ProjectileFlareCounter;
        if (cancelledCharge != 0)
        {
            _charge?.CancelForShinespark();
            samus.ProjectileFlareCounter = 0;
            if (cancelledCharge >= BeamChargeDefinitions.ChargeSoundStartCounter)
            {
                _chargeCancellationSoundRequested = true;
                ChargeCancellationSoundSuppressed = _audioPowerBomb?.IsActive == true;
            }
        }

        // The windup replaces the movement and palette pointers. Flash's live
        // palette timer can admit this launch even after its movement was interrupted.
        samus.CrystalFlash.RelinquishMovementHandler();
        samus.CrystalFlash.RelinquishPaletteHandler();
        // A Reserve-Mode X-Ray cone effect survives, but directional spark setup replaces
        // the same shared special-palette dispatcher with handler six.
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

        VerticalAcceleration = SignedSixteenSixteen.FromParts(7, 0);
        StartStopTimer = 30;
        ShineTimer = 60;
        PaletteType = SamusSpecialPaletteType.Shinesparking;
        PaletteStep = 0;
        Phase = ShinesparkPhase.Windup;
        samus.HurtFlashCounter = 0;
        LaunchSoundRequested = false;
        CrashSoundRequested = false;
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
            // Any other pose is a caller contract violation, not another untranslated arm.
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
            SamusPoseId.AdjacentCodePoseFD or
                SamusPoseId.AdjacentCodePoseFE or
                SamusPoseId.AdjacentCodePoseFF => throw new ArgumentOutOfRangeException(
                nameof(targetPose),
                $"Pose {targetPose} is not one of the six directional shinespark poses."),
            var undefined => throw new InvalidOperationException($"Undefined {nameof(SamusPoseId)} {undefined}."),
        };

        samus.InputSnapshot.ShinesparkPoseInputLocked = true;
        samus.InputSnapshot.AutoJumpInputPending = false;
        if (!deferPoseChange)
            ApplyDirectionalLaunchPose(samus, targetPose);
        samus.HorizontalSpeed.ClearSpeedEchoes();
        LaunchSoundRequested = true;
        LaunchSoundSuppressed = _audioPowerBomb?.IsActive == true;
    }

    /// <summary>Commits the interrupted launch pose after the current frame's animation.</summary>
    internal static void ApplyDirectionalLaunchPose(SamusState samus, SamusPoseId pose)
    {
        samus.Pose = pose;
        SamusPoseMetadata.RefreshCollisionRadii(samus);
        SamusPresentation.InitializeAnimation(samus, initialFrame: 0);
    }

    /// <summary>Runs one installed special-handler frame.</summary>
    /// <param name="terrain">The room terrain Samus's block collision probes: the address space, the level data, and the PLMs that react to touched blocks (#633).</param>
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
                return FinishCrash(samus, frame.Projectiles?.SlotTable.ProjectileCounter ?? frame.ProjectileCounter, frame.Projectiles);


            // Stored/inactive states are not installed special movement handlers. Returning
            // an empty snapshot is defensive; runtime normally never dispatches them here.
            return new ShinesparkMovementResult(
false);
        }

        samus.HorizontalSpeed.ContactDamageIndex = SamusContactDamageIndex.Shinespark;
        samus.HurtFlashCounter = 8;

        // Each active handler samples echoes before motion. Runtime suppresses its ordinary
        // post-motion producer while this special handler owns the frame.
        samus.HorizontalSpeed.CaptureSpeedEchoPosition(frame.GameTimeFrames, samus.Position.Whole);

        BlockMoveResult? horizontal = null;
        BlockMoveResult? vertical = null;
        // Each axis ends by capping how far the camera's previous-position word trails
        // Samus. Only a camera reads that word; fixtures without one pass no checkpoint.
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
        // frame moves once more before EndSuperJump observes that 29 is below the threshold.
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
    /// Runs palette handler one or six, including normal restoration
    /// when its signed countdown expires.
    /// </summary>
    public bool UpdatePalette(SamusColours colours, SuperMetroid.Core.Progression.SamusEquipmentFlags equippedItems)
    {
        Ensure.NotNull(colours);
        if (PaletteType is not (SamusSpecialPaletteType.SpeedBoosterShine or SamusSpecialPaletteType.Shinesparking))
            return false;

        if (PaletteType == SamusSpecialPaletteType.SpeedBoosterShine && ShineTimer == 170)
        {
            StoredShineWarningSoundRequested = true;
            StoredShineWarningSoundSuppressed = _audioPowerBomb?.IsActive == true;
        }

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

    /// <summary>Stops translational spark motion, seeds paired orbit angles and installs the crash handler without clearing aliased angular travel.</summary>
    private void BeginCrash(SamusState samus)
    {
        // The crash entry kills translational velocity before installing the crash handler.
        samus.Kinematics.YSpeed = default;
        samus.Kinematics.YDirection = SamusVerticalDirection.None;
        samus.HorizontalSpeed.ExtraRunSpeed = default;
        samus.HorizontalSpeed.SpeedBoostCounter = default;
        // EndSuperJump retains this frame's contact damage. The shared frame
        // prologue clears it before the newly installed crash handler runs next.
        samus.HurtFlashCounter = 0;
        bool facingLeft = SamusPoseMetadata.IsFacing(samus, SamusFacingDirection.Left);
        FirstCrashEchoAngle = RotationAngle.FromTableIndex(facingLeft ? 32 : 224);
        SecondCrashEchoAngle = RotationAngle.FromTableIndex(facingLeft ? 160 : 96);
        CrashAngularDelta = facingLeft ? 4 : -4;
        FirstReleasedCrashEchoSlot.DisableDrawing();
        CrashOrbitStage = ShinesparkCrashOrbitStage.Expanding;
        CrashRadius = 0;
        // The entry does not initialize the aliased angular-travel/echo-Y
        // word. A preceding echo or crash can therefore shorten this orbit.
        samus.HorizontalSpeed.SetShinesparkCrashEchoState(
            SpeedEchoCursor.FirstSlot,
            samus.Position.Whole,
            samus.Position.Whole);
        Phase = ShinesparkPhase.Crash;
        CrashSoundRequested = true;
        CrashSoundSuppressed = _audioPowerBomb?.IsActive == true;
    }

    /// <summary>
    /// The crash angular delta aliases departing slot-three echo X. Preserve the
    /// complete signed word so any still-owned echo write affects the consumer: the delta reads the echo's X
    /// word as signed, -32768 through 32767, and a write stores it back as that word.
    /// </summary>
    private int CrashAngularDelta
    {
        get => CoordinateAxis.AsSigned(FirstReleasedCrashEchoSlot.Position.X);
        set => FirstReleasedCrashEchoSlot.Position = FirstReleasedCrashEchoSlot.Position.WithX(CoordinateAxis.Wrap(value));
    }

    /// <summary>Runs the crash, including the three overloaded-index substates.</summary>
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
                FirstCrashEchoAngle = FirstCrashEchoAngle.AddTableUnits(CrashAngularDelta);
                SecondCrashEchoAngle = SecondCrashEchoAngle.AddTableUnits(CrashAngularDelta);
                AdvanceCrashAngularTravel();
                // The travel shares its storage with the first echo's Y coordinate and is compared as that
                // coordinate is, so a travel that wrapped to zero, or one seeded far above 128, keeps orbiting until
                // it climbs back to 128.
                if (CoordinateAxis.Difference(CrashAngularTravel, 128) >= 0)
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

        // The projectile sine lookup consumes a byte angle/radius and returns wrapped 16-bit
        // offsets. Both additions below deliberately retain that unsigned sixteen-bit wrapping.
        int orbitRadius = Ensure.BetweenInclusive(0, byte.MaxValue, CrashRadius);
        Offset16<RoomPixels> firstOffset = ProjectileSinLookup(FirstCrashEchoAngle, orbitRadius);
        Offset16<RoomPixels> secondOffset = ProjectileSinLookup(SecondCrashEchoAngle, orbitRadius);
        samus.HorizontalSpeed.SetShinesparkCrashEchoState(
            SpeedEchoCursor.CrashOrbit(CrashOrbitStage, CrashRadius),
            samus.Position.Whole + firstOffset,
            samus.Position.Whole + secondOffset);

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
    /// Ports the crash's end: convert the crash copies into fixed projectile slots three
    /// and four when capacity permits, restore standing, and expire palette handler six.
    /// </summary>
    internal ShinesparkMovementResult FinishCrash(
        SamusState samus,
        int projectileCounter,
        SamusProjectileSystem? projectiles)
    {
        // Resolve the crash axis before publishing the standing pose.
        var departureAngles = ShinesparkEchoDepartures.DepartureAngles(samus.Pose, projectiles?.DrainedCrashDepartures);
        samus.HorizontalSpeed.ClearSpeedEchoes();

        // The game reserves fixed projectile slots three and four. Its projectile counter
        // admits both while below four, only slot four at exactly four, and neither at five
        // or above. The runtime supplies the ordinary projectile count, not bomb count.
        // Capacity failure does not clear existing echo words or slots. In particular,
        // retaining slot three's Y also retains the next crash's angular-travel seed.
        if (CoordinateAxis.Difference(projectileCounter, 5) < 0)
        {
            if (CoordinateAxis.Difference(projectileCounter, 4) < 0)
            {
                FirstReleasedCrashEchoSlot.Initialize(departureAngles.First, samus.Position.Whole);
                projectiles?.Shinespark.InitializeShinesparkEcho(3, departureAngles.First);
            }

            SecondReleasedCrashEchoSlot.Initialize(departureAngles.Second, samus.Position.Whole);
            projectiles?.Shinespark.InitializeShinesparkEcho(4, departureAngles.Second);
        }

        ShineTimer = 1;
        VerticalAcceleration = SignedSixteenSixteen.Zero;
        Phase = ShinesparkPhase.Inactive;
        return new ShinesparkMovementResult(
false,             CrashSequenceFinished: true);
    }

    /// <summary>Commits the crash's transitional standing pose after this frame's animation.</summary>
    public static void ApplyCrashFinishPose(SamusState samus)
    {
        // Transitional command two restores the normal input handler as well as beta. That
        // replaces a drained pose's movement word too: a Samus drained mid-crash is free.
        samus.InputSnapshot.ShinesparkPoseInputLocked = false;
        samus.Drained.RelinquishMovementHandler();
        SamusPoseId standingPose = SamusPoseMetadata.IsFacing(samus, SamusFacingDirection.Left)
            ? SamusPoseId.FacingLeftNormalPose
            : SamusPoseId.FacingRightNormalPose;
        int previousRadius = samus.Kinematics.Radius.Y;
        samus.Pose = standingPose;
        // The transitional-pose handler restores normal movement and
        // aligns the standing body's bottom with the old crash body. Alpha alone
        // publishes the new live radius on the following frame.
        samus.AlignBottomAfterPoseChange(previousRadius, SamusPoseMetadata.ReadPoseYRadius(standingPose));
        SamusPresentation.InitializeAnimation(samus, initialFrame: 0);
    }

    /// <summary>
    /// The projectile radius/angle belong to its current slot owner. Drawing enable and
    /// coordinates are separate words and may survive replacement of that owner.
    /// </summary>
    internal bool StepOwnedReleasedCrashEcho(SamusState samus,
        Position16<RoomPixels> cameraPosition, SamusProjectileSlot projectile)
    {
        ReleasedEchoSlot echo = projectile.SlotIndex switch
        {
            3 => FirstReleasedCrashEchoSlot,
            4 => SecondReleasedCrashEchoSlot,
            _ => throw new InvalidOperationException("Crash echo requires fixed slot three or four.")
        };
        echo.Radius = projectile.Velocity.X.RawValue;
        echo.Angle = projectile.EchoAngle.ToAngle();
        if (!StepReleasedCrashEcho(samus, cameraPosition,
            echo, slotOwner: CrashEchoSlotOwner.Projectile)) return false;
        projectile.Velocity = projectile.Velocity.WithX(SignedEightEight.FromSignedRaw(CoordinateAxis.AsSigned(echo.Radius)));
        projectile.Position = projectile.Position.WithWhole(echo.Position);
        return true;
    }

    /// <summary>Expands an echo around live Samus position and clears its drawing words when either viewport axis rejects it.</summary>
    internal static bool StepReleasedCrashEcho(
        SamusState samus,
        Position16<RoomPixels> cameraPosition,
        ReleasedEchoSlot slot,
        CrashEchoSlotOwner slotOwner = CrashEchoSlotOwner.Echo)
    {
        if (slotOwner != CrashEchoSlotOwner.Projectile && !slot.Active)
            return false;

        // The crash increments the complete word but passes only its low byte as radius.
        // The point is recalculated from the *current* Samus position on every frame; these
        // are expanding radial copies, not ordinary projectiles with independent velocity.
        slot.ExpandRadius();
        Offset16<RoomPixels> offset = ProjectileSinLookup(slot.Angle, slot.PlacedRadius);
        Position16<RoomPixels> point = samus.Position.Whole + offset;
        slot.Position = slot.Position.WithX(point.X);

        // The game performs the X viewport rejection before calculating/publishing Y. The
        // cleared result is identical either way; the branch order follows the game's steps.
        int screenX = CoordinateAxis.Difference(slot.Position.X, cameraPosition.X);
        if (screenX is < 0 or >= 256)
        {
            slot.Clear();
            return false;
        }

        slot.Position = slot.Position.WithY(point.Y);
        int screenY = CoordinateAxis.Difference(slot.Position.Y, cameraPosition.Y);
        if (screenY is < 0 or >= 256)
        {
            slot.Clear();
            return false;
        }
        return true;
    }

    /// <summary>Exact byte-split multiplication used by the crash's sine and cosine.</summary>
    internal static Offset16<RoomPixels> ProjectileSinLookup(
        RotationAngle angle,
        int radius)
    {
        int x = LookupSignedComponent(angle, radius).SignedRawValue;
        int y = LookupSignedComponent(
            angle.AddRaw(-RotationAngle.QuarterTurn.RawValue),
            radius).SignedRawValue;
        return new(x, y);
    }

    /// <summary>Multiplies a positive sine half-wave and restores its sign after truncation, matching the projectile table routine.</summary>
    private static SignedEightEight LookupSignedComponent(
        RotationAngle angle,
        int radius)
    {
        // The game multiplies the positive half-wave, discards the fraction, then
        // restores sign. Shifting a negative product would round differently.
        return Trigonometry.MultiplySignedSine(radius, angle.ToByteAngle());
    }

    /// <summary>
    /// After horizontal spark movement, a previous X more than
    /// 15 pixels behind Samus is pulled to 15 behind, in either direction.
    /// </summary>
    private static void CapPreviousX(SamusState samus, FixedPosition<RoomPixels> checkpoint)
    {
        int delta = CoordinateAxis.Difference(samus.Position.X.Whole, samus.PeekPreviousPositionWords(checkpoint).X);
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
        if (CoordinateAxis.Difference(samus.Position.Y.Whole, samus.PeekPreviousPositionWords(checkpoint).Y) < -14)
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
        VerticalAcceleration += samus.Kinematics.YAcceleration.AsDisplacement();
        samus.Kinematics.YSpeed += VerticalAcceleration;

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
        // than 32753 pixels upward or downward, as the game's word comparison does.
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
        samus.Kinematics.RecordSolidEnemyCollision(SamusCollisionDirection.Up, enemy.EnemySlot);
        if (enemy.Collided)
            return new BlockMoveResult(0, true, EnemyCollision: enemy);

        return SamusBlockCollision.MoveVertical(new SamusTerrain { Level = level, RoomObjects = roomObjects }, samus.Kinematics, samus, new VerticalBlockMove { Displacement = upwardDisplacement, ScanLeftToRight = (frameCounter & 1) == 0, CanBreakBombBlocks = true, IncludeSolidEnemies = false });
    }

    /// <summary>
    /// Mutable storage corresponding to one of fixed projectile slots three/four.
    /// It is deliberately private so callers cannot manufacture a live visual projectile.
    /// </summary>
    internal sealed class ReleasedEchoSlot
    {
        /// <summary>Drawing enable, independent of the slot's current projectile owner.</summary>
        public bool Active { get; private set; }
        /// <summary>Byte-angle used to place the departing echo around Samus.</summary>
        public RotationAngle Angle { get; set; }
        /// <summary>
        /// Expanding radius in pixels, 0 through <see cref="LargestRadius"/>, retained in the owning projectile's X
        /// velocity word: zero when the echo departs and raised only by <see cref="ExpandRadius"/>. The sine lookup
        /// places the echo with the radius taken modulo 256.
        /// </summary>
        public int Radius { get; set; }

        /// <summary>The largest radius the projectile velocity word holds; one more expansion wraps past it to zero.</summary>
        internal const int LargestRadius = ushort.MaxValue;

        /// <summary>The radii the sine lookup places an echo at, in pixels; a radius past them places it from the centre again.</summary>
        private const int PlacementRadii = 256;

        /// <summary>The radius the echo is placed at: <see cref="Radius"/> within the lookup's 256-pixel cycle.</summary>
        public int PlacedRadius => Radius % PlacementRadii;

        /// <summary>Pixels the radius grows on each departing call.</summary>
        private const int RadiusStep = 8;

        /// <summary>Grows the radius by one departing call's step, wrapping past <see cref="LargestRadius"/> to zero.</summary>
        public void ExpandRadius()
        {
            int expanded = Radius + RadiusStep;
            Radius = expanded > LargestRadius ? expanded - (LargestRadius + 1) : expanded;
        }
        /// <summary>Last rendered room position, including the slot-three crash aliases.</summary>
        public Position16<RoomPixels> Position { get; set; }

        /// <summary>The echo's drawing enable and position, as one immutable value.</summary>
        public ShinesparkReleasedEcho Snapshot => new(Active, Position);

        /// <summary>Clears drawing enable while retaining coordinates, projectile ownership and aliased crash travel.</summary>
        public void DisableDrawing() => Active = false;

        /// <summary>Activates a departing echo at its crash position with zero projectile radius.</summary>
        public void Initialize(RotationAngle angle, Position16<RoomPixels> position)
        {
            Active = true;
            Angle = angle;
            // Crash finish clears projectile X velocity (the departing radius). The
            // separate speed-echo drawing field is 64; it is not this projectile word.
            Radius = 0;
            Position = position;
        }

        /// <summary>Zeroes the drawing and position words after a viewport rejection.</summary>
        public void Clear()
        {
            Active = false;
            Angle = RotationAngle.Zero;
            Radius = 0;
            Position = Position16<RoomPixels>.Zero;
        }
    }
}
