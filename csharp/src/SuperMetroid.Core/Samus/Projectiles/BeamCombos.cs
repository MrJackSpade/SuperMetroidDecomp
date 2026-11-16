using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Samus.Poses;
namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>Beam combination attacks (charge plus special weapon): activation, and the ice, plasma, spazer and wave combo steps with the Spacetime colour copy tail.</summary>
internal sealed class BeamCombos
{
    /// <summary>Ends expired special attack particles through the same projectile lifecycle as ordinary beams.</summary>
    private readonly ProjectileAnimation _animation;
    /// <summary>Resets Samus's charge flare while combo particles control shared firing state.</summary>
    private readonly BeamChargeState _charge;
    /// <summary>The shot pool whose first four places a special beam attack's particles take together.</summary>
    private readonly SamusShotPool _pool;
    /// <summary>Spawns periodic Wave and other special attack trails from their live particles.</summary>
    private readonly ProjectileTrails _trails;
    /// <summary>What the Spacetime shot's runaway colour copy overwrites in saved progression.</summary>
    private readonly SpaceTimeProgressionOverwrite<SamusProjectileSegmentBeat, ProgressionOverwrite> _spaceTimeOverwrite;

    /// <summary>Connects special beam activation and particle updates to shared projectile, charge, and trail owners.</summary>
    internal BeamCombos(ProjectileAnimation animation, BeamChargeState charge, SamusShotPool pool, ProjectileTrails trails,
        SpaceTimeProgressionOverwrite<SamusProjectileSegmentBeat, ProgressionOverwrite> spaceTimeOverwrite)
    {
        _animation = animation;
        _charge = charge;
        _pool = pool;
        _trails = trails;
        _spaceTimeOverwrite = Ensure.NotNull(spaceTimeOverwrite);
    }
    /// <summary>
    /// The turn the Ice and Plasma special attacks add to every orbiting particle's angle each update: four steps on,
    /// or four steps back (the turn of a full circle less four) when a left-facing Samus starts the attack; activation
    /// also stores four steps on for Wave and no turn for Spazer, which never read it. Each particle's angle turns on
    /// round its circle, so the turn back carries it the opposite way by the same four steps.
    /// </summary>
    public TrigonometryAngle OrbitTurnPerUpdate { get; internal set; }

