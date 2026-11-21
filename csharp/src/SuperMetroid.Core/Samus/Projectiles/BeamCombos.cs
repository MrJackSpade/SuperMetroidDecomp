using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Samus.Poses;
namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>Beam combination attacks (charge plus special weapon): activation, and the ice, plasma, spazer and wave
/// combo steps with the Spacetime shot's change to saved progress.</summary>
internal sealed class BeamCombos
{
    /// <summary>Ends expired special attack particles through the same projectile lifecycle as ordinary beams.</summary>
    private readonly ProjectileAnimation _animation;
    /// <summary>Resets Samus's charge flare while combo particles control shared firing state.</summary>
    private readonly BeamChargeState _charge;
    /// <summary>The shot pool whose first places a special beam attack's particles take together.</summary>
    private readonly SamusShotPool _pool;
    /// <summary>Spawns periodic Wave and other special attack trails from their live particles.</summary>
    private readonly ProjectileTrails _trails;
    /// <summary>What the Spacetime shot changes in saved progress.</summary>
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
        // A Spazer and Plasma combination has no special attack of its own: the game reads past the attacks into the
        // angles the Ice and Plasma attacks start their particles at. Read as places to continue the attack, those angles lead into the game's live state, so the
        // game executes that state as code, which only full memory and processor emulation could reproduce: the
        // attack is rejected under the emulation scope limit (#1500).
        if (!beam.CanBeEquipped)
            throw new NotSupportedException(
                "A Spazer and Plasma special attack runs the game's live state as code; reproducing it needs full memory " +
                "and processor emulation, which is outside the emulation scope limit (#1500).");
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
            // The Ice and Plasma particles turn round Samus by OrbitTurnOn each update, or by OrbitTurnBack (a full circle
            // less as much) when a left-facing Samus starts the attack, which carries them the opposite way by as much.
            TrigonometryAngle orbitTurn = SamusPoseMetadata.IsFacing(samus, SamusFacingDirection.Right)
                ? BeamComboDefinitions.OrbitTurnOn
                : BeamComboDefinitions.OrbitTurnBack;
            for (int partIndex = 0; partIndex < parts.Count; partIndex++)
            {
                SpecialAttackParticle part = parts[partIndex];
                SamusProjectile shot = particles[partIndex];
                // A Spazer attack's trailing particle is the Spazer echo from the start; every other particle is the
                // combination's charged beam. Each deals the damage of the projectile it plays.
                bool spazerTrail = kind == SamusComboKind.Spazer && part.SpazerTrail;
                BeamComboShotKind shotKind = kind == SamusComboKind.Ice ? BeamComboShotKind.Charged
                    : spazerTrail ? BeamComboShotKind.Echo : BeamComboShotKind.Combo;
                SamusProjectileOccupant held = OccupyComboShot(shot, spazerTrail
                    ? SamusProjectileType.EchoOf(SamusBeamEcho.SpazerComboTrail)
                    : SamusProjectileType.Beam(beam, charged: true), shotKind);
                shot.PicturedBeams = beam;
                shot.Direction = kind == SamusComboKind.Spazer ? SamusProjectileDirection.DownFacingLeft : SamusProjectileDirection.UpFacingRight;
                ParticleInstalls[kind](held, part, orbitTurn);
                // Every particle but a Plasma ring leaves trails, the Spazer beam pair only once the attack falls
                // (SpazerParticleState.LeavesTrails), which starts its countdown then.
                if (kind is SamusComboKind.Wave or SamusComboKind.Ice || (kind == SamusComboKind.Spazer && part.SpazerTrail))
                    shot.Trail = UpdateCountdown.Lasting(ProjectileTimingDefinitions.TrailInterval);
                if (kind is SamusComboKind.Wave or SamusComboKind.Spazer)
                    shot.Position = shot.Position.WithoutFractions();
                if (kind == SamusComboKind.Wave)
                {
                    // A Wave particle starts its wave start away from Samus on each axis, so near the room's top or left edge it
                    // starts above or left of the room's origin, a place OffRoomPlaces gives it.
                    shot.Position = shot.Position.WithWhole(new(OffRoomPlaces.Place(samus.Position.X.Whole + part.WaveStart.X), OffRoomPlaces.Place(samus.Position.Y.Whole + part.WaveStart.Y)));
                }
                StartComboShot(held, shotKind);
            }
            // Each Spazer beam particle pairs with the trailing particle that turns its way.
            if (kind == SamusComboKind.Spazer)
            {
                foreach (SamusProjectile particle in particles)
                    particle.KindStateAs<SpazerParticleState>().Partner = particles.Single(other => !ReferenceEquals(other, particle) &&
                        PartOf(other).SpazerTurn == PartOf(particle).SpazerTurn);
            }
            _pool.ShotCount.CountSpecialAttack(parts.Count);
            // The attack waits the cooldown of its first place's projectile: the combination's charged shot.
            shared.SetSharedCooldown(SamusProjectileCooldowns.Read(beam, BeamShotCooldowns.Shot.Charged));
            sound = BeamComboDefinitions.ActivationSounds[kind];
        }
        if (samus.PowerBombs == 0)
            samus.SelectedHudItem = HudItem.Beam;
            samus.AutoCancelledItem = null;
        return activated;
    }

    /// <summary>
    /// How each combination's special attack installs a particle (#2553): its behaviour and its state, the Ice and Plasma
    /// particles turning round Samus by the attack's orbit turn and the Spazer particles circling by their own.
    /// </summary>
    private static IdentityTable<SamusComboKind, Action<SamusProjectileOccupant, SpecialAttackParticle, TrigonometryAngle>> ParticleInstalls => field ??= new(
        new Dictionary<SamusComboKind, Action<SamusProjectileOccupant, SpecialAttackParticle, TrigonometryAngle>>
        {
            [SamusComboKind.Wave] = static (held, part, _) => held.Install(SamusShotBehaviors.WaveSpecialAttack, new WaveParticleState(part)),
            [SamusComboKind.Ice] = static (held, part, orbitTurn) =>
                held.Install(SamusShotBehaviors.IceSpecialAttackInward, new IceParticleState(part, orbitTurn)),
            [SamusComboKind.Spazer] = static (held, part, _) => held.Install(SamusShotBehaviors.SpazerSpecialAttack,
                new SpazerParticleState(part, SpazerTurn(part.SpazerTurn, BeamComboDefinitions.SpazerCirclingTurn))),
            [SamusComboKind.Plasma] = static (held, part, orbitTurn) =>
                held.Install(SamusShotBehaviors.PlasmaSpecialAttack, new PlasmaParticleState(part, orbitTurn)),
        });

    /// <summary>The projectile whose damage and animation a particle of <paramref name="type"/> takes as the charged beam, the special attack or the Spazer echo.</summary>
    private static SamusProjectileKind ComboShotKind(SamusProjectileType type, BeamComboShotKind shotKind) => shotKind switch
    {
        BeamComboShotKind.Charged => SamusProjectileSelection.BeamShot(type.BeamCombination, charged: true),
        BeamComboShotKind.Echo => SamusProjectileSelection.Echo(
            type.Echo ?? throw new InvalidOperationException("Only an echo projectile takes the echo's damage and animation.")),
        BeamComboShotKind.Combo => SamusProjectileSelection.SpecialAttack(type.BeamCombination)
            ?? throw new InvalidDataException($"{type.BeamCombination}'s special attack has no particles of its own."),
        _ => throw new ArgumentOutOfRangeException(nameof(shotKind), shotKind, "Undefined combo shot kind."),
    };

    /// <summary>
    /// Takes <paramref name="shot"/>'s place with a particle of <paramref name="type"/> dealing the damage of the charged
    /// beam, the special attack or the Spazer echo it plays.
    /// </summary>
    internal static SamusProjectileOccupant OccupyComboShot(SamusProjectile shot, SamusProjectileType type, BeamComboShotKind shotKind) =>
        Ensure.NotNull(shot).Occupy(type, SamusProjectileDamage.Read(ComboShotKind(type, shotKind)));

    /// <summary>Gives a particle the damage and animation of the charged beam, the special attack or the Spazer echo, and starts its first frame.</summary>
    private static void InitializeComboShot(SamusProjectileOccupant held, BeamComboShotKind shotKind)
    {
        held.Damage = SamusProjectileDamage.Read(ComboShotKind(held.Type, shotKind));
        StartComboShot(held, shotKind);
    }

    /// <summary>Starts a particle's animation as the charged beam, the special attack or the Spazer echo, from its first frame.</summary>
    internal static void StartComboShot(SamusProjectileOccupant held, BeamComboShotKind shotKind)
    {
        SamusProjectile shot = held.Projectile;
        SamusProjectileKind projectileKind = ComboShotKind(held.Type, shotKind);
        SamusProjectileSegment segment = shotKind == BeamComboShotKind.Combo
            ? SamusProjectileKinds.SoleAnimation(projectileKind)
            : SamusProjectileKinds.Animation(projectileKind, shot.Direction);
        shot.Animation = SamusProjectilePlayback.Start(segment);
        if (shotKind == BeamComboShotKind.Charged)
            shot.Radius = SamusProjectilePlayback.InitialRadius(segment);
    }

    /// <summary>
    /// Runs one update of an Ice special attack particle orbiting Samus, and returns its optional Samus-library sound
    /// request: the attack's end sound when it is spent, or when its lifetime ends and it turns outward to spread.
    /// </summary>
    internal SoundEffectId? StepIceOrbit(SamusState samus, SamusProjectileOccupant held, SamusBombProjectileSystem shared)
    {
        SamusProjectile shot = held.Projectile;
        IceParticleState ice = shot.KindStateAs<IceParticleState>();
        if (shot.IsSpent)
        {
            _animation.ClearProjectile(shot);
            return SamusSound.IceSpecialAttackEnd;
        }
        StepTrail(held);
        Offset16<RoomPixels> orbit = SamusComboMechanics.GetSineOffset(ice.Angle, ice.Radius);
        // An Ice particle circles Samus at its orbit radius, further than her centre lies from a wall at the room's left edge, so
        // it passes left of the room's origin there, and above it where the room is open at the top, places
        // OffRoomPlaces gives it.
        shot.Position = OffRoomPlaces.WithWholeX(shot.Position, samus.Position.X.Whole + orbit.X);
        shot.Position = OffRoomPlaces.WithWholeY(shot.Position, samus.Position.Y.Whole + orbit.Y);
        ice.Turn();
        SoundEffectId? sound = null;
        if (ice.Lifetime.Count())
        {
            shot.Behavior = SamusShotBehaviors.IceSpecialAttackOutward;
            ice.Radius = BeamComboDefinitions.IceSpreadStartRadius;
            sound = SamusSound.IceSpecialAttackEnd;
        }
        shared.SetSharedCooldown(SamusProjectileCooldownDefinitions.SpecialAttackHold);
        _charge.ClearCount();
        return sound;
    }

    /// <summary>
    /// Runs one update of an Ice special attack particle spreading outward round Samus, its circle growing, until it leaves
    /// the view; it makes no sound.
    /// </summary>
    internal void StepIceSpread(SamusState samus, SamusProjectileOccupant held, SamusBombProjectileSystem shared, Position16<RoomPixels> camera)
    {
        SamusProjectile shot = held.Projectile;
        IceParticleState ice = shot.KindStateAs<IceParticleState>();
        if (shot.IsSpent)
        {
            _animation.ClearProjectile(shot);
            return;
        }
        StepTrail(held);
        Offset16<RoomPixels> orbit = SamusComboMechanics.GetSineOffset(ice.Angle, ice.Radius);
        // The spreading circle grows until its particles leave the view, so beside the room's top or left edge they pass
        // above or left of the room's origin first, places OffRoomPlaces gives them.
        shot.Position = OffRoomPlaces.WithWholeX(shot.Position, samus.Position.X.Whole + orbit.X);
        if (LeftViewHorizontally(held, camera))
        {
            _animation.ClearProjectile(shot);
            return;
        }
        shot.Position = OffRoomPlaces.WithWholeY(shot.Position, samus.Position.Y.Whole + orbit.Y);
        if (LeftViewVertically(held, camera))
        {
            _animation.ClearProjectile(shot);
            return;
        }
        ice.Turn();
        ice.Radius = ComboRingCollapse.Grown(ice.Radius, BeamComboDefinitions.IceSpreadRadiusStep);
        shared.SetSharedCooldown(SamusProjectileCooldownDefinitions.SpecialAttackHold);
        _charge.ClearCount();
    }

    /// <summary>Counts down a particle's trail countdown, and spawns a trail behind it and starts the countdown again when it runs out.</summary>
    private void StepTrail(SamusProjectileOccupant held)
    {
        SamusProjectile shot = held.Projectile;
        if (shot.CountsDownToTrail(ProjectileTimingDefinitions.TrailInterval))
            _trails.SpawnTrail(held);
    }

    /// <summary>
    /// Whether a special attack particle has flown out of the view sideways: more than its margin left of the view's left
    /// edge, or its reach or more right of it. A particle circles Samus within a ring's largest radius, so it stays within
    /// a room's breadth of the view and its distance from the view's edge is an ordinary one.
    /// </summary>
    private static bool LeftViewHorizontally(SamusProjectileOccupant held, Position16<RoomPixels> camera)
    {
        SamusProjectile shot = held.Projectile;
        return CameraPastRoomEdge.FromCamera(shot.Position.X.Whole, camera.X) is
            < -BeamComboDefinitions.ViewLeftMargin or >= BeamComboDefinitions.ViewRightReach;
    }

    /// <summary>
    /// Whether a special attack particle has flown out of the view up or down: above its top reach below the view's top
    /// edge, or at its bottom reach or below.
    /// </summary>
    private static bool LeftViewVertically(SamusProjectileOccupant held, Position16<RoomPixels> camera)
    {
        SamusProjectile shot = held.Projectile;
        return CameraPastRoomEdge.FromCamera(shot.Position.Y.Whole, camera.Y) is
            < BeamComboDefinitions.ViewTopReach or >= BeamComboDefinitions.ViewBottomReach;
    }

    /// <summary>What <see cref="StepPlasmaCombo"/> does for each <see cref="PlasmaRingPhase"/> member, each member's own arm.</summary>
    private IdentityTable<PlasmaRingPhase, Action<SamusProjectileOccupant, Position16<RoomPixels>, PlasmaParticleState>> StepPlasmaComboForPlasmaRingPhase => field ??= new(new Dictionary<PlasmaRingPhase, Action<SamusProjectileOccupant, Position16<RoomPixels>, PlasmaParticleState>>
    {
        [PlasmaRingPhase.Expanding] = (held, camera, ring) =>
        {
            ring.Radius = ComboRingCollapse.Grown(ring.Radius, BeamComboDefinitions.PlasmaRadiusStep);
            if (ring.Radius >= BeamComboDefinitions.PlasmaOuterRadius) ring.Phase = PlasmaRingPhase.Contracting;
        },
        [PlasmaRingPhase.Contracting] = (held, camera, ring) =>
        {
            ring.Radius = ComboRingCollapse.Grown(ring.Radius, -BeamComboDefinitions.PlasmaRadiusStep);
            if (ring.Radius < BeamComboDefinitions.PlasmaInnerThreshold) ring.Phase = PlasmaRingPhase.FinalExpansion;
        },
        [PlasmaRingPhase.FinalExpansion] = (held, camera, ring) =>
        {
            // Only the final expansion checks visibility. The first two phases
            // intentionally keep rings alive beyond the viewport, unlike Ice.
            if (LeftViewHorizontally(held, camera) || LeftViewVertically(held, camera))
                _animation.ClearProjectile(held.Projectile);
            else ring.Radius = ComboRingCollapse.Grown(ring.Radius, BeamComboDefinitions.PlasmaRadiusStep);
        },
    });

    /// <summary>The Plasma ring update: the ring grows, shrinks and grows again by its phase.</summary>
    /// <param name="samus">Samus, whom the ring surrounds.</param>
    /// <param name="held">The ring's shot.</param>
    /// <param name="ring">The ring's state, which its behaviour hands over (<see cref="PlasmaSpecialAttackRing"/>).</param>
    /// <param name="shared">The bombs, whose shared cooldown the ring holds.</param>
    /// <param name="camera">The camera's room position.</param>
    internal void StepPlasmaCombo(SamusState samus,
        SamusProjectileOccupant held, PlasmaParticleState ring, SamusBombProjectileSystem shared, Position16<RoomPixels> camera)
    {
        SamusProjectile shot = held.Projectile;
        if (shot.IsSpent)
        {
            _animation.ClearProjectile(shot);
            return;
        }
        shared.SetSharedCooldown(SamusProjectileCooldownDefinitions.SpecialAttackHold);
        _charge.ClearCount();
        Offset16<RoomPixels> orbit = SamusComboMechanics.GetSineOffset(ring.Angle, ring.Radius);
        // A Plasma ring grows as far as its outer radius round Samus, so near the room's top or left edge it passes above or left
        // of the room's origin, places OffRoomPlaces gives it.
        shot.Position = shot.Position.WithWhole(new(OffRoomPlaces.Place(orbit.X + samus.Position.X.Whole), OffRoomPlaces.Place(orbit.Y + samus.Position.Y.Whole)));
        ring.Turn();
        StepPlasmaComboForPlasmaRingPhase[ring.Phase](held, camera, ring);
    }
    /// <summary>
    /// Runs the Spacetime shot's behaviour, its progression overwrite: from an animation that reaches far enough, it
    /// turns saved progression on and off (<see cref="SpaceTimeProgressionOverwrite{TCursor, TOverwrite}"/>). Before the
    /// shot's first frame is installed, or with no animation, it leaves progression alone; only the overwrite of
    /// progression is kept (owner decision on #1318).
    /// </summary>
    /// <param name="progression">The progression the overwrite turns on and off.</param>
    /// <param name="held">The Spacetime shot.</param>
    internal void RunSpaceTimeProgressionOverwrite(SharedGameState progression, SamusProjectileOccupant held)
    {
        SamusProjectile shot = held.Projectile;
        Ensure.NotNull(progression);
        Ensure.NotNull(shot);
        if (shot.Sprite is null || shot.Playback is not { } playback)
            return;
        if (_spaceTimeOverwrite.For(SamusProjectilePlayback.BeatOf(playback), shot.Place) is { } overwrite)
            progression.Overwrite(overwrite);
    }
    /// <summary>Runs the Spazer combo, including paired-hit cleanup and retagging the primary particles as falling trails.</summary>
    internal SoundEffectId? StepSpazerCombo(SamusState samus,
        SamusProjectileOccupant held, SamusBombProjectileSystem shared, int cameraY)
    {
        SamusProjectile shot = held.Projectile;
        SpazerParticleState spazer = shot.KindStateAs<SpazerParticleState>();
        if (shot.IsSpent)
        {
            SamusProjectile partner = spazer.Partner;
            _animation.ClearProjectile(shot);
            if (spazer.Phase != SpazerComboPhase.Falling)
                // The spent particle ends, with itself, whatever shot holds its partner's place (SamusShotPool.HolderOfPlaceOf).
                _animation.ClearProjectile(_pool.HolderOfPlaceOf(partner));
            return null;
        }
        if (spazer.LeavesTrails)
            StepTrail(held);
        if (spazer.Phase == SpazerComboPhase.Circling)
        {
            PlaceSpazerParticle(samus, held, spazer);
            spazer.Turn();
            if (spazer.Angle == TrigonometryAngle.HalfTurn)
            {
                spazer.Radius = BeamComboDefinitions.SpazerFlyingStartRadius;
                spazer.TurnPerUpdate = SpazerTurn(spazer.Part.SpazerTurn, BeamComboDefinitions.SpazerFlyingTurn);
                shot.Direction = SamusProjectileDirection.UpFacingRight;
                shot.FlyOn();
                spazer.Phase = SpazerComboPhase.FlyingUpTowardsPoint;
            }
        }
        else if (spazer.Phase == SpazerComboPhase.FlyingUpTowardsPoint)
        {
            PlaceSpazerParticle(samus, held, spazer);
            if (CameraPastRoomEdge.FromCamera(shot.Position.Y.Whole, cameraY) < BeamComboDefinitions.SpazerFallScreenLine)
                BeginSpazerFall(held, spazer);
            else
            {
                spazer.Turn();
                // The radius narrows from its flying start by a step that divides it, so it meets zero exactly and never passes it.
                spazer.Radius -= BeamComboDefinitions.SpazerFlyingRadiusStep;
                if (spazer.Radius == 0)
                {
                    spazer.TurnPerUpdate = TrigonometryAngle.Zero - SpazerTurn(spazer.Part.SpazerTurn, BeamComboDefinitions.SpazerFlyingTurn);
                    spazer.Angle += TrigonometryAngle.HalfTurn;
                    spazer.Phase = SpazerComboPhase.FlyingUpAwayFromPoint;
                    shared.SetSharedCooldown(SamusProjectileCooldownDefinitions.SpecialAttackHold);
                    _charge.ClearCount();
                    return spazer.Part.VoicesSpazerConvergence ? SamusSound.SpazerSpecialAttackEnd : null;
                }
            }
        }
        else if (spazer.Phase == SpazerComboPhase.FlyingUpAwayFromPoint)
        {
            PlaceSpazerParticle(samus, held, spazer);
            if (CameraPastRoomEdge.FromCamera(shot.Position.Y.Whole, cameraY) < BeamComboDefinitions.SpazerFallScreenLine)
                BeginSpazerFall(held, spazer);
            else
            {
                spazer.Turn();
                spazer.Radius += BeamComboDefinitions.SpazerFlyingRadiusStep;
                if (spazer.Radius >= BeamComboDefinitions.SpazerFallingRadius) BeginSpazerFall(held, spazer);
            }
        }
        else
        {
            // The remaining member, SpazerComboPhase.Falling.
            shot.Position += new Offset16<RoomPixels>(0, BeamComboDefinitions.SpazerFallSpeed);
            // The particle falls from beside Samus, within a room's height of the view.
            if (CameraPastRoomEdge.FromCamera(shot.Position.Y.Whole, cameraY) >= BeamComboDefinitions.SpazerFallEndBelowCameraTop)
            {
                _animation.ClearProjectile(shot);
                return null;
            }
        }
        shared.SetSharedCooldown(SamusProjectileCooldownDefinitions.SpecialAttackHold);
        _charge.ClearCount();
        return null;
    }

    /// <summary>The part <paramref name="particle"/> plays in its special attack.</summary>
    private static SpecialAttackParticle PartOf(SamusProjectile particle) =>
        particle.KindStateAs<SpecialAttackParticleState>().Part;

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

    /// <summary>
    /// Places a flying Spazer particle on its circle: round Samus while it circles her, round the point above her once it
    /// flies up.
    /// </summary>
    private static void PlaceSpazerParticle(SamusState samus, SamusProjectileOccupant held, SpazerParticleState spazer)
    {
        SamusProjectile shot = held.Projectile;
        Offset16<RoomPixels> orbit = SamusComboMechanics.GetSineOffset(spazer.Angle, spazer.Radius);
        int centreHeight = spazer.Phase == SpazerComboPhase.Circling ? 0 : BeamComboDefinitions.SpazerFlightCentreHeight;
        // A Spazer particle circles Samus close in, then far out round a point above her, so near the room's
        // top or left edge it passes above or left of the room's origin, places OffRoomPlaces gives it.
        shot.Position = shot.Position.WithWhole(new(OffRoomPlaces.Place(samus.Position.X.Whole + orbit.X),
            OffRoomPlaces.Place(samus.Position.Y.Whole + orbit.Y - centreHeight)));
    }

    /// <summary>Moves a circling Spazer particle outward and switches its beam pair to falling echo artwork.</summary>
    private static void BeginSpazerFall(SamusProjectileOccupant held, SpazerParticleState spazer)
    {
        SamusProjectile shot = held.Projectile;
        // The particle can lie left of the room's origin as its flight ends (PlaceSpazerParticle), and the step out moves it
        // its side step either way, so its place is one OffRoomPlaces gives it.
        shot.Position = OffRoomPlaces.WithWholeX(shot.Position, shot.Position.X.Whole +
            BeamComboDefinitions.SpazerFallSideStep.Toward(spazer.Part.SpazerTrail ? HorizontalDirection.Left : HorizontalDirection.Right));
        shot.Direction = SamusProjectileDirection.DownFacingLeft;
        shot.FlyOn();
        shot.Trail = UpdateCountdown.Lasting(ProjectileTimingDefinitions.TrailInterval);
        spazer.Phase = SpazerComboPhase.Falling;
        if (!spazer.Part.SpazerTrail)
        {
            held.Type = SamusProjectileType.EchoOf(SamusBeamEcho.SpazerComboTrail);
            InitializeComboShot(held, BeamComboShotKind.Echo);
        }
    }
    /// <summary>
    /// Runs one update of a Wave special attack particle, which swings back and forth through Samus as it homes on her.
    /// </summary>
    /// <param name="samus">Samus, on whom the particle homes.</param>
    /// <param name="held">The particle's shot.</param>
    /// <param name="wave">The particle's state, which its behaviour hands over (<see cref="WaveSpecialAttackHoming"/>).</param>
    /// <param name="shared">The bombs, whose shared cooldown the particle holds.</param>
    internal SoundEffectId? StepWaveCombo(SamusState samus,
        SamusProjectileOccupant held, WaveParticleState wave, SamusBombProjectileSystem shared)
    {
        SamusProjectile shot = held.Projectile;
        // A hit skips even the lifetime decrement. Expiry and collision both queue
        // the terminal sound, unlike Ice's silent outward-phase removal.
        if (shot.IsSpent)
        {
            _animation.ClearProjectile(shot);
            return BeamComboDefinitions.WaveRemovalSound;
        }
        if (wave.Lifetime.Count())
        {
            _animation.ClearProjectile(shot);
            return BeamComboDefinitions.WaveRemovalSound;
        }
        SignedEightEight oldXSpeed = shot.Velocity.X;
        StepTrail(held);
        // A Wave particle starts its wave start away from Samus on each axis and swings through her as far again, ignoring
        // terrain, so near the room's top or left edge it lies above or left of the room's origin; it homes on her the
        // short way, as DistanceAcrossRoomEdge.Apart gives it.
        shot.Velocity = shot.Velocity.WithX(AccelerateWaveAxis(shot.Velocity.X, DistanceAcrossRoomEdge.Apart(samus.Position.X.Whole, shot.Position.X.Whole)));
        shot.Position = shot.Position.AdvanceX(shot.Velocity.X.ToSixteenSixteenDelta());
        shot.Velocity = shot.Velocity.WithY(AccelerateWaveAxis(shot.Velocity.Y, DistanceAcrossRoomEdge.Apart(samus.Position.Y.Whole, shot.Position.Y.Whole)));
        shot.Position = shot.Position.AdvanceY(shot.Velocity.Y.ToSixteenSixteenDelta());
        shared.SetSharedCooldown(SamusProjectileCooldownDefinitions.SpecialAttackHold);
        _charge.ClearCount();
        return wave.Part.VoicesWavePulse && oldXSpeed.IsNegative != shot.Velocity.X.IsNegative ? BeamComboDefinitions.WavePulseSound : null;
    }

    /// <summary>
    /// Moves one Wave velocity component toward Samus, respecting the acceleration and the speed bound either way. The
    /// speed stays within one acceleration of the bound.
    /// </summary>
    private static SignedEightEight AccelerateWaveAxis(SignedEightEight speed, int targetDelta)
    {
        if (targetDelta < 0)
            return speed > -BeamComboDefinitions.WaveSpeedLimit ? speed - BeamComboDefinitions.WaveAcceleration : speed;
        return speed < BeamComboDefinitions.WaveSpeedLimit ? speed + BeamComboDefinitions.WaveAcceleration : speed;
    }
}
