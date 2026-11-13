using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
namespace SuperMetroid.Verification.Samus.Projectiles;

/// <summary>
/// Confirms that the special beam attacks' particles play the parts the original gave them by projectile place
/// (#1683): each part is set when the attack starts, and the attack's whole run matches the original's place rules,
/// which this check states as its oracle: an Ice or Plasma particle starts a quarter turn per place round its orbit, a
/// Wave particle starts in the corner its place names, an even-placed Spazer particle turns clockwise and an odd one
/// counterclockwise, the last two places are the trailing pair, a spent Spazer particle ends the shot two places from
/// it, and only the first Spazer place and the last Wave place make their sounds.
/// </summary>
internal static class SpecialAttackParticlesVerification
{
    /// <summary>The camera Y the Spazer run uses: far enough above Samus that no particle reaches the top of the view.</summary>
    private const int SpazerCameraY = 0;

    /// <summary>Samus's position for the Spazer run.</summary>
    private static readonly Position16<RoomPixels> SpazerSamus = new(128, 300);

    /// <summary>Samus's position for the spawn and Wave checks.</summary>
    private static readonly Position16<RoomPixels> CentredSamus = new(128, 128);

    /// <summary>Runs every special-attack part check.</summary>
    internal static void VerifySpecialAttackParticleParts()
    {
        Suite(nameof(VerifySpawnedParts), VerifySpawnedParts);
        Suite(nameof(VerifySpazerRun), VerifySpazerRun);
        Suite(nameof(VerifySpentSpazerEndsPartnerPlace), VerifySpentSpazerEndsPartnerPlace);
        Suite(nameof(VerifyWaveRun), VerifyWaveRun);
        Console.WriteLine("Special-attack particles: spawn parts, the whole Spazer and Wave runs and the spent-partner rule match the original's place rules.");
    }

    /// <summary>Checks every particle's part as each of the four attacks starts.</summary>
    private static void VerifySpawnedParts()
    {
        foreach (SamusBeamFlags beam in new[] { SamusBeamFlags.Wave, SamusBeamFlags.Ice, SamusBeamFlags.Spazer, SamusBeamFlags.Plasma })
        {
            (SamusProjectileSystem projectiles, _, SamusState samus) = StartAttack(beam, CentredSamus);
            AssertEqual(4, projectiles.SlotTable.ProjectileCounter, $"{beam} attack counts four shots");
            for (int place = 0; place < 4; place++)
            {
                SamusProjectile particle = projectiles.Slots[place];
                AssertEqual(SpecialAttackParticleDefinitions.InPlaceOrder[place], particle.AttackParticle,
                    $"{beam} particle {place} plays its place's part");
                AssertEqual(beam is SamusBeamFlags.Ice or SamusBeamFlags.Plasma ? new TrigonometryAngle(64 * place) : default,
                    particle.ComboAngle, $"{beam} particle {place} starts its orbit a quarter turn per place");
                if (beam == SamusBeamFlags.Wave)
                    AssertEqual(new Position16<RoomPixels>(samus.Position.X.Whole + (place < 2 ? 128 : -128),
                            samus.Position.Y.Whole + (place is 0 or 3 ? 128 : -128)),
                        particle.Position.Whole, $"Wave particle {place} starts in its place's corner");
                if (beam != SamusBeamFlags.Spazer)
                {
                    AssertTrue(particle.SpazerPartner is null, $"{beam} particle {place} has no Spazer partner");
                    continue;
                }
                AssertEqual(TrigonometryAngle.Wrapped(OriginalSpazerTurn(place) * 4), particle.SpazerTurnPerUpdate,
                    $"Spazer particle {place} circles the way its place's parity turns");
                AssertEqual(place >= 2, particle.Type == SamusProjectileType.EchoOf(SamusBeamEcho.SpazerComboTrail),
                    $"Spazer particle {place} is a trail exactly when it holds one of the last two places");
                AssertEqual(place < 2 ? 0 : 4, (int)particle.TrailTimer.RawValue,
                    $"Spazer particle {place} starts its trail countdown as its place's pair does");
                AssertTrue(ReferenceEquals(projectiles.Slots[place ^ 2], particle.SpazerPartner),
                    $"Spazer particle {place} is paired with the particle two places from it");
            }
        }
    }