    /// <summary>
    /// Fires the special beam attack: debit the selected beam cost before attempting its family.
    /// This is the allocation boundary; the normal producer must call it only after
    /// charge/input eligibility, and particles run their own behaviours.
    /// </summary>
    internal bool TryActivateCombo(SamusState samus,
        SamusBombProjectileSystem shared, out SoundEffectId? sound)
    {
        Ensure.NotNull(samus);
        Ensure.NotNull(shared);
        sound = null;
        if (samus.SelectedHudItem != HudItem.PowerBomb) return false;
        SamusBeamCombination beam = SamusBeamCombinations.Of(samus.EquippedBeams);
        // A Spazer and Plasma combination indexes the special attacks past its twelve entries, into the angles
        // the Ice and Plasma attacks start their particles at. Read as places to continue the attack, those angles lead into the game's live state, so the
        // game executes that state as code, which only full memory and processor emulation could reproduce: the
        // attack is rejected under the emulation scope limit (#1500).
        if (!beam.IsRetail)
            throw new NotSupportedException(
                "A Spazer and Plasma special attack runs the game's live state as code; reproducing it needs full memory " +
                "and processor emulation, which is outside the port's scope.");
        int remaining = samus.PowerBombs - SamusComboMechanics.GetPowerBombCost(beam);
        samus.PowerBombs = remaining < 0 ? 0 : remaining;
        SamusComboKind? combo = SamusComboMechanics.ComboKind(beam);
        // The attack starts once the charge completes. Every particle of a running attack clears the charge on each of
        // its updates, and the charge grows while the particles stand still only under the X-ray once the run button is
        // released, which ends the X-ray within a few updates; so the charge completes only after the last attack's
        // particles have all ended. A particle still orbiting would be replaced like any shot in Samus's first places.
        bool activated = combo is not null;
        if (combo is SamusComboKind kind)
        {
            // The attack's particles replace the shots Samus has in her first places (SpecialAttackReplacesFirstShots),
            // and each plays its part.
            IReadOnlyList<SpecialAttackParticle> parts = SpecialAttackParticleDefinitions.InPlaceOrder;
            SamusProjectile[] particles = SpecialAttackReplacesFirstShots.ParticleShots(parts.Count, _pool.Occupy);
            for (int partIndex = 0; partIndex < parts.Count; partIndex++)
            {
                SpecialAttackParticle part = parts[partIndex];
                SamusProjectile shot = particles[partIndex];
                shot.AttackParticle = part;
                shot.Type = SamusProjectileType.Beam(beam, charged: true);
                shot.PicturedBeams = beam;
                shot.Direction = kind == SamusComboKind.Spazer ? SamusProjectileDirection.DownFacingLeft : SamusProjectileDirection.UpFacingRight;
                shot.Behavior = kind switch
                {
                    SamusComboKind.Wave => SamusShotBehaviors.WaveSpecialAttack,
                    SamusComboKind.Ice => SamusShotBehaviors.IceSpecialAttackInward,
                    SamusComboKind.Spazer => SamusShotBehaviors.SpazerSpecialAttack,
                    SamusComboKind.Plasma => SamusShotBehaviors.PlasmaSpecialAttack,
                    _ => throw new InvalidOperationException($"Undefined combo {kind}."),
                };
                // The Spazer beam pair's trail countdown starts at zero and so runs its whole wrap before it would
                // spawn a trail: only the trailing pair leaves trails until the attack falls.
                if (kind != SamusComboKind.Plasma) shot.TrailTimer = WrappingCountdown.Start(kind == SamusComboKind.Spazer && !part.SpazerTrail ? 0 : ProjectileTimingDefinitions.TrailInterval);
                shot.ComboAngle = kind is SamusComboKind.Ice or SamusComboKind.Plasma
                    ? part.OrbitStart
                    : default;
                if (kind is SamusComboKind.Wave or SamusComboKind.Spazer)
                    shot.Position = shot.Position.WithFractionX(0).WithFractionY(0);
                if (kind is SamusComboKind.Spazer or SamusComboKind.Plasma) shot.ComboOrbitRadius = 40;
                if (kind == SamusComboKind.Spazer) shot.SpazerPhase = SpazerComboPhase.Circling;
                if (kind is SamusComboKind.Wave or SamusComboKind.Ice) shot.ComboLifetime = 600;
                if (kind == SamusComboKind.Spazer) shot.SpazerTurnPerUpdate = SpazerTurn(part.SpazerTurn, BeamComboDefinitions.SpazerCirclingTurn);
                if (kind == SamusComboKind.Plasma) shot.PlasmaPhase = PlasmaRingPhase.Expanding;
                if (kind == SamusComboKind.Wave)
                {
                    shot.Position = shot.Position.WithWhole(new(CoordinateAxis.Wrap(samus.Position.X.Whole + part.WaveStart.X), CoordinateAxis.Wrap(samus.Position.Y.Whole + part.WaveStart.Y)));
                }
                if (kind == SamusComboKind.Spazer && part.SpazerTrail) shot.Type = SamusProjectileType.EchoOf(SamusBeamEcho.SpazerComboTrail);
                InitializeComboShot(shot, kind == SamusComboKind.Ice ? BeamComboShotKind.Charged
                    : kind == SamusComboKind.Spazer && part.SpazerTrail ? BeamComboShotKind.Echo : BeamComboShotKind.Combo);
            }
            // Each Spazer beam particle pairs with the trailing particle that turns its way.
            if (kind == SamusComboKind.Spazer)
            {
                foreach (SamusProjectile particle in particles)
                    particle.SpazerPartner = particles.Single(other => !ReferenceEquals(other, particle) &&
                        PartOf(other).SpazerTurn == PartOf(particle).SpazerTurn);
            }
            _pool.ShotCount.CountSpecialAttack(parts.Count);
            // The attack waits the cooldown of its first place's projectile: the combination's charged shot.
            shared.SetSharedCooldown(SamusProjectileCooldowns.Read(beam, BeamShotCooldowns.Shot.Charged));
            OrbitTurnPerUpdate = kind == SamusComboKind.Spazer ? TrigonometryAngle.Zero
                : kind == SamusComboKind.Wave || SamusPoseMetadata.IsFacing(samus, SamusFacingDirection.Right) ? BeamComboDefinitions.OrbitTurnOn
                : BeamComboDefinitions.OrbitTurnBack;
            sound = kind switch
            {
                SamusComboKind.Wave => SamusSound.WaveSpecialAttack,
                SamusComboKind.Ice => SamusSound.IceSpecialAttack,
                SamusComboKind.Spazer => SamusSound.SpazerSpecialAttack,
                SamusComboKind.Plasma => SamusSound.PlasmaSpecialAttack,
                _ => throw new InvalidOperationException($"Undefined combo {kind}."),
            };
        }
        if (samus.PowerBombs == 0)
            samus.SelectedHudItem = samus.AutoCancelledItem = HudItem.Beam;
        return activated;
    }

