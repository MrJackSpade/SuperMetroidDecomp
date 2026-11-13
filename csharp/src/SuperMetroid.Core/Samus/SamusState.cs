using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Samus.Abilities;
using SuperMetroid.Core.Samus.Grapple;
using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.Core.Samus.Palettes;
using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.Core.Samus.Death;
using SuperMetroid.Core.Samus.Drawing;
using SuperMetroid.Core.Samus.Hud;
using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Samus;

/// <summary>
/// First breakpoint-friendly slice of normal gameplay Samus state and rendering.
/// </summary>
/// <remarks>
/// This class owns the state shared by the exact standing/right-running render, animation,
/// and grounded movement slices. The Landing Site cinematic actor is not part of it.
/// </remarks>
public sealed class SamusState
{
    /// <summary>The <see cref="SamusHudSelection"/> owner of this SamusState.</summary>
    internal SamusHudSelection HudSelection { get; }

    /// <summary>The installed frame-handler pair and the Samus commands that replace it.</summary>
    internal SamusControl Control { get; } = new();

    /// <summary>Samus as room objects read and change her.</summary>
    internal IRoomSamus RoomView => field ??= new SamusRoomView(this);

    /// <summary>The <see cref="SamusInputSnapshot"/> owner of this SamusState.</summary>
    internal SamusInputSnapshot InputSnapshot { get; }

    /// <summary>The <see cref="SamusPresentation"/> owner of this SamusState.</summary>
    internal SamusPresentation Presentation { get; }

    /// <summary>Samus's current pose.</summary>
    /// <remarks>
    /// An ordinary write also models the pose initializer, publishing the
    /// pose's direction and movement type to <see cref="InitializedPose"/>. Only the
    /// writers that skip that initializer use <see cref="WritePoseWithoutInitialization"/>.
    /// </remarks>
    public SamusPoseId Pose
    {
        get;
        set
        {
            field = value;
            InitializedPose = value;
            // Block collision reads the global pose directly while resolving special
            // elevator doors. Mirror every pose write into the collision snapshot so the
            // low-level dispatcher can reproduce that test without owning Samus state.
            Kinematics.CollisionPose = value;
        }
    } = SamusPoseId.FacingRightNormalPose;

    /// <summary>
    /// The pose whose facing and movement type the pose initializer last copied to
    /// the cached facing and movement type. Movement dispatch and camera
    /// tracking read those cached values, not the live pose.
    /// </summary>
    public SamusPoseId InitializedPose { get; private set; } = SamusPoseId.FacingRightNormalPose;

    /// <summary>
    /// Writes the pose as the game does when it skips the pose initializer
    /// (the drained-Samus controllers): the previous pose's
    /// direction and movement type stay in force.
    /// </summary>
    internal void WritePoseWithoutInitialization(SamusPoseId pose)
    {
        SamusPoseId initialized = InitializedPose;
        Pose = pose;
        InitializedPose = initialized;
    }

    /// <summary>
    /// Set when a Samus command run during enemy AI cancels the pending pose transition: the command
    /// clears every prospective pose and command, so this frame's pending
    /// transition is not committed. The runtime consumes it before the pose update.
    /// </summary>
    public bool PendingPoseTransitionCancelled { get; private set; }

    /// <summary>Records the cancellation of the pending pose transition.</summary>
    internal void CancelPendingPoseTransition() => PendingPoseTransitionCancelled = true;

    /// <summary>Returns and clears the pending-transition cancellation.</summary>
    internal bool ConsumePendingPoseTransitionCancellation()
    {
        bool cancelled = PendingPoseTransitionCancelled;
        PendingPoseTransitionCancelled = false;
        return cancelled;
    }

    /// <summary>
    /// The animation frame shown: the number of the step of the pose's animation program the animation is on, which is
    /// also the frame number of the pose's artwork. It is zero or more and never runs away: every frame the animation
    /// reaches names a step of the pose's program or of the program laid out after it, and reading a frame that names no
    /// step fails (<see cref="AnimationStep"/>); the setter rejects a negative frame.
    /// </summary>
    [IntegerQuantity("frame index within the current pose animation")]
    public int AnimationFrame
    {
        get;
        set => field = Ensure.AtLeastZero(value);
    }

    /// <summary>
    /// Animation frame countdown. Zero and negative-after-decrement both advance,
    /// matching the game's zero and positive tests.
    /// </summary>
    public WrappingCountdown AnimationFrameTimer { get; internal set; }

    /// <summary>
    /// Delay, in updates, added to each animation frame for speed and liquid physics: zero, two or three, or the
    /// <see cref="XSpeedDivisor"/> on dry ground, so 0 through
    /// <see cref="SamusHorizontalSpeedState.LargestSpeedDivisor"/>; the setter rejects any other value.
    /// </summary>
    public int AnimationFrameBuffer { get; internal set => field = Ensure.BetweenInclusive(0, SamusHorizontalSpeedState.LargestSpeedDivisor, value); }

    /// <summary>
    /// Exact fixed-point horizontal speeds used by ordinary grounded
    /// right-running movement.
    /// </summary>
    public SamusHorizontalSpeedState HorizontalSpeed { get; } = new();

    /// <summary>
    /// Live room-FX surface values and remembered liquid medium. Physics, animation, and
    /// pose initialization intentionally share this object because the game shares
    /// room-FX values across all three phases.
    /// </summary>
    public SamusLiquidPhysicsState LiquidPhysics { get; }

    /// <summary>
    /// Stored-shine palette countdown and the special shinespark movement-handler state.
    /// Its fields are the values documented on
    /// <see cref="SamusShinesparkState"/> and keep their individual values.
    /// </summary>
    public SamusShinesparkState Shinespark { get; }

