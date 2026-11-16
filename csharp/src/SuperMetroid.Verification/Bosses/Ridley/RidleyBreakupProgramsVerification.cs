using SuperMetroid.TestSupport.MovieReplay;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.AssetExtraction.World.Artwork;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Animation;
using static SuperMetroid.Verification.Enemies.Deaths.EnemyDeathProjectilePrograms;
using SuperMetroid.Core.Enemies.Deaths;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Bosses.Ridley;
using SuperMetroid.Core.Bosses.Ridley.Artwork;
using SuperMetroid.Core.Enemies;
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

    /// <summary>Drives the grab-release death branch and compares all 29 fragment programs with native data.</summary>
    internal static void VerifyRidleyBreakupPrograms()
    {
        var enemies = EnemySystemFixtures.New(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance, services: new VerificationRoomEnemyServices(null));
        enemies.VerificationServices().NextRandom = () => 1;
        Action<RoomEnemyPlace, NorfairRidleyState, SamusState> death = enemies.NorfairRidleyBehavior.Death.TickNorfairRidleyDeathExplosions;
        var process = new Action<RoomEnemyPlace, SamusState, RoomLevelData, Position16<RoomPixels>, GameplayAction>(EnemyAnimationPlayer.AdvanceAnimation);
        var body = enemies.Places.InUpdateOrder[0];
        body.Bind(EnemyDefinitionId.Ridley, body.Definition);
        var state = new NorfairRidleyState { GrabState = RidleyGrabState.CarryingSamus, PhaseTimer = 0 };
        for (int index = 0; index < 7; index++)
        {
            state.Tail.Segments[index] = new RidleyTailSegment
                { Angle = RidleyTailAngle.FromStepsFromStraightDown(index == 6 ? 16 : 0), Position = new Position16<RoomPixels>(128, 128) };
        }
        body.HoldRidley(state);
        death(body, state, new SamusState(GameplayAudioRequests.Standalone(), InstalledFixtures.FramesBeyondAnimation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence(), SuperMetroid.Core.Samus.SamusCompatibility.ForNewSamus(), new SuperMetroid.Core.Samus.Projectiles.BeamChargeState(), global::SuperMetroid.TestSupport.RepositoryInstallation.Verification.InstalledFixtures.SamusArtwork()));
        AssertEqual(RidleyGrabState.Released, state.GrabState, "Ridley death releases Samus before breakup");
        var fragments = enemies.Places.InUpdateOrder.Where(place => place.DefinitionId == EnemyDefinitionId.RidleyBreakupPiece).ToArray();
        AssertEqual(12, fragments.Length, "Ridley death spawns all twelve body/tail fragments");
        AssertEqual(RidleyBreakupFrame.TailTipPointingDownDownLeft, fragments[0].RequireSpeciesState<RidleyExplosionFragmentState>().Picture,
            "reported tail orientation selects the second tip pose ($A6:CA9B)");
        foreach (var fragment in fragments)
        {
            AssertEqual(CommonEnemyEmptyFrameToolingDefinitions.EmptySpritemap, fragment.SpritemapPointer,
                "native breakup spawn installs empty frame before instruction processing");
            process(fragment, RoomEnemyFrameFixtures.StandaloneSamus(), RoomEnemyFrameFixtures.OpenRoom(new(64, 64)), Position16<RoomPixels>.Zero, 0);
            AssertTrue(!CommonEnemyEmptyFrames.DrawsNothing(fragment.Frame), "first breakup instruction replaces empty frame");
        }
        var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        byte[] stockBytes = EnemySpritemapFiles.Extract(rom);
        var stock = EnemySpritemapDocuments.Load(new MemoryStream(stockBytes));
        // VerifyRidleyAnimations matches each typed breakup program to its native list; this draws every pose.
        foreach (RidleyBreakupFrame frame in Enum.GetValues<RidleyBreakupFrame>())
        {
            ushort operand = (ushort)(RidleyNativeProgramOracle.BreakupList(frame) + 2);
            ushort expectedPointer = CartridgeImportSource.Require(rom).ReadCartridgeWordFixedBank(RidleyNativeListAddresses.BodyBankNumber << 16 | operand);
            var fragment = fragments[0];
            RidleyExplosionFragmentState spawned = fragment.RequireSpeciesState<RidleyExplosionFragmentState>();
            fragment.SetSpeciesState(new RidleyExplosionFragmentState { Velocity = spawned.Velocity, Lifetime = spawned.Lifetime, Picture = frame });
            fragment.AnimationClock.StepOnNextUpdate();
            process(fragment, RoomEnemyFrameFixtures.StandaloneSamus(), RoomEnemyFrameFixtures.OpenRoom(new(64, 64)), Position16<RoomPixels>.Zero, 0);
            AssertEqual(expectedPointer, fragment.SpritemapPointer, "breakup selects native sprite");
            AssertEqual((ushort)RidleyBreakupFrameDefinitions.PictureShowingUpdates, fragment.AnimationClock.Hold.RawValue,
                "breakup holds its picture's native frame duration before it sleeps");
            process(fragment, RoomEnemyFrameFixtures.StandaloneSamus(), RoomEnemyFrameFixtures.OpenRoom(new(64, 64)), Position16<RoomPixels>.Zero, 0);
            AssertEqual((ushort)0, fragment.AnimationClock.Hold.RawValue, "breakup sleeps on its picture, its clock left to wrap");
            AssertEqual(expectedPointer, fragment.SpritemapPointer, "breakup holds its picture");
            AssertEqual((ushort)(RidleyNativeProgramOracle.BreakupList(frame) + 4), RidleyNativeProgramOracle.BreakupPointer(fragment), "breakup sleeps after its one frame");
            var actual = new OamBuffer();
            stock.DrawEnemySpritemap(actual, new NativeEnemySpritemapDraw { Bank = 0xa6, Pointer = expectedPointer, Origin = new Position16<ScreenPixels>(128, 112), PaletteBits = 0x0e00, BaseTileIndex = 0, ClipVerticalWrap = false, OriginYIsOnScreen = true });
            var expected = new OamBuffer();
            ImportedSpritemapOracle.DrawEnemy(rom, expected, new NativeEnemySpritemapDraw { Bank = 0xa6, Pointer = expectedPointer, Origin = new(128, 112), PaletteBits = 0x0e00, BaseTileIndex = 0 });
            AssertEqual(expected.NextByteOffset, actual.NextByteOffset, "breakup OAM count");
            AssertTrue(expected.LowTable.SequenceEqual(actual.LowTable), "breakup exact OAM bytes");
            AssertTrue(expected.HighTable.SequenceEqual(actual.HighTable), "breakup exact OAM size/X bits");
        }
        var expiring = fragments[0];
        ushort deathX = expiring.Position.X.Whole, deathY = expiring.Position.Y.Whole;
        RidleyExplosionFragmentState expiringState = expiring.SpeciesState<RidleyExplosionFragmentState>()
            ?? throw new InvalidOperationException("The spawned fragment has no state.");
        expiringState.Lifetime = 0;
        expiringState.Velocity = default;
        expiring.Position = expiring.Position.WithFractionX(0).WithFractionY(0);
        enemies.NorfairRidleyBehavior.Death.RunNorfairRidleyExplosionMain(expiring, RoomEnemyFrameFixtures.StandaloneSamus());
        AssertEqual(EnemyDefinitionId.None, expiring.DefinitionId, "native breakup expiry clears the actor immediately");
        var explosion = enemies.Projectiles.EnemyProjectiles.Single(p => p.Kind == RoomEnemyProjectileKind.EnemyDeathExplosion);
        AssertEqual(deathX, explosion.Position.X.Whole, "fragment death effect retains final X");
        AssertEqual(deathY, explosion.Position.Y.Whole, "fragment death effect retains final Y");
        AssertEqual(EnemyDefinitionId.RidleyBreakupPiece, explosion.DropDefinitionId, "fragment drop retains native header");
        AssertEqual(EnemyDeathProjectileSegment.SmallExplosion,
            ProgramOf(explosion), "fragment expiry selects native small death animation");
        AssertEqual(1, enemies.RoomStatus.EnemiesKilled, "fragment death updates shared kill count");
        Console.WriteLine("Ridley breakup: death while grabbing releases Samus and runs all twelve fragment programs without cartridge access.");
        Console.WriteLine("All 29 breakup programs match native mechanics, frame selection, sleep and installed OAM.");
    }

    /// <summary>Runs the Ridley breakup program and artwork check.</summary>
    [VerificationCommand("--ridley-breakup-programs")]
    internal static int RidleyBreakupProgramsCommand()
    {
        Suite(nameof(VerifyRidleyBreakupPrograms), () => VerifyRidleyBreakupPrograms());
        return 0;
    }
}