    /// <summary>Gives a particle the damage and animation of the charged beam, the special attack or the Spazer echo, and starts its first frame.</summary>
    internal static void InitializeComboShot(SamusProjectile shot, BeamComboShotKind shotKind)
    {
        SamusProjectileKind projectileKind = shotKind switch
        {
            BeamComboShotKind.Charged => SamusProjectileSelection.BeamShot(shot.Type.BeamCombination, charged: true),
            BeamComboShotKind.Echo => SamusProjectileSelection.Echo(
                shot.Type.Echo ?? throw new InvalidOperationException("Only an echo projectile takes the echo's damage and animation.")),
            BeamComboShotKind.Combo => SamusProjectileSelection.SpecialAttack(shot.Type.BeamCombination)
                ?? throw new InvalidDataException($"{shot.Type.BeamCombination}'s special attack has no particles of its own."),
            _ => throw new ArgumentOutOfRangeException(nameof(shotKind), shotKind, "Undefined combo shot kind."),
        };
        shot.Damage = SamusProjectileDamage.Read(projectileKind);
        SamusProjectileSegment segment = shotKind == BeamComboShotKind.Combo
            ? SamusProjectileKinds.SoleAnimation(projectileKind)
            : SamusProjectileKinds.Animation(projectileKind, shot.Direction);
        shot.Playback = SamusProjectilePlayback.Start(segment);
        shot.FrameHold = WrappingCountdown.Start(1);
        if (shotKind == BeamComboShotKind.Charged)
            shot.Radius = SamusProjectilePlayback.InitialRadius(segment);
    }