    /// <summary>
    /// The update's audio requests every sound of hers joins as it is made: her own record until a runtime points her at
    /// its own (<see cref="BindAudioRequests"/>).
    /// </summary>
    internal GameplayAudioRequests AudioRequests { get; private set; } = new();

    /// <summary>Points every sound of hers, her movement's and her spark's among them, at <paramref name="requests"/>.</summary>
    internal void BindAudioRequests(GameplayAudioRequests requests)
    {
        AudioRequests = Ensure.NotNull(requests);
        LiquidPhysics.BindAudioRequests(requests);
        Shinespark.BindAudioRequests(requests);
    }

    /// <summary>
    /// Crystal Flash initiation, ammo cadence, energy restoration, and installed handler
    /// state. It is deliberately separate from <see cref="Shinespark"/> even though the
    /// game shares a few values between the mutually exclusive effects.
    /// </summary>
    public SamusCrystalFlashState CrystalFlash { get; }

    /// <summary>
    /// X-ray admission, dedicated pose input/movement, beam-angle state, visor palette, and
    /// teardown. Revealed-block pictures and the cone window remain presentation-owned outputs.
    /// </summary>
    public SamusXrayState Xray { get; } = new();

    /// <summary>
    /// Fatal-damage death-pose ownership, death tiles/palettes, whiteout, and suit explosion.
    /// The outer music wait and post-explosion room fade remain explicit game-state seams.
    /// </summary>
    public SamusDeathSequenceState DeathSequence { get; } = new();

    /// <summary>
    /// Mother Brain/Baby Metroid drain poses, controller calls, and installed falling
    /// movement handler. Enemy AI remains a separate producer of these literal commands.
    /// </summary>
    public SamusDrainedState Drained { get; } = new();

    /// <summary>
    /// The grabbed-pose hack handler and the narrow owner-position seam.
    /// Draygon's enemy AI owns its own flight path; this child owns every Samus-side value.
    /// </summary>
    public SamusDraygonGrabbedState DraygonGrabbed { get; }

    /// <summary>
    /// Ceres Ridley's dedicated push/fall handler. This is not ordinary damage
    /// knockback: the shove replaces the movement and gamma handlers without
    /// consuming energy, and owns Samus until horizontal room collision.
    /// Keeping that lifetime separate prevents the five-frame hurt timer from ending the
    /// scripted ejection early or making normal controller input steer it.
    /// </summary>
    public SamusCeresRidleyEjectionState CeresRidleyEjection { get; } = new();

    /// <summary>
    /// The X-speed divisor, 0 through 65535, as Samus's own view of <see cref="SamusHorizontalSpeedState.SpeedDivisor"/>:
    /// the one stored value is both a movement-speed divisor and the no-FX animation delay buffer, and its setter
    /// rejects values outside that range.
    /// </summary>
    [IntegerQuantity("right-shift applied to horizontal speed, zero when undivided")]
    public int XSpeedDivisor
    {
        get => HorizontalSpeed.SpeedDivisor;
        set => HorizontalSpeed.SpeedDivisor = value;
    }

    /// <summary>Current energy, also used by the animation's low-energy command.</summary>
    public int Health { get; set; } = 99;

    /// <summary>
    /// The previous energy for the hurt check: initialized from Energy when Samus is initialized,
    /// then compared and refreshed after Samus/projectile drawing by the post-draw input filter.
    /// </summary>
    public int PreviousHealthForHurtCheck { get; set; } = 99;

    /// <summary>
    /// Fractional energy, 0 through 65535 in units of 1/65536 of one <see cref="Health"/> unit. Ordinary HUD and
    /// debug output shows only <see cref="Health"/>, but periodic damage borrows from this fraction
    /// (<see cref="TakeFractionalEnergyDamage"/>) so sub-energy cannot be rounded independently on every frame.
    /// The setter rejects values outside the range.
    /// </summary>
    public int SubunitHealth
    {
        get;
        set => field = Ensure.BetweenInclusive(0, SubunitsPerHealthUnit - 1, value);
    }

    /// <summary>How many <see cref="SubunitHealth"/> units make one <see cref="Health"/> unit.</summary>
    private const int SubunitsPerHealthUnit = 65536;

    /// <summary>
    /// Takes periodic damage as the game's periodic damage does: the fractional damage comes off <see cref="SubunitHealth"/>
    /// first, and when it takes the fraction below zero the fraction wraps to the remainder and one more whole unit is
    /// owed; the whole damage and that borrowed unit then come off <see cref="Health"/>. A result below zero energy
    /// clamps both the energy and its fraction to zero instead.
    /// </summary>
    /// <param name="subunitDamage">Fractional damage, 0 through 65535 subunits.</param>
    /// <param name="wholeDamage">Whole damage in energy units, zero or more.</param>
    internal void TakeFractionalEnergyDamage(int subunitDamage, int wholeDamage)
    {
        Ensure.BetweenInclusive(0, SubunitsPerHealthUnit - 1, subunitDamage);
        Ensure.AtLeastZero(wholeDamage);
        int subunit = SubunitHealth - subunitDamage;
        int borrowed = subunit < 0 ? 1 : 0;
        int health = Health - wholeDamage - borrowed;
        if (health < 0)
        {
            SubunitHealth = 0;
            Health = 0;
            return;
        }

        SubunitHealth = subunit + borrowed * SubunitsPerHealthUnit;
        Health = health;
    }

    /// <summary>Maximum normal energy; defaults to the initial 99.</summary>
    public int MaxHealth { get; set; } = 99;

