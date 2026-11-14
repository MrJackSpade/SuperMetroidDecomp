using SuperMetroid.AssetExtraction;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Core.Input;
using static SuperMetroid.Verification.Areas.Escape.EscapeRoomEffectsVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Hardware;
using static SuperMetroid.Verification.Samus.Grapple.GrappleDoorsVerification;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.RoomObjects;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Grapple;
using SuperMetroid.TestSupport.VerificationImporters;
using SuperMetroid.AssetExtraction.Video;
using static SuperMetroid.Verification.Harness.VerificationHelpersVerification;
namespace SuperMetroid.Verification.Enemies.Animals;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.World;

/// <summary>Verifies the escape-room rescue PLMs using imported room population and the native shot-block sequence.</summary>
internal static class EscapeAnimalBlocksVerification
{
    /// <summary>Checks the escape-room rescue wall against imported PLM behavior: grapple leaves it intact, while a beam shot runs the opening animation and raises the critters event.</summary>
    internal static void VerifyEscapeAnimalBlocks(string romPath)
    {
        var bus = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(romPath);
        RoomTerrainBlock[] terrain = Enumerable.Repeat(RoomTerrainBlock.Of(0x123, BlockMirroring.None, RoomCollisionType.SolidBlock), 256).ToArray();
        var level = CreateRoom(new(16, 16), terrain, new RoomBlockBehavior[terrain.Length]);
        var roomObjects = RoomObjectFixtures.Unbound();
        var system = new SharedGameState(new SharedRandom());
        // The real escape room population installs event ownership; its grey door may
        // continue stepping alongside the rescue animation, as it does in gameplay.
        RoomObjectFixtures.LoadPopulation(ref roomObjects, new RoomObjectRoom(level, system, AreaId.Crateria), RoomObjectPopulationImporter.Read(bus, 0x8412), new RoomObjectServices { Samus = new FixtureRoomSamus(() => null), Bosses = new FixtureAreaBossFlags { IsTorizoDefeated = () => true }, Events = new FixtureEventFlags { HasEvent = system.HasEvent, SetEvent = system.SetEvent } });
        var origin = new Position16<RoomBlocks>(15, 10);
        roomObjects.EscapeAnimals.SetupCrittersEscapeBlock(level, origin);
        AssertEqual(0x4f, RoomBlockBehaviorCodec.Encode(level.GetCollisionBlock(new(15, 10)).Behavior), "rescue origin has the critters-escape behaviour");
        for (int y = 11; y <= 12; y++)
            AssertEqual(RoomTerrainBlock.Of(0x123, BlockMirroring.None, RoomCollisionType.VerticalExtension), level.GetCollisionBlock(new(15, checked((ushort)y))).Terrain, "rescue extensions preserve visual tile");
        for (int y = 10; y <= 12; y++)
        {
            var samus = new SamusState(InstalledFixtures.FramesBeyondAnimation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence(), SuperMetroid.Core.Samus.SamusCompatibility.ForNewSamus(), new SuperMetroid.Core.Samus.Projectiles.BeamChargeState()) { Position = FixedPosition<RoomPixels>.FromWhole(new(248, (ushort)(y * 16 + 8))) };
            SeedStationaryGrappleCollisionProbe(samus);
            int before = roomObjects.SlotTable.ActiveCount;
            SamusGrappleMovement.StepFiring(level, samus,
                ControllerButton.X, roomObjects);
            AssertEqual(GrapplePhase.CancelPending, samus.Grapple.Phase,
                "grapple stops on rescue origin or extension");
            AssertEqual(before, roomObjects.SlotTable.ActiveCount, "zero grapple word cannot allocate the rescue animation");
            AssertEqual(0x4f, RoomBlockBehaviorCodec.Encode(level.GetCollisionBlock(new(15, 10)).Behavior), "grapple leaves rescue wall intact");
            AssertTrue(!system.HasEvent(StoryEvent.CrittersEscaped), "grapple alone cannot rescue animals");
        }
        AssertTrue(!roomObjects.TrySpawnProjectileShotBlock(level, origin, 0x4f, SamusProjectileType.None, true), "native zero grapple word rejected");
        AssertTrue(roomObjects.TrySpawnProjectileShotBlock(level, origin, 0x4f, SamusProjectileType.Beam(SamusBeamCombination.Wave, charged: false), true), "nonzero beam opens rescue wall");
        AssertTrue(!system.HasEvent(StoryEvent.CrittersEscaped), "rescue event waits for animation");
        for (int frame = 0; frame < 16; frame++)
            roomObjects.Step(level);
        AssertTrue(system.HasEvent(StoryEvent.CrittersEscaped), "native animation sets rescue event");
        for (int y = 10; y <= 12; y++)
            AssertEqual(RoomTerrainBlock.Of(0xff, BlockMirroring.None, RoomCollisionType.SolidBlock), level.GetCollisionBlock(new(15, checked((ushort)y))).Terrain, "native rescue opening clears artwork but remains solid to Samus");
        Console.WriteLine("Animal rescue: native setup, ROM break animation, cleared wall and event verified.");
    }

    /// <summary>Runs the animal rescue wall and escape-room effects checks against the supplied cartridge image.</summary>
    [VerificationCommand("--escape-animals", "argument1")]
    internal static int EscapeAnimalsCommand(string[] args)
    {
        Suite(nameof(VerifyEscapeAnimalBlocks), () => VerifyEscapeAnimalBlocks(args[1]));
        foreach (FullRunSuitesVerification.RegisteredSuite suite in EscapeRoomEffectsSuites())
            Suite(suite.Name, suite.Body);
        return 0;
    }
}