    /// <summary>Runs one Ice special attack particle and returns its optional Samus-library sound request.</summary>
    internal SoundEffectId? StepIceCombo(SamusState samus,
        SamusProjectile shot, SamusBombProjectileSystem shared, Position16<RoomPixels> camera)
    {
        if (shot.Behavior is not (IceSpecialAttackOrbit or IceSpecialAttackSpread))
            throw new InvalidOperationException("Ice combo update requires its own behaviour.");
        bool outward = shot.Behavior is IceSpecialAttackSpread;
        if (shot.IsSpent)
        {
            _animation.ClearProjectile(shot);
            return outward ? null : SamusSound.IceSpecialAttackEnd;
        }
        CountdownStep trailStep = shot.TrailTimer.Decrement();
        shot.TrailTimer = trailStep.Remaining;
        if (trailStep.IsZero)
        {
            shot.TrailTimer = WrappingCountdown.Start(ProjectileTimingDefinitions.TrailInterval);
            _trails.SpawnTrail(shot);
        }
        int distance = outward ? shot.ComboOrbitRadius : 32;
        Offset16<RoomPixels> orbit = SamusComboMechanics.GetSineOffset(shot.ComboAngle, distance);
        shot.Position = shot.Position.WithWrappedWholeX(samus.Position.X.Whole + orbit.X);
        if (outward && LeftViewHorizontally(shot, camera))
        {
            _animation.ClearProjectile(shot);
            return null;
        }
        shot.Position = shot.Position.WithWrappedWholeY(samus.Position.Y.Whole + orbit.Y);
        if (outward && LeftViewVertically(shot, camera))
        {
            _animation.ClearProjectile(shot);
            return null;
        }
        shot.ComboAngle += OrbitTurnPerUpdate;
        SoundEffectId? sound = null;
        if (outward)
            shot.ComboOrbitRadius = ComboRingCollapse.Grown(shot.ComboOrbitRadius, 8);
        else
        {
            // The lifetime starts at 600 and the particle turns outward as it leaves one, so it never passes zero.
            int oldLifetime = shot.ComboLifetime;
            shot.ComboLifetime = oldLifetime - 1;
            if (oldLifetime == 1)
            {
                shot.Behavior = SamusShotBehaviors.IceSpecialAttackOutward;
                shot.ComboOrbitRadius = 40;
                sound = SamusSound.IceSpecialAttackEnd;
            }
        }
        shared.SetSharedCooldown(SamusProjectileCooldownDefinitions.SpecialAttackHold);
        _charge.ClearCount();
        return sound;
    }
    /// <summary>
    /// Whether a special attack particle has flown out of the view sideways: more than its margin left of the view's left
    /// edge, or its reach or more right of it.
    /// </summary>
    private static bool LeftViewHorizontally(SamusProjectile shot, Position16<RoomPixels> camera) =>
        CoordinateAxis.AsSigned(shot.Position.X.Whole - camera.X + BeamComboDefinitions.ViewLeftMargin) < 0 ||
        CoordinateAxis.Difference(shot.Position.X.Whole - camera.X, BeamComboDefinitions.ViewRightReach) >= 0;

    /// <summary>
    /// Whether a special attack particle has flown out of the view up or down: above its top reach below the view's top
    /// edge, or at its bottom reach or below.
    /// </summary>
    private static bool LeftViewVertically(SamusProjectile shot, Position16<RoomPixels> camera) =>
        CoordinateAxis.Difference(shot.Position.Y.Whole - camera.Y, BeamComboDefinitions.ViewTopReach) < 0 ||
        CoordinateAxis.Difference(shot.Position.Y.Whole - camera.Y, BeamComboDefinitions.ViewBottomReach) >= 0;