    /// <summary>Current reserve energy.</summary>
    public int ReserveEnergy { get; set; }

    /// <summary>Maximum reserve energy.</summary>
    public int MaxReserveEnergy { get; set; }

    /// <summary>Reserve-tank mode: zero off, one auto, two manual.</summary>
    public ReserveTankMode ReserveTankMode { get; set; }

    /// <summary>Current missiles.</summary>
    public int Missiles { get; set; }

    /// <summary>Maximum missiles.</summary>
    public int MaxMissiles { get; set; }

    /// <summary>
    /// The missile reserve. Super Metroid never exposes a missile-reserve tank in normal
    /// play, but its shared restore still sends overflow here and caps it at either
    /// max missiles or 99. Retaining the otherwise-unused value makes enemy drops exact.
    /// </summary>
    public int ReserveMissiles { get; set; }

    /// <summary>Current super missiles.</summary>
    public int SuperMissiles { get; set; }

    /// <summary>Maximum super missiles.</summary>
    public int MaxSuperMissiles { get; set; }

    /// <summary>Current power bombs.</summary>
    public int PowerBombs { get; set; }

    /// <summary>Maximum power bombs.</summary>
    public int MaxPowerBombs { get; set; }

    /// <summary>Selected HUD item: 0 none, 1 missiles, 2 supers, 3 bombs.</summary>
    public HudItem SelectedHudItem { get; set; }

    /// <summary>HUD auto-cancel index.</summary>
    public HudItem AutoCancelHudItemIndex { get; set; }

    /// <summary>
    /// Equipped beam bitfield. Drained-controller function three replaces
    /// this with the exact charge/wave/plasma plus hyper-beam configuration.
    /// </summary>
    public SamusBeamFlags EquippedBeams { get; set; }

    /// <summary>
    /// Collected-beam bitfield. Unlike
    /// <see cref="EquippedBeams"/>, this retains beams disabled from the pause equipment
    /// screen and is therefore the authoritative ownership record the save file keeps.
    /// </summary>
    public SamusBeamFlags CollectedBeams { get; set; }

    /// <summary>
    /// The beam's charge (#1577): the session's one count, which the projectile pass advances and her movement, bombs,
    /// sounds and palettes read; at least 60 is a full charge, which makes her spin and wall jump a pseudo Screw Attack.
    /// Every clear, hers or the projectiles', clears it for every reader.
    /// </summary>
    internal SuperMetroid.Core.Samus.Projectiles.BeamChargeState BeamCharge { get; }

    /// <summary>
    /// The shared bomb-spread charge timeout. The game clears this
    /// timeout in the morphing-transition pose initializer after a
    /// successful Morph Ball item check, regardless of the humanoid movement family that
    /// selected the transition. Keeping it separate from the beam flare counter prevents a
    /// mid-air morph from accidentally cancelling ordinary arm-cannon charge state.
    /// </summary>
    /// <remarks>
    /// Updates of Down held while a spread is charged, 0 through
    /// <see cref="Projectiles.MorphBallBombDefinitions.DownChargeTimeoutUpdates"/>: its only advance stops on reaching
    /// that count and its other producers clear it, so the setter rejects any other value.
    /// </remarks>
    public int BombSpreadChargeTimeoutCounter
    {
        get;
        set => field = Ensure.BetweenInclusive(0, Projectiles.MorphBallBombDefinitions.DownChargeTimeoutUpdates, value);
    }

    /// <summary>
    /// The shot a pose transition handed to the next update's weapon handler, or null. A pose initializer can hand one
    /// after the current update's weapon handler has already run: a normal jump started on a fresh Shoot press hands
    /// the new pose's direction, and a turn out of a moonwalk hands the moonwalk's. The next update's weapon handler
    /// fires it, and that update then drops it whether or not a shot was fired.
    /// </summary>
    public SamusTransitionShot? PendingTransitionShot { get; internal set; }

    /// <summary>Whether Mother Brain's hyper beam is enabled.</summary>
    public bool HyperBeamEnabled { get; set; }

    /// <summary>
    /// Equipped-item flags. The animation command's equipment test and the Morph-Ball projectile
    /// handler read them directly.
    /// </summary>
    public SamusEquipmentFlags EquippedItems { get; set; }

    /// <summary>
    /// Collected-item bitfield. This normally contains
    /// every equipped item, but equipment-menu toggles can make the two sets differ. Enemy
    /// logic such as the morph-ball eye tests ownership through this set specifically.
    /// </summary>
    public SamusEquipmentFlags CollectedItems { get; set; }

    /// <summary>
    /// The suit palette a draining Metroid or the Baby Metroid makes Samus flash through next, or inactive. Attachment
    /// starts on the Speed Booster palette; the palette handling then alternates it with the normal suit every update
    /// until the attacker lets go.
    /// </summary>
    public SamusMetroidAttachmentPalette MetroidAttachmentPalette { get; internal set; }

    /// <summary>How far a landing Morph Ball or Spring Ball has bounced; see <see cref="SamusBallBounceState"/>.</summary>
    public SamusBallBounceState MorphBallBounceState { get; set; }

    /// <summary>
    /// The bomb jump a bomb's blast asked of Samus: its launch direction, and whether her pose handling has accepted it,
    /// which keeps the same request from arming the jump again.
    /// </summary>
    public SamusBombJumpRequest BombJumpRequest { get; set; }

    /// <summary>True for the bomb jump's one-frame velocity-initialization handler.</summary>
    public bool BombJumpStarting { get; set; }

