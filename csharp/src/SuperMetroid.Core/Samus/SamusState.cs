using SuperMetroid.Core.Compatibility;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Samus.Artwork;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Samus.Abilities;
using SuperMetroid.Core.Samus.Grapple;
using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.Core.Samus.Colours;
using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.Core.Samus.Death;
using SuperMetroid.Core.Samus.Drawing;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Samus.Projectiles;

namespace SuperMetroid.Core.Samus;

/// <summary>
/// Samus's gameplay state: her pose, movement, abilities, projectiles' shared state, presentation and equipment.
/// </summary>
/// <remarks>
/// The movement, pose, ability and drawing systems share it. The Landing Site cinematic actor is not part of it.
/// </remarks>
public sealed class SamusState
{
    /// <summary>The installed input and movement modes and the control setups that replace them.</summary>
    internal SamusControl Control { get; } = new();

    /// <summary>Samus as room objects read and change her.</summary>
    internal IRoomSamus RoomView => field ??= new SamusRoomView(this);

    /// <summary>The <see cref="SamusInputSnapshot"/> owner of this SamusState.</summary>
    internal SamusInputSnapshot InputSnapshot { get; }

    /// <summary>The <see cref="SamusPresentation"/> owner of this SamusState.</summary>
    internal SamusPresentation Presentation { get; }

    /// <summary>Samus's current pose.</summary>
    /// <remarks>
    /// An ordinary write also models the pose setup, publishing the
    /// pose's direction and movement type to <see cref="InitializedPose"/>. Only the
    /// writes that skip that setup use <see cref="WritePoseWithoutInitialization"/>.
    /// </remarks>
    public SamusPoseId Pose
    {
        get;
        set
        {
            field = value;
            InitializedPose = value;
        }
    } = SamusPoseId.FacingRightNormalPose;

    /// <summary>
    /// The pose whose facing and movement type the pose setup last copied to
    /// the cached facing and movement type. Movement and camera
    /// tracking read those cached values, not the live pose.
    /// </summary>
    public SamusPoseId InitializedPose { get; private set; } = SamusPoseId.FacingRightNormalPose;

    /// <summary>
    /// Writes the pose as the game does when it skips the pose setup
    /// (the drained Samus sequences): the previous pose's
    /// direction and movement type stay in force.
    /// </summary>
    internal void WritePoseWithoutInitialization(SamusPoseId pose)
    {
        SamusPoseId initialized = InitializedPose;
        Pose = pose;
        InitializedPose = initialized;
    }

    /// <summary>
    /// A control setup's cancellation of her pending pose transition during the enemies: the setup clears every prospective
    /// pose and momentum response, so this frame's pending transition is not committed; the movement phase's resample takes
    /// it before the pose update.
    /// </summary>
    public PoseTransitionCancellation PoseTransitionCancellation { get; } = new();

    /// <summary>
    /// Where Samus's animation stands (#2048): a frame of a named segment of the animation of <see cref="AnimatedPose"/>.
    /// Every frame the animation reaches names a step of that animation or of the animation a frame read past its end reaches, and reading
    /// a frame that names no step fails (<see cref="AnimationStep"/>).
    /// </summary>
    internal SamusAnimationFrame AnimationFrame { get; set; }

    /// <summary>The pose whose animation <see cref="AnimationFrame"/> is a frame of: the animated pose, or before her animation starts, her pose.</summary>
    internal SamusPoseId AnimationPose => AnimatedPose ?? Pose;

    /// <summary>Sets her animation frame to <paramref name="place"/>, as it lands in the animation she animates by (<see cref="SamusAnimationDelays.Land"/>).</summary>
    internal void SetAnimationFrame(SamusAnimationPlace place) => AnimationFrame = SamusAnimationDelays.Land(AnimationPose, place, FramesBeyondAnimation);

    /// <summary>Moves her animation on to the frame after the one it is on, a frame past her animation's end included.</summary>
    internal void AdvanceAnimationFrame() => AnimationFrame = SamusAnimationDelays.Next(AnimationPose, AnimationFrame, FramesBeyondAnimation);

    /// <summary>Moves her animation back to the first frame of the animation she animates by.</summary>
    internal void RestartAnimation() => AnimationFrame = SamusAnimationDelays.AnimationForPose(AnimationPose).Start;

    /// <summary>Moves her animation to the frame <paramref name="target"/> names, as a cue read on her current frame names it.</summary>
    internal void MoveAnimationTo(SamusAnimationFrame target) =>
        AnimationFrame = SamusAnimationDelays.Target(AnimationPose, AnimationFrame, target, FramesBeyondAnimation);

    /// <summary>
    /// Her animation frame read as a frame of <paramref name="pose"/>'s animation, which must be the animation she animates
    /// by: every path that gives her a pose of another animation starts that animation before her body is drawn.
    /// </summary>
    /// <exception cref="InvalidOperationException"><paramref name="pose"/> animates by another animation than hers.</exception>
    internal SamusAnimationFrame AnimationFrameOf(SamusPoseId pose) =>
        ReferenceEquals(SamusAnimationDelays.AnimationForPose(pose), SamusAnimationDelays.AnimationForPose(AnimationPose))
            ? AnimationFrame
            : throw new InvalidOperationException(
                $"Samus's pose {pose} animates by another animation than the one of pose {AnimationPose}, whose frame {AnimationFrame} she is on.");

