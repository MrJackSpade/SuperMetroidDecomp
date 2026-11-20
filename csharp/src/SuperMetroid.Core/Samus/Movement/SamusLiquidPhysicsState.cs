using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.Core.Samus.Drawing;

namespace SuperMetroid.Core.Samus.Movement;

/// <summary>
/// The room's liquid surfaces and derived liquid state consulted by Samus's physics.
/// </summary>
/// <remarks>
/// Super Metroid does not have one universal "underwater" flag. Different checks
/// compare either Samus's top, bottom, or bottom-minus-one boundary against the live
/// surface, and the remembered medium records the medium selected by the animation/room-load pass.
/// Keeping each surface here lets each liquid check reproduce its own
/// comparison instead of sharing a convenient but incorrect single boolean.
/// </remarks>
public sealed class SamusLiquidPhysicsState
{
    /// <summary>
    /// Takes the room's selected effect into Samus's liquid state, so what the room shows and how Samus moves read one
    /// liquid surface.
    /// </summary>
    public void ConfigureFor(RoomFxOverlayState fx)
    {
        Ensure.NotNull(fx);
        switch (fx.Type)
        {
            case RoomFxType.Water:
            case RoomFxType.TourianEntranceStatue:
                ConfigureWater(LiveSurface(fx), fx.Liquid is RoomWater { IgnoresSurfaceUntilEnabled: true }, fx.Type);
                return;
            case RoomFxType.Lava:
                ConfigureLavaAcid(LiveSurface(fx));
                return;
            case RoomFxType.Acid:
                ConfigureLavaAcid(LiveSurface(fx), RoomFxType.Acid);
                return;
            case RoomFxType.None:
            case RoomFxType.Spores:
            case RoomFxType.Rain:
            case RoomFxType.Fog:
            case RoomFxType.ScrollingSky:
            case RoomFxType.Fireflea:
            case RoomFxType.CeresRidley:
            case RoomFxType.CeresElevator:
            case RoomFxType.CeresHaze:
                // Non-liquid FX still own the FX type, which atmospheric effects keep while the
                // room has neither liquid surface.
                ConfigureNonLiquidRoomFx(fx.Type);
                return;
            default:
                throw new InvalidOperationException($"Undefined RoomFxType {fx.Type}.");
        }
    }

    /// <summary>
    /// The room effect's liquid surface as Samus's physics sees it: none when the effect has no surface, or when a tide
    /// carries it above the room, where the game's below-zero check finds no surface.
    /// </summary>
    private static int? LiveSurface(RoomFxOverlayState fx) => fx.Surface is >= 0 ? fx.Surface : null;

    /// <summary>
    /// Starts Samus's liquid state with the waits her breath bubbles and lava spray keep and the particles her presentation
    /// shows, both of which live as long as she does.
    /// </summary>
    /// <param name="effectWaits">The waits her breath bubbles and lava spray keep for the effects that shared their places.</param>
    /// <param name="particles">The splashes, bubbles, spray and dust her presentation shows.</param>
    /// <param name="audioRequests">Samus's audio requests, which her liquid, landing and footstep sounds join.</param>
    /// <param name="periodicDamage">Samus's periodic damage, which her liquids add to and a dry room's configuration drops.</param>
    internal SamusLiquidPhysicsState(
        BreathBubbleAndLavaSprayWaits effectWaits,
        SamusParticles particles,
        GameplaySoundRequests audioRequests,
        SamusPeriodicDamage periodicDamage)
    {
        AudioRequests = Ensure.NotNull(audioRequests);
        _periodicDamage = Ensure.NotNull(periodicDamage);
        EffectWaits = Ensure.NotNull(effectWaits);
        _particles = Ensure.NotNull(particles);
    }

    /// <summary>The splashes, bubbles, spray and dust Samus's presentation shows.</summary>
    private readonly SamusParticles _particles;

    /// <summary>Samus's periodic damage, which lava, acid and heat-free liquid damage join.</summary>
    private readonly SamusPeriodicDamage _periodicDamage;

    /// <summary>
    /// The waits her breath bubbles and lava spray keep for the effects that shared their places, which decide whether a
    /// bubble draws its random number and requests its sound and whether a spray requests its sound.
    /// </summary>
    public BreathBubbleAndLavaSprayWaits EffectWaits { get; }

    /// <summary>
    /// The update's audio requests her liquid, landing and footstep sounds join as they are made: Samus's (#2057).
    /// </summary>
    internal GameplaySoundRequests AudioRequests { get; }

