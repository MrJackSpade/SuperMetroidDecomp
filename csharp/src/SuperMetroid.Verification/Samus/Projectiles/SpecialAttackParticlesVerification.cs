using SuperMetroid.Core;
using SuperMetroid.Core.Animation;
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
        Suite(nameof(VerifySpecialAttackReplacesFirstShots), VerifySpecialAttackReplacesFirstShots);
        Console.WriteLine("Special-attack particles: spawn parts, the whole Spazer and Wave runs, the spent-partner rule and the replacement of the first shots match the original's place rules.");
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

    /// <summary>
    /// One shot's whole state: what every shot keeps, and the values its kind's own record keeps (<see cref="KindOf"/>),
    /// its Spazer partner by the place it holds.
    /// </summary>
    private sealed record ShotState(bool IsOccupied, int Damage, SamusProjectileType Type, SamusBeamCombination PicturedBeams,
        SamusProjectileDirection Direction, bool IsSpent, FixedPosition<RoomPixels> Position, PixelVelocity<RoomPixels> Velocity,
        Extent16<RoomPixels> Radius, AnimationPlayback<SamusProjectileSegment>? Playback, int? FrameUpdatesLeft,
        ProjectileSpriteId? Sprite, int? TrailUpdatesLeft, object? Kind, SamusShotBehavior? Behavior);

    /// <summary>Reads <paramref name="shot"/>'s whole state.</summary>
    private static ShotState StateOf(SamusProjectile shot) => new(shot.IsOccupied, shot.HeldShot().Damage, shot.HeldShot().Type, shot.PicturedBeams,
        shot.Direction, shot.IsSpent, shot.Position, shot.Velocity, shot.Radius, shot.Playback, shot.Animation?.UpdatesLeft, shot.Sprite,
        global::SuperMetroid.Core.PrivateState.Get<global::SuperMetroid.Core.Samus.Projectiles.SamusProjectileOccupant, global::SuperMetroid.Core.UpdateCountdown?>(shot.HeldShot(), "Trail")?.UpdatesLeft, KindOf(shot), shot.Behavior);

    /// <summary>The values <paramref name="shot"/>'s kind keeps, compared by value; null when it keeps none.</summary>
    private static object? KindOf(SamusProjectile shot) => shot.HeldShot().KindState switch
    {
        null => null,
        SuperMissileShotState superMissile => (superMissile.Ignition, superMissile.LinkChainPosition),
        MissileShotState missile => missile.Ignition,
        IceParticleState ice => (ice.Part, ice.Angle, ice.Radius, global::SuperMetroid.Core.PrivateState.Get<global::SuperMetroid.Core.Samus.Projectiles.OrbitingParticleState, global::SuperMetroid.Core.Spatial.TrigonometryAngle>(ice, "TurnPerUpdate"), ice.Lifetime.UpdatesLeft),
        PlasmaParticleState ring => (ring.Part, ring.Angle, ring.Radius, global::SuperMetroid.Core.PrivateState.Get<global::SuperMetroid.Core.Samus.Projectiles.OrbitingParticleState, global::SuperMetroid.Core.Spatial.TrigonometryAngle>(ring, "TurnPerUpdate"), ring.Phase),
        SpazerParticleState spazer => (spazer.Part, spazer.Angle, spazer.Radius, global::SuperMetroid.Core.PrivateState.Get<global::SuperMetroid.Core.Samus.Projectiles.OrbitingParticleState, global::SuperMetroid.Core.Spatial.TrigonometryAngle>(spazer, "TurnPerUpdate"), spazer.Phase, spazer.Partner.Place,
            spazer.LeavesTrails),
        WaveParticleState wave => (wave.Part, wave.Lifetime.UpdatesLeft),
        ShinesparkEchoShotState echo => (echo.Angle, echo.Radius),
        _ => throw new InvalidOperationException($"Undefined shot kind state {shot.HeldShot().KindState}."),
    };

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
            ShotState[] particlesOnEmptyPool = [.. empty.Shots.Take(4).Select(StateOf)];
            bool carriesFraction = beam is SamusBeamFlags.Ice or SamusBeamFlags.Plasma;
            for (int place = 0; place < 4; place++)
            foreach (PlaceHolding holding in Enum.GetValues<PlaceHolding>())
            {
                (SamusProjectileSystem projectiles, SamusBombProjectileSystem shared, SamusState samus) = NewPool(beam, CentredSamus);
                int fraction = 0x1234 + 0x0F0F * place;
                int carried = Hold(projectiles, place, holding, fraction);
                SamusProjectile[] before = [.. projectiles.Shots];
                ShotState fifthBefore = StateOf(projectiles.Shots[4]);
                string name = $"{beam} attack over place {place} holding {holding}";
                AssertTrue(projectiles.Combos.TryActivateCombo(samus, shared, out SoundEffectId? sound), $"{name} starts");
                AssertEqual(emptySound, sound, $"{name} makes the empty pool's sound");
                AssertEqual(4, projectiles.Pool.ShotCount.Count, $"{name} counts four shots");
                for (int particle = 0; particle < 4; particle++)
                {
                    AssertTrue(!ReferenceEquals(before[particle], projectiles.Shots[particle]) && !before[particle].IsOccupied,
                        $"{name}: place {particle}'s earlier shot ends");
                    int expectedFraction = carriesFraction && particle == place ? carried : 0;
                    ShotState expected = particlesOnEmptyPool[particle] with
                    {
                        Position = particlesOnEmptyPool[particle].Position.WithFractionX(expectedFraction),
                    };
                    AssertEqual(expected, StateOf(projectiles.Shots[particle]),
                        $"{name}: particle {particle} is the empty pool's but for the carried sub-pixel position");
                }
                AssertTrue(ReferenceEquals(before[4], projectiles.Shots[4]), $"{name}: the fifth place's shot flies on");
                AssertEqual(fifthBefore, StateOf(projectiles.Shots[4]), $"{name}: the fifth place's shot is untouched");
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
                projectiles.Pool.ClearAll();
                break;
            case PlaceHolding.Beam:
                _ = FlyingBeam(projectiles, place, fraction, charged: false);
                break;
            case PlaceHolding.ChargedBeam:
                _ = FlyingBeam(projectiles, place, fraction, charged: true);
                break;
            case PlaceHolding.SpentBeam:
                FlyingBeam(projectiles, place, fraction, charged: false).Spend();
                break;
            case PlaceHolding.BeamExplosion:
                SamusProjectile exploding = FlyingBeam(projectiles, place, fraction, charged: false);
                exploding.HeldShot().Type = exploding.HeldShot().Type.AsFamily(SamusProjectileFamily.BeamExplosion);
                exploding.Animation = ProjectileFixturePlaybacks.StartingAfter(SamusProjectileSegment.BeamExplosion, MidPictureUpdates);
                exploding.HeldShot().Damage = 8;
                exploding.Behavior = null;
                break;
            case PlaceHolding.Missile:
                _ = IgnitedMissile(projectiles, place, fraction, SamusProjectileFamily.Missile, SamusShotBehaviors.Missile);
                break;
            case PlaceHolding.SuperMissile:
                IgnitedMissile(projectiles, place, fraction, SamusProjectileFamily.SuperMissile, SamusShotBehaviors.SuperMissile)
                    .KindStateAs<SuperMissileShotState>().LinkChainPosition = FifthPlace;
                _ = SuperMissileLink(projectiles, FifthPlace, fraction);
                return fraction;
            case PlaceHolding.SuperMissileLink:
                _ = SuperMissileLink(projectiles, place, fraction);
                IgnitedMissile(projectiles, FifthPlace, fraction, SamusProjectileFamily.SuperMissile, SamusShotBehaviors.SuperMissile)
                    .KindStateAs<SuperMissileShotState>().LinkChainPosition = place;
                return fraction;
            case PlaceHolding.CrashEcho:
                SamusProjectile echo = projectiles.Pool.OccupyAsFired(place, SamusProjectileType.EchoOf(SamusBeamEcho.ShinesparkEcho));
                Fly(echo, place, fraction);
                BeamCombos.InitializeComboShot(echo.HeldShot(), BeamComboShotKind.Echo);
                echo.HeldShot().Install(SamusShotBehaviors.ShinesparkEcho, new ShinesparkEchoShotState(new TrigonometryAngle(32)) { Radius = 24 });
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
        SamusProjectile shot = projectiles.Pool.Occupy(place, SamusProjectileType.Beam(SamusBeamCombination.Power, charged), SamusProjectileDamage.Read(SamusProjectileSelection.BeamShot(SamusBeamCombination.Power, charged))).Projectile;
        Fly(shot, place, fraction);
        SamusProjectileSegment? segment = SamusProjectileSelection.BeamShotAnimation(SamusBeamCombination.Power, charged, shot.Direction);
        shot.Animation = ProjectileFixturePlaybacks.StartingAfter(
            segment ?? throw new InvalidOperationException("A Power Beam shot has an animation."), MidPictureUpdates);
        shot.Radius = SamusProjectilePlayback.InitialRadius(segment);
        shot.Behavior = SamusShotBehaviors.Beam;
        return shot;
    }

    /// <summary>Puts an ignited missile or Super Missile in <paramref name="place"/>.</summary>
    private static SamusProjectile IgnitedMissile(SamusProjectileSystem projectiles, int place, int fraction,
        SamusProjectileFamily family, SamusShotBehavior behavior)
    {
        SamusProjectileKind kind = SamusProjectileSelection.NonBeam(family);
        SamusProjectile missile = projectiles.Pool.Occupy(place, SamusProjectileType.Of(family), SamusProjectileDamage.Read(kind)).Projectile;
        Fly(missile, place, fraction);
        SamusProjectileSegment segment = SamusProjectileKinds.Animation(kind, missile.Direction);
        missile.Animation = ProjectileFixturePlaybacks.StartingAfter(segment, MidPictureUpdates);
        missile.Radius = SamusProjectilePlayback.InitialRadius(segment);
        MissileShotState ignition = family == SamusProjectileFamily.SuperMissile
            ? new SuperMissileShotState(new SamusMissileIgnition.Pending(), linkPlace: null)
            : new MissileShotState(new SamusMissileIgnition.Pending());
        ignition.Ignite(SamusProjectileMotionDefinitions.MissileIgnitionSpeed);
        missile.HeldShot().Install(behavior, ignition);
        return missile;
    }

    /// <summary>Puts a Super Missile's link in <paramref name="place"/>.</summary>
    private static SamusProjectile SuperMissileLink(SamusProjectileSystem projectiles, int place, int fraction)
    {
        SamusProjectile link = projectiles.Pool.OccupyAtRest(place, SamusProjectileType.Of(SamusProjectileFamily.SuperMissile), SamusProjectileDamage.Read(SamusProjectileKind.SuperMissileLink)).Projectile;
        Fly(link, place, fraction);
        link.Animation = ProjectileFixturePlaybacks.StartingAfter(SamusProjectileKinds.SoleAnimation(SamusProjectileKind.SuperMissileLink), MidPictureUpdates);
        link.HeldShot().Install(SamusShotBehaviors.SuperMissileLink,
            new SuperMissileShotState(new SamusMissileIgnition.Pending(), UnlinkedSuperMissileLink.Place));
        return link;
    }

    /// <summary>The updates a flying fixture shot's animation waits before its first picture, so it stands mid-picture.</summary>
    private const int MidPictureUpdates = 3;

    /// <summary>Gives a shot a flight of its own: somewhere in the room, moving right and mid-trail.</summary>
    private static void Fly(SamusProjectile shot, int place, int fraction)
    {
        shot.PicturedBeams = SamusBeamCombination.Power;
        shot.Direction = SamusProjectileDirection.Right;
        shot.Position = FixedPosition<RoomPixels>.FromWhole(new Position16<RoomPixels>(200 + place, 90 + place))
            .WithFractionX(fraction).WithFractionY(0x4321);
        shot.Velocity = new PixelVelocity<RoomPixels>(SignedEightEight.FromTwoHundredFiftySixthsOfPixel(1200), SignedEightEight.FromTwoHundredFiftySixthsOfPixel(-300));
        shot.HeldShot().Trail = UpdateCountdown.Lasting(2);
    }

    /// <summary>The four beams that start a special attack.</summary>
    private static readonly SamusBeamFlags[] AttackBeams =
        [SamusBeamFlags.Wave, SamusBeamFlags.Ice, SamusBeamFlags.Spazer, SamusBeamFlags.Plasma];

    /// <summary>Checks every particle's part as each of the four attacks starts.</summary>
    private static void VerifySpawnedParts()
    {
        foreach (SamusBeamFlags beam in new[] { SamusBeamFlags.Wave, SamusBeamFlags.Ice, SamusBeamFlags.Spazer, SamusBeamFlags.Plasma })
        {
            (SamusProjectileSystem projectiles, _, SamusState samus) = StartAttack(beam, CentredSamus);
            AssertEqual(4, projectiles.Pool.ShotCount.Count, $"{beam} attack counts four shots");
            for (int place = 0; place < 4; place++)
            {
                SamusProjectile particle = projectiles.Shots[place];
                AssertEqual(SpecialAttackParticleDefinitions.InPlaceOrder[place], particle.KindStateAs<SpecialAttackParticleState>().Part,
                    $"{beam} particle {place} plays its place's part");
                if (beam != SamusBeamFlags.Wave)
                    AssertEqual(beam is SamusBeamFlags.Ice or SamusBeamFlags.Plasma ? new TrigonometryAngle(64 * place) : default,
                        particle.KindStateAs<OrbitingParticleState>().Angle, $"{beam} particle {place} starts its orbit a quarter turn per place");
                if (beam == SamusBeamFlags.Wave)
                    AssertEqual(new Position16<RoomPixels>(samus.Position.X.Whole + (place < 2 ? 128 : -128),
                            samus.Position.Y.Whole + (place is 0 or 3 ? 128 : -128)),
                        particle.Position.Whole, $"Wave particle {place} starts in its place's corner");
                if (beam != SamusBeamFlags.Spazer)
                    continue;
                SpazerParticleState spazer = particle.KindStateAs<SpazerParticleState>();
                AssertEqual(TrigonometryAngle.Wrapped(OriginalSpazerTurn(place) * 4), global::SuperMetroid.Core.PrivateState.Get<global::SuperMetroid.Core.Samus.Projectiles.OrbitingParticleState, global::SuperMetroid.Core.Spatial.TrigonometryAngle>(spazer, "TurnPerUpdate"),
                    $"Spazer particle {place} circles the way its place's parity turns");
                AssertEqual(place >= 2, particle.HeldShot().Type == SamusProjectileType.EchoOf(SamusBeamEcho.SpazerComboTrail),
                    $"Spazer particle {place} is a trail exactly when it holds one of the last two places");
                AssertEqual(place >= 2, spazer.LeavesTrails,
                    $"Spazer particle {place} leaves trails from the start exactly when it is one of the trailing pair");
                if (place >= 2)
                    AssertEqual(ProjectileTimingDefinitions.TrailInterval, global::SuperMetroid.Core.PrivateState.Get<global::SuperMetroid.Core.Samus.Projectiles.SamusProjectileOccupant, global::SuperMetroid.Core.UpdateCountdown?>(particle.HeldShot(), "Trail")?.UpdatesLeft,
                        $"trailing Spazer particle {place} starts its trail countdown");
                AssertTrue(ReferenceEquals(projectiles.Shots[place ^ 2], spazer.Partner),
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
        for (; update < 400 && projectiles.Shots.Any(slot => slot.IsActive); update++)
        {
            foreach (SamusProjectile particle in projectiles.Pool.InUpdateOrder.ToArray())
            {
                if (!particle.IsActive)
                    continue;
                int place = particle.Place;
                SpazerParticleState spazer = particle.KindStateAs<SpazerParticleState>();
                bool wasFalling = spazer.Phase == SpazerComboPhase.Falling;
                TrigonometryAngle angleBefore = spazer.Angle;
                int radiusBefore = spazer.Radius;
                SoundEffectId? sound = projectiles.Combos.StepSpazerCombo(samus, particle.HeldShot(), shared, SpazerCameraY);
                if (sound is not null)
                {
                    AssertEqual(SamusSound.SpazerSpecialAttackEnd, sound.Value, $"update {update} Spazer sound");
                    AssertEqual(0, place, $"update {update}: only the first place voices the Spazer convergence");
                    convergenceSounds++;
                }
                if (!particle.IsActive)
                    continue;
                bool falling = spazer.Phase == SpazerComboPhase.Falling;
                phasesSeen.Add(spazer.Phase);
                if (falling)
                {
                    AssertEqual(SamusProjectileType.EchoOf(SamusBeamEcho.SpazerComboTrail), particle.HeldShot().Type,
                        $"update {update}: falling Spazer particle {place} is drawn as a trail");
                    if (!wasFalling)
                    {
                        falls++;
                        Offset16<RoomPixels> orbit = SamusComboMechanics.GetSineOffset(angleBefore, radiusBefore);
                        AssertEqual(CoordinateAxis.Wrap(samus.Position.X.Whole + orbit.X + (place < 2 ? 16 : -16)),
                            (int)particle.Position.X.Whole,
                            $"update {update}: Spazer particle {place} steps out its place's pair's way as it falls");
                    }
                    continue;
                }
                int expectedStep = spazer.Phase switch
                {
                    SpazerComboPhase.Circling => OriginalSpazerTurn(place) * 4,
                    SpazerComboPhase.FlyingUpTowardsPoint => OriginalSpazerTurn(place) * 2,
                    SpazerComboPhase.FlyingUpAwayFromPoint => -OriginalSpazerTurn(place) * 2,
                    // A falling particle is checked above and turns no more.
                    SpazerComboPhase.Falling or _ => throw new InvalidOperationException($"Undefined {nameof(SpazerComboPhase)} {spazer.Phase}."),
                };
                AssertEqual(TrigonometryAngle.Wrapped(expectedStep), global::SuperMetroid.Core.PrivateState.Get<global::SuperMetroid.Core.Samus.Projectiles.OrbitingParticleState, global::SuperMetroid.Core.Spatial.TrigonometryAngle>(spazer, "TurnPerUpdate"),
                    $"update {update}: Spazer particle {place} turns its place's way in {spazer.Phase}");
            }
        }
        AssertTrue(projectiles.Shots.All(slot => !slot.IsActive), $"the Spazer attack ends within {update} updates");
        AssertEqual(0, projectiles.Pool.ShotCount.Count, "every ended Spazer particle uncounts its shot");
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
            SamusProjectile spent = projectiles.Shots[spentPlace];
            spent.Spend();
            AssertTrue(projectiles.Combos.StepSpazerCombo(samus, spent.HeldShot(), shared, SpazerCameraY) is null,
                $"spent Spazer particle {spentPlace} makes no sound");
            for (int place = 0; place < 4; place++)
                AssertEqual(place != spentPlace && place != (spentPlace ^ 2), projectiles.Shots[place].IsOccupied,
                    $"spent Spazer particle {spentPlace} ends itself and the shot two places from it, not place {place}");
            AssertEqual(2, projectiles.Pool.ShotCount.Count, $"spent Spazer particle {spentPlace} uncounts two shots");
        }

        (SamusProjectileSystem echoProjectiles, SamusBombProjectileSystem echoShared, SamusState echoSamus) =
            StartAttack(SamusBeamFlags.Spazer, SpazerSamus);
        SamusProjectile replaced = echoProjectiles.Shots[3];
        SamusProjectile echo = echoProjectiles.Pool.Occupy(3, SamusProjectileType.EchoOf(SamusBeamEcho.ShinesparkEcho), 1).Projectile;
        echo.HeldShot().Install(SamusShotBehaviors.ShinesparkEcho, new ShinesparkEchoShotState(TrigonometryAngle.Zero));
        AssertTrue(!replaced.IsOccupied, "the crash echo's release ends the trailing particle it replaces");
        SamusProjectile spentBeam = echoProjectiles.Shots[1];
        spentBeam.Spend();
        _ = echoProjectiles.Combos.StepSpazerCombo(echoSamus, spentBeam.HeldShot(), echoShared, SpazerCameraY);
        AssertTrue(!echo.IsOccupied, "a spent Spazer particle ends the crash echo holding its partner's place");
        AssertTrue(ReferenceEquals(echo, echoProjectiles.Shots[3]), "the ended echo is the last shot of its place");
        AssertTrue(echoProjectiles.Shots[0].IsOccupied && echoProjectiles.Shots[2].IsOccupied,
            "the other Spazer pair flies on");
        AssertEqual(2, echoProjectiles.Pool.ShotCount.Count, "the spent particle and the echo uncount two shots");
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
            foreach (SamusProjectile particle in projectiles.Pool.InUpdateOrder.ToArray())
            {
                if (!particle.IsActive)
                    continue;
                int place = particle.Place;
                bool wasNegative = particle.Velocity.X.IsNegative;
                SoundEffectId? sound = projectiles.Combos.StepWaveCombo(samus, particle.HeldShot(), shared);
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
        AssertEqual(0, projectiles.Pool.ShotCount.Count, "every ended Wave particle uncounts its shot");
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
        var projectiles = new SamusProjectileSystem(GameplayAudioRequests.Standalone(), new BeamChargeState(), InstalledFixtures.SamusOverreads,
            new LeftBeamStartFromLastShot(SamusProjectileSystem.PlaceCount), new SamusShotCount(), global::SuperMetroid.TestSupport.RepositoryInstallation.Verification.InstalledFixtures.SamusArtwork(), new global::SuperMetroid.Core.Fx.RoomEarthquake());
        var shared = new SamusBombProjectileSystem(GameplayAudioRequests.Standalone(), global::SuperMetroid.TestSupport.RepositoryInstallation.Verification.InstalledFixtures.SamusArtwork());
        var samus = new SamusState(GameplayAudioRequests.Standalone(), InstalledFixtures.FramesBeyondAnimation, new UnauthoredBoostCadence(),
            SamusCompatibility.ForNewSamus(), new BeamChargeState(), global::SuperMetroid.TestSupport.RepositoryInstallation.Verification.InstalledFixtures.SamusArtwork())
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