    /// <summary>Whether her animation has reached <paramref name="place"/>, as it lies in the animation she animates by.</summary>
    internal bool AnimationHasReached(SamusAnimationPlace place) => SamusAnimationDelays.HasReached(AnimationPose, AnimationFrame, place, FramesBeyondAnimation);

    /// <summary>Whether her animation lies past <paramref name="place"/>, as it lies in the animation she animates by.</summary>
    internal bool AnimationHasPassed(SamusAnimationPlace place) => SamusAnimationDelays.HasPassed(AnimationPose, AnimationFrame, place, FramesBeyondAnimation);

    /// <summary>Whether her animation is on <paramref name="frame"/> of the animation she animates by.</summary>
    internal bool AnimationIsAt(SamusAnimationFrame frame) =>
        AnimationFrame == SamusAnimationDelays.AnimationForPose(AnimationPose).Require(frame);

    /// <summary>Her animation frame, named by its segment, for diagnostics.</summary>
    internal string AnimationFrameName =>
        $"{AnimationFrame.Segment} frame {AnimationFrame.Frame}";

    /// <summary>Whether her animation is on <paramref name="place"/>, as it lands in the animation she animates by.</summary>
    internal bool AnimationIsOn(SamusAnimationPlace place) => AnimationFrame == SamusAnimationDelays.Land(AnimationPose, place, FramesBeyondAnimation);

    /// <summary>
    /// The frame of the run's stride her animation is on, which the running cadence times (<see cref="SamusRunningCadences"/>)
    /// and the footsteps land on: a frame of the running animation's stride, which every pose that runs animates by.
    /// </summary>
    /// <exception cref="InvalidOperationException">Her animation is not on the stride.</exception>
    internal SamusAnimationFrame RunStrideFrame =>
        AnimationFrame.Segment == SamusAnimationSegmentName.Run
            ? AnimationFrame
            : throw new InvalidOperationException($"Samus's animation frame {AnimationFrameName} is not on the run's stride.");

    /// <summary>
    /// Animation frame countdown. Zero and negative-after-decrement both advance,
    /// matching the game's zero and positive tests.
    /// </summary>
    public WrappingCountdown AnimationFrameTimer { get; internal set; }

    /// <summary>
    /// The updates added to each frame of Samus's animation (#2286): two in water, three in lava or acid, and on dry ground
    /// her <see cref="SlowdownLevel"/>, so 0 through <see cref="SamusHorizontalSpeedState.LargestSlowdownLevel"/>; the
    /// setter rejects any other value.
    /// </summary>
    public int ExtraAnimationDelay { get; internal set => field = Ensure.BetweenInclusive(0, SamusHorizontalSpeedState.LargestSlowdownLevel, value); }

    /// <summary>
    /// Her horizontal speed: her running and momentum speeds, the Speed Booster's boost and how slowed she is.
    /// </summary>
    public SamusHorizontalSpeedState HorizontalSpeed { get; }

    /// <summary>
    /// Live room-FX surface values and remembered liquid medium. Physics, animation, and
    /// pose initialization intentionally share this object because the game shares
    /// room-FX values across all three phases.
    /// </summary>
    public SamusLiquidPhysicsState LiquidPhysics { get; }

    /// <summary>The periodic damage liquids, spikes, heat, the grapple's spikes and turrets, and enemies add to through an update.</summary>
    public SamusPeriodicDamage PeriodicDamage { get; } = new();

    /// <summary>
    /// The ground her footstep and landing effects go by (#2416): every room load sets its room's, and a scene that
    /// loads no room sets its own; null until either does.
    /// </summary>
    internal SamusGround? Ground { get; set; }

    /// <summary>The ground her footstep and landing effects go by, once a room or a scene has set it.</summary>
    /// <exception cref="InvalidOperationException">Nothing has set her ground yet.</exception>
    internal SamusGround StandingGround => Ground ?? throw new InvalidOperationException("Samus's ground is read before any room or scene has set it.");

    /// <summary>
    /// Stored-shine colouring countdown and the shinespark's hold on her movement.
    /// Its fields are the values documented on
    /// <see cref="SamusShinesparkState"/> and keep their individual values.
    /// </summary>
    public SamusShinesparkState Shinespark { get; }

    /// <summary>
    /// The update's audio requests every sound of hers, her movement's and her spark's among them, joins as it is made: her
    /// runtime's, or a cinematic's (#2057).
    /// </summary>
    internal GameplaySoundRequests AudioRequests { get; }

    /// <summary>
    /// Crystal Flash initiation, ammo cadence, energy restoration, and its hold on her
    /// movement. It is deliberately separate from <see cref="Shinespark"/> even though the
    /// game shares a few values between the mutually exclusive effects.
    /// </summary>
    public SamusCrystalFlashState CrystalFlash { get; }

    /// <summary>
    /// X-ray admission, dedicated pose input/movement, beam-angle state, visor colours, and
    /// teardown. Revealed-block pictures and the cone remain presentation-owned outputs.
    /// </summary>
    public SamusXrayState Xray { get; }

