using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using static SuperMetroid.Verification.Harness.RetailRuntimeFixtureVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
namespace SuperMetroid.Verification.Samus.Projectiles;
using SuperMetroid.Verification.Harness;

/// <summary>
/// Confirms #2468 against the original's fire routines, which this check states as its oracle: the uncharged, charged and
/// Hyper Beam fires look for the lowest place no shot holds (<c>$90:B8AA</c>, <c>$90:B9A1</c>, <c>$90:BCD8</c>) and, with
/// every place held, fall through to the last place and fire over the shot there, which nothing deletes; the shot count,
/// not the places, admits the fire. The fire writes the shot's own values but not the word its place keeps for its
/// projectile, so a Chainsaw shot fired so runs its Power Bomb fuse (<c>$90:C157</c>) from what the replaced shot left
/// there, and the Power Bomb's boundary step (<c>$94:A06A</c>) reacts with nothing while that fuse burns.
/// </summary>
internal static class BeamFiredOverLastShotVerification
{
    /// <summary>The last of Samus's shot places, which a beam fired with every place held takes.</summary>
    private const int LastPlace = SamusProjectileSystem.PlaceCount - 1;

    /// <summary>
    /// The fuse count at which the Power Bomb fuse speeds up, which the original tests after its decrement
    /// (<c>$90:C168</c>).
    /// </summary>
    private const int FastStage = 15;

    /// <summary>Runs every check of a beam fired with every shot place held.</summary>
    internal static void VerifyBeamFiredOverLastShot()
    {
        Suite(nameof(VerifyEachFireLandsOverTheLastShot), VerifyEachFireLandsOverTheLastShot);
        Suite(nameof(VerifyChainsawBurnsTheFuseItFinds), VerifyChainsawBurnsTheFuseItFinds);
        Suite(nameof(VerifyFuseEachShotLeaves), VerifyFuseEachShotLeaves);
        Suite(nameof(VerifyBoundaryReactsOnlyAfterBurningOut), VerifyBoundaryReactsOnlyAfterBurningOut);
        Console.WriteLine("Beam fired over the last shot: every beam fire lands in the last place over the shot holding it, starts from the fuse each kind of shot leaves there, and a Chainsaw shot fired so burns it with the boundary reactions held until it has burned out.");
    }

    /// <summary>
    /// With every place held and the shot count at four, each beam fire takes the last place over the flying beam there:
    /// it fires, the replaced shot ends, the first four places' shots fly on and the count reaches five. With the count at
    /// five the same fire is refused and replaces nothing.
    /// </summary>
    private static void VerifyEachFireLandsOverTheLastShot()
    {
        RoomLevelData level = EmptyLevel();
        foreach (FirePath path in Enum.GetValues<FirePath>())
        {
            (SamusProjectileSystem projectiles, SamusBombProjectileSystem shared, SamusState samus) =
                NewFixture(path == FirePath.Charged ? SamusBeamFlags.Charge : SamusBeamFlags.None);
            HoldFirstPlaces(projectiles);
            _ = FlyingBeam(projectiles, LastPlace);
            CountShots(projectiles, LastPlace);
            SamusProjectile[] before = [.. projectiles.Shots];

            AssertTrue(Fire(path, projectiles, level, samus, shared) is not null, $"{path}: the fire with every place held fires");
            AssertTrue(!ReferenceEquals(before[LastPlace], projectiles.Shots[LastPlace]) && !before[LastPlace].IsOccupied,
                $"{path}: the last place's shot is replaced and ends");
            AssertTrue(projectiles.Shots[LastPlace].IsOccupied, $"{path}: the new shot holds the last place");
            AssertEqual(path == FirePath.Hyper
                    ? SamusProjectileType.Beam(SamusBeamCombination.Plasma, charged: true)
                    : SamusProjectileType.Beam(SamusBeamCombination.Power, charged: path == FirePath.Charged),
                projectiles.Shots[LastPlace].Type, $"{path}: the last place holds the fired beam");
            for (int place = 0; place < LastPlace; place++)
                AssertTrue(ReferenceEquals(before[place], projectiles.Shots[place]) && before[place].IsOccupied,
                    $"{path}: place {place}'s shot flies on");
            AssertEqual(SamusProjectileSystem.PlaceCount, projectiles.Pool.ShotCount.Count, $"{path}: the fire counts its shot");

            (SamusProjectileSystem full, SamusBombProjectileSystem fullShared, SamusState fullSamus) =
                NewFixture(path == FirePath.Charged ? SamusBeamFlags.Charge : SamusBeamFlags.None);
            HoldFirstPlaces(full);
            SamusProjectile last = FlyingBeam(full, LastPlace);
            CountShots(full, SamusProjectileSystem.PlaceCount);
            AssertTrue(Fire(path, full, level, fullSamus, fullShared) is null, $"{path}: a count of five refuses the fire");
            AssertTrue(ReferenceEquals(last, full.Shots[LastPlace]) && last.IsOccupied, $"{path}: a refused fire replaces nothing");
        }
    }

