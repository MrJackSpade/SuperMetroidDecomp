using SuperMetroid.AssetExtraction;
using SuperMetroid.TestSupport.MovieReplay;
using SuperMetroid.Core;
using static SuperMetroid.Verification.Rooms.Doors.LookupBlueCapVerification;
using static SuperMetroid.Verification.Rooms.Doors.LookupBlueDoorStockVerification;
using SuperMetroid.Verification.Rooms.RoomObjects;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Rooms.Doors;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.RoomObjects;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Installation;
using SuperMetroid.AssetExtraction.Rooms;
using SuperMetroid.TestSupport.CpuBusTestDoubles;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Core.World;
using SuperMetroid.Core.Progression;

namespace SuperMetroid.Verification.Rooms.Doors;

/// <summary>Checks compiled blue-door PLM programs, physical draws, and editable cap artwork.</summary>
internal static class BlueDoorObjectDrawDefinitionsVerification
{
    /// <summary>Runs all four door orientations through opening, closing, and closed lists with native source reads denied.</summary>
    internal static void VerifyBlueDoorObjectDrawDefinitions(SuperMetroidAddressSpace rom)
    {
        Suite(nameof(VerifyBlueDoorProgramDefinitions), () => VerifyBlueDoorProgramDefinitions(rom));

        RoomObjectShotBlockDrawDefinitions.DrawList[] lists =
            BlueDoorObjectDrawDefinitionsTooling.All.OrderBy(list => RoomObjectDrawAddressCodec.Of(list.Draw)).ToArray();
        RoomObjectVisualEntry[] entries = RoomObjectBlueDoorVisualFiles.Editable.Select(draw =>
            new RoomObjectVisualEntry(RoomObjectBlueDoorVisualFiles.VisualId(draw.Draw),
                draw.Runs.Span[0].Blocks.Span
                    .SelectToArray(block => block.Appearance))).ToArray();
        InstalledRoomObjectBlueDoorVisuals stock = InstalledRoomObjectBlueDoorVisuals.Stock();
        ushort leftFirstDraw = CartridgeImportSource.Require(rom).ReadCartridgeWord((0x840000 | (BlueEntry(BlueDoorObjectProgramDefinitions.OpeningPrograms[ColoredDoorOrientation.Left]) + 5)));
        AssertEqual(entries.Single(entry => entry.Id == "left-frame-1").Blocks[0], stock.GetAppearance(NativeObjectDraws.At<BlueDoorCaps.Draw>(leftFirstDraw), 0),
            "stock blue-door visual retains native tile choice");
        Suite(nameof(VerifyBlueCapGeometry), () => VerifyBlueCapGeometry(rom));
        Suite(nameof(VerifyBlueCapCollision), () => VerifyBlueCapCollision(rom));
        Suite(nameof(VerifyBlueCapVisuals), () => VerifyBlueCapVisuals(rom));

        foreach (ColoredDoorOrientation orientation in Enum.GetValues<ColoredDoorOrientation>())
        {
            ushort openingList = BlueEntry(BlueDoorObjectProgramDefinitions.OpeningPrograms[orientation]);
            ushort firstDraw = CartridgeImportSource.Require(rom).ReadCartridgeWord((0x840000 | (openingList + 5)));
            AssertTrue(RoomObjectBlueDoorVisualFiles.TryGet(NativeObjectDraws.At<BlueDoorCaps.Draw>(firstDraw), out var selected),
                $"{orientation} opening program selects a compiled draw");

            const int width = 16;
            const int origin = 4 + 4 * width;
            var level = new RoomLevelData(new Extent16<RoomBlocks>(width, width), new RoomLevelPlanes(new RoomTerrainBlock[width * width], new RoomBlockBehavior[width * width], new RoomBlockAppearance[width * width]), RoomLevelData.NothingBeyondGrid());
            var roomObjects = RoomObjectFixtures.Unbound(new RoomObjectVisuals { RoomObjectBlueDoorVisuals = stock });
            RoomBlockBehavior behavior = RoomBlockBehavior.Reacting(BlockReactionDefinitions.BlueDoorCaps.ReactionFor(orientation));
            AssertTrue(roomObjects.DoorObjects.TrySpawnBlueDoorOpening(level, origin, behavior,
                SamusProjectileType.None),
                $"{orientation} blue door allocates its native opening actor");
            var guarded = NewBlueDoorDrawReadGuard(rom, lists);
            roomObjects.Step(level);
            int stride = orientation is ColoredDoorOrientation.Left or ColoredDoorOrientation.Right
                ? width : 1;
            for (int block = 0; block < 4; block++)
                AssertEqual(selected.Runs.Span[0].Blocks.Span[block],
                    level.GetCollisionBlockByIndex(origin + block * stride).Terrain,
                    $"{orientation} native draw writes physical block {block} without ROM payload reads");
            AssertEqual(0, guarded.ForbiddenReadAttempts,
                $"{orientation} opening avoids compiled program and draw ROM bytes");

            // Continue through every opening draw, the long air-cap hold and Delete.
            for (int frame = 0; frame < 128 && roomObjects.SlotTable.ActiveCount != 0; frame++)
                roomObjects.Step(level);
            AssertEqual(0, roomObjects.SlotTable.ActiveCount,
                $"{orientation} blue-door opening reaches its native Delete");

            foreach (BlueDoorProgram program in new[] { BlueDoorObjectProgramDefinitions.ClosingPrograms[orientation], BlueDoorObjectProgramDefinitions.ClosedPrograms[orientation] })
            {
                AssertTrue(roomObjects.DoorObjects.TrySpawnBlueDoorOpening(level, origin, behavior,
                    SamusProjectileType.None),
                    $"{orientation} allocates actor for {program}");
                roomObjects.StartSoleProgramForVerification(RoomObjectProgramStart.Typed(program));
                for (int frame = 0; frame < 20 && roomObjects.SlotTable.ActiveCount != 0; frame++)
                    roomObjects.Step(level);
                AssertEqual(0, roomObjects.SlotTable.ActiveCount,
                    $"{orientation} {program} reaches its deletion");
                AssertEqual(RoomBlockBehaviorCodec.Encode(behavior),
                    RoomBlockBehaviorCodec.Encode(level.GetCollisionBlockByIndex(origin).Behavior),
                    $"{orientation} {program} restores the blue-door BTS");
            }
            AssertEqual(0, guarded.ForbiddenReadAttempts,
                $"{orientation} complete blue-door lists avoid all compiled ROM sources");
        }

        Suite(nameof(VerifyBlueDoorVisualInstallation), () => VerifyBlueDoorVisualInstallation(rom));

        Console.WriteLine("  Blue-door PLMs: 196 compiled program bytes, all opening/closing/closed lists, twenty physical draws and sixteen compatible visual identities pass with source reads forbidden.");
    }