    /// <summary>
    /// The one special suit colouring holding Samus's suit, which the stored shine, the shinespark, Crystal Flash and the
    /// X-ray visor take from each other.
    /// </summary>
    public SamusSpecialColouring SpecialColouring { get; } = new();

    /// <summary>
    /// Fatal-damage death-pose ownership, death pictures/colours, whiteout, and suit explosion.
    /// The outer music wait and post-explosion room fade belong to the death game states.
    /// </summary>
    public SamusDeathSequenceState DeathSequence { get; } = new();

    /// <summary>
    /// Mother Brain/Baby Metroid drain poses, the drain's operations, and the drained
    /// fall. Enemy AI still runs these drain operations itself.
    /// </summary>
    public SamusDrainedState Drained { get; } = new();

    /// <summary>
    /// Draygon's hold: the grabbed poses, the escape check and the narrow owner-position seam.
    /// Draygon's enemy AI owns its own flight path; this child owns every Samus-side value.
    /// </summary>
    public SamusDraygonGrabbedState DraygonGrabbed { get; }

    /// <summary>
    /// Ceres Ridley's dedicated push and fall. This is not ordinary damage
    /// knockback: the shove takes her movement without
    /// consuming energy, and owns Samus until horizontal room collision.
    /// Keeping that lifetime separate prevents the hurt countdown from ending the
    /// scripted ejection early or making normal controller input steer it.
    /// </summary>
    public SamusCeresRidleyEjectionState CeresRidleyEjection { get; } = new();

    /// <summary>
    /// How slowed Samus is (#2286), 0 through <see cref="SamusHorizontalSpeedState.LargestSlowdownLevel"/>, as her own view
    /// of <see cref="SamusHorizontalSpeedState.SlowdownLevel"/>. The game keeps one level for two effects: her speed is
    /// halved once per level, and on dry ground each level adds an update to every frame of her animation
    /// (<see cref="ExtraAnimationDelay"/>). The setter rejects values outside that range.
    /// </summary>
    [IntegerQuantity("halvings of Samus's horizontal speed, zero when she is not slowed")]
    public int SlowdownLevel
    {
        get => HorizontalSpeed.SlowdownLevel;
        set => HorizontalSpeed.SlowdownLevel = value;
    }

    /// <summary>Current energy, also used by the animation's low-energy cue.</summary>
    public int Health { get; set; } = SamusEnergyDefinitions.StartingEnergy;

    /// <summary>
    /// The previous energy for the hurt check: initialized from Energy when Samus is initialized,
    /// then compared and refreshed after Samus/projectile drawing by the post-draw input filter.
    /// </summary>
    public int PreviousHealthForHurtCheck { get; set; } = SamusEnergyDefinitions.StartingEnergy;

    /// <summary>
    /// The fraction of one energy Samus holds beyond <see cref="Health"/>. The HUD shows only <see cref="Health"/>, but
    /// periodic damage takes its fraction from this one first (<see cref="TakeFractionalEnergyDamage"/>), so the damage
    /// of one update is never rounded on its own; how the fraction reaches the whole energy is the compatibility layer's
    /// (<see cref="PeriodicDamageEnergyFraction"/>).
    /// </summary>
    public PeriodicDamageEnergyFraction EnergyFraction { get; private set; }

    /// <summary>
    /// Takes periodic damage as the game's periodic damage does: the fraction comes off <see cref="EnergyFraction"/>
    /// first, taking one more whole energy when it is more than the fraction held
    /// (<see cref="PeriodicDamageEnergyFraction.Borrows"/>); the whole damage and that one energy then come off
    /// <see cref="Health"/>. A result below zero energy leaves no energy and no fraction instead.
    /// </summary>
    /// <param name="damage">The damage, in whole energy and a fraction of one.</param>
    internal void TakeFractionalEnergyDamage(EnergyAmount damage)
    {
        bool borrows = EnergyFraction.Borrows(damage);
        int health = Health - damage.WholeEnergy - (borrows ? 1 : 0);
        if (health < 0)
        {
            EnergyFraction = PeriodicDamageEnergyFraction.None;
            Health = 0;
            return;
        }

        EnergyFraction = EnergyFraction.After(damage);
        Health = health;
    }

    /// <summary>Maximum normal energy; defaults to the initial 99.</summary>
    public int MaxHealth { get; set; } = SamusEnergyDefinitions.StartingEnergy;

    /// <summary>Current reserve energy.</summary>
    public int ReserveEnergy { get; set; }

    /// <summary>Maximum reserve energy.</summary>
    public int MaxReserveEnergy { get; set; }

    /// <summary>Reserve-tank mode: off, automatic or manual.</summary>
    public ReserveTankMode ReserveTankMode { get; set; }

    /// <summary>Current missiles.</summary>
    public int Missiles { get; set; }

    /// <summary>Maximum missiles.</summary>
    public int MaxMissiles { get; set; }

    /// <summary>Current super missiles.</summary>
    public int SuperMissiles { get; set; }

    /// <summary>Maximum super missiles.</summary>
    public int MaxSuperMissiles { get; set; }

