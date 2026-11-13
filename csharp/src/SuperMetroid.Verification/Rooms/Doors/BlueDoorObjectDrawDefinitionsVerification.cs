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
using SuperMetroid.Core.Progression;
using SuperMetroid.Verification.Harness;

namespace SuperMetroid.Verification.Rooms.Doors;

/// <summary>Checks compiled blue-door PLM programs, physical draws, and editable cap artwork.</summary>
internal static class BlueDoorObjectDrawDefinitionsVerification
{
    /// <summary>Runs all four door orientations through opening, closing, and closed lists with native source reads denied.</summary>
    internal static void VerifyBlueDoorObjectDrawDefinitions(CartridgeImportAddressSpace rom)
    {
        Suite(nameof(VerifyBlueDoorProgramDefinitions), () => VerifyBlueDoorProgramDefinitions(rom));

        RoomObjectShotBlockDrawDefinitions.DrawList[] lists =
            BlueDoorObjectDrawDefinitionsTooling.All.OrderBy(list => RoomObjectDrawAddressCodec.Of(list.Draw)).ToArray();
        InstalledRoomObjectBlueDoorVisuals stock = InstalledRoomObjectBlueDoorVisuals.Stock();
        ushort leftFirstDraw = CartridgeImportSource.Require(rom).ReadCartridgeWord((0x840000 | (DoorObjectNativeListAddresses.Blue(ColoredDoorOrientation.Left).Opening + 5)));
        AssertEqual("left-frame-1", RoomObjectBlueDoorVisualFiles.VisualId(NativeObjectDraws.At<BlueDoorCaps.Draw>(leftFirstDraw)),
            "the left blue door opens on its first opening frame");
        AssertEqual(RoomLevelWordCodec.Decode(CartridgeImportSource.Require(rom).ReadCartridgeWord(0x840000 | (leftFirstDraw + 2))).Appearance,
            stock.Image(NativeObjectDraws.At<BlueDoorCaps.Draw>(leftFirstDraw)).At(Offset16<RoomBlocks>.Zero),
            "stock blue-door visual retains native tile choice");
        Suite(nameof(VerifyBlueCapGeometry), () => VerifyBlueCapGeometry(rom));
        Suite(nameof(VerifyBlueCapCollision), () => VerifyBlueCapCollision(rom));
        Suite(nameof(VerifyBlueCapVisuals), () => VerifyBlueCapVisuals(rom));

        foreach (ColoredDoorOrientation orientation in Enum.GetValues<ColoredDoorOrientation>())
        {
            ushort openingList = DoorObjectNativeListAddresses.Blue(orientation).Opening;
            ushort firstDraw = CartridgeImportSource.Require(rom).ReadCartridgeWord((0x840000 | (openingList + 5)));
            AssertTrue(RoomObjectBlueDoorVisualFiles.TryGet(NativeObjectDraws.At<BlueDoorCaps.Draw>(firstDraw), out var selected),
                $"{orientation} opening program selects a compiled draw");

            const int width = 16;
            const int origin = 4 + 4 * width;
            var level = new RoomLevelData(new Extent16<RoomBlocks>(width, width), new RoomLevelPlanes(new RoomTerrainBlock[width * width], new RoomBlockBehavior[width * width], new RoomBlockAppearance[width * width]), RoomLevelData.NothingBeyondGrid());
            // A blue cap's closing turns into the grey door's frames too, so the fixture binds every installed picture (#1557).
            var roomObjects = RoomObjectFixtures.Unbound();
            RoomBlockBehavior behavior = RoomBlockBehavior.Reacting(BlockReactionDefinitions.BlueDoorCaps.ReactionFor(orientation));
            AssertTrue(roomObjects.DoorObjects.TrySpawnBlueDoorOpening(level, level.BlockAt(origin), behavior,
                SamusProjectileType.None),
                $"{orientation} blue door allocates its native opening actor");
            var guarded = NewBlueDoorDrawReadGuard(rom, lists);
            roomObjects.Step(level);
            int stride = orientation is ColoredDoorOrientation.Left or ColoredDoorOrientation.Right
                ? width : 1;
            for (int block = 0; block < 4; block++)
            {
                RoomTerrainBlock written = level.GetCollisionBlockByIndex(origin + block * stride).Terrain;
                AssertEqual(selected.Runs.Span[0].Collisions.Span[block], written.Collision,
                    $"{orientation} native draw writes physical block {block} without ROM payload reads");
                AssertEqual(stock.Image(selected.Draw).At(selected.Runs.Span[0].CellAt(block)), written.Appearance,
                    $"{orientation} draw shows its installed picture at block {block}");
            }
            AssertEqual(0, guarded.ForbiddenReadAttempts,
                $"{orientation} opening avoids compiled program and draw ROM bytes");

            // Continue through every opening draw, the long air-cap hold and Delete.
            for (int frame = 0; frame < 128 && roomObjects.SlotTable.ActiveCount != 0; frame++)
                roomObjects.Step(level);
            AssertEqual(0, roomObjects.SlotTable.ActiveCount,
                $"{orientation} blue-door opening reaches its native Delete");

            // The cap closing behind Samus, placed by the door transition, closes, is made shootable again and leaves.
            level.SetBehavior(origin, RoomBlockBehavior.None);
            var closingDoor = new DoorDefinition(
                Name: "fixture door",
                DestinationRoom: FixtureRooms.UnloadedDestination,
                Orientation: new(orientation switch
                {
                    ColoredDoorOrientation.Left => DoorDirection.Left,
                    ColoredDoorOrientation.Right => DoorDirection.Right,
                    ColoredDoorOrientation.Up => DoorDirection.Up,
                    ColoredDoorOrientation.Down => DoorDirection.Down,
                    _ => throw new InvalidOperationException($"Undefined {nameof(ColoredDoorOrientation)} {orientation}."),
                }, DoorClosingBehavior.BlueDoorCloses),
                RoomObjectBlock: new(4, 4),
                DestinationScreen: new(0, 0),
                SamusSpeed: DoorSamusSpeed.None,
                Setup: DoorSetupDefinitions.None);
            AssertTrue(roomObjects.DoorObjects.TrySpawnDoorClosingObject(level, closingDoor, new SharedGameState(new SharedRandom())),
                $"{orientation} places the blue cap closing behind Samus");
            AssertTrue(roomObjects.PopulationSlots.Single().Behaviour is BlueDoorCapObject { Phase: BlueDoorCapPhase.Closing } closer && closer.Orientation == orientation,
                $"{orientation} closing cap faces the travel direction");
            for (int frame = 0; frame < 20 && roomObjects.SlotTable.ActiveCount != 0; frame++)
                roomObjects.Step(level);
            AssertEqual(0, roomObjects.SlotTable.ActiveCount,
                $"{orientation} closing cap reaches its deletion");
            AssertEqual(RoomBlockBehaviorCodec.Encode(behavior),
                RoomBlockBehaviorCodec.Encode(level.GetCollisionBlockByIndex(origin).Behavior),
                $"{orientation} closed cap restores the blue-door BTS");
            AssertEqual(0, guarded.ForbiddenReadAttempts,
                $"{orientation} complete blue-door lists avoid all compiled ROM sources");
        }

        Suite(nameof(VerifyBlueDoorVisualInstallation), () => VerifyBlueDoorVisualInstallation(rom));

        Console.WriteLine("  Blue-door PLMs: 196 compiled program bytes, all opening/closing/closed lists, twenty physical draws and sixteen compatible visual identities pass with source reads forbidden.");
    }

    /// <summary>Compares the blue caps' typed programs with their bank-$84 lists.</summary>
    private static void VerifyBlueDoorProgramDefinitions(CartridgeImportAddressSpace rom) =>
        Suite(nameof(RoomObjectProgramsVerification.VerifyBlueDoorPrograms), () => RoomObjectProgramsVerification.VerifyBlueDoorPrograms(rom));

    /// <summary>Checks the extracted stock visuals and that a malformed installed visual file fails loudly where it is read.</summary>
    private static void VerifyBlueDoorVisualInstallation(CartridgeImportAddressSpace rom)
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
