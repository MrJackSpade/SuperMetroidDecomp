using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.Plms;
using SuperMetroid.Core.Samus.Abilities;
using SuperMetroid.Core.Samus.Grapple;
using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.Core.Samus.Palettes;
using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.Core.Samus.Death;
using SuperMetroid.Core.Samus.Drawing;
using SuperMetroid.Core.Samus.Hud;

namespace SuperMetroid.Core.Samus;

/// <summary>
/// First breakpoint-friendly slice of normal gameplay Samus state and rendering.
/// </summary>
/// <remarks>
/// This class owns the state shared by the exact standing/right-running render, animation,
/// and grounded movement slices. It does not claim to port damage, equipment, most poses,
/// or the Landing Site cinematic actor. Keeping those boundaries blunt is preferable to
/// filling unported bank-$90 behavior with plausible-looking host code.
/// </remarks>
public sealed class SamusState
{
    /// <summary>The <see cref="SamusHudSelection"/> owner of this SamusState.</summary>
    internal SamusHudSelection HudSelection { get; }

    /// <summary>The <see cref="SamusScriptLocks"/> owner of this SamusState.</summary>
    internal SamusScriptLocks ScriptLocks { get; }

    /// <summary>The <see cref="SamusInputSnapshot"/> owner of this SamusState.</summary>
    internal SamusInputSnapshot InputSnapshot { get; }

    /// <summary>The <see cref="SamusPresentation"/> owner of this SamusState.</summary>
    internal SamusPresentation Presentation { get; }

    /// <summary>The <see cref="SamusPoseMetadata"/> owner of this SamusState.</summary>
    internal SamusPoseMetadata PoseMetadata { get; }

    /// <summary>The <see cref="SamusPoseTransitions"/> owner of this SamusState.</summary>
    internal SamusPoseTransitions PoseTransitions { get; }

    /// <summary>Current one-byte pose index, corresponding to WRAM <c>$0A1C</c>.</summary>
    /// <remarks>
    /// An ordinary write also models <c>InitializeSamusPose_1</c> ($91:F433), publishing the
    /// pose's direction and movement type to <see cref="InitializedPose"/>. Only native
    /// writers that skip that initializer use <see cref="WritePoseWithoutInitialization"/>.
    /// </remarks>
    public SamusPoseId Pose
    {
        get;
        set
        {
            field = value;
            InitializedPose = value;
            // Bank $94 reads the global pose word directly while resolving special
            // elevator doors. Mirror every pose write into the collision snapshot so the
            // low-level dispatcher can reproduce that test without owning Samus state.
            Kinematics.CollisionPose = value;
        }
    } = SamusPoseId.FacingRightNormalPose;

    /// <summary>
    /// The pose whose definition bytes zero and one <c>InitializeSamusPose_1</c> last copied to
    /// <c>$0A1E</c>/<c>$0A1F</c> (X direction and movement type). Movement dispatch and camera
    /// tracking read those cached bytes, not the live pose.
    /// </summary>
    public SamusPoseId InitializedPose { get; private set; } = SamusPoseId.FacingRightNormalPose;

    /// <summary>
    /// Writes <c>$0A1C</c> as native code does when it skips <c>InitializeSamusPose_1</c>
    /// (the drained-Samus controllers at $91:E571 and $91:E60C): the previous pose's
    /// direction and movement type stay in force.
    /// </summary>
    internal void WritePoseWithoutInitialization(SamusPoseId pose)
    {
        SamusPoseId initialized = InitializedPose;
        Pose = pose;
        InitializedPose = initialized;
    }

    /// <summary>
    /// Set when a Samus command returns carry set during enemy AI: <c>Run_Samus_Command</c>
    /// ($90:F093) then clears every prospective pose and command, so this frame's pending
    /// transition is not committed. The runtime consumes it before the pose update.
    /// </summary>
    public bool PendingPoseTransitionCancelled { get; private set; }

    /// <summary>Records <c>$90:F095-$F0A7</c>'s cancellation of the pending pose transition.</summary>
    internal void CancelPendingPoseTransition() => PendingPoseTransitionCancelled = true;

    /// <summary>Returns and clears the pending-transition cancellation.</summary>
    internal bool ConsumePendingPoseTransitionCancellation()
    {
        bool cancelled = PendingPoseTransitionCancelled;
        PendingPoseTransitionCancelled = false;
        return cancelled;
    }

    /// <summary>Current animation-frame index, corresponding to WRAM <c>$0A96</c>.</summary>
    public ushort AnimationFrame { get; set; }

    /// <summary>
    /// Countdown at WRAM <c>$0A94</c>. Zero and negative-after-decrement both advance,
    /// matching the original routine's BEQ/BPL pair.
    /// </summary>
    public NativeCountdown AnimationFrameTimer { get; internal set; }

    /// <summary>
    /// Delay added for speed/liquid physics at WRAM <c>$0A9A</c>. The translated standing
    /// dry-room slice keeps this equal to <see cref="XSpeedDivisor"/>.
    /// </summary>
    public ushort AnimationFrameBuffer { get; internal set; }

    /// <summary>
    /// Exact bank-$90 fixed-point horizontal-speed registers used by ordinary grounded
    /// right-running movement and exposed independently for debugger inspection.
    /// </summary>
    public SamusHorizontalSpeedState HorizontalSpeed { get; } = new();

    /// <summary>
    /// Live room-FX surface words and remembered liquid medium. Physics, animation, and
    /// pose initialization intentionally share this object because the cartridge shares
    /// WRAM `$195E/$1962/$197E/$0AD2` across all three phases.
    /// </summary>
    public SamusLiquidPhysicsState LiquidPhysics { get; } = new();