    /// <summary>Current power bombs.</summary>
    public int PowerBombs { get; set; }

    /// <summary>Maximum power bombs.</summary>
    public int MaxPowerBombs { get; set; }

    /// <summary>The item selected on the HUD, which decides what Samus fires.</summary>
    public HudItem SelectedHudItem { get; set; }

    /// <summary>
    /// The item chosen for auto-cancel, by pressing Select while Item Cancel is held: the HUD returns to the beam when its
    /// use ends. <see cref="HudItem.Beam"/> when no item was chosen that way.
    /// </summary>
    public HudItem AutoCancelledItem { get; set; }

    /// <summary>Returns the HUD to the beam, with no item chosen for auto-cancel.</summary>
    public void SelectBeam()
    {
        SelectedHudItem = HudItem.Beam;
        AutoCancelledItem = HudItem.Beam;
    }

    /// <summary>
    /// The beams she has equipped. Equipping the Hyper Beam in the drain replaces them with Charge, Wave and Plasma and
    /// the Hyper Beam.
    /// </summary>
    public SamusBeamFlags EquippedBeams { get; set; }

    /// <summary>
    /// The beams she has collected. Unlike
    /// <see cref="EquippedBeams"/>, this retains beams disabled from the pause equipment
    /// screen and is therefore the authoritative ownership record the save file keeps.
    /// </summary>
    public SamusBeamFlags CollectedBeams { get; set; }

    /// <summary>
    /// The beam's charge (#1577): the session's one count, which the projectile pass advances and her movement, bombs,
    /// sounds and colourings read; at least 60 is a full charge, which makes her spin and wall jump a pseudo Screw Attack.
    /// Every clear, hers or the projectiles', clears it for every reader.
    /// </summary>
    internal BeamChargeState BeamCharge { get; }

    /// <summary>
    /// The shared bomb-spread charge timeout. The game clears this
    /// timeout in the morphing-transition pose setup after a
    /// successful Morph Ball item check, regardless of the humanoid movement family that
    /// selected the transition. Keeping it separate from the beam flare counter prevents a
    /// mid-air morph from accidentally cancelling ordinary arm-cannon charge state.
    /// </summary>
    /// <remarks>
    /// The hold of Down while a spread is charged, lasting
    /// <see cref="Projectiles.MorphBallBombDefinitions.DownChargeTimeoutUpdates"/>: its age, the updates Down was held,
    /// chooses the spread's launch, it stops counting once over, and its other producers start it again.
    /// </remarks>
    public UpdateCountdown BombSpreadCharge { get; set; } = UpdateCountdown.Lasting(Projectiles.MorphBallBombDefinitions.DownChargeTimeoutUpdates);

    /// <summary>
    /// The shot a pose transition handed to the next update's weapon firing, or null. A pose setup can hand one
    /// after the current update's weapon firing has already run: a normal jump started on a fresh Shoot press hands
    /// the new pose's direction, and a turn out of a moonwalk hands the moonwalk's. The next update's weapon firing
    /// fires it, and that update then drops it whether or not a shot was fired.
    /// </summary>
    public TransitionShot<SamusTransitionShot> TransitionShot { get; } = new();

    /// <summary>Whether Mother Brain's hyper beam is enabled.</summary>
    public bool HyperBeamEnabled { get; set; }

    /// <summary>
    /// Equipped-item flags. The animation cue's equipment test and the Morph-Ball projectile
    /// producer read them directly.
    /// </summary>
    public SamusEquipmentFlags EquippedItems { get; set; }

    /// <summary>
    /// The items she has collected. This normally contains
    /// every equipped item, but equipment-menu toggles can make the two sets differ. Enemy
    /// logic such as the morph-ball eye tests ownership through this set specifically.
    /// </summary>
    public SamusEquipmentFlags CollectedItems { get; set; }

    /// <summary>
    /// The suit colours a draining Metroid or the Baby Metroid makes Samus flash through next, or inactive. Attachment
    /// starts on the Speed Booster colouring; the colour handling then alternates it with the normal suit every update
    /// until the attacker lets go.
    /// </summary>
    public SamusMetroidAttachmentShade MetroidAttachmentShade { get; internal set; }

    /// <summary>How far a landing Morph Ball or Spring Ball has bounced; see <see cref="SamusBallBounceState"/>.</summary>
    public SamusBallBounceState MorphBallBounceState { get; set; }

    /// <summary>
    /// The bomb jump a bomb's blast asked of Samus: its launch direction, and whether her pose handling has accepted it,
    /// which keeps the same request from arming the jump again.
    /// </summary>
    public SamusBombJumpRequest BombJumpRequest { get; set; }

    /// <summary>True for the bomb jump's one-frame start, which sets her velocity (<see cref="SamusMovementOwner.BombJump"/>).</summary>
    public bool BombJumpStarting { get; set; }

    /// <summary>True while the bomb jump moves Samus along its rising arc (<see cref="SamusMovementOwner.BombJump"/>).</summary>
    public bool BombJumpActive { get; set; }

    /// <summary>The bomb jump locked her pose input; independent of who moves her now.</summary>
    public bool BombJumpPoseInputLocked { get; set; }

