using SuperMetroid.Core.Samus.Poses;
using static SuperMetroid.Verification.Rooms.Doors.LookupRetailDoorHeadersVerification;
using static SuperMetroid.Verification.Rooms.Doors.LookupRetailDoorListsVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Rooms.Doors;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.TestSupport.CpuBusTestDoubles;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.AssetExtraction.Rooms;
using SuperMetroid.AssetExtraction.World.RoomStates;
using SuperMetroid.Core.Bosses.MotherBrain;
using SuperMetroid.Core.Rooms.RoomObjects;
namespace SuperMetroid.Verification.Rooms.Doors;
using SuperMetroid.Verification.Harness;

/// <summary>Checks compiled physical door headers, room door lists, and collision resolution.</summary>
internal static class DoorDefinitionsVerification
{
    /// <summary>Runs retail header and list comparisons plus a no-cartridge-read collision check.</summary>
    internal static void VerifyCompiledDoorDefinitions()
    {
        var bus = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(
            Path.GetFullPath("Super Metroid.smc"));
        AssertEqual(SuperMetroid.AssetExtraction.Cartridge.SupportedCartridge.Sha256.ToUpperInvariant(),
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(bus.Rom)), "Door header oracle revision");
        Suite(nameof(VerifyRetailDoorHeaders), () => VerifyRetailDoorHeaders(bus));
        Suite(nameof(VerifyRetailDoorListMapping), () => VerifyRetailDoorListMapping(bus));
        Suite(nameof(VerifyDoorBlockCollision), () => VerifyDoorBlockCollision());
        Suite(nameof(VerifySharedLevelDoorBlocks), () => VerifySharedLevelDoorBlocks());
        Console.WriteLine("Doors: all596 physical headers, full elevator overlap and261 room lists match original ROM; collision resolves without native reads.");
    }

    /// <summary>
    /// Confirms a door block leads where its room names (#2090): Samus's collision publishes the door its block leads through,
    /// a behaviour write takes the door away so the block leads nowhere, and Mother Brain's escape-door write leads its block
    /// into the escape.
    /// </summary>
    internal static void VerifyDoorBlockCollision()
    {
        var at = new Position16<RoomBlocks>(0, 0);
        var level = new RoomLevelData(new Extent16<RoomBlocks>(1, 1),
            new RoomLevelPlanes([RoomTerrainBlock.Of(0, BlockMirroring.None, RoomCollisionType.DoorBlock)], [RoomBlockBehavior.None], [RoomBlockAppearance.Of(0)]),
            RoomLevelData.NothingBeyondGrid(), [new RoomDoorBlock(at, DoorBlockExit.Physical(CrateriaDoorDefinitions.LandingSiteToParlor))]);
        SamusCollisionPose pose = SamusPoseId.ForwardFacingPowerSuitPose.ForRoomCollision();
        AssertEqual(CrateriaDoorDefinitions.LandingSiteToParlor, level.ResolveDoorCollision(level.GetCollisionBlock(at), pose).Door,
            "the door block leads through the door its room names");
        AssertEqual(CrateriaDoorDefinitions.LandingSiteToParlor, level.PendingDoorTransition,
            "the collision publishes the door's transition");

        level.SetBehavior(at, RoomBlockBehavior.None);
        AssertThrows<InvalidDataException>(() => level.ResolveDoorCollision(level.GetCollisionBlock(at), pose, publishDoorSideEffects: false),
            "a behaviour write takes the block's door away");

        RoomObjectLevelWrites.WriteDoorBlock(level, at, MotherBrainDeathDefinitions.EscapeDoor);
        AssertEqual(RoomCollisionType.DoorBlock, level.GetCollisionBlock(at).CollisionType, "the escape door's write makes a door block");
        AssertEqual(TourianDoorDefinitions.MotherBrainToTourianEscape1,
            level.ResolveDoorCollision(level.GetCollisionBlock(at), pose, publishDoorSideEffects: false).Door,
            "Mother Brain's escape door block leads into the escape's first room");
    }

    /// <summary>
    /// Confirms the installation leads a level several rooms share through each room's own doors (#2090): a save or map
    /// room's level holds the same door blocks in every room using it, and the rooms lead some of them through different
    /// doors.
    /// </summary>
    internal static void VerifySharedLevelDoorBlocks()
    {
        InstalledRoomLevelStreams streams = InstalledFixtures.RoomAssets.LevelStreams;
        var shared = RetailRooms.All
            .SelectMany(room => RoomStateLists.GetStates(room).Select(state => new RoomLevelUse(room, state.Level)))
            .Distinct()
            .GroupBy(use => use.Level)
            .Where(group => group.Count() > 1)
            .ToList();
        AssertTrue(shared.Count > 0, "rooms share levels");
        int differing = 0;
        foreach (var level in shared)
        {
            List<IReadOnlyList<RoomDoorBlock>> rooms = [.. level.Select(use => streams.DoorBlocks(use.Room, use.Level))];
            if (rooms.All(blocks => blocks.Count == 0))
                continue;
            HashSet<string> exits = [.. rooms.Select(blocks => string.Join(";", blocks.OrderBy(block => block.Block.Y).ThenBy(block => block.Block.X)
                .Select(block => $"{block.Block}:{block.Exit}")))];
            AssertTrue(exits.Count > 1, $"the rooms sharing level {level.Key} lead its door blocks through their own doors");
            differing++;
        }
        AssertTrue(differing > 0, "some shared level has door blocks");
    }

    /// <summary>
    /// Enumerates both physical door-header runs around the bank-$83 FX data gap, leaving out the door only the room no door
    /// reaches lists.
    /// </summary>
    internal static IEnumerable<ushort> EnumerateRetailDoorPointers()
    {
        for (int pointer = DoorHeaderRomDataConstants.PreFxBlockStart;
             pointer <= DoorHeaderRomDataConstants.PreFxBlockEnd;
             pointer += DoorHeaderRomDataConstants.RecordByteCount)
        {
            if (pointer != DoorHeaderRomDataConstants.UnreferencedRoomDoor)
                yield return checked((ushort)pointer);
        }

        for (int pointer = DoorHeaderRomDataConstants.PostFxBlockStart;
             pointer <= DoorHeaderRomDataConstants.PostFxBlockEnd;
             pointer += DoorHeaderRomDataConstants.RecordByteCount)
        {
            yield return checked((ushort)pointer);
        }
    }

    /// <summary>Reads a native door word solely for verification against compiled definitions.</summary>
    internal static ushort ReadVerificationWord(ISnesAddressSpace bus, int address) =>
        CartridgeImportSource.Require(bus).ReadCartridgeWord(address);

    /// <summary>Provides an unbacked address space that fails any attempted door collision read or write.</summary>
    internal static CartridgeReadGuard NewDoorNoReadAddressSpace() =>
        CartridgeReadGuard.Unbacked()
            .ForbidAll(address => $"Compiled door collision read address ${address:X6}.")
            .RejectWrites(address => $"Compiled door collision wrote address ${address:X6}.");

    /// <summary>Runs the compiled door definition checks against the retail ROM.</summary>
    [VerificationCommand("--door-definitions")]
    internal static int DoorDefinitionsCommand()
    {
        Suite(nameof(VerifyCompiledDoorDefinitions), () => VerifyCompiledDoorDefinitions());
        return 0;
    }
}