    /// <summary>
    /// The room's effect: lava, acid or water, which Samus moves in, or one of the effects that are no liquid, or none.
    /// </summary>
    public RoomFxType FxType { get; private set; }

    /// <summary>
    /// The water surface's room Y, a line of the room, zero or more, or none when the room has no water surface; movement behaviours then
    /// consult <see cref="LavaAcidSurfaceY"/> instead.
    /// </summary>
    public int? WaterSurfaceY
    {
        get;
        set => field = value is int surface ? Ensure.AtLeastZero(surface) : null;
    }

    /// <summary>The lava or acid surface's room Y, a line of the room, zero or more, or none when the room has no lava or acid surface.</summary>
    public int? LavaAcidSurfaceY
    {
        get;
        set => field = value is int surface ? Ensure.AtLeastZero(surface) : null;
    }

    /// <summary>
    /// The room place the diving splash stands on: the water surface, or, in a room with no water surface, the place
    /// <see cref="AbsentWaterSurfaceLine.SplashPlace"/> gives.
    /// </summary>
    internal int UntestedWaterSurfaceLine => WaterSurfaceY is int surface ? OffRoomPlaces.Place(surface) : AbsentWaterSurfaceLine.SplashPlace;

    /// <summary>
    /// Whether Samus's movement ignores the room's water surface until a room object enables it (the n00b tube's glass),
    /// Samus's copy of <see cref="RoomWater.IgnoresSurfaceUntilEnabled"/>. While set, water does not affect her even when
    /// the geometric surface comparison says she is below it.
    /// </summary>
    public bool IgnoresWaterSurface { get; private set; }

    /// <summary>
    /// Remembered medium. This is intentionally stateful:
    /// Space Jump reads it, and the animation's liquid steps use changes to detect entry/exit.
    /// </summary>
    public SamusLiquidMedium MediumLastEntered { get; private set; }

    /// <summary>
    /// Configures the water surface, or none, and whether Samus ignores it, while preserving the ordinary or statue room's
    /// FX identity; the room has no lava or acid surface.
    /// </summary>
    public void ConfigureWater(int? surfaceY, bool ignoresSurface, RoomFxType type)
    {
        if (!RoomFxTypes.UsesWater(type))
            throw new ArgumentOutOfRangeException(nameof(type), type, "A water configuration requires a water effect.");
        FxType = type;
        WaterSurfaceY = surfaceY;
        LavaAcidSurfaceY = null;
        IgnoresWaterSurface = ignoresSurface;
    }

    /// <summary>Configures a lava or acid surface, or none; the room has no water surface.</summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="type"/> is neither lava nor acid.</exception>
    public void ConfigureLavaAcid(int? surfaceY, RoomFxType type = RoomFxType.Lava)
    {
        FxType = type is RoomFxType.Lava or RoomFxType.Acid ? type : throw new ArgumentOutOfRangeException(nameof(type), type, "Only lava or acid has a lava/acid surface.");
        WaterSurfaceY = null;
        LavaAcidSurfaceY = surfaceY;
        IgnoresWaterSurface = false;
    }

    /// <summary>
    /// Selects a non-liquid room FX, which leaves the room with neither liquid surface.
    /// Rain and other atmospheric owners still need their FX type,
    /// so clearing the entire state would discard observable animation behavior.
    /// </summary>
    public void ConfigureNonLiquidRoomFx(RoomFxType type)
    {
        if (type is RoomFxType.Water or RoomFxType.TourianEntranceStatue or RoomFxType.Lava or RoomFxType.Acid)
            throw new ArgumentOutOfRangeException(nameof(type), type, "Use the liquid-specific configuration method.");
        Clear();
        FxType = type;
    }

    /// <summary>Restores the no-FX state of a dry room, which has neither liquid surface.</summary>
    public void Clear()
    {
        FxType = RoomFxType.None;
        WaterSurfaceY = null;
        LavaAcidSurfaceY = null;
        IgnoresWaterSurface = false;
        MediumLastEntered = SamusLiquidMedium.Air;
        _periodicDamage.Clear();
        EffectWaits.Clear();
        _particles.Clear();
    }

