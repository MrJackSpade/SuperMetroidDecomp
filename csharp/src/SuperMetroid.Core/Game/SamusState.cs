using SuperMetroid.Core.Hardware;
using static SuperMetroid.Core.Hardware.SnesAddressMath;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Game;

/// <summary>
/// First breakpoint-friendly slice of normal gameplay Samus state and rendering.
/// </summary>
/// <remarks>
/// This class owns the state shared by the exact standing/right-running render, animation,
/// and grounded movement slices. It does not claim to port damage, equipment, most poses,
/// or the Landing Site cinematic actor. Keeping those boundaries blunt is preferable to
/// filling unported bank-$90 behavior with plausible-looking host code.
/// </remarks>
public sealed partial class SamusState
{

    private byte _pose = SamusPoseIds.FacingRightNormalPose;

    /// <summary>Current one-byte pose index, corresponding to WRAM <c>$0A1C</c>.</summary>
    public byte Pose
    {
        get => _pose;
        set
        {
            _pose = value;
            // Bank $94 reads the global pose word directly while resolving special
            // elevator doors. Mirror every pose write into the collision snapshot so the
            // low-level dispatcher can reproduce that test without owning Samus state.
            Kinematics.CollisionPose = value;
        }
    }

    /// <summary>
    /// Typed view of <see cref="Pose"/> for debugger watches and typed gameplay APIs.
    /// Explicit casting preserves undefined cartridge bytes for strict diagnostics.
    /// </summary>
    public SamusPoseId PoseId
    {
        get => (SamusPoseId)_pose;
        set => Pose = (byte)value;
    }

    /// <summary>Current animation-frame index, corresponding to WRAM <c>$0A96</c>.</summary>
    public ushort AnimationFrame { get; set; }

    /// <summary>
    /// Countdown at WRAM <c>$0A94</c>. Zero and negative-after-decrement both advance,
    /// matching the original routine's BEQ/BPL pair.
    /// </summary>
    public ushort AnimationFrameTimer { get; private set; }

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
    /// Compatibility/debugger view of <c>samus_x_speed_divisor</c> at WRAM <c>$0A66</c>.
    /// The backing word belongs to <see cref="HorizontalSpeed"/>, just as the native word
    /// is both a movement-speed divisor and the no-FX animation delay buffer.
    /// </summary>
    public ushort XSpeedDivisor
    {
        get => HorizontalSpeed.SpeedDivisor;
        set => HorizontalSpeed.SpeedDivisor = value;
    }

    /// <summary>Current energy at WRAM `$09C2`, also used by animation command `$F6`.</summary>
    public ushort Health { get; set; } = 99;

    /// <summary>
    /// Fractional energy word consumed by `$90:E9CE` before the whole-energy subtraction.
    /// Ordinary HUD/debug output shows only <see cref="Health"/>, but lava/acid damage uses
    /// this word's borrow so sub-energy cannot be rounded independently on every frame.
    /// </summary>
    public ushort SubunitHealth { get; set; }

    /// <summary>Maximum normal energy at WRAM `$09C4`; defaults to the initial 99.</summary>
    public ushort MaxHealth { get; set; } = 99;

    /// <summary>Current reserve energy at WRAM `$09D6`.</summary>
    public ushort ReserveEnergy { get; set; }

    /// <summary>Maximum reserve energy at WRAM `$09D4`.</summary>
    public ushort MaxReserveEnergy { get; set; }

    /// <summary>Reserve-tank mode at WRAM `$09C0`: zero off, one auto, two manual.</summary>
    public ushort ReserveTankMode { get; set; }

    /// <summary>Current missiles at WRAM `$09C6`.</summary>
    public ushort Missiles { get; set; }

    /// <summary>Maximum missiles at WRAM `$09C8`.</summary>
    public ushort MaxMissiles { get; set; }

    /// <summary>
    /// Retail WRAM $09D8. Super Metroid never exposes a missile-reserve tank in normal
    /// play, but its shared restore routine still sends overflow here and caps it at either
    /// max missiles or 99. Retaining the otherwise-unused word makes enemy drops exact and
    /// lets a debugger observe the original quirk.
    /// </summary>
    public ushort ReserveMissiles { get; set; }