    /// <summary>Which way the pending or running knockback moves Samus, or none.</summary>
    public Movement.SamusKnockbackDirection KnockbackDirection { get; set; }

    /// <summary>Which way the pending or active knockback moves Samus horizontally.</summary>
    public Movement.KnockbackHorizontalDirection KnockbackXDirection { get; set; }

    /// <summary>
    /// The count her hurt flash runs on, which a knockback, a health loss and a moving shinespark write; the hurt flash's
    /// shading advances it and clears it on reaching sixty (<see cref="SamusHurtFlashDefinitions.EndingUpdate"/>).
    /// </summary>
    public SharedHurtFlashCount HurtFlash { get; }

    /// <summary>
    /// Whether the charging-beam sound resumes on the next post-draw sound pass if Shoot is still held (#1682). Hurt-flash
    /// recovery sets it at counter forty when a non-spinning Samus is still holding a sufficiently charged beam, and the
    /// post-draw sound check consumes it later in the same gameplay pass; a spin's end sets it for the next pass.
    /// </summary>
    public ChargingSoundResumption ChargingSound { get; } = new();

    /// <summary>
    /// The general Samus invincibility timer tested by enemy, projectile,
    /// ordinary spike, spike-air, and grapple-swing damage producers. The gameplay update
    /// decrements it once near the end of every gameplay frame, after all those producers
    /// have had an opportunity to reject or publish damage.
    /// </summary>
    public WrappingCountdown InvincibilityTimer { get; set; }

    /// <summary>
    /// The knockback hold: started for five updates by enemy damage collision and counted once per gameplay update. The
    /// knockback holds her movement only while it runs.
    /// </summary>
    public UpdateCountdown KnockbackHold { get; set; } = UpdateCountdown.Ended();

    /// <summary>Whether no hit holds Samus: her knockback hold has run out and no knockback is pending or moving her.</summary>
    public bool HasNoKnockback => KnockbackHold.IsOver && KnockbackDirection == Movement.SamusKnockbackDirection.None;

    /// <summary>Clears Samus's two own hurt timers: no invincibility is left and no knockback is held.</summary>
    public void ClearHurtTimers()
    {
        InvincibilityTimer = WrappingCountdown.Start(0);
        KnockbackHold = UpdateCountdown.Ended();
    }

    /// <summary>
    /// Publishes a hit's request: Samus is invincible for <paramref name="invincibilityUpdates"/> and her knockback is held
    /// for <paramref name="knockbackUpdates"/>, moving her <paramref name="direction"/> once she admits it.
    /// </summary>
    public void RequestKnockback(int invincibilityUpdates, int knockbackUpdates, Movement.KnockbackHorizontalDirection direction)
    {
        InvincibilityTimer = WrappingCountdown.Start(invincibilityUpdates);
        KnockbackHold = UpdateCountdown.For(knockbackUpdates);
        KnockbackXDirection = direction;
    }

    /// <summary>
    /// Decrements Samus's two own hurt timers.
    /// This deliberately lives outside every individual movement
    /// owner: grapple spikes can start the timers without installing knockback movement,
    /// and the end of the gameplay update ages them even while time is frozen.
    /// </summary>
    public void DecrementHurtTimers()
    {
        InvincibilityTimer = InvincibilityTimer.DecrementToZero();
        KnockbackHold.Step();
    }

    /// <summary>
    /// Her grapple beam: its phase, its anchor and her swing on it.
    /// </summary>
    public SamusGrappleState Grapple { get; }

    /// <summary>
    /// The arm cannon's independent opening cover and its one-picture draw, used by aimed weapon
    /// poses. Its state is driven by HUD selection rather than by the body animation timer.
    /// </summary>
    public SamusArmCannonState ArmCannon { get; }

    /// <summary>The installed artwork her body, cannon cover, particles, death explosion and grapple beam draw from.</summary>
    public SamusArtworkBinding Artwork { get; }

    /// <summary>
    /// The room's visor shade cycle, advancing every five updates, used only by backdrop-blending rooms.
    /// </summary>
    public SamusVisorShadeState VisorShades { get; } = new();

    /// <summary>
    /// Whether the knockback moves Samus (<see cref="SamusMovementOwner.Knockback"/>). It is separate from
    /// the knockback movement type because the game also uses that type for crystal-flash art.
    /// </summary>
    public bool KnockbackActive { get; set; }

    /// <summary>
    /// The pose the animation was last started for, whose animation <see cref="AnimationFrame"/> steps through; none before
    /// the animation is first started. Changing the pose to one with another animation without restarting the animation
    /// is a defect the next animation tick reports.
    /// </summary>
    public SamusPoseId? AnimatedPose { get; internal set; }

    /// <summary>The step of the animated pose's animation that Samus's animation frame is on.</summary>
    /// <exception cref="InvalidOperationException">The animation has not been started.</exception>
    internal SamusPoseAnimationStep AnimationStep => SamusAnimationDelays.StepFor(
        AnimatedPose ?? throw new InvalidOperationException("Samus's animation has not been started for any pose."), AnimationFrame,
        FramesBeyondAnimation);