    /// <summary>
    /// Selects air/water/lava physics for steps that test Samus's bottom boundary and
    /// bypass all liquid behavior when the Gravity Suit is equipped.
    /// </summary>
    public SamusLiquidMedium DetermineMovementMedium(SamusState samus)
    {
        Ensure.NotNull(samus);
        if (samus.EquippedItems.HasAny(SamusEquipmentFlags.GravitySuit))
            return SamusLiquidMedium.Air;
        // Samus's bottom boundary samples the current pose definition, even before the next
        // movement pass publishes its radius to the live collision state.
        return DetermineRawMediumAtBoundary(PoseBottomPixel(samus));
    }

    /// <summary>
    /// True when the spin's top-boundary test finds Samus
    /// fully under water/lava. The spin applies the Gravity Suit's exemption itself.
    /// </summary>
    public bool IsTopBoundarySubmerged(SamusState samus)
    {
        Ensure.NotNull(samus);
        return DetermineRawMediumAtBoundary(TopLine(samus)) != SamusLiquidMedium.Air;
    }

    /// <summary>
    /// True when Samus's full bottom collision boundary is below an active water or
    /// lava/acid surface, without applying the Gravity Suit exemption.
    /// </summary>
    /// <remarks>
    /// This is the exact environmental question asked by Samus's suit colouring.
    /// It deliberately differs from
    /// <see cref="DetermineMovementMedium"/>: the suit colouring checks for the Gravity
    /// Suit first, then performs the raw bottom-boundary liquid test. Keeping those two
    /// decisions separate lets the colouring reproduce both the submerged Power/Varia early
    /// return and Gravity Suit's unconditional colour-animation bypass. Water whose surface
    /// Samus ignores still reads as dry, while a room with no water surface goes on to test the
    /// independent lava/acid surface exactly as the game does.
    /// </remarks>
    public bool IsBottomBoundarySubmerged(SamusState samus)
    {
        Ensure.NotNull(samus);
        return DetermineRawMediumAtBoundary(samus.Kinematics.BottomBoundary) != SamusLiquidMedium.Air;
    }

    /// <summary>
    /// Reproduces the liquid-flag update, which runs after the
    /// current grapple phase and therefore affects the following grapple frame.
    /// </summary>
    /// <remarks>
    /// This test is intentionally not the ordinary movement-medium selector. The game's grapple
    /// checks whether Samus wears the Gravity Suit, requires the room to have an effect at all, and compares only the
    /// water surface against Samus's bottom. It does not consult whether Samus ignores the
    /// water's surface and never falls back to the lava/acid surface. Those quirks are retained rather than
    /// replacing the game's single flag with a friendlier general notion of “submerged.”
    /// </remarks>
    public bool DetermineGrappleSubmersion(SamusState samus)
    {
        Ensure.NotNull(samus);
        return !samus.EquippedItems.HasAny(SamusEquipmentFlags.GravitySuit) &&
            FxType != RoomFxType.None &&
            IsBelowSurface(WaterSurfaceY, samus.Kinematics.BottomBoundary);
    }

    /// <summary>
    /// Returns the extra animation delay the pose setup gives a changed pose (#2286): the liquid's delay, or the
    /// slowdown level on dry ground. That step samples <c>Y + radius - 1</c>, not the normal bottom boundary.
    /// </summary>
    public int DeterminePoseChangeExtraAnimationDelay(SamusState samus)
    {
        Ensure.NotNull(samus);
        if (samus.EquippedItems.HasAny(SamusEquipmentFlags.GravitySuit))
            return samus.SlowdownLevel;

        SamusLiquidMedium medium = DetermineRawMediumAtBoundary(PoseBottomPixel(samus));
        return medium switch
        {
            SamusLiquidMedium.Water => SamusAnimationDelayDefinitions.WaterAnimationDelay,
            SamusLiquidMedium.LavaOrAcid => SamusAnimationDelayDefinitions.LavaOrAcidAnimationDelay,
            SamusLiquidMedium.Air => samus.SlowdownLevel,
            _ => throw new InvalidOperationException($"Undefined SamusLiquidMedium {medium}."),
        };
    }