    /// <summary>Current super missiles at WRAM `$09CA`.</summary>
    public ushort SuperMissiles { get; set; }

    /// <summary>Maximum super missiles at WRAM `$09CC`.</summary>
    public ushort MaxSuperMissiles { get; set; }

    /// <summary>Current power bombs at WRAM `$09CE`.</summary>
    public ushort PowerBombs { get; set; }

    /// <summary>Maximum power bombs at WRAM `$09D0`.</summary>
    public ushort MaxPowerBombs { get; set; }

    /// <summary>Selected HUD item at WRAM <c>$09D2</c>: 0 none, 1 missiles, 2 supers, 3 bombs.</summary>
    public ushort SelectedHudItem { get; set; }

    /// <summary>Native HUD auto-cancel index at WRAM <c>$0A04</c>.</summary>
    public ushort AutoCancelHudItemIndex { get; set; }

    /// <summary>
    /// Debugger-readable identity of the normal-versus-locked Samus state-handler pair.
    /// Mother Brain command five/<c>$18</c> locks input; command one unlocks it after the
    /// rainbow beam has narrowed. Movement type alone cannot represent this independent word.
    /// </summary>
    public bool InputLocked { get; set; }

    /// <summary>
    /// Equipped beam bitfield at WRAM `$09A6`. Drained-controller function three replaces
    /// this with `$1009`, the exact charge/wave/plasma plus hyper-beam configuration.
    /// </summary>
    public ushort EquippedBeams { get; set; }

    /// <summary>
    /// Collected-beam bitfield at WRAM <c>$09A8</c>. Unlike
    /// <see cref="EquippedBeams"/>, this retains beams disabled from the pause equipment
    /// screen and is therefore the authoritative ownership word saved to SRAM.
    /// </summary>
    public ushort CollectedBeams { get; set; }

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
    public ushort BombSpreadChargeTimeoutCounter { get; set; }

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
    public ushort PoseTransitionShotDirection { get; private set; }

    /// <summary>WRAM `$0A76`; `$8000` marks Mother Brain's hyper beam as enabled.</summary>
    public ushort HyperBeam { get; set; }

    /// <summary>
    /// Equipped-item bitfield corresponding to WRAM <c>$09A2</c>. Bit two ($0004) is Morph
    /// Ball, bit one ($0002) is Spring Ball, and bit twelve ($1000) is Bombs. Animation
    /// command $F9 and the Morph-Ball projectile handler read this word directly.
    /// </summary>
    public ushort EquippedItems { get; set; }

    /// <summary>
    /// Collected-item bitfield corresponding to WRAM <c>$09A4</c>. This normally contains
    /// every equipped item, but equipment-menu toggles can make the two words differ. Enemy
    /// logic such as the morph-ball eye tests ownership through this word specifically.
    /// </summary>
    public ushort CollectedItems { get; set; }

    /// <summary>
    /// WRAM <c>samus_special_super_palette_flags</c>. Ordinary Metroid command $12 writes
    /// one while attached; bank-$91 palette handling then increments the word every frame,
    /// alternating the speed-boost and normal suit palettes until command $13 clears it.
    /// Bit $8000 belongs to the separate drained/rainbow sequence modeled by
    /// <see cref="Drained"/> and is therefore never synthesized by Metroid AI.
    /// </summary>
    public ushort SpecialSuperPaletteFlags { get; internal set; }

    /// <summary>
    /// Morph-ball bounce state at WRAM <c>$0B20</c>: zero is not bouncing, one is the first
    /// rebound, and two is the second rebound. Spring Ball also uses the high byte, but that
    /// separate movement family is intentionally not folded into the ordinary-ball state.
    /// </summary>
    public ushort MorphBallBounceState { get; set; }

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

    /// <summary>WRAM `$0A54`: zero moves knockback left; one moves it right.</summary>
    public ushort KnockbackXDirection { get; set; }

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
    /// and special-movement producers and makes the complete effect debugger-watchable.
    /// </remarks>
    public ushort HurtFlashCounter { get; set; }