    /// <summary>
    /// Stored-shine palette countdown and the special shinespark movement-handler state.
    /// Its fields map to the reused WRAM words documented on
    /// <see cref="SamusShinesparkState"/> and remain individually debugger-visible.
    /// </summary>
    public SamusShinesparkState Shinespark { get; } = new();

    /// <summary>
    /// Crystal Flash initiation, ammo cadence, energy restoration, and installed handler
    /// state. It is deliberately separate from <see cref="Shinespark"/> even though the
    /// original aliases a few WRAM words between the mutually exclusive effects.
    /// </summary>
    public SamusCrystalFlashState CrystalFlash { get; } = new();

    /// <summary>
    /// X-ray admission, dedicated pose input/movement, beam-angle state, visor palette, and
    /// teardown. Revealed-block tilemaps and window HDMA remain presentation-owned outputs.
    /// </summary>
    public SamusXrayState Xray { get; } = new();

    /// <summary>
    /// Fatal-damage `$D7/$D8` ownership, death tiles/palettes, whiteout, and suit explosion.
    /// The outer music wait and post-explosion room fade remain explicit game-state seams.
    /// </summary>
    public SamusDeathSequenceState DeathSequence { get; } = new();

    /// <summary>
    /// Mother Brain/Baby Metroid drain poses, controller calls, and installed falling
    /// movement handler. Enemy AI remains a separate producer of these literal commands.
    /// </summary>
    public SamusDrainedState Drained { get; } = new();

    /// <summary>
    /// Bank-$90's grabbed-pose hack handler and the narrow bank-$A5 owner-position seam.
    /// Draygon's enemy AI owns its own flight path; this child owns every Samus-side word.
    /// </summary>
    public SamusDraygonGrabbedState DraygonGrabbed { get; } = new();

    /// <summary>
    /// Ceres Ridley's dedicated bank-$90 push/fall handler. This is not ordinary damage
    /// knockback: `$90:E119` replaces the movement and gamma-handler pointers without
    /// consuming energy, and `$90:E1C8` owns Samus until horizontal room collision.
    /// Keeping that lifetime separate prevents the five-frame hurt timer from ending the
    /// scripted ejection early or making normal controller input steer it.
    /// </summary>
    public SamusCeresRidleyEjectionState CeresRidleyEjection { get; } = new();

    /// <summary>
    /// The X-speed divisor, 0 through 65535, as Samus's own view of <see cref="SamusHorizontalSpeedState.SpeedDivisor"/>:
    /// the one stored value is both a movement-speed divisor and the no-FX animation delay buffer, and its setter
    /// rejects values outside that range.
    /// </summary>
    public int XSpeedDivisor
    {
        get => HorizontalSpeed.SpeedDivisor;
        set => HorizontalSpeed.SpeedDivisor = value;
    }

    /// <summary>Current energy at WRAM `$09C2`, also used by animation command `$F6`.</summary>
    public int Health { get; set; } = 99;