    /// <summary>True while the bomb jump's main handler owns the rising arc.</summary>
    public bool BombJumpActive { get; set; }

    /// <summary>Bomb command installed an empty pose-input handler; independent of the current movement handler.</summary>
    public bool BombJumpPoseInputLocked { get; set; }

    /// <summary>Which way the pending or running knockback moves Samus, or none.</summary>
    public Movement.SamusKnockbackDirection KnockbackDirection { get; set; }

    /// <summary>Which way the pending or active knockback moves Samus horizontally.</summary>
    public Movement.KnockbackHorizontalDirection KnockbackXDirection { get; set; }

    /// <summary>
    /// The count her hurt flash runs on, which a knockback, a health loss and a moving shinespark write; the hurt-flash
    /// palette advances it and clears it on reaching sixty (<see cref="Palettes.SamusHurtFlashPalette.EndingCall"/>).
    /// </summary>
    public SharedHurtFlashCount HurtFlash { get; }

    /// <summary>
    /// Whether the charging-beam sound resumes on the next post-draw sound pass if Shoot is still held (#1682). Hurt-flash
    /// recovery sets it at counter forty when a non-spinning Samus is still holding a sufficiently charged beam, and the
    /// post-draw sound handler consumes it later in the same gameplay pass; a spin's end sets it for the next pass.
    /// </summary>
    public bool ResumeChargingBeamSoundPending { get; internal set; }

    /// <summary>
    /// The general Samus invincibility timer tested by enemy, projectile,
    /// ordinary spike, spike-air, and grapple-swing damage producers. The gameplay tail
    /// decrements it once near the end of every gameplay frame, after all those producers
    /// have had an opportunity to reject or publish damage.
    /// </summary>
    public WrappingCountdown InvincibilityTimer { get; set; }

    /// <summary>
    /// Initialized to five by enemy damage collision and decremented once
    /// per enemy-processing pass. The special handler owns movement only while
    /// this counter remains nonzero.
    /// </summary>
    public int KnockbackTimer { get; set; }

    /// <summary>
    /// Decrements Samus's two own hurt timers.
    /// This deliberately lives outside every individual movement
    /// handler: grapple spikes can start the timers without installing knockback movement,
    /// and the gameplay tail ages them even while time is frozen.
    /// </summary>
    public void DecrementHurtTimers()
    {
        InvincibilityTimer = InvincibilityTimer.DecrementToZero();
        KnockbackTimer = Math.Max(0, KnockbackTimer - 1);
    }

    /// <summary>
    /// The grapple-beam state. Keeping this as a named child object makes the grapple
    /// values individually watchable without polluting ordinary Samus kinematics.
    /// </summary>
    public SamusGrappleState Grapple { get; } = new();

    /// <summary>
    /// The independent opening cover, one-sprite draw, and character load used by aimed weapon
    /// poses. Its state is driven by HUD selection rather than by the body animation timer.
    /// </summary>
    public SamusArmCannonState ArmCannon { get; } = new();

    /// <summary>
    /// The room's visor shade cycle, advancing every five updates, used only by backdrop-blending rooms.
    /// </summary>
    public SamusVisorPaletteState VisorPalette { get; } = new();

    /// <summary>
    /// The enemy to shake, written when an ordinary or grapple wall jump
    /// launches from a solid/frozen enemy instead of room terrain.
    /// </summary>
    /// <remarks>
    /// Holds the enemy slot, or null for none. Enemy AI owns consumption.
    /// Retaining the slot now makes the movement result complete even before the general
    /// live-enemy update loop is modelled.
    /// </remarks>
    public int? EnemySlotToShake { get; set; }

    /// <summary>
    /// Host-visible equivalent of the knockback movement handler. It is separate from
    /// the knockback movement type because the game also uses that type for crystal-flash art.
    /// </summary>
    public bool KnockbackActive { get; set; }

    /// <summary>
    /// The pose the animation was last started for, whose program <see cref="AnimationFrame"/> steps through; none before
    /// the animation is first started. Changing the pose to one with another program without restarting the animation
    /// is a defect the next animation tick reports.
    /// </summary>
    public SamusPoseId? AnimatedPose { get; internal set; }

    /// <summary>The step of the animated pose's program that Samus's animation frame is on.</summary>
    /// <exception cref="InvalidOperationException">The animation has not been started.</exception>
    internal SamusAnimationProgramStep AnimationStep => SamusAnimationDelays.StepFor(
        AnimatedPose ?? throw new InvalidOperationException("Samus's animation has not been started for any pose."), AnimationFrame,
        AnimationContinuation);

    /// <summary>
    /// What an animation frame past her pose's program reads, the session's installed service, which decides the frames her
    /// body can show.
    /// </summary>
    internal LaidOutProgramContinuation<SamusPoseId, SamusAnimationProgramStep> AnimationContinuation { get; }

    /// <summary>
    /// Operand published by the animation's pose-transition command through the “super-special
    /// prospective pose” seam. Null means the animation has not requested that transition.
    /// </summary>
    /// <remarks>
    /// This is host storage for the super-special prospective pose's command-three route. The grounded slice
    /// consumes it after animation, at the same pose-transition point used by the game.
    /// Keeping it explicit makes the turn's final facing change its own step.
    /// </remarks>
    public SamusPoseId? PendingTransitionalPose { get; internal set; }

    /// <summary>Exact fixed-point position and radius values consumed by collision.</summary>
    public SamusKinematicsState Kinematics { get; }

    /// <summary>
    /// The motion her movement and the camera record and her shots inherit at launch. A new Samus starts with it zeroed;
    /// the movement records are zero between frames too, and the camera records its subspeed again every frame.
    /// </summary>
    public ProjectileLaunchInheritance ProjectileInheritance { get; }