    /// <summary>
    /// WRAM <c>ResumeChargingBeamSFXFlag</c>. Hurt-flash recovery sets one at counter forty
    /// when a non-spinning Samus is still holding a sufficiently charged beam; the native
    /// post-draw sound handler consumes and clears it later in the same gameplay pass.
    /// </summary>
    public ushort ResumeChargingBeamSoundFlag { get; internal set; }

    /// <summary>
    /// WRAM `$18A8`, the general Samus invincibility timer tested by enemy, projectile,
    /// ordinary spike, spike-air, and grapple-swing damage producers. Bank `$A0:9169`
    /// decrements it once near the end of every gameplay frame, after all those producers
    /// have had an opportunity to reject or publish damage.
    /// </summary>
    public ushort InvincibilityTimer { get; set; }

    /// <summary>
    /// WRAM `$18AA`, initialized to five by bank `$A0` damage collision and decremented once
    /// per enemy-processing pass. The translated special handler owns movement only while
    /// this counter remains nonzero.
    /// </summary>
    public ushort KnockbackTimer { get; set; }

    /// <summary>
    /// Ports the two Samus-owned words from <c>DecrementSamusHurtTimers</c> at
    /// <c>$A0:9169-$9178</c>. This deliberately lives outside every individual movement
    /// handler: grapple spikes can start the timers without installing knockback movement,
    /// and the native gameplay tail ages them even while time is frozen.
    /// </summary>
    public void DecrementHurtTimers()
    {
        if (InvincibilityTimer != 0)
            InvincibilityTimer--;
        if (KnockbackTimer != 0)
            KnockbackTimer--;
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
    /// Enemy AI owns consumption of this word. Retaining the slot index now makes the
    /// movement result complete even before the general live-enemy update loop is translated.
    /// </remarks>
    public ushort EnemyIndexToShake { get; set; } = 0xffff;

    /// <summary>
    /// Host-visible equivalent of movement-handler pointer `$90:DF38`. It is separate from
    /// movement type `$0A` because the cartridge also uses that type for crystal-flash art.
    /// </summary>
    public bool KnockbackActive { get; set; }

    /// <summary>Bank-$91 address of the active pose's byte-oriented delay program.</summary>
    public int AnimationDelayListAddress { get; private set; }

    /// <summary>
    /// Most recent high-bit delay command, or null when the last advance selected a plain
    /// delay. This is a host-only debugger watch rather than original WRAM.
    /// </summary>
    public byte? LastAnimationDelayCommand { get; private set; }

    /// <summary>
    /// Operand published by animation command $F8 through the native “super-special
    /// prospective pose” seam. Null means the animation has not requested that transition.
    /// </summary>
    /// <remarks>
    /// This is host storage for WRAM $0A2A/$0A2C's command-three route. The grounded slice
    /// consumes it after animation, at the same pose-transition point used by the cartridge.
    /// Keeping it explicit makes the turn's final facing change visible to a debugger.
    /// </remarks>
    public byte? PendingTransitionalPose { get; private set; }

    /// <summary>Exact fixed-point position/radius words consumed by bank-$94 collision.</summary>
    public SamusKinematicsState Kinematics { get; }

    /// <summary>
    /// Room-dependent admission for spike-block BTS zero, populated by the alpha-phase
    /// inside-block owner before beta movement reaches a directional collision scan.
    /// </summary>
    internal bool OrdinarySpikeBlockBtsZeroDamageEnabled { get; set; } = true;

    public SamusState()
    {
        Kinematics = new SamusKinematicsState(this);
        // The backing field supplies the retail standing-right default without invoking
        // the setter before property initializers have constructed Kinematics.
        Kinematics.CollisionPose = _pose;
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

    /// <summary>Debugger/rendering view of the whole-pixel world X position.</summary>
    public ushort XPosition
    {
        get => Kinematics.XPosition;
        set => Kinematics.XPosition = value;
    }

    /// <summary>Debugger/rendering view of the whole-pixel world Y position.</summary>
    public ushort YPosition
    {
        get => Kinematics.YPosition;
        set => Kinematics.YPosition = value;
    }

    /// <summary>
    /// Applies the crouching table's direct `$27/$71/$73/$85 -> $01` and mirrored
    /// `$28/$72/$74/$86 -> $02` exits. These are real six-byte transition-table records;
    /// unlike the animated `$F7-$FC` stand-up family, they install the final standing pose
    /// immediately after pose-change collision has made room for its larger radius.
    /// </summary>
    /// <returns>
    /// False when the two-sided pose-change collision resolver falls back to stable
    /// crouch because both the floor and ceiling constrain the larger body.
    /// </returns>

    private enum LargerPoseCollisionOutcome
    {
        Allowed,
        RetainSource,
        CrouchFallback,
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
    private LargerPoseCollisionOutcome ResolveLargerPoseCollision(
        ISnesAddressSpace bus,
        RoomLevelData level,
        byte targetPose,
        ushort nmiFrameCounter,
        RoomPlmSystem? plms,
        out int centerAdjustment)
    {
        int targetDefinition = AddWithinBank(
            SamusMovementRomData.Poses.Definitions,
            targetPose * SamusMovementRomData.Poses.DefinitionByteCount);
        ushort targetRadius = bus.ReadByte(AddWithinBank(targetDefinition, 6));
        if (targetRadius <= Kinematics.YRadius)
        {
            centerAdjustment = 0;
            return LargerPoseCollisionOutcome.Allowed;
        }

        int radiusDifference = targetRadius - Kinematics.YRadius;

        // The probes run against independent copies. Native stores both available-distance
        // results before choosing a branch, so neither probe may move the live body early.
        BlockMoveResult upward = ProbeChangedPoseVertical(
            bus,
            level,
            displacement: unchecked(-radiusDifference << 16),
            scanLeftToRight: (nmiFrameCounter & 1) == 0,
            plms);
        BlockMoveResult downward = ProbeChangedPoseVertical(
            bus,
            level,
            displacement: radiusDifference << 16,
            scanLeftToRight: (nmiFrameCounter & 1) == 0,
            plms);

        if (upward.Collided && downward.Collided)
        {
            centerAdjustment = 0;
            // `$91:FFA7` compares the OLD live radius with eight. Radius seven means
            // Morph Ball, and that branch restores PreviousPose instead of trying to fit
            // stable crouch. Every non-morph compact/standing body still uses crouch.
            return Kinematics.YRadius < 8
                ? LargerPoseCollisionOutcome.RetainSource
                : LargerPoseCollisionOutcome.CrouchFallback;
        }

        centerAdjustment = 0;
        if (downward.Collided)
        {
            // `$91:FF49`: move away from the floor by
            // radiusDifference-spaceToMoveDown, preserving the old bottom boundary.
            int freeWholePixels = Math.Max(0, downward.AcceptedDisplacement >> 16);
            centerAdjustment = -(radiusDifference - freeWholePixels);

            // `$91:FF58` checks the proposed upward correction against the opposite side.
            // A collision here takes `$91:FE82` and restores PreviousPose; it does not use
            // `$91:FFA7`'s crouch selector because the initial flags were not both set.
            SamusKinematicsState oppositeProbe = CopyKinematics(Kinematics);
            BlockMoveResult opposite = SamusBlockCollision.MoveVertical(
                bus,
                level,
                oppositeProbe,
                displacement: centerAdjustment << 16,
                scanLeftToRight: (nmiFrameCounter & 1) == 0,
                plms: plms);
            if (opposite.Collided)
                return LargerPoseCollisionOutcome.RetainSource;
        }
        else if (upward.Collided)
        {
            // `$91:FF20` is the mirror: move down only as far as necessary to clear the
            // ceiling while admitting the enlarged body.
            int acceptedWhole = unchecked((short)(upward.AcceptedDisplacement >> 16));
            int freeWholePixels = Math.Max(0, -acceptedWhole);
            centerAdjustment = radiusDifference - freeWholePixels;

            // `$91:FF2B` is the downward mirror of the opposite-side check above.
            SamusKinematicsState oppositeProbe = CopyKinematics(Kinematics);
            BlockMoveResult opposite = SamusBlockCollision.MoveVertical(
                bus,
                level,
                oppositeProbe,
                displacement: centerAdjustment << 16,
                scanLeftToRight: (nmiFrameCounter & 1) == 0,
                plms: plms);
            if (opposite.Collided)
                return LargerPoseCollisionOutcome.RetainSource;
        }

        return LargerPoseCollisionOutcome.Allowed;
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
    /// </remarks>
    private BlockMoveResult ProbeChangedPoseVertical(
        ISnesAddressSpace bus,
        RoomLevelData level,
        int displacement,
        bool scanLeftToRight,
        RoomPlmSystem? plms,
        bool includeSolidEnemies = true)
    {
        short wholePixels = unchecked((short)(displacement >> 16));
        if ((Math.Abs(wholePixels) & 0xfff8) != 0)
        {
            // `$94:96C0` computes `(signedWhole & $FFF0) | 8`. Retain the literal
            // two's-complement expression rather than replacing it with a magnitude
            // shortcut; negative probes deliberately produce -8 for the known pose radii.
            short intermediateWhole = unchecked((short)((wholePixels & 0xfff0) | 8));
            BlockMoveResult intermediate = Probe(intermediateWhole << 16);
            if (intermediate.Collided)
                return intermediate;
        }

        return Probe(displacement);

        BlockMoveResult Probe(int amount)
        {
            SamusKinematicsState probe = CopyKinematics(Kinematics);
            BlockMoveResult result = SamusBlockCollision.MoveVertical(
                bus, level, probe, amount, scanLeftToRight,
                includeSolidEnemies: includeSolidEnemies, plms: plms,
                publishQuicksandGrounding: false);
            // Native block dispatch clamps the live fractional Y word even for a
            // changed-pose observation. Preserve that write, but never copy the
            // probe's whole-position movement into the live body.
            if (result.Collided)
                Kinematics.YSubposition = probe.YSubposition;
            return result;
        }
    }

    /// <summary>
    /// Applies `$91:FFA7`'s non-morph fallback after simultaneous above/below collision.
    /// Direction metadata selects `$27/$28`; an aimed crouch consequently loses its aim.
    /// </summary>
    private void ApplyPoseChangeCollisionCrouchFallback(ISnesAddressSpace bus, byte sourcePose)
    {
        ushort oldRadius = Kinematics.YRadius;
        byte fallbackPose = IsFacingLeft(bus)
            ? SamusPoseIds.CrouchingLeftPose
            : SamusPoseIds.CrouchingRightPose;
        if (sourcePose == fallbackPose)
            return;

        Pose = fallbackPose;
        RefreshCollisionRadii(bus);
        if (oldRadius < Kinematics.YRadius)
        {
            // `$91:FFD4-$91:FFE9` subtracts the extra crouch radius from center Y. This is
            // normally invisible for radius-16 aimed crouches, but compact radius ten must
            // move up six pixels when simultaneous initial probes force `$27/$28`.
            Kinematics.YPosition = unchecked((ushort)(
                Kinematics.YPosition - (Kinematics.YRadius - oldRadius)));
        }
        InitializeAnimation(bus, initialFrame: 0);
    }

    private static SamusKinematicsState CopyKinematics(SamusKinematicsState source) => new()
    {
        CollisionPose = source.CollisionPose,
        XPosition = source.XPosition,
        XSubposition = source.XSubposition,
        YPosition = source.YPosition,
        YSubposition = source.YSubposition,
        XRadius = source.XRadius,
        YRadius = source.YRadius,
        YSpeed = source.YSpeed,
        YSubspeed = source.YSubspeed,
        YDirection = source.YDirection,
        SandCollisionArea = source.SandCollisionArea,
        YAcceleration = source.YAcceleration,
        YSubacceleration = source.YSubacceleration,
        HorizontalSlopeCollisionEnable = source.HorizontalSlopeCollisionEnable,
        PositionAdjustedBySlope = source.PositionAdjustedBySlope,
        // Prospective-pose probes still call the native solid-enemy detector before blocks.
        // Share the immutable per-frame actor snapshots while copying only Samus's geometry.
        InteractiveEnemies = source.InteractiveEnemies,
    };
}