    /// <summary>Compares the blue caps' typed programs with their bank-$84 lists.</summary>
    private static void VerifyBlueDoorProgramDefinitions(SuperMetroidAddressSpace rom) =>
        Suite(nameof(RoomObjectProgramsVerification.VerifyBlueDoorPrograms), () => RoomObjectProgramsVerification.VerifyBlueDoorPrograms(rom));

    /// <summary>The native list a blue program begins at.</summary>
    private static ushort BlueEntry(BlueDoorProgram program) =>
        DoorObjectNativeListAddresses.BlueEntries.Single(entry => entry.Program == program).Entry;

    /// <summary>Checks the extracted stock visuals and that a malformed installed visual file fails loudly where it is read.</summary>
    private static void VerifyBlueDoorVisualInstallation(SuperMetroidAddressSpace rom)
    {
        using PrivateInstallationCopy copy = InstalledFixtures.CreatePrivateCopyOf(layout => layout.RoomObjectBlueDoorVisualDirectory);
        GameInstallation installation = copy.Installation;
        RoomObjectBlueDoorVisualFilesConstants.ValidateStock(
            installation.RoomObjectBlueDoorVisualDirectory);
        VerifyBlueDoorStockMapping(rom, installation.LoadRoomObjectBlueDoorVisuals());

        string stockPath = Path.Combine(installation.RoomObjectBlueDoorVisualDirectory,
            RoomObjectBlueDoorVisualFiles.VisualFileName);
        File.WriteAllText(stockPath, "{}");
        AssertThrows<InvalidDataException>(
            () => installation.LoadRoomObjectBlueDoorVisuals(),
            "tampered blue-door stock fails loudly where it is read");
    }

    /// <summary>Forbids production reads from compiled blue-door instructions and all selected draw-list payloads.</summary>
    private static CartridgeReadGuard NewBlueDoorDrawReadGuard(
        ISnesAddressSpace source,
        RoomObjectShotBlockDrawDefinitions.DrawList[] lists) =>
        new CartridgeReadGuard(source)
            .Forbid(address => address is >= 0x84c489 and <= 0x84c54c,
                address => $"Production blue-door instruction reread bank-$84 byte ${address:X6}.")
            .Forbid(address => lists.Any(list =>
                    address >= (0x840000 | RoomObjectDrawAddressCodec.Of(list.Draw)) &&
                    address < (0x840000 | RoomObjectDrawAddressCodec.Of(list.Draw)) + 12),
                address => $"Production blue-door draw reread bank-$84 payload ${address:X6}.");
}