    /// <summary>The Plasma ring update: the ring grows, shrinks and grows again by its phase.</summary>
    internal void StepPlasmaCombo(SamusState samus,
        SamusProjectile shot, SamusBombProjectileSystem shared, Position16<RoomPixels> camera)
    {
        if (shot.Behavior is not PlasmaSpecialAttackRing)
            throw new InvalidOperationException("Plasma update requires its own behaviour.");
        if (shot.IsSpent)
        {
            _animation.ClearProjectile(shot);
            return;
        }
        shared.SetSharedCooldown(SamusProjectileCooldownDefinitions.SpecialAttackHold);
        _charge.ClearCount();
        Offset16<RoomPixels> orbit = SamusComboMechanics.GetSineOffset(
            shot.ComboAngle, shot.ComboOrbitRadius);
        shot.Position = shot.Position.WithWhole(new(CoordinateAxis.Wrap(orbit.X + samus.Position.X.Whole), CoordinateAxis.Wrap(orbit.Y + samus.Position.Y.Whole)));
        shot.ComboAngle += OrbitTurnPerUpdate;
        switch (shot.PlasmaPhase)
        {
            case PlasmaRingPhase.Expanding:
                shot.ComboOrbitRadius = ComboRingCollapse.Grown(shot.ComboOrbitRadius, BeamComboDefinitions.PlasmaRadiusStep);
                if (shot.ComboOrbitRadius >= BeamComboDefinitions.PlasmaOuterRadius) shot.PlasmaPhase = PlasmaRingPhase.Contracting;
                break;
            case PlasmaRingPhase.Contracting:
                shot.ComboOrbitRadius = ComboRingCollapse.Grown(shot.ComboOrbitRadius, -BeamComboDefinitions.PlasmaRadiusStep);
                if (shot.ComboOrbitRadius < BeamComboDefinitions.PlasmaInnerThreshold) shot.PlasmaPhase = PlasmaRingPhase.FinalExpansion;
                break;
            case PlasmaRingPhase.FinalExpansion:
                // Only the final expansion checks visibility. The first two phases
                // intentionally keep rings alive beyond the viewport, unlike Ice.
                if (LeftViewHorizontally(shot, camera) || LeftViewVertically(shot, camera))
                    _animation.ClearProjectile(shot);
                else shot.ComboOrbitRadius = ComboRingCollapse.Grown(shot.ComboOrbitRadius, BeamComboDefinitions.PlasmaRadiusStep);
                break;
            default: throw new InvalidOperationException($"Undefined {nameof(PlasmaRingPhase)} {shot.PlasmaPhase}.");
        }
    }
    /// <summary>
    /// Runs the Spacetime shot's behaviour, its progression overwrite: from an animation that reaches far enough, it
    /// turns saved progression on and off (<see cref="SpaceTimeProgressionOverwrite{TCursor, TOverwrite}"/>). Before the
    /// shot's first frame is installed, or with no animation, it leaves progression alone; only the overwrite of
    /// progression is kept (owner decision on #1318).
    /// </summary>
    /// <param name="progression">The session's progression, which the copy overwrites; required when it reaches it.</param>
    /// <param name="shot">The Spacetime shot.</param>
    internal void RunSpaceTimeProgressionOverwrite(SharedGameState? progression, SamusProjectile shot)
    {
        Ensure.NotNull(shot);
        if (shot.Sprite is null || shot.Playback is not { } playback)
            return;
        if (_spaceTimeOverwrite.For(SamusProjectilePlayback.BeatOf(playback), shot.Place) is { } overwrite)
            (progression ?? throw new InvalidOperationException(
                "The Spacetime shot's progression overwrite writes progression; the frame must pass it.")).Overwrite(overwrite);
    }
    /// <summary>Runs the Spazer combo, including paired-hit cleanup and retagging the primary particles as falling trails.</summary>
    internal SoundEffectId? StepSpazerCombo(SamusState samus,
        SamusProjectile shot, SamusBombProjectileSystem shared, int cameraY)
    {
        if (shot.Behavior is not (SpazerSpecialAttackFlight or SpazerSpecialAttackFall))
            throw new InvalidOperationException("Spazer update requires its own behaviour.");
        bool falling = shot.Behavior is SpazerSpecialAttackFall;
        if (shot.IsSpent)
        {
            SamusProjectile partner = shot.SpazerPartner ?? throw new InvalidOperationException(
                $"The Spazer particle in place {shot.Place} has no partner.");
            _animation.ClearProjectile(shot);
            if (!falling)
                // The spent particle ends, with itself, whatever shot holds its partner's place (SamusShotPool.HolderOfPlaceOf).
                _animation.ClearProjectile(_pool.HolderOfPlaceOf(partner));
            return null;
        }
        CountdownStep trailStep = shot.TrailTimer.Decrement();
        shot.TrailTimer = trailStep.Remaining;
        if (trailStep.IsZero)
        {
            shot.TrailTimer = WrappingCountdown.Start(ProjectileTimingDefinitions.TrailInterval);
            _trails.SpawnTrail(shot);
        }
        if (falling)
        {
            shot.Position += new Offset16<RoomPixels>(0, 8);
            if (CoordinateAxis.Difference(shot.Position.Y.Whole - cameraY, 248) >= 0)
            {
                _animation.ClearProjectile(shot);
                return null;
            }
        }
        else
        {
            Offset16<RoomPixels> orbit = SamusComboMechanics.GetSineOffset(
                shot.ComboAngle, shot.ComboOrbitRadius);
            shot.Position = shot.Position.WithWhole(new(CoordinateAxis.Wrap(samus.Position.X.Whole + orbit.X), CoordinateAxis.Wrap(samus.Position.Y.Whole + orbit.Y - (shot.SpazerPhase == SpazerComboPhase.Circling ? 0 : BeamComboDefinitions.SpazerFlightCentreHeight))));
            switch (shot.SpazerPhase)
            {
                case SpazerComboPhase.Circling:
                    AdvanceSpazerAngle(shot);
                    if (shot.ComboAngle == TrigonometryAngle.HalfTurn)
                    {
                        shot.ComboOrbitRadius = 160;
                        shot.SpazerTurnPerUpdate = SpazerTurn(PartOf(shot).SpazerTurn, BeamComboDefinitions.SpazerFlyingTurn);
                        shot.Direction = SamusProjectileDirection.UpFacingRight;
                        shot.IsSpent = false;
                        shot.SpazerPhase = SpazerComboPhase.FlyingUpTowardsPoint;
                    }
                    break;
                case SpazerComboPhase.FlyingUpTowardsPoint:
                    if (CoordinateAxis.Difference(shot.Position.Y.Whole - cameraY, 16) < 0)
                        BeginSpazerFall(shot);
                    else
                    {
                        AdvanceSpazerAngle(shot);
                        // The radius falls from 160 by five an update, so it meets zero exactly and never passes it.
                        shot.ComboOrbitRadius -= 5;
                        if (shot.ComboOrbitRadius == 0)
                        {
                            shot.SpazerTurnPerUpdate = TrigonometryAngle.Zero - SpazerTurn(PartOf(shot).SpazerTurn, BeamComboDefinitions.SpazerFlyingTurn);
                            shot.ComboAngle += TrigonometryAngle.HalfTurn;
                            shot.SpazerPhase = SpazerComboPhase.FlyingUpAwayFromPoint;
                            shared.SetSharedCooldown(SamusProjectileCooldownDefinitions.SpecialAttackHold);
                            _charge.ClearCount();
                            return PartOf(shot).VoicesSpazerConvergence ? SamusSound.SpazerSpecialAttackEnd : null;
                        }
                    }
                    break;
                case SpazerComboPhase.FlyingUpAwayFromPoint:
                    if (CoordinateAxis.Difference(shot.Position.Y.Whole - cameraY, 16) < 0)
                        BeginSpazerFall(shot);
                    else
                    {
                        AdvanceSpazerAngle(shot);
                        shot.ComboOrbitRadius += 5;
                        if (shot.ComboOrbitRadius >= 96) BeginSpazerFall(shot);
                    }
                    break;
                default: throw new InvalidOperationException($"Undefined {nameof(SpazerComboPhase)} {shot.SpazerPhase}.");
            }
        }
        shared.SetSharedCooldown(SamusProjectileCooldownDefinitions.SpecialAttackHold);
        _charge.ClearCount();
        return null;
    }