    /// <summary>
    /// Runs the animation's complete liquid step before the generic animation
    /// timer: liquid delay, medium changes, four particle producers, sound requests, and
    /// lava/acid damage accumulation.
    /// </summary>
    [IndependentFlag("Boss presence is an independent fact of the room the frame runs in.")]
    public void PrepareAnimationFrame(
        SamusState samus,
        SharedRandom random,
        int frameCounter,
        bool bossPresent)
    {
        Ensure.NotNull(samus);

        SamusMovementType movementType = SamusPoseMetadata.ReadMovementType(samus);
        TrySpawnRunningFootsteps(samus, movementType, bossPresent);

        // The animation's liquid steps read Samus's occupied bottom pixel, Y + pose radius - 1.
        // The exclusive collision boundary would
        // enter liquid one frame early and can prematurely cancel lava momentum.
        int bottom = samus.Kinematics.BottomPixel.Y;
        RoomFxType fxKind = FxType;
        bool gravitySuit = samus.EquippedItems.HasAny(SamusEquipmentFlags.GravitySuit);

        if (RoomFxTypes.UsesWater(fxKind) && WaterAffectsBoundary(bottom))
        {
            // Water's animation delay is published before the remembered medium is tested. A
            // transition into water queues the splash sound and creates either a diving splash or
            // two grounded splashes; every submerged update then gets the bubble opportunity.
            bool enteredWater = MediumLastEntered != SamusLiquidMedium.Water;
            MediumLastEntered = SamusLiquidMedium.Water;
            samus.ExtraAnimationDelay = SamusAnimationDelayDefinitions.WaterAnimationDelay;
            if (enteredWater)
            {
                QueueSound(EnemySound.SplashIn, priority: SoundRequestPriority.Normal);
                SpawnWaterSplash(samus, movementType, bottom);
            }

            TrySpawnAirBubbles(
                samus,
                frameCounter,
                random);

            // Both forward-facing poses and every Gravity-Suit pose clear the
            // extra animation delay while spawning air bubbles, after all particle/audio side effects have run.
            if (SamusPoseMetadata.IsForwardFacingPose(samus.Pose) || gravitySuit)
                samus.ExtraAnimationDelay = 0;
            return;
        }

        if (fxKind is RoomFxType.Lava or RoomFxType.Acid &&
            IsBelowSurface(LavaAcidSurfaceY, bottom))
        {
            if (fxKind == RoomFxType.Lava && samus.HorizontalSpeed.SpeedBoostCounter.IsBuilding)
            {
                // Lava alone cancels the speed boost before the Gravity-Suit branch.
                // Cancelling the speed boost owns the momentum/counter/shade/echo transition;
                // the two extra-run speeds are then cleared explicitly by the liquid step.
                // Acid deliberately skips all three writes.
                samus.HorizontalSpeed.CancelRunningMomentum(SamusPoseMetadata.ReadFacingDirection(samus));
                samus.HorizontalSpeed.ExtraRunSpeed = default;
            }

            if (fxKind == RoomFxType.Lava && gravitySuit)
            {
                // Lava does nothing more to a Gravity Suit: no damage and no surface spray.
                // Acid intentionally has no equivalent early exit and still hurts at quarter
                // strength when periodic damage runs later in the same movement phase.
                MediumLastEntered = SamusLiquidMedium.LavaOrAcid;
                samus.ExtraAnimationDelay = 0;
                return;
            }

            _periodicDamage.Accumulate(fxKind == RoomFxType.Lava
                ? SamusLiquidDamageDefinitions.Lava
                : SamusLiquidDamageDefinitions.Acid);
            if (SamusCadenceDefinitions.LiquidDamageSoundEveryEighthUpdate.IsDue(frameCounter) &&
                samus.Health >= SamusLiquidDamageDefinitions.DamageSoundLeastEnergy)
                QueueSound(RoomSound.IncrementalEnergy, priority: SoundRequestPriority.Low);

            // Both lava and acid now share one submerged delay.
            MediumLastEntered = SamusLiquidMedium.LavaOrAcid;
            samus.ExtraAnimationDelay = SamusAnimationDelayDefinitions.LavaOrAcidAnimationDelay;
            TrySpawnLavaSurfaceSpray(samus, frameCounter);
            if (SamusPoseMetadata.IsForwardFacingPose(samus.Pose) || gravitySuit)
                samus.ExtraAnimationDelay = 0;
            return;
        }

        // Out of liquid the slowdown level becomes the extra animation delay and the remembered medium is cleared. Only
        // water owns an exit splash; lava/acid simply clears the remembered medium.
        samus.ExtraAnimationDelay = samus.SlowdownLevel;
        bool exitedWater = MediumLastEntered == SamusLiquidMedium.Water;
        MediumLastEntered = SamusLiquidMedium.Air;
        if (exitedWater)
        {
            QueueSound(EnemySound.SplashOut, priority: SoundRequestPriority.Normal);
            if (!gravitySuit && movementType is
                SamusMovementType.SpinJumping or SamusMovementType.WallJumping)
                QueueSound(SamusSound.ResumedSpinJump, priority: SoundRequestPriority.Normal);
            SpawnWaterSplash(samus, movementType, bottom);
            TrySpawnAirBubbles(
                samus,
                frameCounter,
                random);
        }
    }