    /// <summary>
    /// A Chainsaw shot fired with every place held over an ignited missile starts its fuse from the missile's word, which
    /// its ignition left at the ignition bitset the cartridge adds (<c>$90:C301</c>): the shot lives while the fuse burns,
    /// its animation moves to the fast stage on the update the fuse reaches fifteen, and the Power Bomb blast starts at the
    /// shot on the update it reaches zero. The boundary step reacts with nothing on every one of those updates, the last
    /// included, and once the fuse has burned out it reacts: every block of the fixture's room is bombable and has nothing
    /// to break it, so a reaction fails loudly.
    /// </summary>
    private static void VerifyChainsawBurnsTheFuseItFinds()
    {
        var retail = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        int ignitedWord = CartridgeImportSource.Require(retail).ReadCartridgeWord(0x90C301);

        (SamusProjectileSystem projectiles, SamusBombProjectileSystem shared, SamusState samus) =
            NewFixture(SamusBeamFlags.Wave | SamusBeamFlags.Spazer | SamusBeamFlags.Plasma);
        HoldFirstPlaces(projectiles);
        SamusProjectile missile = projectiles.Pool.Occupy(LastPlace);
        missile.Type = SamusProjectileType.Of(SamusProjectileFamily.Missile);
        missile.Direction = SamusProjectileDirection.Right;
        missile.Damage = SamusProjectileDamage.Read(SamusProjectileSelection.NonBeam(SamusProjectileFamily.Missile));
        var ignition = new MissileShotState(SamusMissileIgnition.Pending);
        ignition.Ignite(SamusProjectileMotionDefinitions.MissileIgnitionSpeed);
        missile.Install(SamusShotBehaviors.Missile, ignition);
        CountShots(projectiles, LastPlace);

        AssertTrue(Fire(FirePath.Uncharged, projectiles, EmptyLevel(), samus, shared) is not null,
            "the Chainsaw fire with every place held fires");
        SamusProjectile chainsaw = projectiles.Shots[LastPlace];
        AssertEqual(SamusShotBehaviors.ChainsawPowerBomb, chainsaw.Behavior, "the last place holds the Chainsaw shot");

        // The fixture stands the shot's animation where the fast stage's move lands on an animation position, as a slow
        // Power Bomb's does, so the move is visible without the shot's own animation advancing.
        chainsaw.Animation = SamusProjectilePlayback.Start(SamusProjectileSegment.PowerBomb);
        RoomLevelData bombable = BombableLevel();
        for (int update = 1; update <= ignitedWord; update++)
        {
            AnimationPlayback<SamusProjectileSegment> playback = chainsaw.Playback
                ?? throw new InvalidOperationException("The burning Chainsaw shot has an animation.");
            AnimationPlayback<SamusProjectileSegment> expected = ignitedWord - update == FastStage
                ? SamusProjectilePlayback.PlaybackAt(InstalledFixtures.SamusOverreads.Projectiles.ChainsawFuseShift.Shift(
                    SamusProjectilePlayback.BeatOf(playback)))
                : playback;
            projectiles.Chainsaw.RunPowerBombFuse(bombable, chainsaw, shared, SamusRoomObjectFixtures.Uninstalled);
            AssertTrue(chainsaw.IsOccupied, $"update {update}: the Chainsaw shot lives while its fuse burns");
            AssertEqual(expected, chainsaw.Playback, $"update {update}: the animation moves only as the fuse reaches fifteen");
            AssertEqual(update == ignitedWord, shared.PowerBombExplosion.IsActive,
                $"update {update}: the blast starts only as the fuse runs out");
        }
        AssertThrows<InvalidOperationException>(
            () => projectiles.Chainsaw.RunPowerBombFuse(bombable, chainsaw, shared, SamusRoomObjectFixtures.Uninstalled),
            "the burned-out fuse's boundary step reacts at the blast's edge");
    }