    /// <summary>
    /// WRAM $0A12, PreviousEnergyHurtCheck: initialized from Energy by $91:E164,
    /// then compared and refreshed after Samus/projectile drawing by $90:EAD7-EAFD.
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
        get => _subunitHealth;
        set => _subunitHealth = Ensure.BetweenInclusive(0, SubunitsPerHealthUnit - 1, value);
    }

    /// <summary>Backing value of <see cref="SubunitHealth"/>, always 0 through 65535.</summary>
    private int _subunitHealth;

    /// <summary>How many <see cref="SubunitHealth"/> units make one <see cref="Health"/> unit.</summary>
    private const int SubunitsPerHealthUnit = 65536;

    /// <summary>
    /// Takes periodic damage as <c>$90:E9CE</c> does: the fractional damage comes off <see cref="SubunitHealth"/>
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

    /// <summary>Maximum normal energy at WRAM `$09C4`; defaults to the initial 99.</summary>
    public int MaxHealth { get; set; } = 99;

    /// <summary>Current reserve energy at WRAM `$09D6`.</summary>
    public int ReserveEnergy { get; set; }

    /// <summary>Maximum reserve energy at WRAM `$09D4`.</summary>
    public int MaxReserveEnergy { get; set; }

    /// <summary>Reserve-tank mode at WRAM `$09C0`: zero off, one auto, two manual.</summary>
    public ReserveTankMode ReserveTankMode { get; set; }

    /// <summary>Current missiles at WRAM `$09C6`.</summary>
    public int Missiles { get; set; }

    /// <summary>Maximum missiles at WRAM `$09C8`.</summary>
    public int MaxMissiles { get; set; }

    /// <summary>
    /// Retail WRAM $09D8. Super Metroid never exposes a missile-reserve tank in normal
    /// play, but its shared restore routine still sends overflow here and caps it at either
    /// max missiles or 99. Retaining the otherwise-unused word makes enemy drops exact and
    /// lets a debugger observe the original quirk.
    /// </summary>
    public int ReserveMissiles { get; set; }

    /// <summary>Current super missiles at WRAM `$09CA`.</summary>
    public int SuperMissiles { get; set; }

    /// <summary>Maximum super missiles at WRAM `$09CC`.</summary>
    public int MaxSuperMissiles { get; set; }

    /// <summary>Current power bombs at WRAM `$09CE`.</summary>
    public int PowerBombs { get; set; }

    /// <summary>Maximum power bombs at WRAM `$09D0`.</summary>
    public int MaxPowerBombs { get; set; }

    /// <summary>Selected HUD item at WRAM <c>$09D2</c>: 0 none, 1 missiles, 2 supers, 3 bombs.</summary>
    public HudItem SelectedHudItem { get; set; }

    /// <summary>Native HUD auto-cancel index at WRAM <c>$0A04</c>.</summary>
    public HudItem AutoCancelHudItemIndex { get; set; }

    /// <summary>
    /// Debugger-readable identity of the normal-versus-locked Samus state-handler pair.
    /// Mother Brain command five/<c>$18</c> locks input; command one unlocks it after the
    /// rainbow beam has narrowed. Movement type alone cannot represent this independent word.
    /// </summary>
    public bool InputLocked
    {
        get;
        set
        {
            field = value;
            if (!value)
            {
                ScriptLocks.StationaryScriptControlLocked = false;
                ScriptLocks.RefillStationLocked = false;
            }
        }
    }

    /// <summary>
    /// Equipped beam bitfield at WRAM `$09A6`. Drained-controller function three replaces
    /// this with `$1009`, the exact charge/wave/plasma plus hyper-beam configuration.
    /// </summary>
    public SamusBeamFlags EquippedBeams { get; set; }

    /// <summary>
    /// Collected-beam bitfield at WRAM <c>$09A8</c>. Unlike
    /// <see cref="EquippedBeams"/>, this retains beams disabled from the pause equipment
    /// screen and is therefore the authoritative ownership word saved to SRAM.
    /// </summary>
    public SamusBeamFlags CollectedBeams { get; set; }

    /// <summary>
    /// WRAM <c>SamusProjectile_FlareCounter</c>. Values at least $003C mean the charge
    /// beam is fully charged; spin and wall-jump movement use that producer-owned word to
    /// publish contact-damage index four without owning the projectile charge lifecycle.
    /// </summary>
    public ushort ProjectileFlareCounter { get; set; }

    /// <summary>
    /// WRAM <c>$0CD0</c>, the shared bomb-spread charge timeout. The cartridge clears this
    /// word in <c>InitializeSamusPose_MorphingTransition</c> at <c>$91:F7E7</c> after a
    /// successful Morph Ball item check, regardless of the humanoid movement family that
    /// selected the transition. Keeping it separate from the beam flare counter prevents a
    /// mid-air morph from accidentally cancelling ordinary arm-cannon charge state.
    /// </summary>
    /// <remarks>
    /// Updates of Down held while a spread is charged, 0 through 65535; the setter rejects values outside that range.
    /// Its only advance stops once bits six and seven are both set, so it never passes 65535 and never wraps; the
    /// clears and the native snapshot import are its other producers.
    /// </remarks>
    public int BombSpreadChargeTimeoutCounter
    {
        get => _bombSpreadChargeTimeoutCounter;
        set => _bombSpreadChargeTimeoutCounter = Ensure.BetweenInclusive(0, ushort.MaxValue, value);
    }

    /// <summary>Backing value of <see cref="BombSpreadChargeTimeoutCounter"/>, always 0 through 65535.</summary>
    private int _bombSpreadChargeTimeoutCounter;

    /// <summary>
    /// WRAM <c>$0B5E</c>, <c>PoseTransitionShotDirection</c>. A pose initializer can
    /// publish one shot direction after the current frame's projectile handler has already
    /// run. The following frame's bank-$90 producer consumes the low byte, then the normal
    /// current-state epilogue clears the complete word whether or not allocation succeeded.
    /// </summary>
    /// <remarks>
    /// The high byte is meaningful provenance, not part of the direction: `$8000` marks a
    /// fresh-shot normal-jump transition and `$0100` marks a moonwalk turn. Keeping the raw
    /// word visible makes both native producers and the single consumer debugger-auditable.
    /// </remarks>
    public ushort PoseTransitionShotDirection { get; internal set; }

    /// <summary>WRAM `$0A76`: whether Mother Brain's hyper beam is enabled (native `$8000`).</summary>
    public bool HyperBeamEnabled { get; set; }

    /// <summary>
    /// Equipped-item bitfield corresponding to WRAM <c>$09A2</c>. Bit two ($0004) is Morph
    /// Ball, bit one ($0002) is Spring Ball, and bit twelve ($1000) is Bombs. Animation
    /// command $F9 and the Morph-Ball projectile handler read this word directly.
    /// </summary>
    public SamusEquipmentFlags EquippedItems { get; set; }

    /// <summary>
    /// Collected-item bitfield corresponding to WRAM <c>$09A4</c>. This normally contains
    /// every equipped item, but equipment-menu toggles can make the two words differ. Enemy
    /// logic such as the morph-ball eye tests ownership through this word specifically.
    /// </summary>
    public SamusEquipmentFlags CollectedItems { get; set; }

    /// <summary>
    /// The Metroid-attachment palette phase, 0 through 65535 (native <c>samus_special_super_palette_flags</c>). Zero
    /// is inactive. Ordinary Metroid command $12 writes one while attached; bank-$91 palette handling then advances
    /// the phase every frame, alternating the speed-boost and normal suit palettes until command $13 clears it. A
    /// phase of 32768 or more is the separate drained/rainbow branch modeled by <see cref="Drained"/>: Metroid AI
    /// never produces it and the alternation never advances it. The setter rejects values outside the range.
    /// </summary>
    public int SpecialSuperPaletteFlags
    {
        get => _specialSuperPaletteFlags;
        internal set => _specialSuperPaletteFlags = Ensure.BetweenInclusive(0, ushort.MaxValue, value);
    }

    /// <summary>Backing value of <see cref="SpecialSuperPaletteFlags"/>, always 0 through 65535.</summary>
    private int _specialSuperPaletteFlags;

    /// <summary>
    /// Morph-ball bounce state at WRAM <c>$0B20</c>: zero is not bouncing, one is the first
    /// rebound, and two is the second rebound. Spring Ball also stores high byte $06 as its
    /// bounce family; see <see cref="SamusBallBounceState"/>.
    /// </summary>
    public SamusBallBounceState MorphBallBounceState { get; set; }

    /// <summary>
    /// WRAM `$0A56`: low byte 1/2/3 is left/straight/right; bit `$0800` marks the bank-$91
    /// special prospective command as accepted and prevents the same explosion rearming it.
    /// </summary>
    public ushort BombJumpDirection { get; set; }

    /// <summary>True for `$90:E025`'s one-frame velocity-initialization handler.</summary>
    public bool BombJumpStarting { get; set; }

    /// <summary>True while `$90:E032` owns the rising bomb-jump arc.</summary>
    public bool BombJumpActive { get; set; }

    /// <summary>Bomb command installed RTS pose input; independent of the current movement handler.</summary>
    public bool BombJumpPoseInputLocked { get; set; }

    /// <summary>
    /// WRAM `$0A52`: zero means no knockback, one/two mean up-left/up-right, and four/five
    /// mean down-left/down-right. Value three is the retail routine's unused straight-up
    /// table entry and is intentionally rejected by the translated live path.
    /// </summary>
    public ushort KnockbackDirection { get; set; }

    /// <summary>Which way the pending or active knockback moves Samus horizontally.</summary>
    public Movement.KnockbackHorizontalDirection KnockbackXDirection { get; set; }

    /// <summary>
    /// WRAM <c>$0A48</c>, the shared hurt-flash palette counter. Ordinary knockback starts
    /// it at one, while an active shinespark repeatedly publishes eight and its crash
    /// handler clears it. Bank <c>$91:D8A5</c> is the sole ordinary consumer: values one
    /// through six alternate hurt and suit palettes, value forty restores interrupted
    /// movement audio, and value sixty ends the lifetime.
    /// </summary>
    /// <remarks>
    /// This word belongs to Samus rather than the shinespark child state. Keeping it here
    /// preserves the cartridge's intentional sharing between unrelated damage, palette,
    /// and special-movement producers and makes the complete effect debugger-watchable. Its value is 0 through 65535:
    /// gameplay keeps it below sixty, and a debugger-written value at or above 32768 advances with the cartridge's
    /// 16-bit wrap (<see cref="Palettes.SamusHurtFlashPalette"/>) until the signed sixty test ends it.
    /// </remarks>
    public int HurtFlashCounter { get; set => field = Ensure.BetweenInclusive(0, ushort.MaxValue, value); }

    /// <summary>
    /// WRAM <c>ResumeChargingBeamSFXFlag</c>. Hurt-flash recovery sets one at counter forty
    /// when a non-spinning Samus is still holding a sufficiently charged beam; the native
    /// post-draw sound handler consumes and clears it later in the same gameplay pass.
    /// </summary>
    public ResumeChargingBeamSoundLatch ResumeChargingBeamSound { get; internal set; }

    /// <summary>
    /// WRAM `$18A8`, the general Samus invincibility timer tested by enemy, projectile,
    /// ordinary spike, spike-air, and grapple-swing damage producers. Bank `$A0:9169`
    /// decrements it once near the end of every gameplay frame, after all those producers
    /// have had an opportunity to reject or publish damage.
    /// </summary>
    public NativeCountdown InvincibilityTimer { get; set; }

    /// <summary>
    /// WRAM `$18AA`, initialized to five by bank `$A0` damage collision and decremented once
    /// per enemy-processing pass. The translated special handler owns movement only while
    /// this counter remains nonzero.
    /// </summary>
    public NativeCountdown KnockbackTimer { get; set; }

    /// <summary>
    /// Ports the two Samus-owned words from <c>DecrementSamusHurtTimers</c> at
    /// <c>$A0:9169-$9178</c>. This deliberately lives outside every individual movement
    /// handler: grapple spikes can start the timers without installing knockback movement,
    /// and the native gameplay tail ages them even while time is frozen.
    /// </summary>
    public void DecrementHurtTimers()
    {
        InvincibilityTimer = InvincibilityTimer.DecrementToZero();
        KnockbackTimer = KnockbackTimer.DecrementToZero();
    }

    /// <summary>
    /// Bank-$9B's grapple-beam state. Keeping this as a named child object makes the original
    /// WRAM words individually watchable without polluting ordinary Samus kinematics.
    /// </summary>
    public SamusGrappleState Grapple { get; } = new();

    /// <summary>
    /// The independent opening cover, one-OBJ draw, and tile-$1F DMA used by aimed weapon
    /// poses. Its state is driven by HUD selection rather than by the body animation timer.
    /// </summary>
    public SamusArmCannonState ArmCannon { get; } = new();

    /// <summary>
    /// Packed five-call visor color cycle used only by backdrop-color-math rooms.
    /// </summary>
    public SamusVisorPaletteState VisorPalette { get; } = new();

    /// <summary>
    /// Native WRAM <c>EnemyIndexToShake</c>, written when an ordinary or grapple wall jump
    /// launches from a solid/frozen enemy instead of room terrain.
    /// </summary>
    /// <remarks>
    /// Holds the enemy slot, or null for the native `$FFFF` none. Enemy AI owns consumption.
    /// Retaining the slot now makes the movement result complete even before the general
    /// live-enemy update loop is translated.
    /// </remarks>
    public int? EnemySlotToShake { get; set; }

    /// <summary>
    /// Host-visible equivalent of movement-handler pointer `$90:DF38`. It is separate from
    /// movement type `$0A` because the cartridge also uses that type for crystal-flash art.
    /// </summary>
    public bool KnockbackActive { get; set; }

    /// <summary>Bank-$91 address of the active pose's byte-oriented delay program.</summary>
    public int AnimationDelayListAddress { get; internal set; }

    /// <summary>
    /// Operand published by animation command $F8 through the native “super-special
    /// prospective pose” seam. Null means the animation has not requested that transition.
    /// </summary>
    /// <remarks>
    /// This is host storage for WRAM $0A2A/$0A2C's command-three route. The grounded slice
    /// consumes it after animation, at the same pose-transition point used by the cartridge.
    /// Keeping it explicit makes the turn's final facing change visible to a debugger.
    /// </remarks>
    public SamusPoseId? PendingTransitionalPose { get; internal set; }

    /// <summary>Exact fixed-point position/radius words consumed by bank-$94 collision.</summary>
    public SamusKinematicsState Kinematics { get; }

    /// <summary>
    /// The projectile-inheritance block her movement and the camera publish and her shots read at launch. A new Samus
    /// starts with it zeroed; the cartridge's movement records are zero between frames too, and the scrolling routine
    /// republishes the camera subspeed every frame.
    /// </summary>
    public ProjectileInheritanceRecords ProjectileInheritance { get; } = new();

    /// <summary>
    /// The session's Mode 7 transfer queue residue, which her bank-$91 low-half delay reads alias. The room loader, attract
    /// driver and intro bind it when they create her; a detached Samus that reaches such a read fails loudly.
    /// </summary>
    public SuperMetroid.Core.Bosses.Ridley.Mode7TransferQueueResidue? Mode7QueueResidue { get; set; }

    /// <summary>
    /// Room-dependent admission for spike-block BTS zero, populated by the alpha-phase
    /// inside-block owner before beta movement reaches a directional collision scan.
    /// </summary>
    internal bool OrdinarySpikeBlockBtsZeroDamageEnabled { get; set; } = true;

    /// <summary>Creates independent player state and owned movement/special-sequence children, binding live kinematics back to this owner and seeding its collision pose with the standing-right default.</summary>
    /// <remarks>Construction performs no cartridge reads and does not initialize room position, pose-dependent radii, animation data, or installed artwork; gameplay setup supplies those separately.</remarks>
    public SamusState()
    {
        HudSelection = new SamusHudSelection(this);
        ScriptLocks = new SamusScriptLocks(this);
        InputSnapshot = new SamusInputSnapshot(this);
        Presentation = new SamusPresentation(this);
        PoseMetadata = new SamusPoseMetadata(this);
        PoseTransitions = new SamusPoseTransitions(this);
        Kinematics = new SamusKinematicsState(this);
        // The backing field supplies the retail standing-right default without invoking
        // the setter before property initializers have constructed Kinematics.
        Kinematics.CollisionPose = Pose;
    }

    /// <summary>
    /// Native 16.16 horizontal distance-plus-one from the most recent scrolling pass.
    /// This is <c>absolute_moved_last_frame_x</c> and its fractional companion, not
    /// an unbiased absolute delta. Yard consumes the same words as camera tracking.
    /// </summary>
    public uint AbsoluteMovedLastFrameXFixed { get; internal set; }

    /// <summary>
    /// WRAM <c>SamusSolidVerticalCollisionResult</c>. Most translated movement methods
    /// return collision results directly; movement type `$1A` has no displacement and its
    /// sole bank-$90 side effect is explicitly clearing this otherwise-stale word.
    /// </summary>
    public ushort SolidVerticalCollisionResult { get; set; }

    /// <summary>Samus's room-pixel 16.16 position, owned by <see cref="Kinematics"/>.</summary>
    public FixedPosition<RoomPixels> Position
    {
        get => Kinematics.Position;
        set => Kinematics.Position = value;
    }

    /// <summary>
    /// Resolves the block-only portion of `HandlePoseChangeCollision` at `$91:FDAE` for
    /// a target whose Y radius is larger than the live body's radius.
    /// </summary>
    /// <remarks>
    /// The cartridge first probes the radius difference upward and downward while retaining
    /// the old radius. A one-sided hit calculates a compensating center shift, then probes
    /// that shift toward the opposite surface before committing it. Failure of that second
    /// probe restores the source pose; simultaneous initial hits select stable crouch. Solid-
    /// enemy collision is deliberately outside the current room slice, with no substitute.
    /// </remarks>
    internal LargerPoseCollisionOutcome ResolveLargerPoseCollision(

        RoomLevelData level,
        SamusPoseId targetPose,
        int nmiFrameCounter,
        RoomPlmSystem? plms,
        out int centerAdjustment)
    {
        ushort targetRadius = SamusPoseMetadata.ReadPoseYRadius(targetPose);
        if (targetRadius <= Kinematics.Radius.Y)
        {
            centerAdjustment = 0;
            return LargerPoseCollisionOutcome.Allowed;
        }

        int radiusDifference = targetRadius - Kinematics.Radius.Y;

        // Native checks enemies first, then terrain in a separate pass. Mixing the two
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

        // The probes run against independent copies. Native stores both available-distance
        // results before choosing a branch, so neither probe may move the live body early.
        BlockMoveResult upward = ProbeChangedPoseVertical(

            level,
            displacement: unchecked(SnesSixteenSixteen.FromWhole(-radiusDifference)),
            scanLeftToRight: (nmiFrameCounter & 1) == 0,
            targetPose, plms);
        BlockMoveResult downward = ProbeChangedPoseVertical(

            level,
            displacement: SnesSixteenSixteen.FromWhole(radiusDifference),
            scanLeftToRight: (nmiFrameCounter & 1) == 0,
            targetPose, plms);

        if (upward.Collided && downward.Collided)
        {
            centerAdjustment = 0;
            // `$91:FFA7` compares the OLD live radius with eight. Radius seven means
            // Morph Ball, and that branch restores PreviousPose instead of trying to fit
            // stable crouch. Every non-morph compact/standing body still uses crouch.
            return Kinematics.Radius.Y < 8
                ? LargerPoseCollisionOutcome.RetainSource
                : LargerPoseCollisionOutcome.CrouchFallback;
        }

        centerAdjustment = 0;
        if (downward.Collided)
        {
            // `$91:FF49`: move away from the floor by
            // radiusDifference-spaceToMoveDown, preserving the old bottom boundary.
            int freeWholePixels = Math.Max(0, SnesSixteenSixteen.WholeOf(downward.AcceptedDisplacement));
            centerAdjustment = -(radiusDifference - freeWholePixels);

            // `$91:FF58` checks the proposed upward correction against the opposite side.
            // A collision here takes `$91:FE82` and restores PreviousPose; it does not use
            // `$91:FFA7`'s crouch selector because the initial flags were not both set.
            BlockMoveResult opposite = ProbeChangedPoseVertical(
level,
                displacement: SnesSixteenSixteen.FromWhole(centerAdjustment),
                scanLeftToRight: (nmiFrameCounter & 1) == 0,
                targetPose, plms: plms);
            if (opposite.Collided)
                return LargerPoseCollisionOutcome.RetainSource;
            if (enemyUp.Collided)
                return BothSides();
        }
        else if (upward.Collided)
        {
            // `$91:FF20` is the mirror: move down only as far as necessary to clear the
            // ceiling while admitting the enlarged body.
            int acceptedWhole = unchecked((short)(SnesSixteenSixteen.WholeOf(upward.AcceptedDisplacement)));
            int freeWholePixels = Math.Max(0, -acceptedWhole);
            centerAdjustment = radiusDifference - freeWholePixels;

            // `$91:FF2B` is the downward mirror of the opposite-side check above.
            BlockMoveResult opposite = ProbeChangedPoseVertical(
level,
                displacement: SnesSixteenSixteen.FromWhole(centerAdjustment),
                scanLeftToRight: (nmiFrameCounter & 1) == 0,
                targetPose, plms: plms);
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

        // Native successful correction branches publish current AND previous Y.
        // A zero correction in a collision branch still performs that write;
        // the no-collision branch does not replace the scrolling checkpoint.
        if (upward.Collided || downward.Collided || enemyUp.Collided || enemyDown.Collided)
            WritePreviousYPosition(unchecked((ushort)(Kinematics.Position.Y.Whole + centerAdjustment)));
        return LargerPoseCollisionOutcome.Allowed;

        LargerPoseCollisionOutcome BothSides() => Kinematics.Radius.Y < 8
            ? LargerPoseCollisionOutcome.RetainSource : LargerPoseCollisionOutcome.CrouchFallback;

        SolidEnemyCollisionResult ProbeEnemy(SamusCollisionDirection direction, int distance, ushort radius)
        {
            SamusKinematicsState probe = Kinematics.CreateCollisionProbe();
            probe.Radius = probe.Radius.WithY(radius);
            SolidEnemyCollisionResult result = SamusSolidEnemyCollision.Probe(
                probe, probe.InteractiveEnemies, direction, unchecked((ushort)distance), 0);
            Kinematics.Position = Kinematics.Position.WithFractionY(probe.Position.Y.Fraction);
            return result;
        }
    }

    /// <summary>
    /// Executes the unusual two-pass distance probe in
    /// <c>Samus_CollDetectChangedPose</c> at <c>$94:96AB</c>.
    /// </summary>
    /// <remarks>
    /// A pose expansion of eight pixels or more is not tested only at its final leading
    /// boundary. Native first tests the signed displacement rounded to the intermediate
    /// eight-pixel boundary and returns immediately if that scan collides, then tests the
    /// complete displacement only when the intermediate scan was clear. That first scan is
    /// observable on square half-blocks: an eleven-pixel compact-to-standing expansion can
    /// cross from the solid lower quadrant into an air upper quadrant at its final boundary.
    /// A single final-boundary scan therefore misses the floor, expands Samus downward, and
    /// lets her fall through the platform. Each probe uses a separate kinematics copy because
    /// the cartridge collision routine publishes a distance without moving live Samus.
    /// The probe carries the pose being installed: `$91:F404` writes it to samus_pose
    /// before calling `$91:FDAE`, so the elevator pseudo-door's below-$09 gate tests it.
    /// </remarks>
    internal BlockMoveResult ProbeChangedPoseVertical(

        RoomLevelData level,
        int displacement,
        bool scanLeftToRight,
        SamusPoseId targetPose,
        RoomPlmSystem? plms)
    {
        short wholePixels = unchecked((short)(SnesSixteenSixteen.WholeOf(displacement)));
        if ((Math.Abs(wholePixels) & ~7) != 0)
        {
            // `$94:96C0` computes `(signedWhole & $FFF0) | 8`. Retain the literal
            // two's-complement expression rather than replacing it with a magnitude
            // shortcut; negative probes deliberately produce -8 for the known pose radii.
            short intermediateWhole = unchecked((short)((wholePixels & ~15) | 8));
            BlockMoveResult intermediate = Probe(SnesSixteenSixteen.FromWhole(intermediateWhole));
            if (intermediate.Collided)
                return intermediate;
        }

        return Probe(displacement);

        BlockMoveResult Probe(int amount)
        {
            SamusKinematicsState probe = Kinematics.CreateCollisionProbe();
            probe.CollisionPose = targetPose;
            BlockMoveResult result = SamusBlockCollision.ProbeChangedPoseVertical(
level, probe, amount, scanLeftToRight, plms);
            // Submerging sand clears native speed/gravity even during an observation.
            // Preserve these reaction writes without committing the probe's position.
            Kinematics.YSpeed = probe.YSpeed;
            Kinematics.YSubspeed = probe.YSubspeed;
            Kinematics.YAcceleration = probe.YAcceleration;
            Kinematics.YSubacceleration = probe.YSubacceleration;
            // Native block dispatch clamps the live fractional Y word even for a
            // changed-pose observation. Preserve that write, but never copy the
            // probe's whole-position movement into the live body.
            if (result.Collided)
                Kinematics.Position = Kinematics.Position.WithFractionY(probe.Position.Y.Fraction);
            return result;
        }
    }

    /// <summary>
    /// Applies `$91:FFA7`'s non-morph fallback after simultaneous above/below collision.
    /// Direction metadata selects `$27/$28`; an aimed crouch consequently loses its aim.
    /// </summary>
    internal void ApplyPoseChangeCollisionCrouchFallback(SamusPoseId sourcePose)
    {
        ushort oldRadius = Kinematics.Radius.Y;
        SamusPoseId fallbackPose = PoseMetadata.IsFacingLeft()
            ? SamusPoseId.CrouchingLeftPose
            : SamusPoseId.CrouchingRightPose;
        if (sourcePose == fallbackPose)
            return;

        Pose = fallbackPose;
        ushort fallbackRadius = SamusPoseMetadata.ReadPoseYRadius(fallbackPose);
        // The collision fallback reads the prospective radius for center correction,
        // but leaves the live radius untouched until the next ordinary alpha phase.
        if (oldRadius < fallbackRadius)
        {
            // `$91:FFD4-$91:FFE9` subtracts the extra crouch radius from center Y. This is
            // normally invisible for radius-16 aimed crouches, but compact radius ten must
            // move up six pixels when simultaneous initial probes force `$27/$28`.
            Kinematics.Position = Kinematics.Position.WithWholeY(unchecked((ushort)(
                Kinematics.Position.Y.Whole - (fallbackRadius - oldRadius))));
            WritePreviousYPosition(Kinematics.Position.Y.Whole);
        }
        Presentation.InitializeAnimation(initialFrame: 0);
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
    /// <summary>Masked writes to previous X/Y fractions $0B12/$0B16; zero masks mean no write.</summary>
    private PreviousFractionWrites _previousFractionWrites;

    /// <summary>
    /// Records a direct write of SamusPreviousXPosition ($0B10) by an owner that runs
    /// before MainScrollingRoutine, such as the landed gunship placing Samus at $A2:A925.
    /// Only the whole word changes; the previous X fraction ($0B12) is untouched.
    /// </summary>
    internal void WritePreviousXPosition(ushort xPosition) =>
        _previousXPositionWrite = xPosition;

    /// <summary>
    /// Records a write to bits of SamusPreviousXSubPosition ($0B12): a byte store sets mask
    /// $FF00, a word store $FFFF. Later writes this frame replace the bits they cover.
    /// </summary>
    internal void WritePreviousXSubposition(ushort mask, ushort value) =>
        _previousFractionWrites = _previousFractionWrites with { X = _previousFractionWrites.X.Write(mask, value) };

    /// <summary>Records a write to bits of SamusPreviousYSubPosition ($0B16), as for X.</summary>
    internal void WritePreviousYSubposition(ushort mask, ushort value) =>
        _previousFractionWrites = _previousFractionWrites with { Y = _previousFractionWrites.Y.Write(mask, value) };

    /// <summary>
    /// Applies $90:EC7E Samus_AlignBottomWithPrevPose after installing a new pose:
    /// preserve the feet and shift the previous whole-Y checkpoint by the same delta.
    /// Neither fractional word changes.
    /// </summary>
    internal void AlignBottomAfterPoseChange(ushort previousRadius, ushort targetRadius)
    {
        int delta = previousRadius - targetRadius;
        Position += new Offset16<RoomPixels>(0, unchecked((short)delta));
        _poseAlignmentPreviousYDelta += delta;
    }

    /// <summary>
    /// Records a direct write of SamusPreviousYPosition ($0B14), such as bank-$91
    /// changed-pose collision correction or MakeSamusFaceForward's lift. The previous
    /// fraction is deliberately not changed: native writes only $0B14, not $0B16.
    /// The write is absolute, discarding any earlier bottom-alignment shift this frame.
    /// </summary>
    internal void WritePreviousYPosition(ushort correctedY)
    {
        _poseCollisionPreviousYPosition = correctedY;
        _poseAlignmentPreviousYDelta = 0;
    }

    /// <summary>
    /// The live SamusPreviousXPosition/SamusPreviousYPosition words ($0B10/$0B14): the
    /// frame-start checkpoint with this frame's writes so far, without consuming them.
    /// </summary>
    internal (ushort X, ushort Y) PeekPreviousPositionWords(FixedPosition<RoomPixels> previous) => (
        checked((ushort)(_previousXPositionWrite ?? previous.X.Whole)),
        unchecked((ushort)((_poseCollisionPreviousYPosition ?? previous.Y.Whole) + _poseAlignmentPreviousYDelta)));

    /// <summary>
    /// Applies this frame's previous-position word writes before $90:94EC calculates
    /// distance. Its normal tail subsequently replaces the complete checkpoint.
    /// </summary>
    internal FixedPosition<RoomPixels> ApplyPreviousPositionWrites(FixedPosition<RoomPixels> previous)
    {
        ushort correctedY = unchecked((ushort)((_poseCollisionPreviousYPosition ?? previous.Y.Whole)
            + _poseAlignmentPreviousYDelta));
        ushort xPosition = checked((ushort)(_previousXPositionWrite ?? previous.X.Whole));
        ushort xSubposition = _previousFractionWrites.X.ApplyTo(previous.X.Fraction);
        ushort ySubposition = _previousFractionWrites.Y.ApplyTo(previous.Y.Fraction);
        _poseCollisionPreviousYPosition = null;
        _poseAlignmentPreviousYDelta = 0;
        _previousXPositionWrite = null;
        _previousFractionWrites = default;
        return new FixedPosition<RoomPixels>(
            new SnesFixedPosition(xPosition, xSubposition),
            new SnesFixedPosition(correctedY, ySubposition));
    }

    /// <summary>
    /// Bit-masked pending writes to one previous-fraction word: bits set in
    /// <see cref="Mask"/> are replaced by the matching <see cref="Value"/> bits on apply.
    /// </summary>
    private readonly record struct MaskedWordWrite(ushort Mask, ushort Value)
    {
        /// <summary>Records a byte ($FF00) or word ($FFFF) store; later stores replace the bits they cover.</summary>
        public MaskedWordWrite Write(ushort mask, ushort value) =>
            new((ushort)(Mask | mask), (ushort)((Value & ~mask) | (value & mask)));

        /// <summary>Applies only the bits written this frame to an earlier fraction word.</summary>
        public ushort ApplyTo(ushort word) => (ushort)((word & ~Mask) | Value);
    }

    /// <summary>The pending masked writes to both previous-fraction words as one value.</summary>
    [SpatialComponents("Spatial value type: the pending masked writes to the previous X/Y fraction words $0B12/$0B16, one per axis.")]
    private readonly record struct PreviousFractionWrites(MaskedWordWrite X, MaskedWordWrite Y);

    /// <summary>Persistent $0A6A warning state, shared by native beta and reserve recovery.</summary>
    public SamusHealthWarningState HealthWarning => field ??= new();

    /// <summary>
    /// Copies the inventory and energy words at WRAM $09A2-$09D8, which lie below the Samus
    /// RAM block ($0A02 onward) that <c>InitializeSamus</c> ($91:E00D) clears. A Samus created
    /// fresh and given these words is that routine's result for the same player.
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
    /// $91:E83A/E867, called by unpause command $0C: reconcile Morph/Spring Ball
    /// equipment without changing position, velocity, or vertical direction.
    /// </summary>
    public void ReconcilePauseBallEquipment()
    {
        bool springEquipped = EquippedItems.HasAny(SamusEquipmentFlags.SpringBall);
        SamusMovementType movement = PoseMetadata.ReadMovementType();
        bool enable = springEquipped && movement is
            SamusMovementType.MorphBallGround or SamusMovementType.MorphBallFalling;
        bool disable = !springEquipped && movement is
            SamusMovementType.SpringBallGround or SamusMovementType.SpringBallInAir or SamusMovementType.SpringBallFalling;
        if (!enable && !disable)
            return;

        // The native handlers select the stationary pose even while airborne. Keeping
        // ascent intact here permits the equipment-toggle mid-air Spring Ball jump.
        bool left = PoseMetadata.IsFacingLeft();
        Pose = enable
            ? left ? SamusPoseId.SpringBallGroundLeftPose : SamusPoseId.SpringBallGroundRightPose
            : left ? SamusPoseId.MorphBallGroundLeftPose : SamusPoseId.MorphBallGroundRightPose;
        PoseMetadata.RefreshCollisionRadii();
        // $91:F9F4/FA56 preserve animation only within the same ball family. Crossing
        // families through the equipment menu therefore runs the normal frame-zero setup.
        Presentation.InitializeAnimation();
    }

    /// <summary>
    /// Transition-owned WRAM $0A20-$0A27. Lazy construction also covers legacy
    /// debugger graphs whose constructors are bypassed and which lack these words.
    /// A legacy graph has no recoverable historical value; zero is not a claim
    /// that its actual cartridge history was zero.
    /// </summary>
    public SamusPoseHistoryState PoseHistory => field ??= new();

    /// <summary>
    /// Commits current pose metadata through the native four-word history shift.
    /// Transition owners must call this after their final pose/metadata selection,
    /// not for intermediate assignments, rollbacks, or ordinary frame sampling.
    /// </summary>
    internal void CommitPoseHistory()
    {
        ushort metadata = (ushort)(SnesLittleEndian.Word(PoseMetadata.ReadPoseXDirection(), (byte)PoseMetadata.ReadMovementType()));
        PoseHistory.CommitTransition(Pose, metadata);
    }
}