    /// <summary>
    /// Her shinespark's upward acceleration, which a suit pickup transforming her overwrites with its light beam's
    /// progress.
    /// </summary>
    internal SuitPickupSparkCarryover SparkCarryover { get; }

    /// <summary>
    /// The count her Crystal Flash's ammunition drain and her escape from Draygon share, on which a suit pickup
    /// transforming her leaves its stage.
    /// </summary>
    internal SharedEscapeAndDrainCount EscapeAndDrainCount { get; }

    /// <summary>
    /// The session's unauthored Speed Booster cadence, which times her run when a full sound queue sends the boost onto
    /// it, with the delays Ceres Ridley's getaway left behind.
    /// </summary>
    internal UnauthoredBoostCadence UnauthoredBoostCadence { get; }

    /// <summary>
    /// Room-dependent admission for the spike block's first common reaction, populated by the alpha-phase
    /// inside-block owner before beta movement reaches a directional collision scan.
    /// </summary>
    internal bool HeavySpikeDamageEnabled { get; set; } = true;

    /// <summary>Creates independent player state and owned movement/special-sequence children, binding live kinematics back to this owner and seeding its collision pose with the standing-right default.</summary>
    /// <remarks>Construction reads no game data and does not initialize room position, pose-dependent radii, animation data, or installed artwork; gameplay setup supplies those separately.</remarks>
    /// <param name="animationContinuation">What an animation frame past her pose's program reads, the session's installed service.</param>
    /// <param name="unauthoredBoostCadence">The session's unauthored Speed Booster cadence, which outlives her.</param>
    /// <param name="compatibility">The compatibility services she owns, created with her by whoever builds her.</param>
    /// <param name="beamCharge">The session's beam charge, which outlives her and which the projectile pass advances.</param>
    internal SamusState(LaidOutProgramContinuation<SamusPoseId, SamusAnimationProgramStep> animationContinuation, UnauthoredBoostCadence unauthoredBoostCadence,
        SamusCompatibility compatibility, SuperMetroid.Core.Samus.Projectiles.BeamChargeState beamCharge)
    {
        BeamCharge = Ensure.NotNull(beamCharge);
        AnimationContinuation = Ensure.NotNull(animationContinuation);
        UnauthoredBoostCadence = Ensure.NotNull(unauthoredBoostCadence);
        Ensure.NotNull(compatibility);
        ProjectileInheritance = compatibility.LaunchInheritance;
        SparkCarryover = compatibility.SparkCarryover;
        EscapeAndDrainCount = compatibility.EscapeAndDrainCount;
        HurtFlash = compatibility.HurtFlash;
        Shinespark = new SamusShinesparkState(compatibility.SparkCarryover, compatibility.CrashOrbitTravel);
        CrystalFlash = new SamusCrystalFlashState(compatibility.EscapeAndDrainCount);
        DraygonGrabbed = new SamusDraygonGrabbedState(compatibility.EscapeAndDrainCount);
        HudSelection = new SamusHudSelection();
        InputSnapshot = new SamusInputSnapshot();
        Presentation = new SamusPresentation();
        LiquidPhysics = new SamusLiquidPhysicsState(compatibility.EffectWaits, Presentation.Particles);
        Kinematics = new SamusKinematicsState();
        BindAudioRequests(AudioRequests);
        // The backing field supplies the retail standing-right default without invoking
        // the setter before property initializers have constructed Kinematics.
        Kinematics.CollisionPose = Pose;
    }

    /// <summary>
    /// How far Samus moved horizontally in the most recent scrolling pass, plus one pixel, as the camera measured it
    /// for its speed: not an unbiased distance. A Yard kick reads it.
    /// </summary>
    public SignedSixteenSixteen HorizontalDistanceMovedPlusOne { get; internal set; }

    /// <summary>
    /// The solid vertical collision result the latest vertical movement or wall jump published. Most
    /// movement methods return collision results directly; Draygon-held movement has no displacement and its sole
    /// side effect is explicitly clearing this otherwise-stale result.
    /// </summary>
    public SamusVerticalCollisionResult SolidVerticalCollisionResult { get; set; }

    /// <summary>Samus's room-pixel 16.16 position, owned by <see cref="Kinematics"/>.</summary>
    public FixedPosition<RoomPixels> Position
    {
        get => Kinematics.Position;
        set => Kinematics.Position = value;
    }