    /// <summary>
    /// Plays the landing's sound and effects, then each area's landing reaction.
    /// Call this after downward collision has placed Samus against the
    /// surface but before animation and pose-change cleanup erase the impact velocity/radius.
    /// </summary>
    public void HandleLandingSoundEffectsAndGraphics(
        SamusState samus,
        SamusMovementType previousMovementType,
        SamusPoseId previousPose,
        UnsignedSixteenSixteen impactYSpeed)
    {
        Ensure.NotNull(samus);

        // A landing ends the spin-loop sound separately from the impact. The test is
        // on PREVIOUS movement type and pose because landing selection has not run yet.
        // Cinematics suppress both the ordinary-spin and Screw Attack end-sound requests.
        bool cinematic = samus.Presentation.Mode == SamusPresentationMode.Cinematic;
        if (!cinematic && previousMovementType is
            SamusMovementType.SpinJumping or SamusMovementType.WallJumping)
        {
            QueueSound(SamusPoseMetadata.IsScrewAttackPose(previousPose)
                        ? SamusSound.ScrewAttackEnd
                        : SamusSound.SpinJumpEnd,
                priority: SoundRequestPriority.Normal);
        }

        // A landing that is not hard and has no fraction of speed returns before both sound and graphics; otherwise it
        // is a soft landing.
        bool hardLanding = SamusSpeedMagnitude.IsHardLanding(impactYSpeed, SamusMovementDefinitions.HardLandingSpeed);
        if (!hardLanding && impactYSpeed.IsWholePixels)
            return;

        if (!cinematic)
        {
            QueueSound(hardLanding
                    ? RoomSound.HardLanding
                    : RoomSound.Landing,
                priority: SoundRequestPriority.Normal);
        }

        HandleLandingGraphics(samus);
    }

    /// <summary>
    /// Raises the landing's splash on wet ground and its dust on dry ground whose landings raise dust, and otherwise shows
    /// nothing; the landing chooses its impact sound itself.
    /// </summary>
    private void HandleLandingGraphics(SamusState samus)
    {
        SamusGround ground = samus.StandingGround;
        if (IsWet(samus, ground.Wetness))
            SpawnLandingPairUnlessSubmerged(samus, SamusParticleKind.SurfaceSplash);
        else if (ground.DryLandingsRaiseDust)
            SpawnLandingPairUnlessSubmerged(samus, SamusParticleKind.Dust);
        else
            EndLandingEffectWait();
    }

    /// <summary>
    /// Whether ground of <paramref name="wetness"/> is wet where Samus stands: the Landing Site only while it rains, which
    /// is not a generic water test, and the Wrecked Ship entrance at and below its waterline.
    /// </summary>
    private bool IsWet(SamusState samus, SamusGroundWetness wetness) => wetness switch
    {
        SamusGroundWetness.Dry => false,
        SamusGroundWetness.Wet => true,
        SamusGroundWetness.WetWhileRaining => FxType == RoomFxType.Rain,
        SamusGroundWetness.WetBelowWaterline => samus.Position.Y.Whole >= SamusAtmosphericEffectDefinitions.WreckedShipEntranceWaterlineY,
        _ => throw new InvalidOperationException($"Undefined ground wetness {wetness}."),
    };

    /// <summary>
    /// Starts the landing's pair of dust or splash particles when Samus's last occupied pixel is above active liquid; the
    /// right-hand particle holds up breath bubbles while it shows.
    /// </summary>
    private void SpawnLandingPairUnlessSubmerged(SamusState samus, SamusParticleKind kind)
    {
        // The bottom boundary uses the current pose definition and includes the last
        // occupied pixel; the live collision radius can still belong to the old pose.
        int bottom = PoseBottomPixel(samus);

        // Both landing particle steps suppress their particles when Samus's current
        // bottom is genuinely below active water or lava/acid. The raw-medium determination
        // preserves the absent surfaces and the water whose surface Samus ignores
        // (IgnoresWaterSurface) used by those steps. Importantly, suppression RETURNS and leaves the effect waits as they were.
        if (DetermineRawMediumAtBoundary(bottom) != SamusLiquidMedium.Air)
            return;

        SamusLandingParticlePair pair = kind == SamusParticleKind.SurfaceSplash
            ? SamusLiquidParticleDefinitions.LandingSplashes
            : SamusLiquidParticleDefinitions.LandingDust;
        int firstX = samus.Position.X.Whole + pair.Right.X;
        int secondX = firstX - pair.Separation;
        int y = bottom + pair.Right.Y;

        // Both begin immediately on their first step, which shows for three updates.
        EffectWaits.HoldBubbles(SpawnParticle(kind, Position16<RoomPixels>.Of(firstX, y), SamusLiquidParticleDefinitions.AtOnce));
        _particles.Spawn(kind, Position16<RoomPixels>.Of(secondX, y), SamusLiquidParticleDefinitions.AtOnce);
    }

