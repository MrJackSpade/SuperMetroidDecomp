using SuperMetroid.TestSupport.MovieReplay;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.AssetExtraction.World.Artwork;
using SuperMetroid.Core.Input;
using static SuperMetroid.Verification.Enemies.Deaths.EnemyDeathProjectilePrograms;
using SuperMetroid.Core.Enemies.Deaths;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Bosses.Ridley;
using SuperMetroid.Core.Bosses.Ridley.Artwork;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Enemies.Drawing;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Enemies;
using SuperMetroid.Core;
using SuperMetroid.TestSupport.VerificationOracles;
namespace SuperMetroid.Verification.Bosses.Ridley;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.World;

/// <summary>Checks Ridley's death breakup fragments, instruction programs, installed OAM, and expiry.</summary>
internal static class RidleyBreakupProgramsVerification
{

    /// <summary>
    /// Runs his death's explosions while he carries Samus through his body's main AI, which releases her and breaks him up,
    /// and compares the pictures all 29 breakup programs show with native data (#2554).
    /// </summary>
    internal static void VerifyRidleyBreakupPrograms()
    {
        var enemies = EnemySystemFixtures.New(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance, services: new VerificationRoomEnemyServices(null));
        (RoomEnemyPlace body, NorfairRidleyState state) = NorfairRidleyDeathFixtures.PlaceRidley(enemies);
        enemies.VerificationServices().NextRandom = () => 1;
        var process = new Action<RoomEnemyPlace, SamusState, RoomLevelData, Position16<RoomPixels>, GameplayAction>(EnemyAnimationPlayer.AdvanceAnimation);
        state.GrabState = RidleyGrabState.CarryingSamus;
        RidleyTailFixtures.BlankLinks(state.Tail);
        foreach (RidleyTailLink link in state.Tail.Links)
            link.Position = new Position16<RoomPixels>(128, 128);
        state.Tail.Tip.Angle = RidleyTailAngle.FromStepsFromStraightDown(16);
        NorfairRidleyDeathFixtures.RunPhase(body, state, NorfairRidleyPhase.DeathExplosions, 1, new SamusState(GameplayAudioRequests.Standalone(), InstalledFixtures.FramesBeyondAnimation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence(), SuperMetroid.Core.Samus.SamusCompatibility.ForNewSamus(), new SuperMetroid.Core.Samus.Projectiles.BeamChargeState(), global::SuperMetroid.TestSupport.RepositoryInstallation.Verification.InstalledFixtures.SamusArtwork()));
        AssertEqual(RidleyGrabState.Released, state.GrabState, "Ridley death releases Samus before breakup");
        RoomEnemyPlace[] fragments = NorfairRidleyDeathFixtures.Pieces(enemies);
        AssertEqual(12, fragments.Length, "Ridley death spawns all twelve body/tail fragments");
        foreach (var fragment in fragments)
        {
            AssertEqual(CommonEnemyEmptyFrameToolingDefinitions.EmptySpritemap, fragment.SpritemapPointer,
                "native breakup spawn installs empty frame before instruction processing");
            process(fragment, RoomEnemyFrameFixtures.StandaloneSamus(), RoomEnemyFrameFixtures.OpenRoom(new(64, 64)), Position16<RoomPixels>.Zero, 0);
            AssertTrue(!CommonEnemyEmptyFrames.DrawsNothing(fragment.Frame), "first breakup instruction replaces empty frame");
        }
        AssertEqual<EnemyFrameId?>(EnemyFrameId.RidleyTailTipPointingToward157, fragments[0].Frame,
            "reported tail orientation selects the second tip pose ($A6:CA9B)");
        var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        byte[] stockBytes = EnemySpritemapFiles.Extract(rom);
        var stock = EnemySpritemapDocuments.Load(new MemoryStream(stockBytes));
        // VerifyRidleyAnimations matches each typed breakup program to its native list; this draws every pose each death's
        // pieces show, his tail tip's under every orientation it points in.
        var shownPoses = new HashSet<EnemyFrameId>();
        foreach (RoomEnemyPlace fragment in Enumerable.Range(0, 32).SelectMany(BreakupPieces))
        {
            EnemyFrameId frame = NorfairRidleyDeathFixtures.ShowPicture(fragment);
            if (!shownPoses.Add(frame))
                continue;
            ushort operand = (ushort)(RidleyNativeProgramOracle.BreakupList(frame) + 2);
            ushort expectedPointer = rom.ReadCartridgeWordFixedBank(RidleyNativeListAddresses.BodyBankNumber << 16 | operand);
            AssertEqual(expectedPointer, fragment.SpritemapPointer, "breakup selects native sprite");
            AssertEqual(RidleyBreakupFrameDefinitions.PictureShowingUpdates, fragment.AnimationClock.Hold,
                "breakup holds its picture's native frame duration before it sleeps");
            process(fragment, RoomEnemyFrameFixtures.StandaloneSamus(), RoomEnemyFrameFixtures.OpenRoom(new(64, 64)), Position16<RoomPixels>.Zero, 0);
            AssertTrue(fragment.AnimationClock.IsStopped, "breakup sleeps on its picture, its clock stopped");
            AssertEqual(expectedPointer, fragment.SpritemapPointer, "breakup holds its picture");
            var actual = new OamBuffer();
            stock.DrawEnemySpritemap(actual, new NativeEnemySpritemapDraw { Bank = 0xa6, Pointer = expectedPointer, Origin = new Position16<ScreenPixels>(128, 112), PaletteBits = 0x0e00, BaseTileIndex = 0, ClipVerticalWrap = false, OriginYIsOnScreen = true });
            var expected = new OamBuffer();
            ImportedSpritemapOracle.DrawEnemy(rom, expected, new NativeEnemySpritemapDraw { Bank = 0xa6, Pointer = expectedPointer, Origin = new(128, 112), PaletteBits = 0x0e00, BaseTileIndex = 0 });
            AssertEqual(expected.NextByteOffset, actual.NextByteOffset, "breakup OAM count");
            AssertTrue(expected.LowTable.SequenceEqual(actual.LowTable), "breakup exact OAM bytes");
            AssertTrue(expected.HighTable.SequenceEqual(actual.HighTable), "breakup exact OAM size/X bits");
        }
        AssertTrue(RidleyNativeListAddresses.BreakupEntries.Select(entry => entry.Frame).ToHashSet().SetEquals(shownPoses),
            "his deaths' pieces show every breakup pose");
        // The first piece flies by its own main AI until its lifetime ends, when it becomes a small enemy death where it
        // last stood.
        var expiring = fragments[0];
        FixedPosition<RoomPixels> spawned = expiring.Position;
        int flown = NorfairRidleyDeathFixtures.FlyUntilExpiry(expiring, RoomEnemyFrameFixtures.StandaloneSamus());
        AssertTrue(expiring.Occupant is null, "native breakup expiry clears the actor immediately");
        // Its last update moves it before its lifetime ends: the explosion stands where that flight ends.
        FixedPosition<RoomPixels> landed = NorfairRidleyDeathFixtures.Flight(spawned, NorfairRidleyDeathFixtures.SpeedFrom(1), flown);
        var explosion = enemies.Projectiles.EnemyProjectiles.Single(p => p.Kind == RoomEnemyProjectileKind.EnemyDeathExplosion);
        AssertEqual(landed.X.Whole, explosion.Position.X.Whole, "fragment death effect retains final X");
        AssertEqual(landed.Y.Whole, explosion.Position.Y.Whole, "fragment death effect retains final Y");
        AssertEqual(EnemyDefinitionId.RidleyBreakupPiece, enemies.DeathsAndDrops.Explosions.Of(explosion).Dropper?.DefinitionId, "fragment drop retains native header");
        AssertEqual(EnemyDeathAndDropSegment.SmallExplosion,
            ProgramOf(explosion), "fragment expiry selects native small death animation");
        AssertEqual(1, enemies.RoomStatus.EnemiesKilled, "fragment death updates shared kill count");
        Console.WriteLine("Ridley breakup: death while grabbing releases Samus and runs all twelve fragment programs without cartridge access.");
        Console.WriteLine("All 29 breakup programs match native mechanics, frame selection, sleep and installed OAM.");

        // The pieces of a death with his tail tip turned (death % 16) steps from straight down, facing left for the first
        // sixteen deaths and turning for the rest.
        static IEnumerable<RoomEnemyPlace> BreakupPieces(int death)
        {
            var system = EnemySystemFixtures.New(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance, services: new VerificationRoomEnemyServices(null));
            (RoomEnemyPlace dying, NorfairRidleyState ridley) = NorfairRidleyDeathFixtures.PlaceRidley(system);
            system.VerificationServices().NextRandom = () => 1;
            RidleyTailFixtures.BlankLinks(ridley.Tail);
            foreach (RidleyTailLink link in ridley.Tail.Links)
                link.Position = new Position16<RoomPixels>(128, 128);
            ridley.Flight.FacingDirection = death < 16 ? RidleyFacing.Left : RidleyFacing.Turning;
            ridley.Tail.Tip.Angle = RidleyTailAngle.FromStepsFromStraightDown((death % 16) << 4);
            NorfairRidleyDeathFixtures.RunPhase(dying, ridley, NorfairRidleyPhase.DeathExplosions, 1, RoomEnemyFrameFixtures.StandaloneSamus());
            return NorfairRidleyDeathFixtures.Pieces(system);
        }
    }

    /// <summary>Runs the Ridley breakup program and artwork check.</summary>
    [VerificationCommand("--ridley-breakup-programs")]
    internal static int RidleyBreakupProgramsCommand()
    {
        Suite(nameof(VerifyRidleyBreakupPrograms), () => VerifyRidleyBreakupPrograms());
        return 0;
    }
}
