using SuperMetroid.AssetExtraction;
using SuperMetroid.Core;
using SuperMetroid.Core.Hardware;
using static SuperMetroid.Verification.Rooms.Doors.EyeDoorObjectsVerification;
using static SuperMetroid.Verification.Rooms.Doors.LookupEyeDoorLayoutsVerification;
using SuperMetroid.Verification.Rooms.RoomObjects;
using static SuperMetroid.Verification.Rooms.Doors.LookupEyeDoorStockVerification;
using static SuperMetroid.Verification.Rooms.Doors.LookupEyeDoorVisualIdsVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using static SuperMetroid.Verification.Harness.SyntheticRoomCartridgeFixturesVerification;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms.Doors;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.RoomObjects;
using SuperMetroid.Core.Samus;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Installation;
using SuperMetroid.AssetExtraction.Rooms;
using SuperMetroid.TestSupport.CpuBusTestDoubles;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.TestSupport.VerificationImporters;
using SuperMetroid.AssetExtraction.Video;
namespace SuperMetroid.Verification.Rooms.Doors;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.World;

/// <summary>Checks eye-door PLM programs, mirrored physical draws, and installed visuals.</summary>
internal static class EyeDoorObjectDrawDefinitionsVerification
{
    /// <summary>Checks both eye-door orientations from compiled draw data through opening and room reload.</summary>
    internal static void VerifyEyeDoorObjectDrawDefinitions(CartridgeImportAddressSpace rom)
    {
        Suite(nameof(VerifyEyeDoorVisualIds), () => VerifyEyeDoorVisualIds());
        Suite(nameof(VerifyEyeDoorProgramDefinitions), () => VerifyEyeDoorProgramDefinitions(rom));
        Suite(nameof(VerifyEyeDoorLayoutGeometry), () => VerifyEyeDoorLayoutGeometry(rom));
        Suite(nameof(VerifyEyeDoorLayoutCollision), () => VerifyEyeDoorLayoutCollision(rom));
        Suite(nameof(VerifyEyeDoorLayoutVisuals), () => VerifyEyeDoorLayoutVisuals(rom));
        RoomObjectShotBlockDrawDefinitions.DrawList[] lists =
            RoomObjectEyeDoorVisualFiles.All.OrderBy(list => RoomObjectDrawAddressCodec.Of(list.Draw)).ToArray();
        RoomObjectShotBlockDrawDefinitions.DrawList[] entries = RoomObjectEyeDoorVisualFiles.Editable.ToArray();
        AssertEqual(24, lists.Length,
            "mirrored eye, middle and bottom components include both four-block opening clears");
        AssertEqual(23, entries.Length, "the mirrored clear preserves the existing authored visual identities");
        Suite(nameof(VerifyEyeDoorNativeDrawPath), () => VerifyEyeDoorNativeDrawPath(EyeDoorOrientation.Left, lists));
        Suite(nameof(VerifyEyeDoorNativeDrawPath), () => VerifyEyeDoorNativeDrawPath(EyeDoorOrientation.Right, lists));
        Suite(nameof(VerifyEyeDoorRetailProgramPath), () => VerifyEyeDoorRetailProgramPath(rom, EyeDoorOrientation.Left, lists));
        Suite(nameof(VerifyEyeDoorRetailProgramPath), () => VerifyEyeDoorRetailProgramPath(rom, EyeDoorOrientation.Right, lists));
        Suite(nameof(VerifyEyeDoorVisualInstallation), () => VerifyEyeDoorVisualInstallation(rom));
        Console.WriteLine(
            "  Eye doors: 622 compiled instruction bytes, guarded mirrored lifecycles, 24 physical draws and 23 compatible authored identities preserve collision.");
    }

    /// <summary>Checks eye-door instruction control, timing, draw, callback, and hit-count words.</summary>
    private static void VerifyEyeDoorProgramDefinitions(CartridgeImportAddressSpace rom)
    {
        Suite(nameof(RoomObjectProgramsVerification.VerifyEyeDoorPrograms), () => RoomObjectProgramsVerification.VerifyEyeDoorPrograms(rom));
    }