    /// <summary>
    /// Resolves the block-only portion of the pose-change collision for
    /// a target whose Y radius is larger than the live body's radius.
    /// </summary>
    /// <remarks>
    /// The game first probes the radius difference upward and downward while retaining
    /// the old radius. A one-sided hit calculates a compensating center shift, then probes
    /// that shift toward the opposite surface before committing it. Failure of that second
    /// probe restores the source pose; simultaneous initial hits select stable crouch. Solid-
    /// enemy collision is deliberately outside the current room slice, with no substitute.
    /// </remarks>
    internal LargerPoseCollisionOutcome ResolveLargerPoseCollision(

        RoomLevelData level,
        SamusPoseId targetPose,
        int frameCounter,
        ISamusRoomObjects? roomObjects,
        out int centerAdjustment)
    {
        int targetRadius = SamusPoseMetadata.ReadPoseYRadius(targetPose);
        if (targetRadius <= Kinematics.Radius.Y)
        {
            centerAdjustment = 0;
            return LargerPoseCollisionOutcome.Allowed;
        }

        int radiusDifference = targetRadius - Kinematics.Radius.Y;

        // The game checks enemies first, then terrain in a separate pass. Mixing the two
        // probes changes both available space and the order of their live Y-fraction
        // writes (enemy tangency clears it; a later floor scan can set it to FFFF).
        SolidEnemyCollisionResult enemyUp = ProbeEnemy(SamusCollisionDirection.Up, radiusDifference, Kinematics.Radius.Y);
        SolidEnemyCollisionResult enemyDown = ProbeEnemy(SamusCollisionDirection.Down, radiusDifference, Kinematics.Radius.Y);
        int enemyAdjustment = 0;
        if (enemyUp.Collided != enemyDown.Collided)
        {
            bool above = enemyUp.Collided;
            int remaining = radiusDifference - (above ? enemyUp.Distance : enemyDown.Distance);
            SolidEnemyCollisionResult opposite = ProbeEnemy(
                above ? SamusCollisionDirection.Down : SamusCollisionDirection.Up,
                remaining, targetRadius);
            if (opposite.Collided)
            {
                centerAdjustment = 0;
                return LargerPoseCollisionOutcome.RetainSource;
            }
            enemyAdjustment = above ? opposite.Distance : -opposite.Distance;
        }

        // The probes run against independent copies. The game stores both available-distance
        // results before choosing a branch, so neither probe may move the live body early.
        BlockMoveResult upward = ProbeChangedPoseVertical(

            level,
            displacement: SignedSixteenSixteen.FromWholePixels(-radiusDifference),
            scanLeftToRight: SamusCadenceDefinitions.BlockScansLeftToRight.IsDue(frameCounter),
            targetPose, roomObjects);
        BlockMoveResult downward = ProbeChangedPoseVertical(

            level,
            displacement: SignedSixteenSixteen.FromWholePixels(radiusDifference),
            scanLeftToRight: SamusCadenceDefinitions.BlockScansLeftToRight.IsDue(frameCounter),
            targetPose, roomObjects);

        if (upward.Collided && downward.Collided)
        {
            centerAdjustment = 0;
            // Compares the OLD live radius with eight. Radius seven means
            // Morph Ball, and that branch restores PreviousPose instead of trying to fit
            // stable crouch. Every non-morph compact/standing body still uses crouch.
            return Kinematics.Radius.Y < 8
                ? LargerPoseCollisionOutcome.RetainSource
                : LargerPoseCollisionOutcome.CrouchFallback;
        }

        centerAdjustment = 0;
        if (downward.Collided)
        {
            // Move away from the floor by
            // the radius difference less the space to move down, preserving the old bottom boundary.
            int freeWholePixels = Math.Max(0, downward.AcceptedDisplacement.WholePixels);
            centerAdjustment = -(radiusDifference - freeWholePixels);

            // Checks the proposed upward correction against the opposite side.
            // A collision here takes the collision exit and restores PreviousPose; it does not use
            // the crouch selector because the initial flags were not both set.
            BlockMoveResult opposite = ProbeChangedPoseVertical(
level,
                displacement: SignedSixteenSixteen.FromWholePixels(centerAdjustment),
                scanLeftToRight: SamusCadenceDefinitions.BlockScansLeftToRight.IsDue(frameCounter),
                targetPose, roomObjects: roomObjects);
            if (opposite.Collided)
                return LargerPoseCollisionOutcome.RetainSource;
            if (enemyUp.Collided)
                return BothSides();
        }
        else if (upward.Collided)
        {
            // Is the mirror: move down only as far as necessary to clear the
            // ceiling while admitting the enlarged body.
            int freeWholePixels = Math.Max(0, -upward.AcceptedDisplacement.WholePixels);
            centerAdjustment = radiusDifference - freeWholePixels;

            // Is the downward mirror of the opposite-side check above.
            BlockMoveResult opposite = ProbeChangedPoseVertical(
level,
                displacement: SignedSixteenSixteen.FromWholePixels(centerAdjustment),
                scanLeftToRight: SamusCadenceDefinitions.BlockScansLeftToRight.IsDue(frameCounter),
                targetPose, roomObjects: roomObjects);
            if (opposite.Collided)
                return LargerPoseCollisionOutcome.RetainSource;
            if (enemyDown.Collided)
                return BothSides();
        }
        else
        {
            if (enemyUp.Collided && enemyDown.Collided)
                return BothSides();
            centerAdjustment = enemyAdjustment;
        }

        // The game's successful correction branches publish current AND previous Y.
        // A zero correction in a collision branch still performs that write;
        // the no-collision branch does not replace the scrolling checkpoint.
        if (upward.Collided || downward.Collided || enemyUp.Collided || enemyDown.Collided)
            WritePreviousYPosition(CoordinateAxis.Wrap(Kinematics.Position.Y.Whole + centerAdjustment));
        return LargerPoseCollisionOutcome.Allowed;

        LargerPoseCollisionOutcome BothSides() => Kinematics.Radius.Y < 8
            ? LargerPoseCollisionOutcome.RetainSource : LargerPoseCollisionOutcome.CrouchFallback;

        SolidEnemyCollisionResult ProbeEnemy(SamusCollisionDirection direction, int distance, int radius)
        {
            SamusKinematicsState probe = Kinematics.CreateCollisionProbe();
            probe.Radius = probe.Radius.WithY(radius);
            SolidEnemyCollisionResult result = SamusSolidEnemyCollision.Probe(
                probe, probe.InteractiveEnemies, direction, CoordinateAxis.Wrap(distance), 0);
            Kinematics.Position = Kinematics.Position.WithFractionY(probe.Position.Y.Fraction);
            return result;
        }
    }