    /// <summary>
    /// Runs a whole Spazer attack, every live particle each update in update order, and checks each update's angle steps,
    /// sounds and fall steps against the original's place rules, through every phase until the last particle ends.
    /// </summary>
    private static void VerifySpazerRun()
    {
        (SamusProjectileSystem projectiles, SamusBombProjectileSystem shared, SamusState samus) =
            StartAttack(SamusBeamFlags.Spazer, SpazerSamus);
        var phasesSeen = new HashSet<SpazerComboPhase>();
        int convergenceSounds = 0;
        int falls = 0;
        int update = 0;
        for (; update < 400 && projectiles.Slots.Any(slot => slot.IsActive); update++)
        {
            foreach (SamusProjectile particle in projectiles.SlotTable.InUpdateOrder.ToArray())
            {
                if (!particle.IsActive)
                    continue;
                int place = particle.Place;
                bool wasFalling = particle.FrameBehavior == SamusProjectileFrameBehavior.SpazerComboFalling;
                TrigonometryAngle angleBefore = particle.ComboAngle;
                int radiusBefore = particle.ComboOrbitRadius;
                SoundEffectId? sound = projectiles.Combos.StepSpazerCombo(samus, particle, shared, SpazerCameraY);
                if (sound is not null)
                {
                    AssertEqual(SamusSound.SpazerSpecialAttackEnd, sound.Value, $"update {update} Spazer sound");
                    AssertEqual(0, place, $"update {update}: only the first place voices the Spazer convergence");
                    convergenceSounds++;
                }
                if (!particle.IsActive)
                    continue;
                bool falling = particle.FrameBehavior == SamusProjectileFrameBehavior.SpazerComboFalling;
                if (falling)
                {
                    AssertEqual(SamusProjectileType.EchoOf(SamusBeamEcho.SpazerComboTrail), particle.Type,
                        $"update {update}: falling Spazer particle {place} is drawn as a trail");
                    if (!wasFalling)
                    {
                        falls++;
                        Offset16<RoomPixels> orbit = SamusComboMechanics.GetSineOffset(angleBefore, radiusBefore);
                        AssertEqual((int)CoordinateAxis.Wrap(samus.Position.X.Whole + orbit.X + (place < 2 ? 16 : -16)),
                            (int)particle.Position.X.Whole,
                            $"update {update}: Spazer particle {place} steps out its place's pair's way as it falls");
                    }
                    continue;
                }
                phasesSeen.Add(particle.SpazerPhase);
                int expectedStep = particle.SpazerPhase switch
                {
                    SpazerComboPhase.Circling => OriginalSpazerTurn(place) * 4,
                    SpazerComboPhase.FlyingUpTowardsPoint => OriginalSpazerTurn(place) * 2,
                    SpazerComboPhase.FlyingUpAwayFromPoint => -OriginalSpazerTurn(place) * 2,
                    _ => throw new InvalidOperationException($"Undefined {nameof(SpazerComboPhase)} {particle.SpazerPhase}."),
                };
                AssertEqual(TrigonometryAngle.Wrapped(expectedStep), particle.SpazerTurnPerUpdate,
                    $"update {update}: Spazer particle {place} turns its place's way in {particle.SpazerPhase}");
            }
        }
        AssertTrue(projectiles.Slots.All(slot => !slot.IsActive), $"the Spazer attack ends within {update} updates");
        AssertEqual(0, projectiles.SlotTable.ProjectileCounter, "every ended Spazer particle uncounts its shot");
        AssertEqual(1, convergenceSounds, "the Spazer convergence sounds once");
        AssertEqual(4, falls, "every Spazer particle falls");
        foreach (SpazerComboPhase phase in Enum.GetValues<SpazerComboPhase>())
            AssertTrue(phasesSeen.Contains(phase), $"the Spazer run passes through {phase}");
    }