    /// <summary>Checks each eye-door component's collision blocks.</summary>
    private static void VerifyEyeDoorNativeDrawPath(
        EyeDoorOrientation orientation,
        RoomObjectShotBlockDrawDefinitions.DrawList[] lists)
    {
        const int width = 16;
        var bus = new TestAddressSpace();
        SeedEyeDoorFixtureRom(bus, orientation);
        ushort[] pointers = orientation == EyeDoorOrientation.Left
            ? [0x9c03, 0x9c2b, 0x9c3d]
            : [0x9c5b, 0x9c83, 0x9c95];
        SeedObjectProgramWords(bus, 0x041a, pointers[0]);
        SeedObjectProgramWords(bus, 0x050e, pointers[1]);
        SeedObjectProgramWords(bus, 0x060e, pointers[2]);

        var level = new RoomLevelData(new Extent16<RoomBlocks>(width, width), new RoomLevelPlanes(new RoomTerrainBlock[width * width], new RoomBlockBehavior[width * width], new RoomBlockAppearance[width * width]), RoomLevelData.NothingBeyondGrid());
        var roomObjects = RoomObjectFixtures.Unbound();
        var guarded = NewEyeDoorDrawGuard(bus, lists);
        AssertEqual(3, RoomObjectFixtures.LoadPopulation(ref roomObjects, new RoomObjectRoom(level, new SharedGameState(new SharedRandom()), AreaId.Brinstar), RoomObjectPopulationImporter.Read(guarded, 0x9000), new RoomObjectServices { Samus = new FixtureRoomSamus(() => new SamusState(InstalledFixtures.FramesBeyondAnimation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence(), SuperMetroid.Core.Samus.SamusCompatibility.ForNewSamus(), new SuperMetroid.Core.Samus.Projectiles.BeamChargeState())), Bosses = new FixtureAreaBossFlags { IsTorizoDefeated = () => false }, Enemies = new FixtureObjectEnemyEffects { EyeDoorProjectile = _ => { } } }),
            $"{orientation} eye-door three-component population loads");
        roomObjects.Step(level);
        InstalledRoomObjectEyeDoorVisuals pictures = InstalledRoomObjectEyeDoorVisuals.Stock();
        int[] origins = [4 * width + 7, 6 * width + 7, 8 * width + 7];
        for (int component = 0; component < pointers.Length; component++)
        {
            AssertTrue(RoomObjectEyeDoorVisualFiles.TryGet(NativeObjectDraws.At<EyeDoorDraws.Draw>(pointers[component]), out var list),
                $"{orientation} component {component} selects compiled draw data");
            RoomObjectShotBlockDrawDefinitions.Run run = list.Runs.Span[0];
            int stride = run.Direction == RoomObjectRunDirection.Vertical ? width : 1;
            for (int block = 0; block < run.Count; block++)
            {
                RoomTerrainBlock written = level.GetCollisionBlockByIndex(origins[component] + block * stride).Terrain;
                AssertEqual(run.Collisions.Span[block], written.Collision,
                    $"{orientation} component {component} draws physical block {block}");
                AssertEqual(pictures.Image(list.Draw).At(run.CellAt(block)), written.Appearance,
                    $"{orientation} component {component} shows its installed picture at block {block}");
            }
        }
        AssertEqual(0, guarded.ForbiddenReadAttempts,
            $"{orientation} eye-door first draws avoid bank-$84 payload reads");
    }