    /// <summary>
    /// A beam fired with every place held starts from the count the shot it replaces left in its place, which the
    /// original's own arithmetic gives: a missile's ignition adds the ignition bitset (<c>$90:C301</c>) to the zero of a
    /// free place or the head start a reflection writes (the operand of <c>$90:BE39</c>); a Super Missile's link keeps the
    /// count's high byte and writes the link's index, twice its place, below it (<c>$90:BF8B</c>); a crash echo's count is
    /// its angle (<c>$90:D454</c>); a link's is the zero of its free place; an explosion keeps its shot's; and a beam's is
    /// the fuse it carries. A beam fired into a free place carries none, whatever the place's last shot carried.
    /// </summary>
    private static void VerifyFuseEachShotLeaves()
    {
        var retail = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        int ignitedWord = CartridgeImportSource.Require(retail).ReadCartridgeWord(0x90C301);
        int reflectionWord = CartridgeImportSource.Require(retail).ReadCartridgeWord(0x90BE3A);
        AssertEqual(ignitedWord, SamusProjectileMotionDefinitions.MissileIgnitionSpeed.InTwoHundredFiftySixthsOfPixel,
            "a missile ignites at the speed its ignition adds");
        AssertEqual(ignitedWord + reflectionWord,
            SamusProjectileMotionDefinitions.ReflectedMissileIgnitionSpeed.InTwoHundredFiftySixthsOfPixel,
            "a reflected missile ignites at the speed its ignition adds to the reflection's head start");
        const int linkPlace = 2;
        int linkedWord = (ignitedWord & 0xFF00) + linkPlace * 2;
        const int echoAngle = 0xB6;
        const int carriedFuse = 300;
        const int chainsawFuseLeft = 100;

        (string Holder, Action<SamusProjectileSystem> Hold, int Expected)[] cases =
        [
            ("a missile not yet ignited", projectiles => _ = Missile(projectiles, SamusMissileIgnition.Pending, null), 0),
            ("a reflected missile waiting to re-ignite",
                projectiles => _ = Missile(projectiles, SamusMissileIgnition.PendingAfterReflection, null), reflectionWord),
            ("an ignited missile", projectiles => _ = Missile(projectiles, SamusMissileIgnition.Pending,
                SamusProjectileMotionDefinitions.MissileIgnitionSpeed), ignitedWord),
            ("a missile re-ignited after a reflection", projectiles => _ = Missile(projectiles,
                SamusMissileIgnition.PendingAfterReflection, SamusProjectileMotionDefinitions.ReflectedMissileIgnitionSpeed),
                ignitedWord + reflectionWord),
            ("an ignited missile's explosion", projectiles =>
            {
                SamusProjectile exploding = Missile(projectiles, SamusMissileIgnition.Pending,
                    SamusProjectileMotionDefinitions.MissileIgnitionSpeed);
                exploding.Type = exploding.Type.AsFamily(SamusProjectileFamily.MissileExplosion);
                exploding.Behavior = null;
            }, ignitedWord),
            ("an ignited Super Missile naming its link's place", projectiles => _ = SuperMissile(projectiles,
                SamusMissileIgnition.Pending, UnlinkedSuperMissileLink.Place, linkPlace), linkedWord),
            ("an ignited Super Missile that found no place for its link", projectiles => _ = SuperMissile(projectiles,
                SamusMissileIgnition.Pending, UnlinkedSuperMissileLink.Place, UnlinkedSuperMissileLink.Place),
                ignitedWord & 0xFF00),
            ("a reflected Super Missile re-ignited with no place for its link", projectiles => _ = SuperMissile(projectiles,
                SamusMissileIgnition.PendingAfterReflection, linkPlace: null, ignitedLinkPlace: null),
                ignitedWord + reflectionWord),
            ("a Super Missile's link", projectiles =>
            {
                SamusProjectile link = projectiles.Pool.OccupyAtRest(LastPlace);
                link.Type = SamusProjectileType.Of(SamusProjectileFamily.SuperMissile);
                link.Damage = SamusProjectileDamage.Read(SamusProjectileKind.SuperMissileLink);
                link.Install(SamusShotBehaviors.SuperMissileLink,
                    new SuperMissileShotState(SamusMissileIgnition.Pending, UnlinkedSuperMissileLink.Place));
            }, 0),
            ("a crash echo", projectiles =>
            {
                SamusProjectile echo = projectiles.Pool.Occupy(LastPlace);
                echo.Type = SamusProjectileType.EchoOf(SamusBeamEcho.ShinesparkEcho);
                BeamCombos.InitializeComboShot(echo, BeamComboShotKind.Echo);
                echo.Install(SamusShotBehaviors.ShinesparkEcho, new ShinesparkEchoShotState(new TrigonometryAngle(echoAngle)));
            }, echoAngle),
            ("a beam carrying a fuse", projectiles => FlyingBeam(projectiles, LastPlace).BeamFuse = carriedFuse, carriedFuse),
            ("a Chainsaw shot part-way through its fuse", projectiles =>
            {
                SamusProjectile chainsaw = FlyingBeam(projectiles, LastPlace);
                chainsaw.Type = SamusProjectileType.Beam(SamusBeamCombination.SpazerPlasmaWave, charged: false);
                chainsaw.Install(SamusShotBehaviors.ChainsawPowerBomb, kindState: null);
                chainsaw.BeamFuse = chainsawFuseLeft;
            }, chainsawFuseLeft),
            ("a free place whose last beam carried a fuse", projectiles =>
            {
                SamusProjectile ended = FlyingBeam(projectiles, LastPlace);
                ended.BeamFuse = carriedFuse;
                projectiles.Animation.ClearProjectile(ended);
            }, 0),
        ];

        foreach ((string holder, Action<SamusProjectileSystem> hold, int expected) in cases)
        {
            (SamusProjectileSystem projectiles, SamusBombProjectileSystem shared, SamusState samus) = NewFixture(SamusBeamFlags.None);
            HoldFirstPlaces(projectiles);
            hold(projectiles);
            CountShots(projectiles, LastPlace);
            AssertTrue(Fire(FirePath.Uncharged, projectiles, EmptyLevel(), samus, shared) is not null, $"a beam fired over {holder} fires");
            AssertEqual(SamusProjectileType.Beam(SamusBeamCombination.Power, charged: false), projectiles.Shots[LastPlace].Type,
                $"the beam fired over {holder} holds the last place");
            AssertEqual(expected, projectiles.Shots[LastPlace].BeamFuse, $"a beam fired over {holder} starts from the count it left");
        }
        Console.WriteLine($"  Fuse each shot leaves: {cases.Length} holdings of the last place leave the count the original's arithmetic gives.");
    }