    /// <summary>
    /// A landing that shows no effect ends the wait its predecessor's right-hand particle kept on breath bubbles, as the
    /// game's landing cleared both of its effects; particles already shown run out on their own.
    /// </summary>
    private void EndLandingEffectWait() => EffectWaits.ReleaseBubbles();

    /// <summary>Starts a particle on its first step and returns the updates it is shown, which a wait it holds lasts.</summary>
    private int SpawnParticle(SamusParticleKind kind, Position16<RoomPixels> position, SamusParticleStart start)
    {
        _particles.Spawn(kind, position, start);
        return SamusParticle.UpdatesUntilFinished(kind, AnimationPlace.StartOfAnimation, start);
    }

    /// <summary>Creates either the diving surface particle or paired shallow splashes selected by movement type.</summary>
    internal void SpawnWaterSplash(
        SamusState samus,
        SamusMovementType movementType,
        int bottom)
    {
        if (SamusAtmosphericEffects.WaterSplashFor(movementType) ==
            WaterSplashKind.Diving)
        {
            // A splash takes the spray's leading place and does not hold it up.
            _particles.Spawn(SamusParticleKind.DivingSplash, new(samus.Position.X.Whole, UntestedWaterSurfaceLine),
                SamusLiquidParticleDefinitions.DivingSplashStart);
            EffectWaits.ReleaseSpray();
            return;
        }

        foreach (Offset16<RoomPixels> splash in SamusLiquidParticleDefinitions.ShallowSplashes)
            _particles.Spawn(SamusParticleKind.SurfaceSplash,
                new(samus.Position.X.Whole + splash.X, bottom + splash.Y), SamusLiquidParticleDefinitions.AtOnce);
        EffectWaits.ReleaseSpray();
    }

    /// <summary>Creates a bubble every 128 frames only when Samus's mouth is underwater and nothing holds bubbles up.</summary>
    private void TrySpawnAirBubbles(
        SamusState samus,
        int frameCounter,
        SharedRandom random)
    {
        // Leaving water for a room configured without a water surface still runs this test, which then finds the mouth
        // under water unless it lies above the room (AbsentWaterSurfaceLine).
        int top = TopLine(samus);
        int mouthY = top - SamusLiquidParticleDefinitions.BreathTestAboveTop;
        bool mouthUnderWater = WaterSurfaceY is int surface ? mouthY >= surface : AbsentWaterSurfaceLine.MouthIsUnder(mouthY);
        if (!mouthUnderWater ||
            !SamusCadenceDefinitions.BreathBubbleEvery128thUpdate.IsDue(frameCounter) ||
            !EffectWaits.BubbleCanForm)
        {
            return;
        }

        EffectWaits.HoldBubbles(SpawnParticle(SamusParticleKind.Bubbles,
            new(samus.Position.X.Whole, top + SamusLiquidParticleDefinitions.BubbleBelowTop), SamusLiquidParticleDefinitions.AtOnce));
        int sample = random.Generate();
        QueueSound(SamusBreathBubbleDefinitions.SoundsLow.IsTakenBy(sample) ? EnemySound.LowBubbles : EnemySound.HighBubbles, priority: SoundRequestPriority.Normal);
    }