    /// <summary>Runs a retail eye-door program through Super Missile opening and reopened-room conversion.</summary>
    private static void VerifyEyeDoorRetailProgramPath(
        CartridgeImportAddressSpace rom,
        EyeDoorOrientation orientation,
        RoomObjectShotBlockDrawDefinitions.DrawList[] lists)
    {
        const int width = 16;
        const byte eyeX = 7;
        const byte eyeY = 4;
        const ushort doorBit = 5;
        ushort[] headers = orientation == EyeDoorOrientation.Left
            ? [RoomObjectNativeWords.Word(RoomObjectKind.EyeDoorEyeFacingLeft),
                RoomObjectNativeWords.Word(RoomObjectKind.EyeDoorFacingLeft),
                RoomObjectNativeWords.Word(RoomObjectKind.EyeDoorBottomFacingLeft)]
            : [RoomObjectNativeWords.Word(RoomObjectKind.EyeDoorEyeFacingRight),
                RoomObjectNativeWords.Word(RoomObjectKind.EyeDoorFacingRight),
                RoomObjectNativeWords.Word(RoomObjectKind.EyeDoorBottomFacingRight)];
        var bank84 = new byte[0x8000];
        for (int offset = 0; offset < bank84.Length; offset++)
            bank84[offset] = EyeDoorRetailProgramPathRomByte(rom, 0x848000 + offset);
        TestAddressSpace bus = CreateBank84Cartridge(bank84);
        const ushort population = 0x9000;
        SeedRoomObjectPopulation(bus, population,
        [
            unchecked((byte)headers[0]), LittleEndianBytes.HighByte(headers[0]), eyeX, eyeY,
            unchecked((byte)doorBit), 0,
            unchecked((byte)headers[1]), LittleEndianBytes.HighByte(headers[1]), eyeX, eyeY + 2,
            unchecked((byte)doorBit), 0,
            unchecked((byte)headers[2]), LittleEndianBytes.HighByte(headers[2]), eyeX, eyeY + 4,
            unchecked((byte)doorBit), 0,
            0, 0,
        ]);
        var level = new RoomLevelData(new Extent16<RoomBlocks>(width, width), new RoomLevelPlanes(new RoomTerrainBlock[width * width], new RoomBlockBehavior[width * width], new RoomBlockAppearance[width * width]), RoomLevelData.NothingBeyondGrid());
        var samus = new SamusState(InstalledFixtures.FramesBeyondAnimation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence(), SuperMetroid.Core.Samus.SamusCompatibility.ForNewSamus(), new SuperMetroid.Core.Samus.Projectiles.BeamChargeState())
        {
            Position = FixedPosition<RoomPixels>.FromWhole(new(eyeX * 16 + 8, eyeY * 16 + 8)),
        };
        var requests = new List<EyeDoorProjectileRequest>();
        var system = new SharedGameState(new SharedRandom());
        var roomObjects = RoomObjectFixtures.Unbound();
        var guarded = NewEyeDoorDrawGuard(bus, lists);
        AssertEqual(3, RoomObjectFixtures.LoadPopulation(ref roomObjects, new RoomObjectRoom(level, system, AreaId.Brinstar), RoomObjectPopulationImporter.Read(guarded, population), new RoomObjectServices { Samus = new FixtureRoomSamus(() => samus), Bosses = new FixtureAreaBossFlags { IsTorizoDefeated = () => false }, Enemies = new FixtureObjectEnemyEffects { EyeDoorProjectile = requests.Add } }),
            $"{orientation} retail eye-door program loads all three components");

        int eyeBlock = eyeY * width + eyeX;
        for (int frame = 0; frame < 1024 &&
             roomObjects.EyeDoorSnapshots.Single(door => door.Component == EyeDoorComponent.Eye)
                 .FrameBehavior != DoorsNativeWords.EyeDoorMissileWake; frame++)
            roomObjects.Step(level);
        AssertEqual(DoorsNativeWords.EyeDoorMissileWake,
            roomObjects.EyeDoorSnapshots.Single(door => door.Component == EyeDoorComponent.Eye)
                .FrameBehavior,
            $"{orientation} retail eye-door program arms its missile pre-instruction");
        AssertTrue(roomObjects.TryNotifyColoredDoorHit(new(eyeX, eyeY),
                SamusProjectileType.Of(SamusProjectileFamily.SuperMissile)),
            $"{orientation} retail eye door accepts a Super Missile collision");
        roomObjects.Step(level);
        for (int frame = 0; frame < 1024 && roomObjects.SlotTable.ActiveCount != 0; frame++)
            roomObjects.Step(level);
        AssertTrue(system.HasOpenedDoorBit(doorBit),
            $"{orientation} retail eye-door opening persists the door bit");
        AssertEqual(0, roomObjects.SlotTable.ActiveCount,
            $"{orientation} retail eye and passive components complete and delete");
        RoomCollisionBlock cap = level.GetCollisionBlockByIndex(eyeBlock - width);
        AssertEqual(RoomCollisionType.ShootableBlock, cap.CollisionType,
            $"{orientation} retail eye opening constructs the blue cap");
        AssertEqual(orientation == EyeDoorOrientation.Left
                ? RoomBlockBehaviorCodec.Encode(RoomBlockBehavior.Reacting(BlockReactionDefinitions.BlueDoorCaps.ReactionFor(ColoredDoorOrientation.Left)))
                : RoomBlockBehaviorCodec.Encode(RoomBlockBehavior.Reacting(BlockReactionDefinitions.BlueDoorCaps.ReactionFor(ColoredDoorOrientation.Right))),
            RoomBlockBehaviorCodec.Encode(cap.Behavior),
            $"{orientation} retail eye opening selects the blue-cap orientation");
        AssertEqual(0, guarded.ForbiddenReadAttempts,
            $"{orientation} retail eye-door lifecycle avoids instruction and draw ROM reads");

        var reopenedLevel = new RoomLevelData(new Extent16<RoomBlocks>(width, width), new RoomLevelPlanes(new RoomTerrainBlock[width * width], new RoomBlockBehavior[width * width], new RoomBlockAppearance[width * width]), RoomLevelData.NothingBeyondGrid());
        var reopened = RoomObjectFixtures.Unbound();
        AssertEqual(3, RoomObjectFixtures.LoadPopulation(ref reopened, new RoomObjectRoom(reopenedLevel, system, AreaId.Brinstar), RoomObjectPopulationImporter.Read(guarded, population), new RoomObjectServices { Samus = new FixtureRoomSamus(() => samus), Bosses = new FixtureAreaBossFlags { IsTorizoDefeated = () => false }, Enemies = new FixtureObjectEnemyEffects { EyeDoorProjectile = requests.Add } }),
            $"{orientation} opened eye-door room reloads three components");
        for (int frame = 0; frame < 16 && reopened.SlotTable.ActiveCount != 0; frame++)
            reopened.Step(reopenedLevel);
        AssertEqual(0, reopened.SlotTable.ActiveCount,
            $"{orientation} opened eye-door reload converts to blue and removes passive parts");
        AssertEqual(0, guarded.ForbiddenReadAttempts,
            $"{orientation} opened eye-door reload avoids instruction and draw ROM reads");
    }