    /// <summary>
    /// The Power Bomb's boundary step reacts only from a fuse that had already burned out, a blast under way or not: never
    /// while the fuse burns, at its fast stage included, and never on the step that burns it out (<c>$94:A06A</c>).
    /// </summary>
    private static void VerifyBoundaryReactsOnlyAfterBurningOut()
    {
        foreach (bool blastUnderWay in new[] { false, true })
        {
            AssertTrue(SamusPowerBombFuse.Step(0, blastUnderWay).BoundaryReacts, $"blast {blastUnderWay}: a burned-out fuse reacts");
            AssertTrue(!SamusPowerBombFuse.Step(1, blastUnderWay).BoundaryReacts, $"blast {blastUnderWay}: the step burning the fuse out reacts with nothing");
            AssertTrue(!SamusPowerBombFuse.Step(2, blastUnderWay).BoundaryReacts, $"blast {blastUnderWay}: a burning fuse reacts with nothing");
            AssertTrue(!SamusPowerBombFuse.Step(FastStage + 1, blastUnderWay).BoundaryReacts, $"blast {blastUnderWay}: the fast stage reacts with nothing");
        }
    }

    /// <summary>Puts a missile in the last place at <paramref name="ignition"/>, ignited at <paramref name="ignitedAt"/> when given.</summary>
    private static SamusProjectile Missile(SamusProjectileSystem projectiles, SamusMissileIgnition ignition, SignedEightEight? ignitedAt)
    {
        SamusProjectile missile = projectiles.Pool.Occupy(LastPlace);
        missile.Type = SamusProjectileType.Of(SamusProjectileFamily.Missile);
        missile.Direction = SamusProjectileDirection.Right;
        missile.Damage = SamusProjectileDamage.Read(SamusProjectileSelection.NonBeam(SamusProjectileFamily.Missile));
        var state = new MissileShotState(ignition);
        if (ignitedAt is { } speed)
            state.Ignite(speed);
        missile.Install(SamusShotBehaviors.Missile, state);
        return missile;
    }