    /// <summary>
    /// What an animation frame past her pose's animation reads, the session's installed service, which decides the frames her
    /// body can show.
    /// </summary>
    internal FramesBeyondAnimation<SamusAnimationName, SamusAnimationFrame, SamusAnimationPlaceName, SamusPoseAnimationStep> FramesBeyondAnimation { get; }

    /// <summary>
    /// The pose the animation's pose-transition cue named, which outranks every other pose transition; it names none while
    /// the animation has not reached such a cue.
    /// </summary>
    /// <remarks>
    /// This holds the pose between the animation's cue and the pose update that installs it. Samus's pose handling
    /// consumes it after animation, at the same pose-transition point used by the game.
    /// Keeping it explicit makes the turn's final facing change its own step.
    /// </remarks>
    public AnimationCuePose<SamusPoseId> CuePose { get; } = new();

    /// <summary>The position, to a fraction of a pixel, and the radii that collision reads.</summary>
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
    internal SuitPickupSparkCarryover<SamusSuitPickupStage> SparkCarryover { get; }

    /// <summary>
    /// The count her Crystal Flash's ammunition drain and her escape from Draygon share, on which a suit pickup
    /// transforming her leaves its stage.
    /// </summary>
    internal SharedEscapeAndDrainCount<SamusSuitPickupStage> EscapeAndDrainCount { get; }

    /// <summary>
    /// The session's unauthored Speed Booster cadence, which times her run when a full sound queue sends the boost onto
    /// it, with the delays Ceres Ridley's getaway left behind.
    /// </summary>
    internal UnauthoredBoostCadence UnauthoredBoostCadence { get; }

    /// <summary>Creates independent player state and owned movement/special-sequence children, binding live kinematics back to this owner and seeding its collision pose with the standing-right default.</summary>
    /// <remarks>Construction reads no game data and does not initialize room position, pose-dependent radii or animation data; gameplay setup supplies those separately.</remarks>
    /// <param name="audioRequests">The update's audio requests of whoever runs her, which every sound of hers joins (#2057).</param>
    /// <param name="framesBeyondAnimation">What an animation frame past her pose's animation reads, the session's installed service.</param>
    /// <param name="unauthoredBoostCadence">The session's unauthored Speed Booster cadence, which outlives her.</param>
    /// <param name="compatibility">The compatibility services she owns, created with her by whoever builds her.</param>
    /// <param name="beamCharge">The session's beam charge, which outlives her and which the projectile pass advances.</param>
    /// <param name="artwork">The installed artwork of whoever builds her, which outlives her and follows its rebinding.</param>
    internal SamusState(GameplaySoundRequests audioRequests,
        FramesBeyondAnimation<SamusAnimationName, SamusAnimationFrame, SamusAnimationPlaceName, SamusPoseAnimationStep> framesBeyondAnimation,
        UnauthoredBoostCadence unauthoredBoostCadence, SamusCompatibility compatibility, BeamChargeState beamCharge,
        SamusArtworkBinding artwork)
    {
        AudioRequests = Ensure.NotNull(audioRequests);
        BeamCharge = Ensure.NotNull(beamCharge);
        Artwork = Ensure.NotNull(artwork);
        Grapple = new SamusGrappleState(artwork);
        ArmCannon = new SamusArmCannonState(artwork);
        FramesBeyondAnimation = Ensure.NotNull(framesBeyondAnimation);
        UnauthoredBoostCadence = Ensure.NotNull(unauthoredBoostCadence);
        Ensure.NotNull(compatibility);
        HorizontalSpeed = new SamusHorizontalSpeedState(compatibility);
        ProjectileInheritance = compatibility.LaunchInheritance;
        SparkCarryover = compatibility.SparkCarryover;
        EscapeAndDrainCount = compatibility.EscapeAndDrainCount;
        HurtFlash = compatibility.HurtFlash;
        Shinespark = new SamusShinesparkState(compatibility.SparkCarryover, compatibility.CrashOrbitTravel, SpecialColouring, AudioRequests);
        CrystalFlash = new SamusCrystalFlashState(compatibility.EscapeAndDrainCount, SpecialColouring);
        Xray = new SamusXrayState(SpecialColouring);
        DraygonGrabbed = new SamusDraygonGrabbedState(compatibility.EscapeAndDrainCount);
        InputSnapshot = new SamusInputSnapshot();
        Presentation = new SamusPresentation(artwork);
        LiquidPhysics = new SamusLiquidPhysicsState(compatibility.EffectWaits, Presentation.Particles, AudioRequests, PeriodicDamage);
        Kinematics = new SamusKinematicsState();
    }

    /// <summary>
    /// The solid vertical collision result the latest vertical movement or wall jump published. Most
    /// movement methods return collision results directly; Draygon-held movement has no displacement and its sole
    /// side effect is explicitly clearing this otherwise-stale result.
    /// </summary>
    public SamusVerticalCollisionResult SolidVerticalCollisionResult { get; set; }

    /// <summary>Samus's room position, to a fraction of a pixel, owned by <see cref="Kinematics"/>.</summary>
    public FixedPosition<RoomPixels> Position
    {
        get => Kinematics.Position;
        set => Kinematics.Position = value;
    }