    /// <summary>Starts the four-particle lava spray while Samus's top is above the surface and nothing holds the spray up.</summary>
    private void TrySpawnLavaSurfaceSpray(
        SamusState samus,
        int frameCounter)
    {
        // The spray runs only while Samus is below the lava or acid surface, so the surface is present.
        int surface = LavaAcidSurfaceY ?? throw new InvalidOperationException("Lava surface spray requires a lava or acid surface.");
        if (TopLine(samus) >= surface || !EffectWaits.SprayCanStart)
            return;

        // The outer particles show at once and the middle two one update late; the first two drift right and the last two
        // left. The first holds up the next spray while it shows, and the third holds up breath bubbles.
        int x = samus.Position.X.Whole;
        _particles.SpawnLavaSpray(new(x + SamusLiquidParticleDefinitions.LavaSprayOuterOffset, surface), SamusLiquidParticleDefinitions.AtOnce, SamusLiquidParticleDefinitions.LavaSprayDrift);
        EffectWaits.HoldSpray(SamusParticle.UpdatesUntilFinished(SamusParticleKind.LavaSpray, AnimationPlace.StartOfAnimation, SamusLiquidParticleDefinitions.AtOnce));
        _particles.SpawnLavaSpray(new(x, surface), SamusLiquidParticleDefinitions.OneUpdateLate, SamusLiquidParticleDefinitions.LavaSprayDrift);
        _particles.SpawnLavaSpray(new(x, surface), SamusLiquidParticleDefinitions.OneUpdateLate, -SamusLiquidParticleDefinitions.LavaSprayDrift);
        EffectWaits.HoldBubbles(SamusParticle.UpdatesUntilFinished(SamusParticleKind.LavaSpray, AnimationPlace.StartOfAnimation, SamusLiquidParticleDefinitions.OneUpdateLate));
        _particles.SpawnLavaSpray(new(x - SamusLiquidParticleDefinitions.LavaSprayOuterOffset, surface), SamusLiquidParticleDefinitions.AtOnce, -SamusLiquidParticleDefinitions.LavaSprayDrift);

        if (SamusCadenceDefinitions.LavaSpraySoundEverySecondUpdate.IsDue(frameCounter))
            QueueSound(EnemySound.LiquidDamage, priority: SoundRequestPriority.Normal);
    }

    /// <summary>Emits wet-ground or boosted footstep art at running contact frames and applies the game's sound suppression.</summary>
    private void TrySpawnRunningFootsteps(
        SamusState samus,
        SamusMovementType movementType,
        bool bossPresent)
    {
        if (movementType != SamusMovementType.Running ||
            !samus.AnimationFrameTimer.IsAt(1) ||
            !SamusAtmosphericEffects.IsRunningFootContact(samus.RunStrideFrame))
        {
            return;
        }

        bool cinematic = samus.Presentation.Mode == SamusPresentationMode.Cinematic;
        if (IsWet(samus, samus.StandingGround.Wetness))
            SpawnFootstepPair(samus, SamusParticleKind.SurfaceSplash);
        else if (samus.HorizontalSpeed.IsActivelySpeedBoosting &&
                 DetermineRawMediumAtBoundary(samus.Kinematics.BottomBoundary) == SamusLiquidMedium.Air)
            SpawnFootstepPair(samus, SamusParticleKind.Dust);

        // The footstep particles show whatever the sound does. A cinematic, a boss, a held shine and the boost's full
        // stage each silence the footsteps; the boost's cycle shade timer, already nonzero while the boost builds, does not.
        if (!cinematic &&
            !bossPresent &&
            !SamusInputSnapshot.HoldsShine(samus) &&
            !samus.HorizontalSpeed.SpeedBoostCounter.IsAtFullStage)
        {
            QueueSound(RoomSound.Footsteps, priority: SoundRequestPriority.Normal);
        }
    }

    /// <summary>Places the two running-footstep particles on opposite sides of Samus according to facing.</summary>
    /// <remarks>
    /// The leading particle, which shows one update late, takes the spray's leading place: boosted dust holds the spray up
    /// while it shows, and a wet footstep's splash ends any wait there.
    /// </remarks>
    private void SpawnFootstepPair(SamusState samus, SamusParticleKind kind)
    {
        bool facingLeft = SamusPoseMetadata.IsFacing(samus, SamusFacingDirection.Left);
        int firstX = facingLeft
            ? samus.Position.X.Whole - SamusLiquidParticleDefinitions.FootstepLeadingAhead
            : samus.Position.X.Whole + SamusLiquidParticleDefinitions.FootstepLeadingAhead;
        int secondX = facingLeft
            ? samus.Position.X.Whole + SamusLiquidParticleDefinitions.FootstepTrailingBehind
            : samus.Position.X.Whole - SamusLiquidParticleDefinitions.FootstepTrailingBehind;
        int y = samus.Position.Y.Whole + SamusLiquidParticleDefinitions.FootstepBelowCentre;
        int leadingUpdates = SpawnParticle(kind, Position16<RoomPixels>.Of(firstX, y), SamusLiquidParticleDefinitions.OneUpdateLate);
        _particles.Spawn(kind, Position16<RoomPixels>.Of(secondX, y), SamusLiquidParticleDefinitions.AtOnce);
        if (kind == SamusParticleKind.Dust)
            EffectWaits.HoldSpray(leadingUpdates);
        else
            EffectWaits.ReleaseSpray();
    }