    /// <summary>
    /// Puts a Super Missile in the last place, starting at <paramref name="ignition"/> naming <paramref name="linkPlace"/>,
    /// then ignited as its ignition does, at its speed, naming <paramref name="ignitedLinkPlace"/>.
    /// </summary>
    private static SamusProjectile SuperMissile(SamusProjectileSystem projectiles, SamusMissileIgnition ignition, int? linkPlace,
        int? ignitedLinkPlace)
    {
        SamusProjectile missile = projectiles.Pool.Occupy(LastPlace);
        missile.Type = SamusProjectileType.Of(SamusProjectileFamily.SuperMissile);
        missile.Direction = SamusProjectileDirection.Right;
        missile.Damage = SamusProjectileDamage.Read(SamusProjectileSelection.NonBeam(SamusProjectileFamily.SuperMissile));
        var state = new SuperMissileShotState(ignition, linkPlace);
        state.Ignite(ignition == SamusMissileIgnition.PendingAfterReflection
            ? SamusProjectileMotionDefinitions.ReflectedMissileIgnitionSpeed
            : SamusProjectileMotionDefinitions.MissileIgnitionSpeed);
        state.LinkPlace = ignitedLinkPlace;
        missile.Install(SamusShotBehaviors.SuperMissile, state);
        return missile;
    }

    /// <summary>The fire routines a beam leaves by.</summary>
    private enum FirePath
    {
        /// <summary>The uncharged fire.</summary>
        Uncharged,

        /// <summary>The charged fire.</summary>
        Charged,

        /// <summary>The Hyper Beam's fire.</summary>
        Hyper,
    }

    /// <summary>Fires a beam by <paramref name="path"/>: the shot, or null when the fire is refused.</summary>
    private static BeamShot? Fire(FirePath path, SamusProjectileSystem projectiles, RoomLevelData level, SamusState samus,
        SamusBombProjectileSystem shared) => path switch
    {
        FirePath.Uncharged => projectiles.Firing.TryFireBeam(level, samus, GameplayAction.Shoot, shared,
            SamusRoomObjectFixtures.Uninstalled, charged: false),
        FirePath.Charged => projectiles.Firing.TryFireBeam(level, samus, GameplayAction.Shoot, shared,
            SamusRoomObjectFixtures.Uninstalled, charged: true),
        FirePath.Hyper => projectiles.Firing.TryFireHyperBeam(level, samus, shared, SamusRoomObjectFixtures.Uninstalled),
        _ => throw new ArgumentOutOfRangeException(nameof(path), path, $"Undefined {nameof(FirePath)}."),
    };