    /// <summary>
    /// Resolves the pose-change collision for a target whose Y radius is larger than the live body's radius: solid
    /// enemies first, then blocks.
    /// </summary>
    /// <remarks>
    /// The game first probes the radius difference upward and downward while retaining
    /// the old radius. A one-sided hit calculates a compensating center shift, then probes
    /// that shift toward the opposite surface before committing it. Failure of that second
    /// probe restores the source pose; simultaneous initial hits select stable crouch.
    /// </remarks>
    internal LargerPoseCollisionOutcome ResolveLargerPoseCollision(
        RoomLevelData level,
        SamusPoseId targetPose,
        int frameCounter,
        ISamusRoomObjects roomObjects,
        out int centerAdjustment)
    {
        int targetRadius = SamusPoseMetadata.ReadPoseYRadius(targetPose);
        if (targetRadius <= Kinematics.Radius.Y)
        {
            centerAdjustment = 0;
            return LargerPoseCollisionOutcome.Allowed;
        }

        int radiusDifference = targetRadius - Kinematics.Radius.Y;

        // Enemies are probed first, then terrain, separately. Mixing the two probes changes both the space
        // available and which of them last sets Samus's Y fraction (touching an enemy clears it; a later
        // floor probe can set it to its largest value).
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

        // Both probes test the room from where her body stands now, on copies of it; neither moves her.
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
            // A body shorter than an upright one, the Morph Ball's, keeps its previous pose; any other body crouches.
            return Kinematics.Radius.Y < SamusPoseCollisionDefinitions.SmallestUprightYRadius
                ? LargerPoseCollisionOutcome.RetainSource
                : LargerPoseCollisionOutcome.CrouchFallback;
        }