    /// <summary>
    /// Checks a spent Spazer particle ends the shot two places from it with itself, as the original does: its partner,
    /// for a beam and a trailing particle alike, or a crash echo that has replaced the partner in its place.
    /// </summary>
    private static void VerifySpentSpazerEndsPartnerPlace()
    {
        foreach (int spentPlace in new[] { 0, 1, 2, 3 })
        {
            (SamusProjectileSystem projectiles, SamusBombProjectileSystem shared, SamusState samus) =
                StartAttack(SamusBeamFlags.Spazer, SpazerSamus);
            SamusProjectile spent = projectiles.Slots[spentPlace];
            spent.IsSpent = true;
            AssertTrue(projectiles.Combos.StepSpazerCombo(samus, spent, shared, SpazerCameraY) is null,
                $"spent Spazer particle {spentPlace} makes no sound");
            for (int place = 0; place < 4; place++)
                AssertEqual(place != spentPlace && place != (spentPlace ^ 2), projectiles.Slots[place].IsOccupied,
                    $"spent Spazer particle {spentPlace} ends itself and the shot two places from it, not place {place}");
            AssertEqual(2, projectiles.SlotTable.ProjectileCounter, $"spent Spazer particle {spentPlace} uncounts two shots");
        }

        (SamusProjectileSystem echoProjectiles, SamusBombProjectileSystem echoShared, SamusState echoSamus) =
            StartAttack(SamusBeamFlags.Spazer, SpazerSamus);
        SamusProjectile replaced = echoProjectiles.Slots[3];
        SamusProjectile echo = echoProjectiles.SlotTable.Occupy(3);
        echo.Type = SamusProjectileType.EchoOf(SamusBeamEcho.ShinesparkEcho);
        echo.Damage = 1;
        echo.FrameBehavior = SamusProjectileFrameBehavior.ShinesparkEcho;
        AssertTrue(!replaced.IsOccupied, "the crash echo's release ends the trailing particle it replaces");
        SamusProjectile spentBeam = echoProjectiles.Slots[1];
        spentBeam.IsSpent = true;
        _ = echoProjectiles.Combos.StepSpazerCombo(echoSamus, spentBeam, echoShared, SpazerCameraY);
        AssertTrue(!echo.IsOccupied, "a spent Spazer particle ends the crash echo holding its partner's place");
        AssertTrue(ReferenceEquals(echo, echoProjectiles.Slots[3]), "the ended echo is the last shot of its place");
        AssertTrue(echoProjectiles.Slots[0].IsOccupied && echoProjectiles.Slots[2].IsOccupied,
            "the other Spazer pair flies on");
        AssertEqual(2, echoProjectiles.SlotTable.ProjectileCounter, "the spent particle and the echo uncount two shots");
    }

    /// <summary>
    /// Runs a whole Wave attack, every live particle each update in update order, and checks every sound against the
    /// original's rule: only the last place pulses, each time its horizontal speed changes sign, and every particle
    /// makes the removal sound as its lifetime ends.
    /// </summary>
    private static void VerifyWaveRun()
    {
        (SamusProjectileSystem projectiles, SamusBombProjectileSystem shared, SamusState samus) =
            StartAttack(SamusBeamFlags.Wave, CentredSamus);
        int pulses = 0;
        int removals = 0;
        for (int update = 0; update < 600; update++)
        {
            foreach (SamusProjectile particle in projectiles.SlotTable.InUpdateOrder.ToArray())
            {
                if (!particle.IsActive)
                    continue;
                int place = particle.Place;
                bool wasNegative = particle.Velocity.X.IsNegative;
                SoundEffectId? sound = projectiles.Combos.StepWaveCombo(samus, particle, shared);
                SoundEffectId? expected = !particle.IsActive
                    ? BeamComboDefinitions.WaveRemovalSound
                    : place == 3 && wasNegative != particle.Velocity.X.IsNegative
                        ? BeamComboDefinitions.WavePulseSound
                        : null;
                AssertEqual(expected, sound, $"update {update}: Wave particle {place}'s sound");
                if (!particle.IsActive)
                    removals++;
                else if (sound is not null)
                    pulses++;
            }
        }
        AssertTrue(pulses > 0, "the last Wave particle pulses as it swings past Samus");
        AssertEqual(4, removals, "every Wave particle's lifetime ends");
        AssertEqual(0, projectiles.SlotTable.ProjectileCounter, "every ended Wave particle uncounts its shot");
    }

    /// <summary>The original's Spazer turn by place: plus one, clockwise, for an even place, minus one for an odd.</summary>
    private static int OriginalSpazerTurn(int place) => (place & 1) == 0 ? 1 : -1;

    /// <summary>Starts <paramref name="beam"/>'s special attack with Samus facing right at <paramref name="samusAt"/>.</summary>
    private static (SamusProjectileSystem Projectiles, SamusBombProjectileSystem Shared, SamusState Samus) StartAttack(
        SamusBeamFlags beam, Position16<RoomPixels> samusAt)
    {
        var projectiles = new SamusProjectileSystem(new BeamChargeState(), InstalledFixtures.SamusOverreads,
            new LeftMuzzleSubpixelCarry(SamusProjectileSystem.SlotCount));
        var shared = new SamusBombProjectileSystem();
        var samus = new SamusState(InstalledFixtures.AnimationContinuation, new UnauthoredBoostCadence(),
            SamusCompatibility.ForNewSamus(), new BeamChargeState())
        {
            Pose = SamusPoseId.FacingRightNormalPose,
            Position = FixedPosition<RoomPixels>.FromWhole(samusAt),
            EquippedBeams = beam,
            SelectedHudItem = HudItem.PowerBomb,
            PowerBombs = 10,
        };
        AssertTrue(projectiles.Combos.TryActivateCombo(samus, shared, out _), $"{beam} special attack starts");
        return (projectiles, shared, samus);
    }
}