    /// <summary>
    /// Executes the unusual two-pass distance probe of the
    /// changed-pose collision detection.
    /// </summary>
    /// <remarks>
    /// A pose expansion of eight pixels or more is not tested only at its final leading
    /// boundary. The game first tests the signed displacement rounded to the intermediate
    /// eight-pixel boundary and returns immediately if that scan collides, then tests the
    /// complete displacement only when the intermediate scan was clear. That first scan is
    /// observable on square half-blocks: an eleven-pixel compact-to-standing expansion can
    /// cross from the solid lower quadrant into an air upper quadrant at its final boundary.
    /// A single final-boundary scan therefore misses the floor, expands Samus downward, and
    /// lets her fall through the platform. Each probe uses a separate kinematics copy because
    /// the collision check publishes a distance without moving live Samus.
    /// The probe carries the pose being installed: the pose initializer writes it to Samus's pose
    /// before the larger-radius check, so the elevator pseudo-door's standing-pose gate tests it.
    /// </remarks>
    internal BlockMoveResult ProbeChangedPoseVertical(

        RoomLevelData level,
        SignedSixteenSixteen displacement,
        bool scanLeftToRight,
        SamusPoseId targetPose,
        ISamusRoomObjects? roomObjects)
    {
        int wholePixels = displacement.WholePixels;
        if (Math.Abs(wholePixels) >= 8)
        {
            // The intermediate probe is the centre of the block step the displacement lies in, rounded toward lower
            // values rather than by magnitude: negative probes deliberately produce -8 for the known pose radii.
            BlockMoveResult intermediate = Probe(SignedSixteenSixteen.FromWholePixels(RoomGridConversions.BlockStepCentre(wholePixels)));
            if (intermediate.Collided)
                return intermediate;
        }

        return Probe(displacement);

        BlockMoveResult Probe(SignedSixteenSixteen amount)
        {
            SamusKinematicsState probe = Kinematics.CreateCollisionProbe();
            probe.CollisionPose = targetPose;
            BlockMoveResult result = SamusBlockCollision.ProbeChangedPoseVertical(
level, probe, this, amount, scanLeftToRight, roomObjects);
            // Submerging sand clears speed/gravity even during an observation.
            // Preserve these reaction writes without committing the probe's position.
            Kinematics.YSpeed = probe.YSpeed;
            Kinematics.YAcceleration = probe.YAcceleration;
            // Block dispatch clamps the live fractional Y even for a
            // changed-pose observation. Preserve that write, but never copy the
            // probe's whole-position movement into the live body.
            if (result.Collided)
                Kinematics.Position = Kinematics.Position.WithFractionY(probe.Position.Y.Fraction);
            return result;
        }
    }

    /// <summary>
    /// Applies the crouch selector's non-morph fallback after simultaneous above/below collision.
    /// Direction metadata selects ordinary crouching; an aimed crouch consequently loses its aim.
    /// </summary>
    internal void ApplyPoseChangeCollisionCrouchFallback(SamusPoseId sourcePose)
    {
        int oldRadius = Kinematics.Radius.Y;
        SamusPoseId fallbackPose = SamusPoseMetadata.IsFacing(this, SamusFacingDirection.Left)
            ? SamusPoseId.CrouchingLeftPose
            : SamusPoseId.CrouchingRightPose;
        if (sourcePose == fallbackPose)
            return;

        Pose = fallbackPose;
        int fallbackRadius = SamusPoseMetadata.ReadPoseYRadius(fallbackPose);
        // The collision fallback reads the prospective radius for center correction,
        // but leaves the live radius untouched until the next ordinary alpha phase.
        if (oldRadius < fallbackRadius)
        {
            // Subtracts the extra crouch radius from center Y. This is
            // normally invisible for radius-16 aimed crouches, but compact radius ten must
            // move up six pixels when simultaneous initial probes force ordinary crouching.
            Kinematics.Position = Kinematics.Position.WithWrappedWholeY(
                Kinematics.Position.Y.Whole - (fallbackRadius - oldRadius));
            WritePreviousYPosition(Kinematics.Position.Y.Whole);
        }
        SamusPresentation.InitializeAnimation(this, initialFrame: 0);
    }

    /// <summary>
    /// Absolute previous whole-pixel Y, 0 through 65535, written by a pose-collision correction this frame; null when
    /// nothing wrote it. Only <see cref="WritePreviousYPosition"/> stores it, and its readers wrap its sum with the
    /// alignment delta at sixteen bits, as the room coordinate does.
    /// </summary>
    private int? _poseCollisionPreviousYPosition;
    /// <summary>Relative previous-Y correction accumulated while preserving feet across a pose change.</summary>
    private int _poseAlignmentPreviousYDelta;
    /// <summary>
    /// Absolute previous whole-pixel X, 0 through 65535, written by a room or pose owner this frame; null when nothing
    /// wrote it. Only <see cref="WritePreviousXPosition"/> stores it.
    /// </summary>
    private int? _previousXPositionWrite;

    /// <summary>
    /// Records a direct write of Samus's previous X position by an owner that runs
    /// before the main scrolling, such as the landed gunship placing Samus.
    /// Only the whole pixels change; the previous X fraction is untouched (Mother Brain's
    /// rainbow-beam push records its fraction in <see cref="RainbowBeamCameraCheckpoint"/>).
    /// </summary>
    internal void WritePreviousXPosition(int xPosition) =>
        _previousXPositionWrite = xPosition;