    /// <summary>Reads one retail bank-$84 program byte for the fixture oracle.</summary>
    private static byte EyeDoorRetailProgramPathRomByte(ISnesAddressSpace rom, int address) => rom.ReadCartridgeByte(address);

    /// <summary>Checks the extracted stock visuals and that a malformed installed visual file fails loudly where it is read.</summary>
    private static void VerifyEyeDoorVisualInstallation(CartridgeImportAddressSpace rom)
    {
        using PrivateInstallationCopy copy = InstalledFixtures.CreatePrivateCopyOf(layout => layout.RoomObjectEyeDoorVisualDirectory);
        GameInstallation installation = copy.Installation;
        RoomObjectEyeDoorVisualFilesConstants.ValidateStock(
            installation.RoomObjectEyeDoorVisualDirectory);
        VerifyEyeDoorStockMapping(rom, installation.LoadRoomObjectEyeDoorVisuals());
        string stockPath = Path.Combine(installation.RoomObjectEyeDoorVisualDirectory,
            RoomObjectEyeDoorVisualFiles.VisualFileName);
        File.WriteAllText(stockPath, "{}");
        AssertThrows<InvalidDataException>(
            () => installation.LoadRoomObjectEyeDoorVisuals(),
            "tampered eye-door stock fails loudly where it is read");
    }

    /// <summary>Rejects runtime reads of compiled eye-door instructions and physical draw payloads.</summary>
    private static MutableCartridgeReadGuard NewEyeDoorDrawGuard(
        ISnesAddressSpace source, RoomObjectShotBlockDrawDefinitions.DrawList[] lists)
    {
        var guard = new MutableCartridgeReadGuard(source);
        guard.Forbid(address => address is >= 0x84d81e and <= 0x84da8b,
                address => $"Eye door reread compiled instruction ${address:X6}.")
            .Forbid(address => lists.Any(list =>
                {
                    int first = 0x840000 | RoomObjectDrawAddressCodec.Of(list.Draw);
                    int length = 4 + list.Runs.Span[0].Count * 2;
                    return address >= first && address < first + length;
                }),
                address => $"Eye door reread compiled draw payload ${address:X6}.");
        return guard;
    }
}