        centerAdjustment = 0;
        if (downward.Collided)
        {
            // She moves up, away from the floor, by the radius difference less the room she has below, so her feet stay
            // where they were.
            int freeWholePixels = Math.Max(0, downward.AcceptedDisplacement.WholePixels);
            centerAdjustment = -(radiusDifference - freeWholePixels);

            // The move up is tested against the ceiling. A hit keeps her previous pose; she does not crouch, since only one
            // side blocked her at first.
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
            // She moves down, away from the ceiling, only as far as her larger body needs.
            int freeWholePixels = Math.Max(0, -upward.AcceptedDisplacement.WholePixels);
            centerAdjustment = radiusDifference - freeWholePixels;

            // The move down is tested against the floor, as the move up is against the ceiling.
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

        // When any probe hit, the correction also sets her previous vertical position, even a correction of zero; when
        // none hit, her previous vertical position is left as it was. The corrected height lies in the room: a move up is
        // kept only when her body clears the terrain that far up, and block collision reads past the room's top as solid,
        // so her top stays in the room; a move down is kept only when her grown body clears the terrain below; and the
        // enemy correction runs only with both terrain probes clear by the whole growth, which it never exceeds. On a
        // departing up-elevator, the only time her centre lies above the top, the locked input mode selects no pose and a
        // hurt pose does not exceed the boarding pose's radius the ride keeps, so nothing grows her there.
        if (upward.Collided || downward.Collided || enemyUp.Collided || enemyDown.Collided)
            WritePreviousYPosition(Kinematics.Position.Y.Whole + centerAdjustment);
        return LargerPoseCollisionOutcome.Allowed;

        LargerPoseCollisionOutcome BothSides() => Kinematics.Radius.Y < SamusPoseCollisionDefinitions.SmallestUprightYRadius
            ? LargerPoseCollisionOutcome.RetainSource : LargerPoseCollisionOutcome.CrouchFallback;

        SolidEnemyCollisionResult ProbeEnemy(SamusCollisionDirection direction, int distance, int radius)
        {
            SamusKinematicsState probe = Kinematics.CreateCollisionProbe();
            probe.Radius = probe.Radius.WithY(radius);
            SolidEnemyCollisionResult result = SamusSolidEnemyCollision.Probe(
                probe, probe.InteractiveEnemies, direction, UnsignedSixteenSixteen.FromWholePixels(distance));
            Kinematics.Position = Kinematics.Position.WithFractionY(probe.Position.Y.FractionOfPixel);
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
    /// The probe carries the pose being installed: the pose setup writes it to Samus's pose
    /// before the larger-radius check, so the elevator pseudo-door's standing-pose gate tests it.
    /// </remarks>
    internal BlockMoveResult ProbeChangedPoseVertical(
        RoomLevelData level,
        SignedSixteenSixteen displacement,
        bool scanLeftToRight,
        SamusPoseId targetPose,
        ISamusRoomObjects roomObjects)
    {
        int wholePixels = displacement.WholePixels;
        if (Math.Abs(wholePixels) >= SamusPoseCollisionDefinitions.IntermediateProbeExpansion)
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
            BlockMoveResult result = SamusBlockCollision.ProbeChangedPoseVertical(
                level, probe, this, targetPose, amount, scanLeftToRight, roomObjects);
            // Submerging sand clears speed/gravity even during an observation.
            // Those reactions take hold without the probe's position becoming hers.
            Kinematics.YSpeed = probe.YSpeed;
            Kinematics.YAcceleration = probe.YAcceleration;
            // The block reaction clamps the live fractional Y even for a
            // changed-pose observation; the probe's whole-pixel movement never
            // becomes hers.
            if (result.Collided)
                Kinematics.Position = Kinematics.Position.WithFractionY(probe.Position.Y.FractionOfPixel);
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
        // but leaves the live radius untouched until the next ordinary input phase.
        if (oldRadius < fallbackRadius)
        {
            // The fallback moves her centre up by the extra crouch radius. This is
            // normally invisible for radius-16 aimed crouches, but compact radius ten must
            // move up six pixels when simultaneous initial probes force ordinary crouching. The fallback follows only an
            // upright body, radius 10 or more, whose top block collision keeps in the room, so the lift of at most six
            // pixels leaves her centre in it.
            Kinematics.Position = Kinematics.Position.WithWholeY(
                Kinematics.Position.Y.Whole - (fallbackRadius - oldRadius));
            WritePreviousYPosition(Kinematics.Position.Y.Whole);
        }
        SamusPresentation.InitializeAnimation(this);
    }

    /// <summary>How this update corrects the camera's record of where Samus was before its scrolling (#2562).</summary>
    public SamusPreviousPositionCorrections PreviousPositionCorrections { get; } = new();

    /// <summary>
    /// Records Samus's previous X position as set by an owner that runs
    /// before the main scrolling, such as the landed gunship placing Samus.
    /// Only the whole pixels change; the previous X fraction is untouched (Mother Brain's
    /// rainbow-beam push hides its fraction through <see cref="SamusPreviousPositionCorrections.RainbowPush"/>).
    /// </summary>
    internal void WritePreviousXPosition(int xPosition) => PreviousPositionCorrections.WriteX(xPosition);

    /// <summary>
    /// Aligns Samus's bottom with the previous pose after installing a new pose:
    /// preserve the feet and shift the previous whole-Y checkpoint by the same delta.
    /// Neither fraction changes.
    /// </summary>
    internal void AlignBottomAfterPoseChange(int previousRadius, int targetRadius)
    {
        int delta = previousRadius - targetRadius;
        Position += new Offset16<RoomPixels>(0, delta);
        PreviousPositionCorrections.ShiftFeet(delta);
    }

    /// <summary>
    /// Records Samus's previous Y position as set by something such as
    /// changed-pose collision correction or facing forward's lift. The previous
    /// fraction is deliberately not changed: the game writes only the whole pixels, not the fraction.
    /// The write is absolute, discarding any earlier bottom-alignment shift this frame.
    /// </summary>
    internal void WritePreviousYPosition(int correctedY) => PreviousPositionCorrections.WriteY(correctedY);

    /// <summary>
    /// Samus's live previous X and Y positions: the
    /// frame-start checkpoint with this frame's writes so far, without handing them over.
    /// </summary>
    internal Position16<RoomPixels> PeekPreviousPosition(FixedPosition<RoomPixels> previous) =>
        PreviousPositionCorrections.Peek(previous);

    /// <summary>
    /// Applies this frame's previous-position corrections before scrolling calculates distance: the whole writes, keeping
    /// the fractions, then what the rainbow-beam push hides of its movement within a pixel. Later in the update the whole
    /// position replaces the checkpoint.
    /// </summary>
    internal FixedPosition<RoomPixels> ApplyPreviousPositionWrites(FixedPosition<RoomPixels> previous) =>
        PreviousPositionCorrections.HandOver(previous);

    /// <summary>Persistent low-energy warning state, shared by the movement phase and reserve recovery.</summary>
    public SamusHealthWarningState HealthWarning => field ??= new();

    /// <summary>
    /// Copies the inventory and energy from <paramref name="source"/>: what Samus keeps when she is set up again. A Samus
    /// created fresh and given these values is the same player set up again.
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
    }

    /// <summary>
    /// Called on resuming from pause: reconcile Morph/Spring Ball
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

        // The movement owners select the stationary pose even while airborne. Keeping
        // ascent intact here permits the equipment-toggle mid-air Spring Ball jump.
        bool left = SamusPoseMetadata.IsFacing(this, SamusFacingDirection.Left);
        Pose = enable
            ? left ? SamusPoseId.SpringBallGroundLeftPose : SamusPoseId.SpringBallGroundRightPose
            : left ? SamusPoseId.MorphBallGroundLeftPose : SamusPoseId.MorphBallGroundRightPose;
        SamusPoseMetadata.RefreshCollisionRadii(this);
        // Switching between the Morph Ball and the Spring Ball through the equipment menu
        // restarts the ball's animation from its first frame.
        SamusPresentation.InitializeAnimation(this);
    }

    /// <summary>Transition-owned pose history.</summary>
    public SamusPoseHistoryState PoseHistory { get; } = new();

    /// <summary>
    /// Commits current pose metadata, shifting her pose history: the pose this transition published and the movement
    /// types of her last two transitions.
    /// Transition owners must call this after their final pose/metadata selection,
    /// not for intermediate assignments, rollbacks, or ordinary frame sampling.
    /// </summary>
    internal void CommitPoseHistory()
    {
        PoseHistory.CommitTransition(Pose, SamusPoseMetadata.ReadMovementType(InitializedPose));
    }
}
