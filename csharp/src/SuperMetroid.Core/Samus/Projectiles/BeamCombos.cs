using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Samus.Poses;
namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>Beam combination attacks (charge plus special weapon): activation, and the ice, plasma, spazer and wave combo steps with the Spacetime palette copy tail.</summary>
internal sealed class BeamCombos
{
    /// <summary>Clears expired combo slots through the same projectile lifecycle as ordinary beams.</summary>
    private readonly ProjectileAnimation _animation;
    /// <summary>Resets Samus's charge flare while combo particles control shared firing state.</summary>
    private readonly BeamChargeState _charge;
    /// <summary>The shot pool whose first four places a special beam attack's particles take together.</summary>
    private readonly ProjectileSlotTable _slotTable;
    /// <summary>Spawns periodic Wave and other combo trails from their live particle slots.</summary>
    private readonly ProjectileTrails _trails;
    /// <summary>What the Spacetime shot's runaway palette copy overwrites in saved progression.</summary>
    private readonly SpaceTimeProgressionOverwrite<SamusProjectileSegmentBeat, ProgressionOverwrite> _spaceTimeOverwrite;

    /// <summary>Connects special beam activation and particle updates to shared projectile, charge, and trail owners.</summary>
    internal BeamCombos(ProjectileAnimation animation, BeamChargeState charge, ProjectileSlotTable slotTable, ProjectileTrails trails,
        SpaceTimeProgressionOverwrite<SamusProjectileSegmentBeat, ProgressionOverwrite> spaceTimeOverwrite)
    {
        _animation = animation;
        _charge = charge;
        _slotTable = slotTable;
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
    /// charge/input eligibility, and particles require their own frame behaviour updates.
    /// </summary>
    internal bool TryActivateCombo(SamusState samus,
        SamusBombProjectileSystem shared, out SoundEffectId? sound)
    {
        Ensure.NotNull(samus);
        Ensure.NotNull(shared);
        sound = null;
        if (samus.SelectedHudItem != HudItem.PowerBomb) return false;
        SamusBeamCombination beam = SamusBeamCombinations.Of(samus.EquippedBeams);
        // A Spazer and Plasma combination indexes the special-attack dispatch past its twelve entries, into the angles
        // the Ice and Plasma attacks start their slots at. Read as places to continue the attack, those angles lead into the game's live state, so the
        // game executes that state as code, which only full memory and processor emulation could reproduce: the
        // attack is rejected under the emulation scope limit (#1500).
        if (!beam.IsRetail)
            throw new NotSupportedException(
                "A Spazer and Plasma special attack runs the game's live state as code; reproducing it needs full memory " +
                "and processor emulation, which is outside the port's scope.");
        int remaining = samus.PowerBombs - SamusComboMechanics.GetPowerBombCost(beam);
        samus.PowerBombs = remaining < 0 ? 0 : remaining;
        SamusComboKind? combo = SamusComboMechanics.ComboKind(beam);
        // A live Ice or Plasma combo is not restarted: its first particle holds the first place.
        bool activated = combo is not null &&
            !(combo == SamusComboKind.Ice && _slotTable.Slots[0].FrameBehavior is SamusProjectileFrameBehavior.IceCombo or SamusProjectileFrameBehavior.IceComboOutward ||
              combo == SamusComboKind.Plasma && _slotTable.Slots[0].FrameBehavior == SamusProjectileFrameBehavior.PlasmaCombo);
        if (activated && combo is SamusComboKind kind)
        {
            // The attack's four particles take the first four places, the last first, replacing whatever projectiles
            // hold them; each continues from the sub-pixel position of the projectile it replaces (LeftMuzzleSubpixelCarry),
            // and plays the part its place gives it.
            IReadOnlyList<SpecialAttackParticle> parts = SpecialAttackParticleDefinitions.InPlaceOrder;
            var particles = new SamusProjectile[parts.Count];
            for (int place = parts.Count - 1; place >= 0; place--)
            {
                SpecialAttackParticle part = parts[place];
                SamusProjectile slot = _slotTable.Occupy(place);
                particles[place] = slot;
                slot.AttackParticle = part;
                slot.Type = SamusProjectileType.Beam(beam, charged: true);
                slot.PicturedBeams = beam;
                slot.Direction = kind == SamusComboKind.Spazer ? SamusProjectileDirection.DownFacingLeft : SamusProjectileDirection.UpFacingRight;
                slot.FrameBehavior = kind switch
                {
                    SamusComboKind.Wave => SamusProjectileFrameBehavior.WaveCombo,
                    SamusComboKind.Ice => SamusProjectileFrameBehavior.IceCombo,
                    SamusComboKind.Spazer => SamusProjectileFrameBehavior.SpazerCombo,
                    SamusComboKind.Plasma => SamusProjectileFrameBehavior.PlasmaCombo,
                    _ => throw new InvalidOperationException($"Undefined combo {kind}."),
                };
                // The Spazer beam pair's trail countdown starts at zero and so runs its whole wrap before it would
                // spawn a trail: only the trailing pair leaves trails until the attack falls.
                if (kind != SamusComboKind.Plasma) slot.TrailTimer = WrappingCountdown.FromRaw(kind == SamusComboKind.Spazer && !part.SpazerTrail ? 0 : 4);
                slot.ComboAngle = kind is SamusComboKind.Ice or SamusComboKind.Plasma
                    ? part.OrbitStart
                    : default;
                if (kind is SamusComboKind.Wave or SamusComboKind.Spazer)
                    slot.Position = slot.Position.WithFractionX(0).WithFractionY(0);
                if (kind is SamusComboKind.Spazer or SamusComboKind.Plasma) slot.ComboOrbitRadius = 40;
                if (kind == SamusComboKind.Spazer) slot.SpazerPhase = SpazerComboPhase.Circling;
                if (kind is SamusComboKind.Wave or SamusComboKind.Ice) slot.ComboLifetime = 600;
                if (kind == SamusComboKind.Spazer) slot.SpazerTurnPerUpdate = SpazerTurn(part.SpazerTurn, BeamComboDefinitions.SpazerCirclingTurn);
                if (kind == SamusComboKind.Plasma) slot.PlasmaPhase = PlasmaRingPhase.Expanding;
                if (kind == SamusComboKind.Wave)
                {
                    slot.Position = slot.Position.WithWhole(new(CoordinateAxis.Wrap(samus.Position.X.Whole + part.WaveStart.X), CoordinateAxis.Wrap(samus.Position.Y.Whole + part.WaveStart.Y)));
                }
                if (kind == SamusComboKind.Spazer && part.SpazerTrail) slot.Type = SamusProjectileType.EchoOf(SamusBeamEcho.SpazerComboTrail);
                InitializeComboData(slot, kind == SamusComboKind.Ice ? BeamComboData.Charged
                    : kind == SamusComboKind.Spazer && part.SpazerTrail ? BeamComboData.Echo : BeamComboData.Combo);
            }
            // Each Spazer beam particle pairs with the trailing particle that turns its way.
            if (kind == SamusComboKind.Spazer)
            {
                foreach (SamusProjectile particle in particles)
                    particle.SpazerPartner = particles.Single(other => !ReferenceEquals(other, particle) &&
                        PartOf(other).SpazerTurn == PartOf(particle).SpazerTurn);
            }
            _slotTable.ProjectileCounter = parts.Count;
            // The attack waits the cooldown of its first slot's projectile: the combination's charged shot.
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
            samus.SelectedHudItem = samus.AutoCancelHudItemIndex = HudItem.Beam;
        return activated;
    }

    /// <summary>Selects damage and animation data from the charged, combo, or Spazer echo tables and initializes its first frame.</summary>
    internal static void InitializeComboData(SamusProjectile slot, BeamComboData comboData)
    {
        SamusProjectileKind projectileKind = comboData switch
        {
            BeamComboData.Charged => SamusProjectileSelection.BeamShot(slot.Type.BeamCombination, charged: true),
            BeamComboData.Echo => SamusProjectileSelection.Echo(
                slot.Type.Echo ?? throw new InvalidOperationException("Echo data initializes an echo projectile.")),
            BeamComboData.Combo => SamusProjectileSelection.SpecialAttack(slot.Type.BeamCombination)
                ?? throw new InvalidDataException($"{slot.Type.BeamCombination}'s special attack has no particles of its own."),
            _ => throw new ArgumentOutOfRangeException(nameof(comboData), comboData, "Undefined combo data."),
        };
        slot.Damage = SamusProjectileDamage.Read(projectileKind);
        SamusProjectileSegment segment = comboData == BeamComboData.Combo
            ? SamusProjectileKinds.SoleAnimation(projectileKind)
            : SamusProjectileKinds.Animation(projectileKind, slot.Direction);
        slot.Playback = SamusProjectilePlayback.Start(segment);
        slot.FrameHold = WrappingCountdown.Start(1);
        if (comboData == BeamComboData.Charged)
            slot.Radius = SamusProjectilePlayback.InitialRadius(segment);
    }

    /// <summary>Runs one Ice combo slot and returns its optional Samus-library sound request.</summary>
    internal SoundEffectId? StepIceCombo(SamusState samus,
        SamusProjectile slot, SamusBombProjectileSystem shared, Position16<RoomPixels> camera)
    {
        bool outward = slot.FrameBehavior == SamusProjectileFrameBehavior.IceComboOutward;
        if (!outward && slot.FrameBehavior != SamusProjectileFrameBehavior.IceCombo)
            throw new InvalidOperationException("Ice combo update requires its own frame behaviour.");
        if (slot.IsSpent)
        {
            _animation.ClearProjectile(slot);
            return outward ? null : SamusSound.IceSpecialAttackEnd;
        }
        CountdownStep trailStep = slot.TrailTimer.Decrement();
        slot.TrailTimer = trailStep.Remaining;
        if (trailStep.IsZero)
        {
            slot.TrailTimer = WrappingCountdown.Start(4);
            _trails.SpawnTrail(slot);
        }
        int distance = outward ? slot.ComboOrbitRadius : 32;
        Offset16<RoomPixels> orbit = SamusComboMechanics.GetSineOffset(slot.ComboAngle, distance);
        slot.Position = slot.Position.WithWrappedWholeX(samus.Position.X.Whole + orbit.X);
        if (outward && (CoordinateAxis.AsSigned(slot.Position.X.Whole - camera.X + 32) < 0 ||
                        CoordinateAxis.Difference(slot.Position.X.Whole - camera.X, 288) >= 0))
        {
            _animation.ClearProjectile(slot);
            return null;
        }
        slot.Position = slot.Position.WithWrappedWholeY(samus.Position.Y.Whole + orbit.Y);
        if (outward && (CoordinateAxis.Difference(slot.Position.Y.Whole - camera.Y, 16) < 0 ||
                        CoordinateAxis.Difference(slot.Position.Y.Whole - camera.Y, 256) >= 0))
        {
            _animation.ClearProjectile(slot);
            return null;
        }
        slot.ComboAngle += OrbitTurnPerUpdate;
        SoundEffectId? sound = null;
        if (outward)
            slot.ComboOrbitRadius = SamusComboMechanics.GrownOrbitRadius(slot.ComboOrbitRadius, 8);
        else
        {
            // The lifetime starts at 600 and the particle turns outward as it leaves one, so it never passes zero.
            int oldLifetime = slot.ComboLifetime;
            slot.ComboLifetime = oldLifetime - 1;
            if (oldLifetime == 1)
            {
                slot.FrameBehavior = SamusProjectileFrameBehavior.IceComboOutward;
                slot.ComboOrbitRadius = 40;
                sound = SamusSound.IceSpecialAttackEnd;
            }
        }
        shared.SetSharedCooldown(2);
        _charge.ClearCount();
        return sound;
    }
    /// <summary>The Plasma ring update: the ring grows, shrinks and grows again by its phase.</summary>
    internal void StepPlasmaCombo(SamusState samus,
        SamusProjectile slot, SamusBombProjectileSystem shared, Position16<RoomPixels> camera)
    {
        if (slot.FrameBehavior != SamusProjectileFrameBehavior.PlasmaCombo)
            throw new InvalidOperationException("Plasma update requires its own frame behaviour.");
        if (slot.IsSpent)
        {
            _animation.ClearProjectile(slot);
            return;
        }
        shared.SetSharedCooldown(2);
        _charge.ClearCount();
        Offset16<RoomPixels> orbit = SamusComboMechanics.GetSineOffset(
            slot.ComboAngle, slot.ComboOrbitRadius);
        slot.Position = slot.Position.WithWhole(new(CoordinateAxis.Wrap(orbit.X + samus.Position.X.Whole), CoordinateAxis.Wrap(orbit.Y + samus.Position.Y.Whole)));
        slot.ComboAngle += OrbitTurnPerUpdate;
        switch (slot.PlasmaPhase)
        {
            case PlasmaRingPhase.Expanding:
                slot.ComboOrbitRadius = SamusComboMechanics.GrownOrbitRadius(slot.ComboOrbitRadius, BeamComboDefinitions.PlasmaRadiusStep);
                if (slot.ComboOrbitRadius >= BeamComboDefinitions.PlasmaOuterRadius) slot.PlasmaPhase = PlasmaRingPhase.Contracting;
                break;
            case PlasmaRingPhase.Contracting:
                slot.ComboOrbitRadius = SamusComboMechanics.GrownOrbitRadius(slot.ComboOrbitRadius, -BeamComboDefinitions.PlasmaRadiusStep);
                if (slot.ComboOrbitRadius < BeamComboDefinitions.PlasmaInnerThreshold) slot.PlasmaPhase = PlasmaRingPhase.FinalExpansion;
                break;
            case PlasmaRingPhase.FinalExpansion:
                // Only the final expansion checks visibility. The first two phases
                // intentionally keep rings alive beyond the viewport, unlike Ice.
                if (CoordinateAxis.AsSigned(slot.Position.X.Whole - camera.X + 32) < 0 ||
                    CoordinateAxis.Difference(slot.Position.X.Whole - camera.X, 288) >= 0 ||
                    CoordinateAxis.Difference(slot.Position.Y.Whole - camera.Y, 16) < 0 ||
                    CoordinateAxis.Difference(slot.Position.Y.Whole - camera.Y, 256) >= 0)
                    _animation.ClearProjectile(slot);
                else slot.ComboOrbitRadius = SamusComboMechanics.GrownOrbitRadius(slot.ComboOrbitRadius, BeamComboDefinitions.PlasmaRadiusStep);
                break;
            default: throw new InvalidOperationException($"Undefined {nameof(PlasmaRingPhase)} {slot.PlasmaPhase}.");
        }
    }
    /// <summary>
    /// Runs the Spacetime shot's callback, which falls back into the beam palette's copy loop and, from an animation that
    /// sends the copy far enough, copies on through saved progression (<see cref="SpaceTimeProgressionOverwrite{TCursor, TOverwrite}"/>). Before the
    /// shot's first frame is installed, or with no animation, the copy starts low and ends long before progression, and
    /// only the progression part of the copy is kept (owner decision on #1318).
    /// </summary>
    /// <param name="progression">The session's progression, which the copy overwrites; required when it reaches it.</param>
    /// <param name="slot">The Spacetime shot.</param>
    internal void RunSpacetimePaletteCopyTail(SharedGameState? progression, SamusProjectile slot)
    {
        Ensure.NotNull(slot);
        if (slot.Sprite is null || slot.Playback is not { } playback)
            return;
        if (_spaceTimeOverwrite.For(SamusProjectilePlayback.BeatOf(playback), slot.Place) is { } overwrite)
            (progression ?? throw new InvalidOperationException(
                "SpaceTime's palette-copy overwrite writes progression; the frame must pass it.")).Overwrite(overwrite);
    }
    /// <summary>Runs the Spazer combo, including paired-hit cleanup and retagging the primary particles as falling trails.</summary>
    internal SoundEffectId? StepSpazerCombo(SamusState samus,
        SamusProjectile slot, SamusBombProjectileSystem shared, int cameraY)
    {
        bool falling = slot.FrameBehavior == SamusProjectileFrameBehavior.SpazerComboFalling;
        if (!falling && slot.FrameBehavior != SamusProjectileFrameBehavior.SpazerCombo)
            throw new InvalidOperationException("Spazer update requires its own frame behaviour.");
        if (slot.IsSpent)
        {
            SamusProjectile partner = slot.SpazerPartner ?? throw new InvalidOperationException(
                $"The Spazer particle in place {slot.Place} has no partner.");
            _animation.ClearProjectile(slot);
            if (!falling)
                _animation.ClearProjectile(SpentSpazerParticleEndsPartnerPlace.ShotToEnd(partner, _slotTable.HolderOfPlaceOf));
            return null;
        }
        CountdownStep trailStep = slot.TrailTimer.Decrement();
        slot.TrailTimer = trailStep.Remaining;
        if (trailStep.IsZero)
        {
            slot.TrailTimer = WrappingCountdown.Start(4);
            _trails.SpawnTrail(slot);
        }
        if (falling)
        {
            slot.Position += new Offset16<RoomPixels>(0, 8);
            if (CoordinateAxis.Difference(slot.Position.Y.Whole - cameraY, 248) >= 0)
            {
                _animation.ClearProjectile(slot);
                return null;
            }
        }
        else
        {
            Offset16<RoomPixels> orbit = SamusComboMechanics.GetSineOffset(
                slot.ComboAngle, slot.ComboOrbitRadius);
            slot.Position = slot.Position.WithWhole(new(CoordinateAxis.Wrap(samus.Position.X.Whole + orbit.X), CoordinateAxis.Wrap(samus.Position.Y.Whole + orbit.Y - (slot.SpazerPhase == SpazerComboPhase.Circling ? 0 : 114))));
            switch (slot.SpazerPhase)
            {
                case SpazerComboPhase.Circling:
                    AdvanceSpazerAngle(slot);
                    if (slot.ComboAngle == TrigonometryAngle.HalfTurn)
                    {
                        slot.ComboOrbitRadius = 160;
                        slot.SpazerTurnPerUpdate = SpazerTurn(PartOf(slot).SpazerTurn, BeamComboDefinitions.SpazerFlyingTurn);
                        slot.Direction = SamusProjectileDirection.UpFacingRight;
                        slot.IsSpent = false;
                        slot.SpazerPhase = SpazerComboPhase.FlyingUpTowardsPoint;
                    }
                    break;
                case SpazerComboPhase.FlyingUpTowardsPoint:
                    if (CoordinateAxis.Difference(slot.Position.Y.Whole - cameraY, 16) < 0)
                        BeginSpazerFall(slot);
                    else
                    {
                        AdvanceSpazerAngle(slot);
                        // The radius falls from 160 by five an update, so it meets zero exactly and never passes it.
                        slot.ComboOrbitRadius -= 5;
                        if (slot.ComboOrbitRadius == 0)
                        {
                            slot.SpazerTurnPerUpdate = TrigonometryAngle.Zero - SpazerTurn(PartOf(slot).SpazerTurn, BeamComboDefinitions.SpazerFlyingTurn);
                            slot.ComboAngle += TrigonometryAngle.HalfTurn;
                            slot.SpazerPhase = SpazerComboPhase.FlyingUpAwayFromPoint;
                            shared.SetSharedCooldown(2);
                            _charge.ClearCount();
                            return PartOf(slot).VoicesSpazerConvergence ? SamusSound.SpazerSpecialAttackEnd : null;
                        }
                    }
                    break;
                case SpazerComboPhase.FlyingUpAwayFromPoint:
                    if (CoordinateAxis.Difference(slot.Position.Y.Whole - cameraY, 16) < 0)
                        BeginSpazerFall(slot);
                    else
                    {
                        AdvanceSpazerAngle(slot);
                        slot.ComboOrbitRadius += 5;
                        if (slot.ComboOrbitRadius >= 96) BeginSpazerFall(slot);
                    }
                    break;
                default: throw new InvalidOperationException($"Undefined {nameof(SpazerComboPhase)} {slot.SpazerPhase}.");
            }
        }
        shared.SetSharedCooldown(2);
        _charge.ClearCount();
        return null;
    }

    /// <summary>Turns the Spazer particle on round its circle by its turn per update.</summary>
    private static void AdvanceSpazerAngle(SamusProjectile slot) =>
        slot.ComboAngle += slot.SpazerTurnPerUpdate;

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
    private static void BeginSpazerFall(SamusProjectile slot)
    {
        slot.Position = slot.Position.WithWrappedWholeX(slot.Position.X.Whole + (PartOf(slot).SpazerTrail ? -16 : 16));
        slot.Direction = SamusProjectileDirection.DownFacingLeft;
        slot.IsSpent = false;
        slot.TrailTimer = WrappingCountdown.Start(4);
        slot.FrameBehavior = SamusProjectileFrameBehavior.SpazerComboFalling;
        if (!PartOf(slot).SpazerTrail)
        {
            slot.Type = SamusProjectileType.EchoOf(SamusBeamEcho.SpazerComboTrail);
            InitializeComboData(slot, BeamComboData.Echo);
        }
    }
    /// <summary>Runs the Wave combo's homing oscillator, preserving 8.8 acceleration and 16.16 position overflow.</summary>
    internal SoundEffectId? StepWaveCombo(SamusState samus,
        SamusProjectile slot, SamusBombProjectileSystem shared)
    {
        if (slot.FrameBehavior != SamusProjectileFrameBehavior.WaveCombo)
            throw new InvalidOperationException("Wave update requires its own frame behaviour.");
        // A hit skips even the lifetime decrement. Expiry and collision both queue
        // the terminal sound, unlike Ice's silent outward-phase removal.
        if (slot.IsSpent)
        {
            _animation.ClearProjectile(slot);
            return BeamComboDefinitions.WaveRemovalSound;
        }
        // The lifetime starts at 600 and the particle ends when it reaches zero, so it never passes zero.
        slot.ComboLifetime -= 1;
        if (slot.ComboLifetime <= 0)
        {
            _animation.ClearProjectile(slot);
            return BeamComboDefinitions.WaveRemovalSound;
        }
        SignedEightEight oldXSpeed = slot.Velocity.X;
        CountdownStep trailStep = slot.TrailTimer.Decrement();
        slot.TrailTimer = trailStep.Remaining;
        if (trailStep.IsZero)
        {
            slot.TrailTimer = WrappingCountdown.Start(4);
            _trails.SpawnTrail(slot);
        }
        slot.Velocity = slot.Velocity.WithX(AccelerateWaveAxis(slot.Velocity.X, CoordinateAxis.Difference(samus.Position.X.Whole, slot.Position.X.Whole)));
        slot.Position = slot.Position.AdvanceX(slot.Velocity.X.ToSixteenSixteenDelta());
        slot.WaveComboVerticalSpeed = AccelerateWaveAxis(slot.WaveComboVerticalSpeed, CoordinateAxis.Difference(samus.Position.Y.Whole, slot.Position.Y.Whole));
        slot.Position = slot.Position.AdvanceY(slot.WaveComboVerticalSpeed.ToSixteenSixteenDelta());
        shared.SetSharedCooldown(2);
        _charge.ClearCount();
        return PartOf(slot).VoicesWavePulse && oldXSpeed.IsNegative != slot.Velocity.X.IsNegative ? BeamComboDefinitions.WavePulseSound : null;
    }

    /// <summary>
    /// Moves one Wave velocity component toward Samus, respecting the acceleration and signed speed bound. The speed
    /// stays within one acceleration of the bound either way, far inside the 8.8 range.
    /// </summary>
    private static SignedEightEight AccelerateWaveAxis(SignedEightEight speed, int targetDelta)
    {
        if (targetDelta < 0)
            return speed > -BeamComboDefinitions.WaveSpeedLimit ? speed - BeamComboDefinitions.WaveAcceleration : speed;
        return speed < BeamComboDefinitions.WaveSpeedLimit ? speed + BeamComboDefinitions.WaveAcceleration : speed;
    }
}