    /// <summary>
    /// Aligns Samus's bottom with the previous pose after installing a new pose:
    /// preserve the feet and shift the previous whole-Y checkpoint by the same delta.
    /// Neither fraction changes.
    /// </summary>
    internal void AlignBottomAfterPoseChange(int previousRadius, int targetRadius)
    {
        int delta = previousRadius - targetRadius;
        Position += new Offset16<RoomPixels>(0, delta);
        _poseAlignmentPreviousYDelta += delta;
    }

    /// <summary>
    /// Records a direct write of Samus's previous Y position, such as
    /// changed-pose collision correction or facing forward's lift. The previous
    /// fraction is deliberately not changed: the game writes only the whole pixels, not the fraction.
    /// The write is absolute, discarding any earlier bottom-alignment shift this frame.
    /// </summary>
    internal void WritePreviousYPosition(int correctedY)
    {
        _poseCollisionPreviousYPosition = correctedY;
        _poseAlignmentPreviousYDelta = 0;
    }

    /// <summary>
    /// Samus's live previous X and Y positions: the
    /// frame-start checkpoint with this frame's writes so far, without consuming them.
    /// </summary>
    internal Position16<RoomPixels> PeekPreviousPosition(FixedPosition<RoomPixels> previous) => new Position16<RoomPixels>(        _previousXPositionWrite ?? previous.X.Whole,
        CoordinateAxis.Wrap((_poseCollisionPreviousYPosition ?? previous.Y.Whole) + _poseAlignmentPreviousYDelta));

    /// <summary>
    /// Applies this frame's whole previous-position writes before scrolling calculates
    /// distance; the fractions are kept. Its normal tail subsequently replaces the complete checkpoint. The written X is
    /// a whole pixel of the room's coordinates, which <see cref="FixedCoordinate"/> holds (#1627).
    /// </summary>
    internal FixedPosition<RoomPixels> ApplyPreviousPositionWrites(FixedPosition<RoomPixels> previous)
    {
        var written = new FixedPosition<RoomPixels>(
            FixedCoordinate.Of(_previousXPositionWrite ?? previous.X.Whole, previous.X.Fraction),
            FixedCoordinate.Of(CoordinateAxis.Wrap((_poseCollisionPreviousYPosition ?? previous.Y.Whole) + _poseAlignmentPreviousYDelta),
                previous.Y.Fraction));
        _poseCollisionPreviousYPosition = null;
        _poseAlignmentPreviousYDelta = 0;
        _previousXPositionWrite = null;
        return written;
    }

    /// <summary>Persistent low-energy warning state, shared by beta and reserve recovery.</summary>
    public SamusHealthWarningState HealthWarning => field ??= new();

    /// <summary>
    /// Copies the inventory and energy, which lie below the Samus
    /// state that Samus's initialization clears. A Samus created
    /// fresh and given these values is the initialization's result for the same player.
    /// </summary>
    internal void CopyInventoryFrom(SamusState source)
    {
        Ensure.NotNull(source);
        EquippedItems = source.EquippedItems;
        CollectedItems = source.CollectedItems;
        EquippedBeams = source.EquippedBeams;
        CollectedBeams = source.CollectedBeams;
        ReserveTankMode = source.ReserveTankMode;
        Health = source.Health;
        MaxHealth = source.MaxHealth;
        Missiles = source.Missiles;
        MaxMissiles = source.MaxMissiles;
        SuperMissiles = source.SuperMissiles;
        MaxSuperMissiles = source.MaxSuperMissiles;
        PowerBombs = source.PowerBombs;
        MaxPowerBombs = source.MaxPowerBombs;
        MaxReserveEnergy = source.MaxReserveEnergy;
        ReserveEnergy = source.ReserveEnergy;
        ReserveMissiles = source.ReserveMissiles;
    }

    /// <summary>
    /// Called by the unpause command: reconcile Morph/Spring Ball
    /// equipment without changing position, velocity, or vertical direction.
    /// </summary>
    public void ReconcilePauseBallEquipment()
    {
        bool springEquipped = EquippedItems.HasAny(SamusEquipmentFlags.SpringBall);
        SamusMovementType movement = SamusPoseMetadata.ReadMovementType(this);
        bool enable = springEquipped && movement is
            SamusMovementType.MorphBallGround or SamusMovementType.MorphBallFalling;
        bool disable = !springEquipped && movement is
            SamusMovementType.SpringBallGround or SamusMovementType.SpringBallInAir or SamusMovementType.SpringBallFalling;
        if (!enable && !disable)
            return;

        // The handlers select the stationary pose even while airborne. Keeping
        // ascent intact here permits the equipment-toggle mid-air Spring Ball jump.
        bool left = SamusPoseMetadata.IsFacing(this, SamusFacingDirection.Left);
        Pose = enable
            ? left ? SamusPoseId.SpringBallGroundLeftPose : SamusPoseId.SpringBallGroundRightPose
            : left ? SamusPoseId.MorphBallGroundLeftPose : SamusPoseId.MorphBallGroundRightPose;
        SamusPoseMetadata.RefreshCollisionRadii(this);
        // Preserve animation only within the same ball family. Crossing
        // families through the equipment menu therefore runs the normal frame-zero setup.
        SamusPresentation.InitializeAnimation(this);
    }

    /// <summary>Transition-owned pose history.</summary>
    public SamusPoseHistoryState PoseHistory { get; } = new();

    /// <summary>
    /// Commits current pose metadata through the four-entry history shift.
    /// Transition owners must call this after their final pose/metadata selection,
    /// not for intermediate assignments, rollbacks, or ordinary frame sampling.
    /// </summary>
    internal void CommitPoseHistory()
    {
        PoseHistory.CommitTransition(Pose, SamusPoseDispatch.ReadMovement(InitializedPose));
    }
}