    /// <summary>An empty shot pool and a Samus standing facing right mid-room with <paramref name="beams"/> equipped.</summary>
    private static (SamusProjectileSystem Projectiles, SamusBombProjectileSystem Shared, SamusState Samus) NewFixture(
        SamusBeamFlags beams)
    {
        var samus = new SamusState(GameplayAudioRequests.Standalone(), InstalledFixtures.FramesBeyondAnimation,
            new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence(), SamusCompatibility.ForNewSamus(), new BeamChargeState(),
            InstalledFixtures.SamusArtwork())
        {
            Pose = SamusPoseId.FacingRightNormalPose,
            Position = FixedPosition<RoomPixels>.FromWhole(new(128, 128)),
            EquippedBeams = beams,
            SelectedHudItem = HudItem.Beam,
        };
        return (CreateProjectileFixture(), CreateBombFixture(), samus);
    }

    /// <summary>Fills the first four places with flying beams.</summary>
    private static void HoldFirstPlaces(SamusProjectileSystem projectiles)
    {
        for (int place = 0; place < LastPlace; place++)
            _ = FlyingBeam(projectiles, place);
    }

    /// <summary>Counts <paramref name="shots"/> shots out, as firing them did.</summary>
    private static void CountShots(SamusProjectileSystem projectiles, int shots)
    {
        for (int shot = 0; shot < shots; shot++)
            projectiles.Pool.ShotCount.CountShot();
    }

    /// <summary>Puts a flying uncharged Power Beam shot in <paramref name="place"/>, somewhere in the room.</summary>
    private static SamusProjectile FlyingBeam(SamusProjectileSystem projectiles, int place)
    {
        SamusProjectile shot = projectiles.Pool.Occupy(place);
        shot.Type = SamusProjectileType.Beam(SamusBeamCombination.Power, charged: false);
        shot.Direction = SamusProjectileDirection.Right;
        shot.Position = FixedPosition<RoomPixels>.FromWhole(new Position16<RoomPixels>(200 + place, 90 + place));
        shot.Damage = SamusProjectileDamage.Read(SamusProjectileSelection.BeamShot(SamusBeamCombination.Power, charged: false));
        shot.Install(SamusShotBehaviors.Beam, kindState: null);
        return shot;
    }

    /// <summary>A sixteen-block-square room of air.</summary>
    private static RoomLevelData EmptyLevel() => new(new Extent16<RoomBlocks>(16, 16),
        new RoomLevelPlanes(new RoomTerrainBlock[256], new RoomBlockBehavior[256], new RoomBlockAppearance[256]),
        RoomLevelData.NothingBeyondGrid(), []);

    /// <summary>A sixteen-block-square room whose every block is bombable.</summary>
    private static RoomLevelData BombableLevel()
    {
        RoomTerrainBlock[] blocks = Enumerable.Repeat(
            RoomTerrainBlock.Of(0, BlockMirroring.None, RoomCollisionType.BombableBlock), 256).ToArray();
        return new(new Extent16<RoomBlocks>(16, 16),
            new RoomLevelPlanes(blocks, new RoomBlockBehavior[256], new RoomBlockAppearance[256]),
            RoomLevelData.NothingBeyondGrid(), []);
    }

    /// <summary>Runs the beam-fired-over-the-last-shot checks from the command line.</summary>
    [VerificationCommand("--beam-fired-over-last-shot")]
    internal static int BeamFiredOverLastShotCommand()
    {
        Suite(nameof(VerifyBeamFiredOverLastShot), VerifyBeamFiredOverLastShot);
        return 0;
    }
}