    /// <summary>Turns the Spazer particle on round its circle by its turn per update.</summary>
    private static void AdvanceSpazerAngle(SamusProjectile shot) =>
        shot.ComboAngle += shot.SpazerTurnPerUpdate;

    /// <summary>The part <paramref name="particle"/> plays in its special attack.</summary>
    private static SpecialAttackParticle PartOf(SamusProjectile particle) =>
        particle.AttackParticle ?? throw new InvalidOperationException(
            $"The projectile in place {particle.Place} runs a special attack's update but plays no part in one.");

    /// <summary>
    /// The turn per update that carries a Spazer particle <paramref name="by"/> round its circle <paramref name="turn"/>:
    /// the turn itself clockwise, a full circle less it counterclockwise.
    /// </summary>
    private static TrigonometryAngle SpazerTurn(RotationDirection turn, TrigonometryAngle by) => turn switch
    {
        RotationDirection.Clockwise => by,
        RotationDirection.Counterclockwise => TrigonometryAngle.Zero - by,
        _ => throw new ArgumentOutOfRangeException(nameof(turn), turn, $"Undefined {nameof(RotationDirection)}."),
    };

    /// <summary>Moves a circling Spazer particle outward and switches its beam pair to falling echo artwork.</summary>
    private static void BeginSpazerFall(SamusProjectile shot)
    {
        shot.Position = shot.Position.WithWrappedWholeX(shot.Position.X.Whole + (PartOf(shot).SpazerTrail ? -16 : 16));
        shot.Direction = SamusProjectileDirection.DownFacingLeft;
        shot.IsSpent = false;
        shot.TrailTimer = WrappingCountdown.Start(ProjectileTimingDefinitions.TrailInterval);
        shot.Behavior = SamusShotBehaviors.SpazerSpecialAttackFalling;
        if (!PartOf(shot).SpazerTrail)
        {
            shot.Type = SamusProjectileType.EchoOf(SamusBeamEcho.SpazerComboTrail);
            InitializeComboShot(shot, BeamComboShotKind.Echo);
        }
    }
    /// <summary>Runs the Wave combo's homing oscillator, preserving 1/256-pixel acceleration and 1/65536-pixel position overflow.</summary>
    internal SoundEffectId? StepWaveCombo(SamusState samus,
        SamusProjectile shot, SamusBombProjectileSystem shared)
    {
        if (shot.Behavior is not WaveSpecialAttackHoming)
            throw new InvalidOperationException("Wave update requires its own behaviour.");
        // A hit skips even the lifetime decrement. Expiry and collision both queue
        // the terminal sound, unlike Ice's silent outward-phase removal.
        if (shot.IsSpent)
        {
            _animation.ClearProjectile(shot);
            return BeamComboDefinitions.WaveRemovalSound;
        }
        // The lifetime starts at 600 and the particle ends when it reaches zero, so it never passes zero.
        shot.ComboLifetime -= 1;
        if (shot.ComboLifetime <= 0)
        {
            _animation.ClearProjectile(shot);
            return BeamComboDefinitions.WaveRemovalSound;
        }
        SignedEightEight oldXSpeed = shot.Velocity.X;
        CountdownStep trailStep = shot.TrailTimer.Decrement();
        shot.TrailTimer = trailStep.Remaining;
        if (trailStep.IsZero)
        {
            shot.TrailTimer = WrappingCountdown.Start(ProjectileTimingDefinitions.TrailInterval);
            _trails.SpawnTrail(shot);
        }
        shot.Velocity = shot.Velocity.WithX(AccelerateWaveAxis(shot.Velocity.X, CoordinateAxis.Difference(samus.Position.X.Whole, shot.Position.X.Whole)));
        shot.Position = shot.Position.AdvanceX(shot.Velocity.X.ToSixteenSixteenDelta());
        shot.WaveComboVerticalSpeed = AccelerateWaveAxis(shot.WaveComboVerticalSpeed, CoordinateAxis.Difference(samus.Position.Y.Whole, shot.Position.Y.Whole));
        shot.Position = shot.Position.AdvanceY(shot.WaveComboVerticalSpeed.ToSixteenSixteenDelta());
        shared.SetSharedCooldown(SamusProjectileCooldownDefinitions.SpecialAttackHold);
        _charge.ClearCount();
        return PartOf(shot).VoicesWavePulse && oldXSpeed.IsNegative != shot.Velocity.X.IsNegative ? BeamComboDefinitions.WavePulseSound : null;
    }

    /// <summary>
    /// Moves one Wave velocity component toward Samus, respecting the acceleration and signed speed bound. The speed
    /// stays within one acceleration of the bound either way, far inside the 1/256-pixel range.
    /// </summary>
    private static SignedEightEight AccelerateWaveAxis(SignedEightEight speed, int targetDelta)
    {
        if (targetDelta < 0)
            return speed > -BeamComboDefinitions.WaveSpeedLimit ? speed - BeamComboDefinitions.WaveAcceleration : speed;
        return speed < BeamComboDefinitions.WaveSpeedLimit ? speed + BeamComboDefinitions.WaveAcceleration : speed;
    }
}
