using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Artwork;
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
        Suite(nameof(VerifyLeadParticleHoldsOffOrbitingAttack), VerifyLeadParticleHoldsOffOrbitingAttack);
        Suite(nameof(VerifySpecialAttackReplacesFirstShots), VerifySpecialAttackReplacesFirstShots);
        Console.WriteLine("Special-attack particles: spawn parts, the whole Spazer and Wave runs, the spent-partner rule, the lead-particle hold and the replacement of the first shots match the original's place rules.");
    }

    /// <summary>What one of Samus's first four shot places holds as a special attack starts.</summary>
    private enum PlaceHolding
    {
        /// <summary>No shot has been fired into the place.</summary>
        NeverFired,

        /// <summary>A shot held the place and was deleted from it.</summary>
        Ended,

        /// <summary>A shot held the place when Samus's shots were cleared all at once; the place keeps its sub-pixel position.</summary>
        KeptByClear,

        /// <summary>An uncharged beam still flying: the charge's first shot.</summary>
        Beam,

        /// <summary>A charged beam still flying.</summary>
        ChargedBeam,

        /// <summary>A beam that struck something this update and has not yet run its next update.</summary>
        SpentBeam,

        /// <summary>A beam's explosion still playing.</summary>
        BeamExplosion,

        /// <summary>An ignited missile.</summary>
        Missile,

        /// <summary>An ignited Super Missile whose link holds the fifth place.</summary>
        SuperMissile,

        /// <summary>A Super Missile's link whose Super Missile holds the fifth place.</summary>
        SuperMissileLink,

        /// <summary>A shinespark crash echo.</summary>
        CrashEcho,
    }

    /// <summary>One shot's whole state, its Spazer partner by the place it holds.</summary>
    private sealed record ShotState(bool IsOccupied, int Damage, SamusProjectileType Type, SamusBeamCombination PicturedBeams,
        SamusProjectileDirection Direction, bool IsSpent, FixedPosition<RoomPixels> Position, EightEightVector<RoomPixels> Velocity,
        Extent16<RoomPixels> Radius, AnimationPlayback<SamusProjectileSegment>? Playback, WrappingCountdown FrameHold,
        ProjectileSpriteId? Sprite, WrappingCountdown TrailTimer, SamusMissileIgnition MissileIgnition, int? SuperMissileLinkPlace,
        TrigonometryAngle ComboAngle, SignedEightEight WaveComboVerticalSpeed, int ComboOrbitRadius, int ComboLifetime,
        PlasmaRingPhase PlasmaPhase, TrigonometryAngle SpazerTurnPerUpdate, SpazerComboPhase SpazerPhase,
        SpecialAttackParticle? AttackParticle, int? PartnerPlace, int EchoRadius, TrigonometryAngle EchoAngle,
        WrappingCountdown ChainsawPowerBombFuse, SamusProjectileFrameBehavior FrameBehavior);

    /// <summary>Reads <paramref name="shot"/>'s whole state.</summary>
    private static ShotState StateOf(SamusProjectile shot) => new(shot.IsOccupied, shot.Damage, shot.Type, shot.PicturedBeams,
        shot.Direction, shot.IsSpent, shot.Position, shot.Velocity, shot.Radius, shot.Playback, shot.FrameHold, shot.Sprite,
        shot.TrailTimer, shot.MissileIgnition, shot.SuperMissileLinkPlace, shot.ComboAngle, shot.WaveComboVerticalSpeed,
        shot.ComboOrbitRadius, shot.ComboLifetime, shot.PlasmaPhase, shot.SpazerTurnPerUpdate, shot.SpazerPhase,
        shot.AttackParticle, shot.SpazerPartner?.Place, shot.EchoRadius, shot.EchoAngle, shot.ChainsawPowerBombFuse,
        shot.FrameBehavior);

    /// <summary>
    /// Checks the replacement of the first shots (<see cref="SpecialAttackReplacesFirstShots"/>) against the original's
    /// starts, which write each particle into its place without testing or deleting what the place holds: for each of the
    /// four attacks, with each of the first four places holding each <see cref="PlaceHolding"/> and the fifth a flying
    /// beam, the attack starts as on an empty pool, ends the shot it replaces, sets off particles identical to the empty
    /// pool's but for the horizontal sub-pixel position an Ice or Plasma particle carries from the shot or the clear it
    /// replaces, makes the empty pool's sound, counts four shots, and leaves the fifth place's shot as it was.
    /// </summary>
    private static void VerifySpecialAttackReplacesFirstShots()
    {
        int checkedStarts = 0;
        foreach (SamusBeamFlags beam in AttackBeams)
        {
            (SamusProjectileSystem empty, SamusBombProjectileSystem emptyShared, SamusState emptySamus) = NewPool(beam, CentredSamus);
            AssertTrue(empty.Combos.TryActivateCombo(emptySamus, emptyShared, out SoundEffectId? emptySound), $"{beam} attack starts on an empty pool");
            ShotState[] particlesOnEmptyPool = [.. empty.Slots.Take(4).Select(StateOf)];
            bool carriesFraction = beam is SamusBeamFlags.Ice or SamusBeamFlags.Plasma;
            for (int place = 0; place < 4; place++)
            foreach (PlaceHolding holding in Enum.GetValues<PlaceHolding>())
            {
                (SamusProjectileSystem projectiles, SamusBombProjectileSystem shared, SamusState samus) = NewPool(beam, CentredSamus);
                int fraction = 0x1234 + 0x0F0F * place;
                int carried = Hold(projectiles, place, holding, fraction);
                SamusProjectile[] before = [.. projectiles.Slots];
                ShotState fifthBefore = StateOf(projectiles.Slots[4]);
                string name = $"{beam} attack over place {place} holding {holding}";
                AssertTrue(projectiles.Combos.TryActivateCombo(samus, shared, out SoundEffectId? sound), $"{name} starts");
                AssertEqual(emptySound, sound, $"{name} makes the empty pool's sound");
                AssertEqual(4, projectiles.SlotTable.ProjectileCounter, $"{name} counts four shots");
                for (int particle = 0; particle < 4; particle++)
                {
                    AssertTrue(!ReferenceEquals(before[particle], projectiles.Slots[particle]) && !before[particle].IsOccupied,
                        $"{name}: place {particle}'s earlier shot ends");
                    int expectedFraction = carriesFraction && particle == place ? carried : 0;
                    ShotState expected = particlesOnEmptyPool[particle] with
                    {
                        Position = particlesOnEmptyPool[particle].Position.WithFractionX(expectedFraction),
                    };
                    AssertEqual(expected, StateOf(projectiles.Slots[particle]),
                        $"{name}: particle {particle} is the empty pool's but for the carried sub-pixel position");
                }
                AssertTrue(ReferenceEquals(before[4], projectiles.Slots[4]), $"{name}: the fifth place's shot flies on");
                AssertEqual(fifthBefore, StateOf(projectiles.Slots[4]), $"{name}: the fifth place's shot is untouched");
                checkedStarts++;
            }
        }
        Console.WriteLine($"  First-shot replacement: {checkedStarts} starts over every holding of the first four places match the original's starts.");
    }

    /// <summary>
    /// Leaves <paramref name="place"/> holding <paramref name="holding"/>, and the fifth place a flying beam or the Super
    /// Missile partner the holding names. Returns the horizontal sub-pixel position the place hands its next projectile.
    /// </summary>
    private static int Hold(SamusProjectileSystem projectiles, int place, PlaceHolding holding, int fraction)
    {
        const int FifthPlace = 4;
        switch (holding)
        {
            case PlaceHolding.NeverFired:
                break;
            case PlaceHolding.Ended:
                projectiles.Animation.ClearProjectile(FlyingBeam(projectiles, place, fraction, charged: false));
                break;
            case PlaceHolding.KeptByClear:
                _ = FlyingBeam(projectiles, place, fraction, charged: false);
                projectiles.SlotTable.ClearAll();
                break;
            case PlaceHolding.Beam:
                _ = FlyingBeam(projectiles, place, fraction, charged: false);
                break;
            case PlaceHolding.ChargedBeam:
                _ = FlyingBeam(projectiles, place, fraction, charged: true);
                break;
            case PlaceHolding.SpentBeam:
                FlyingBeam(projectiles, place, fraction, charged: false).IsSpent = true;
                break;
            case PlaceHolding.BeamExplosion:
                SamusProjectile exploding = FlyingBeam(projectiles, place, fraction, charged: false);
                exploding.Type = exploding.Type.AsFamily(SamusProjectileFamily.BeamExplosion);
                exploding.Playback = SamusProjectilePlayback.Start(SamusProjectileSegment.BeamExplosion);
                exploding.FrameHold = WrappingCountdown.Start(1);
                exploding.Damage = 8;
                exploding.FrameBehavior = SamusProjectileFrameBehavior.None;
                break;
            case PlaceHolding.Missile:
                _ = IgnitedMissile(projectiles, place, fraction, SamusProjectileFamily.Missile, SamusProjectileFrameBehavior.Missile);
                break;
            case PlaceHolding.SuperMissile:
                IgnitedMissile(projectiles, place, fraction, SamusProjectileFamily.SuperMissile, SamusProjectileFrameBehavior.SuperMissile)
                    .SuperMissileLinkPlace = FifthPlace;
                _ = SuperMissileLink(projectiles, FifthPlace, fraction);
                return fraction;
            case PlaceHolding.SuperMissileLink:
                _ = SuperMissileLink(projectiles, place, fraction);
                IgnitedMissile(projectiles, FifthPlace, fraction, SamusProjectileFamily.SuperMissile, SamusProjectileFrameBehavior.SuperMissile)
                    .SuperMissileLinkPlace = place;
                return fraction;
            case PlaceHolding.CrashEcho:
                SamusProjectile echo = projectiles.SlotTable.Occupy(place);
                Fly(echo, place, fraction);
                echo.Type = SamusProjectileType.EchoOf(SamusBeamEcho.ShinesparkEcho);
                BeamCombos.InitializeComboData(echo, BeamComboData.Echo);
                echo.FrameBehavior = SamusProjectileFrameBehavior.ShinesparkEcho;
                echo.EchoAngle = new TrigonometryAngle(32);
                echo.EchoRadius = 24;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(holding), holding, $"Undefined {nameof(PlaceHolding)}.");
        }
        _ = FlyingBeam(projectiles, FifthPlace, fraction + 1, charged: false);
        return holding is PlaceHolding.NeverFired or PlaceHolding.Ended ? 0 : fraction;
    }

    /// <summary>Puts a flying uncharged or charged Power Beam shot in <paramref name="place"/>.</summary>
    private static SamusProjectile FlyingBeam(SamusProjectileSystem projectiles, int place, int fraction, bool charged)
    {
        SamusProjectile shot = projectiles.SlotTable.Occupy(place);
        Fly(shot, place, fraction);
        shot.Type = SamusProjectileType.Beam(SamusBeamCombination.Power, charged);
        shot.Damage = SamusProjectileDamage.Read(SamusProjectileSelection.BeamShot(SamusBeamCombination.Power, charged));
        SamusProjectileSegment? segment = SamusProjectileSelection.BeamShotAnimation(SamusBeamCombination.Power, charged, shot.Direction);
        shot.Playback = SamusProjectilePlayback.Start(segment);
        shot.Radius = SamusProjectilePlayback.InitialRadius(segment);
        shot.FrameBehavior = SamusProjectileFrameBehavior.NoWaveBeam;
        return shot;
    }

    /// <summary>Puts an ignited missile or Super Missile in <paramref name="place"/>.</summary>
    private static SamusProjectile IgnitedMissile(SamusProjectileSystem projectiles, int place, int fraction,
        SamusProjectileFamily family, SamusProjectileFrameBehavior behavior)
    {
        SamusProjectile missile = projectiles.SlotTable.Occupy(place);
        Fly(missile, place, fraction);
        SamusProjectileKind kind = SamusProjectileSelection.NonBeam(family);
        missile.Type = SamusProjectileType.Of(family);
        missile.Damage = SamusProjectileDamage.Read(kind);
        SamusProjectileSegment segment = SamusProjectileKinds.Animation(kind, missile.Direction);
        missile.Playback = SamusProjectilePlayback.Start(segment);
        missile.Radius = SamusProjectilePlayback.InitialRadius(segment);
        missile.MissileIgnition = SamusMissileIgnition.Ignited;
        missile.FrameBehavior = behavior;
        return missile;
    }

    /// <summary>Puts a Super Missile's link in <paramref name="place"/>.</summary>
    private static SamusProjectile SuperMissileLink(SamusProjectileSystem projectiles, int place, int fraction)
    {
        SamusProjectile link = projectiles.SlotTable.OccupyAtRest(place);
        Fly(link, place, fraction);
        link.Type = SamusProjectileType.Of(SamusProjectileFamily.SuperMissile);
        link.Damage = SamusProjectileDamage.Read(SamusProjectileKind.SuperMissileLink);
        link.Playback = SamusProjectilePlayback.Start(SamusProjectileKinds.SoleAnimation(SamusProjectileKind.SuperMissileLink));
        link.FrameBehavior = SamusProjectileFrameBehavior.SuperMissileLink;
        link.SuperMissileLinkPlace = UnlinkedSuperMissileLink.Place;
        return link;
    }

    /// <summary>Gives a shot a flight of its own: somewhere in the room, moving right, mid-trail and mid-picture.</summary>
    private static void Fly(SamusProjectile shot, int place, int fraction)
    {
        shot.PicturedBeams = SamusBeamCombination.Power;
        shot.Direction = SamusProjectileDirection.Right;
        shot.Position = FixedPosition<RoomPixels>.FromWhole(new Position16<RoomPixels>(200 + place, 90 + place))
            .WithFractionX(fraction).WithFractionY(0x4321);
        shot.Velocity = EightEightVector<RoomPixels>.FromTwoHundredFiftySixthsOfPixel(1200, -300);
        shot.FrameHold = WrappingCountdown.Start(3);
        shot.TrailTimer = WrappingCountdown.Start(2);
    }

    /// <summary>The four beams that start a special attack.</summary>
    private static readonly SamusBeamFlags[] AttackBeams =
        [SamusBeamFlags.Wave, SamusBeamFlags.Ice, SamusBeamFlags.Spazer, SamusBeamFlags.Plasma];

    /// <summary>
    /// The states a running attack can leave its four places in before another attack is tried: every particle running,
    /// the Ice particles turned outward, the lead particle ended with the rest running, another particle ended with the
    /// lead running, every particle ended, and the lead's place taken by an ordinary shot once it ended.
    /// </summary>
    private enum AttackLeftState
    {
        /// <summary>Every particle still runs.</summary>
        AllRunning,

        /// <summary>Every Ice particle has turned outward; other attacks' particles run on.</summary>
        TurnedOutward,

        /// <summary>The lead particle has ended; the rest run on.</summary>
        LeadEnded,

        /// <summary>A particle other than the lead has ended; the lead runs on.</summary>
        OtherEnded,

        /// <summary>Every particle has ended.</summary>
        AllEnded,

        /// <summary>The lead particle has ended and an ordinary shot has taken its place.</summary>
        LeadPlaceReshot,
    }

    /// <summary>
    /// Checks the lead-particle hold (<see cref="LeadParticleHoldsOffOrbitingAttack"/>) against the original's start test,
    /// which refuses an Ice attack while the first projectile place runs an Ice particle's update, inward or outward, and a
    /// Plasma attack while it runs the Plasma ring's: after each of the four attacks, in each state it can leave its
    /// places in, each of the four attacks is tried, and it starts exactly when the original's test passes, replacing the
    /// four places.
    /// </summary>
    private static void VerifyLeadParticleHoldsOffOrbitingAttack()
    {
        int refused = 0;
        int started = 0;
        foreach (SamusBeamFlags first in AttackBeams)
        foreach (AttackLeftState left in Enum.GetValues<AttackLeftState>())
        foreach (SamusBeamFlags next in AttackBeams)
        {
            (SamusProjectileSystem projectiles, SamusBombProjectileSystem shared, SamusState samus) = StartAttack(first, CentredSamus);
            Leave(projectiles, left);
            SamusProjectileFrameBehavior firstPlace = projectiles.Slots[0].FrameBehavior;
            bool originalStarts = !(next == SamusBeamFlags.Ice && firstPlace is SamusProjectileFrameBehavior.IceCombo or SamusProjectileFrameBehavior.IceComboOutward ||
                next == SamusBeamFlags.Plasma && firstPlace == SamusProjectileFrameBehavior.PlasmaCombo);
            SamusProjectile[] before = [.. projectiles.Slots.Take(4)];
            samus.EquippedBeams = next;
            string name = $"{next} after {first} left {left}";
            AssertEqual(originalStarts, projectiles.Combos.TryActivateCombo(samus, shared, out _), $"{name} starts as the first place's test decides");
            for (int place = 0; place < 4; place++)
                AssertEqual(originalStarts, !ReferenceEquals(before[place], projectiles.Slots[place]),
                    $"{name}: place {place} is replaced exactly when the attack starts");
            if (originalStarts) started++;
            else refused++;
        }
        AssertTrue(refused > 0 && started > 0, "the hold both refuses and admits attacks");
        Console.WriteLine($"  Lead-particle hold: {started} starts and {refused} refusals across every attack, left state and next attack match the original's first-place test.");
    }

    /// <summary>Leaves a just-started attack's places in <paramref name="left"/>.</summary>
    private static void Leave(SamusProjectileSystem projectiles, AttackLeftState left)
    {
        switch (left)
        {
            case AttackLeftState.AllRunning:
                break;
            case AttackLeftState.TurnedOutward:
                foreach (SamusProjectile particle in projectiles.Slots.Take(4))
                    if (particle.FrameBehavior == SamusProjectileFrameBehavior.IceCombo)
                        particle.FrameBehavior = SamusProjectileFrameBehavior.IceComboOutward;
                break;
            case AttackLeftState.LeadEnded:
                projectiles.Animation.ClearProjectile(projectiles.Slots[0]);
                break;
            case AttackLeftState.OtherEnded:
                projectiles.Animation.ClearProjectile(projectiles.Slots[2]);
                break;
            case AttackLeftState.AllEnded:
                foreach (SamusProjectile particle in projectiles.Slots.Take(4).ToArray())
                    projectiles.Animation.ClearProjectile(particle);
                break;
            case AttackLeftState.LeadPlaceReshot:
                projectiles.Animation.ClearProjectile(projectiles.Slots[0]);
                SamusProjectile shot = projectiles.SlotTable.Occupy(0);
                shot.Type = SamusProjectileType.Beam(SamusBeamCombination.Power, charged: false);
                shot.Damage = 1;
                shot.FrameBehavior = SamusProjectileFrameBehavior.NoWaveBeam;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(left), left, $"Undefined {nameof(AttackLeftState)}.");
        }
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
        (SamusProjectileSystem projectiles, SamusBombProjectileSystem shared, SamusState samus) = NewPool(beam, samusAt);
        AssertTrue(projectiles.Combos.TryActivateCombo(samus, shared, out _), $"{beam} special attack starts");
        return (projectiles, shared, samus);
    }

    /// <summary>
    /// An empty shot pool and a Samus facing right at <paramref name="samusAt"/> with <paramref name="beam"/> equipped and
    /// Power Bombs selected, ready to start its special attack.
    /// </summary>
    private static (SamusProjectileSystem Projectiles, SamusBombProjectileSystem Shared, SamusState Samus) NewPool(
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
        return (projectiles, shared, samus);
    }
}