    /// <summary>Queues a bounded sound request now.</summary>
    private void QueueSound(SoundEffectId soundEffect, SoundRequestPriority priority) =>
        AudioRequests.QueueSound(soundEffect, priority);

    /// <summary>
    /// Movement tells water from lava by whether the room has a water surface, then tests whether the boundary lies
    /// below that surface (<see cref="IsBelowSurface"/>).
    /// </summary>
    private SamusLiquidMedium DetermineRawMediumAtBoundary(int boundary)
    {
        if (WaterSurfaceY is not null)
            return WaterAffectsBoundary(boundary) ? SamusLiquidMedium.Water : SamusLiquidMedium.Air;
        return IsBelowSurface(LavaAcidSurfaceY, boundary) ? SamusLiquidMedium.LavaOrAcid : SamusLiquidMedium.Air;
    }

    /// <summary>Applies whether Samus ignores the water's surface before comparing a boundary with it.</summary>
    internal bool WaterAffectsBoundary(int boundary) =>
        !IgnoresWaterSurface &&
        IsBelowSurface(WaterSurfaceY, boundary);

    /// <summary>
    /// Whether <paramref name="boundary"/>, a line of Samus's body, lies below a present surface; no surface is never above
    /// it. A surface is a line of the room. So is her bottom, whose feet are still in the top row as a departing
    /// up-elevator leaves, and her top's line is less than zero where it lies above the room (<see cref="TopLine"/>), so
    /// the two compare plainly.
    /// </summary>
    internal static bool IsBelowSurface(int? surface, int boundary) =>
        surface is int y &&
        y < boundary;

    /// <summary>
    /// The last pixel row the current pose's body covers: Samus's centre plus that pose's radius, less one. The movement,
    /// pose-change, landing and wall-jump tests read it while the live radius can still be the previous pose's.
    /// </summary>
    /// <remarks>
    /// Her centre lies in the room whenever these run: only a departing up-elevator carries it above the room's top, and
    /// that ride moves her facing forward, which runs none of her own movement and changes no pose, lands nowhere and
    /// jumps off no wall. Her bottom therefore lies in the room too.
    /// </remarks>
    private static int PoseBottomPixel(SamusState samus) =>
        samus.Position.Y.Whole + SamusPoseMetadata.ReadPoseYRadius(samus.Pose) - 1;

    /// <summary>
    /// The line of Samus's top, her centre less her live radius: a line of the room, or less than zero where her top lies
    /// above the room's top.
    /// </summary>
    /// <remarks>
    /// Her centre lies in the room whenever her top is tested: only a departing up-elevator carries it above the room's
    /// top, and that ride faces her forward, so she neither spins nor jumps off a wall, and rises clear of every liquid long
    /// before her centre reaches the top row, so no breath bubble or lava spray is tried.
    /// </remarks>
    private static int TopLine(SamusState samus) => samus.Position.Y.Whole - samus.Kinematics.Radius.Y;

    /// <summary>
    /// Kicks up the wall jump's dust as its pose starts, shared by ordinary and grapple wall jumps. The dust holds nothing up.
    /// </summary>
    internal void SpawnWallJumpDust(SamusState samus)
    {
        // Samus's bottom boundary uses the last occupied pixel of the current
        // pose's definition radius, not the live collision radius, which keeps the spin body's radius
        // through the wall-jump frame. Liquid suppresses the dust even when Gravity Suit makes movement itself behave as
        // though the room were dry.
        int bottom = PoseBottomPixel(samus);
        if (DetermineRawMediumAtBoundary(bottom) != SamusLiquidMedium.Air)
            return;
        int offset = SamusWallJumpDustDefinitions.HorizontalOffset.Toward(
            SamusPoseMetadata.IsFacing(samus, SamusFacingDirection.Right) ? HorizontalDirection.Left : HorizontalDirection.Right);
        _particles.Spawn(SamusWallJumpDustDefinitions.Kind,
            Position16<RoomPixels>.Of(samus.Position.X.Whole + offset, bottom),
            SamusParticleStart.Shown(SamusWallJumpDustDefinitions.FirstStepUpdates));
    }
}
