using SuperMetroid.AssetExtraction;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Rooms;
using SuperMetroid.Core.Bosses.Phantoon;
using SuperMetroid.Core.Bosses.Phantoon.Artwork;
using SuperMetroid.Core.Bosses;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.RoomObjects;
using SuperMetroid.Core.World;
using SuperMetroid.Core.Compatibility;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.TestSupport.RepositoryInstallation;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Verification.Areas.Escape;
using SuperMetroid.Verification.Rooms.RoomObjects;
using static SuperMetroid.Verification.Bosses.Phantoon.PhantoonAnimationsVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using static SuperMetroid.Verification.Harness.VerificationHelpersVerification;
using static SuperMetroid.Verification.Rooms.RoomObjects.InstalledFramePictureAssertions;
namespace SuperMetroid.Verification.Bosses.Phantoon;
using SuperMetroid.Verification.Harness;

/// <summary>
/// Checks Phantoon's two door changes (#1678) against the original's spawns, lists and draw lists: the opening's and the
/// death's requests, the place each change holds, the door's top block held solid until the change draws, and the
/// terrain each draw leaves.
/// </summary>
internal static class PhantoonDoorObjectsVerification
{
    /// <summary>The door frames' draw-list addresses and published names.</summary>
    private static readonly LookupEscapeGateStockFrame[] Frames = [new(0x93ef, "fight-door"), new(0x9407, "restored-door")];

    /// <summary>The cells the two draw lists write in all: two columns of four, then one.</summary>
    private const int WrittenCells = 12;

    /// <summary>The bank-$84 PLM program bank, as a long-address base.</summary>
    private const int ProgramBank = 0x840000;

    /// <summary>The bank-$84 address of the shared setup that keeps only a block's solid bit.</summary>
    private const ushort DeactivatingSetup = 0xb3c1;

    /// <summary>The bank-$84 address of the hard-coded room-object spawn.</summary>
    private const ushort HardcodedSpawn = 0x83d7;

    /// <summary>The level-word mask the deactivating setup applies: the solid bit and the block's picture.</summary>
    private const ushort DeactivationMask = 0x8fff;

    /// <summary>A room wide and tall enough to hold the door's two columns from the door's top block.</summary>
    private static readonly Extent16<RoomBlocks> RoomSize = new(16, 16);

    /// <summary>The door's spawn calls, change programs and draws against the ROM, then both changes through the room.</summary>
    internal static void VerifyCompiledPhantoonDoorObjects()
    {
        CartridgeImportAddressSpace rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        Suite(nameof(VerifyPhantoonDoorSpawnCalls), () => VerifyPhantoonDoorSpawnCalls(rom));
        Suite(nameof(BossRoomObjectProgramsVerification.VerifyPhantoonDoorPrograms), () => BossRoomObjectProgramsVerification.VerifyPhantoonDoorPrograms(rom));
        Suite(nameof(VerifyPhantoonDoorDraws), () => VerifyPhantoonDoorDraws(rom));
        Suite(nameof(VerifyPhantoonFightDoor), () => VerifyPhantoonFightDoor(rom));
        Suite(nameof(VerifyPhantoonRestoredDoor), () => VerifyPhantoonRestoredDoor(rom));
        Console.WriteLine("Phantoon door: both spawns, their programs and draws match the ROM; each holds the door's top block " +
            "solid until it draws, holds its place for two updates and leaves the original's terrain.");
    }

    /// <summary>
    /// Both of Phantoon's door spawns are hard-coded spawns at the door's top block of the header whose setup is the
    /// deactivating setup and whose list is the change's.
    /// </summary>
    private static void VerifyPhantoonDoorSpawnCalls(CartridgeImportAddressSpace rom)
    {
        IImportCartridgeSource cartridge = CartridgeImportSource.Require(rom);
        (int Call, ushort Header, PhantoonDoorChange Change)[] spawns =
            [(0xa7d4e5, 0xb781, PhantoonDoorChange.FightDoor), (0xa7db89, 0xb78b, PhantoonDoorChange.RestoredDoor)];
        foreach ((int call, ushort header, PhantoonDoorChange change) in spawns)
        {
            string at = $"Phantoon {change} spawn";
            AssertEqual(0x22, (int)rom.ReadCartridgeByte(call), $"{at} is a long call");
            AssertEqual(HardcodedSpawn, cartridge.ReadCartridgeWord(call + 1), $"{at} calls the hard-coded spawn");
            AssertEqual(0x84, (int)rom.ReadCartridgeByte(call + 3), $"{at} calls the room objects' bank");
            AssertEqual((int)PhantoonDoorChangeDefinitions.Block.X, (int)rom.ReadCartridgeByte(call + 4), $"{at} column");
            AssertEqual((int)PhantoonDoorChangeDefinitions.Block.Y, (int)rom.ReadCartridgeByte(call + 5), $"{at} row");
            AssertEqual(header, cartridge.ReadCartridgeWord(call + 6), $"{at} header");
            AssertEqual(DeactivatingSetup, cartridge.ReadCartridgeWord(ProgramBank | header), $"{at} setup deactivates its block");
            ushort entry = BossRoomObjectNativeListAddresses.PhantoonDoorEntries.Single(listed => listed.Program == change).Entry;
            AssertEqual(entry, cartridge.ReadCartridgeWord(ProgramBank | (header + 2)), $"{at} list");
        }
    }

    /// <summary>The door frames' geometry, collision and installed pictures are the ROM draw lists'.</summary>
    private static void VerifyPhantoonDoorDraws(CartridgeImportAddressSpace rom)
    {
        RoomObjectFrameFamily<PhantoonDoorDraw> family = RoomObjectPhantoonDoorVisualFiles.Family;
        AssertLayoutsAsNative(rom, family, Frames, collision: true);
        InstalledPhantoonDoorFrames installed = InstalledPhantoonDoorFrames.Stock();
        AssertPicturedAsNative(rom, installed.Images, family, Frames, WrittenCells);
        AssertImportAndRejections(rom, installed.Images, family, RepositoryInstallation.Installation.RoomObjectPhantoonDoorVisualDirectory);
        foreach (LookupEscapeGateStockFrame frame in Frames)
        {
            if (!PhantoonDoorDraws.TryDescribe(NativeObjectDraws.At<PhantoonDoorDraw>(frame.Pointer), out RoomBlockShape? described))
                throw new InvalidOperationException($"Phantoon door frame {frame.Id} has no shape.");
            RoomBlockShape shape = described;
            Dictionary<Offset16<RoomBlocks>, RoomTerrainBlock> native = NativeTerrain(rom, frame.Pointer, out _);
            AssertEqual(native.Count, shape.Cells.Count, $"Phantoon door {frame.Id} shape covers the draw list's cells");
            foreach ((Offset16<RoomBlocks> cell, RoomTerrainBlock terrain) in native)
                AssertEqual(terrain.Collision, shape.CollisionAt(cell), $"Phantoon door {frame.Id} collides as its draw list at {cell}");
        }
        AssertTrue(!PhantoonDoorDraws.TryDescribe(NativeObjectDraws.At<PhantoonDoorDraw>(0x93f1), out _),
            "no Phantoon door frame starts inside the fight door's draw list");
    }

    /// <summary>
    /// The opening asks for the fight's door only as the eighth starting flame spawns,
    /// and the change it asks for holds the door's top block solid and draws the fight's door.
    /// </summary>
    private static void VerifyPhantoonFightDoor(CartridgeImportAddressSpace rom)
    {
        RoomEnemySystem enemies = CreatePhantoonSystem();
        PhantoonEnemyState state = enemies.PhantoonBehavior.Combat.Phantoon
            ?? throw new InvalidOperationException("Verification requires Phantoon's state.");
        BossFrameResultBuilder results = enemies.BossFrameResults;
        state.BodyState.StartingFlameIndex = 6;
        state.BodyState.FunctionTimer = 1;
        enemies.PhantoonBehavior.Opening.RunPhantoonStartingFlameSpawner(state.Body, state);
        AssertEqual((PhantoonDoorChange?)null, results.PhantoonDoor, "the seventh starting flame asks for no door change");
        state.BodyState.FunctionTimer = 1;
        enemies.PhantoonBehavior.Opening.RunPhantoonStartingFlameSpawner(state.Body, state);
        AssertEqual((PhantoonDoorChange?)PhantoonDoorChange.FightDoor, results.PhantoonDoor, "the eighth starting flame asks for the fight's door");

        // Before the fight the door shows its own frame, with the grey door's closed cap beside it.
        Dictionary<Offset16<RoomBlocks>, RoomTerrainBlock> before = NativeTerrain(rom, Frames[1].Pointer, out _);
        for (int row = 0; row < 4; row++)
            before[new(1, row)] = RoomTerrainBlock.Of(0x0c0 + row, BlockMirroring.None,
                row == 0 ? RoomCollisionType.ShootableBlock : RoomCollisionType.VerticalExtension);
        RoomLevelData level = DoorRoom(before);
        VerifyChange(rom, level, results.PhantoonDoor ?? throw new InvalidOperationException("The opening asked for no door change."), before, Frames[0].Pointer, Frames[1].Pointer);

    }

    /// <summary>
    /// The death asks for the restored door as the Wrecked Ship's power finishes coming back, and the change holds the
    /// door's top block solid and draws the door's own frame, leaving the fight's cap beside it.
    /// </summary>
    private static void VerifyPhantoonRestoredDoor(CartridgeImportAddressSpace rom)
    {
        RoomEnemySystem enemies = CreatePhantoonSystem();
        PhantoonEnemyState state = enemies.PhantoonBehavior.Combat.Phantoon
            ?? throw new InvalidOperationException("Verification requires Phantoon's state.");
        BossFrameResultBuilder results = enemies.BossFrameResults;
        // The power's return scatters Phantoon's drops, which choose by Samus's health and ammunition and redraw a zero
        // roll, so the fixture's random source yields a nonzero roll.
        enemies.VerificationServices().NextRandom = () => 0x5a5a;
        enemies.DeathsAndDrops.BindSamus(new SamusState(InstalledFixtures.AnimationContinuation, new UnauthoredBoostCadence(),
            SamusCompatibility.ForNewSamus(), new BeamChargeState()) { Health = 99, MaxHealth = 99 });
        // The boss bit is the room services' to keep; this fixture has none, so the bit counts as already set.
        state.BossDefeatPersisted = true;
        state.BodyState.FunctionTimer = 0;
        // Past the power's last step: this update completes the power coming back.
        state.EyeState.FadeNumerator = 14;
        enemies.PhantoonBehavior.Death.ActivateWreckedShipAfterPhantoon(state.Body, state);
        AssertTrue(state.WreckedShipPowerPaletteComplete, "the Wrecked Ship's power has come back");
        AssertEqual((PhantoonDoorChange?)PhantoonDoorChange.RestoredDoor, results.PhantoonDoor, "the power's return asks for the restored door");

        Dictionary<Offset16<RoomBlocks>, RoomTerrainBlock> before = NativeTerrain(rom, Frames[0].Pointer, out _);
        VerifyChange(rom, DoorRoom(before), results.PhantoonDoor ?? throw new InvalidOperationException("The death asked for no door change."), before, Frames[1].Pointer, Frames[0].Pointer);
    }

    /// <summary>
    /// Spawns <paramref name="change"/> into <paramref name="level"/>, whose door columns hold <paramref name="before"/>,
    /// and checks it against the original: one place taken; the door's top block held as the deactivating setup leaves
    /// the word the list at <paramref name="priorList"/> wrote there, everything else untouched; on its first update the
    /// draw list at <paramref name="drawList"/>'s terrain, its place still held; on its second, its place freed. No
    /// block's behaviour changes.
    /// </summary>
    private static void VerifyChange(CartridgeImportAddressSpace rom, RoomLevelData level, PhantoonDoorChange change,
        Dictionary<Offset16<RoomBlocks>, RoomTerrainBlock> before, ushort drawList, ushort priorList)
    {
        IImportCartridgeSource cartridge = CartridgeImportSource.Require(rom);
        Position16<RoomBlocks> top = PhantoonDoorChangeDefinitions.Block;
        RoomBlockBehavior[] behaviors = [.. before.Keys.Select(cell => level.GetCollisionBlock(top + cell).Behavior)];
        RoomObjectSystem roomObjects = RoomObjectFixtures.Unbound();
        string at = $"Phantoon {change}";

        AssertTrue(roomObjects.BossRoomMutations.PhantoonDoor.TrySpawn(level, change), $"{at} spawns");
        AssertEqual(1, roomObjects.SlotTable.ActiveCount, $"{at} takes one room-object place");
        ushort priorTopWord = cartridge.ReadCartridgeWord(ProgramBank | (priorList + 2));
        AssertEqual(RoomLevelWordCodec.Decode((ushort)(priorTopWord & DeactivationMask)), level.GetCollisionBlock(top).Terrain,
            $"{at} holds the door's top block solid, as the deactivating setup leaves its word");
        AssertEqual(RoomCollisionType.SolidBlock, level.GetCollisionBlock(top).Terrain.Collision, $"{at} door's top block is solid until it draws");
        foreach ((Offset16<RoomBlocks> cell, RoomTerrainBlock terrain) in before)
            if (cell != Offset16<RoomBlocks>.Zero)
                AssertEqual(terrain, level.GetCollisionBlock(top + cell).Terrain, $"{at} setup leaves {cell} as it was");

        Dictionary<Offset16<RoomBlocks>, RoomTerrainBlock> drawn = NativeTerrain(rom, drawList, out _);
        roomObjects.Step(level);
        AssertEqual(1, roomObjects.SlotTable.ActiveCount, $"{at} holds its place through its first update");
        AssertTerrain(level, before, drawn, $"{at} first update");
        roomObjects.Step(level);
        AssertEqual(0, roomObjects.SlotTable.ActiveCount, $"{at} leaves the room on its second update");
        AssertTerrain(level, before, drawn, $"{at} after it leaves");
        int index = 0;
        foreach (Offset16<RoomBlocks> cell in before.Keys)
            AssertEqual(behaviors[index++], level.GetCollisionBlock(top + cell).Behavior, $"{at} keeps the behaviour at {cell}");
    }

    /// <summary>Each door cell shows what <paramref name="drawn"/> wrote there, or what it held <paramref name="before"/>.</summary>
    private static void AssertTerrain(RoomLevelData level, Dictionary<Offset16<RoomBlocks>, RoomTerrainBlock> before,
        Dictionary<Offset16<RoomBlocks>, RoomTerrainBlock> drawn, string at)
    {
        Position16<RoomBlocks> top = PhantoonDoorChangeDefinitions.Block;
        foreach ((Offset16<RoomBlocks> cell, RoomTerrainBlock terrain) in before)
            AssertEqual(drawn.TryGetValue(cell, out RoomTerrainBlock written) ? written : terrain,
                level.GetCollisionBlock(top + cell).Terrain, $"{at}: the original's terrain at {cell}");
    }

    /// <summary>
    /// A room whose door columns, from the door's top block, hold <paramref name="door"/>; the door's blocks link to a
    /// door and the cap's lead to the block above, as the room's own do.
    /// </summary>
    private static RoomLevelData DoorRoom(Dictionary<Offset16<RoomBlocks>, RoomTerrainBlock> door)
    {
        var terrain = new RoomTerrainBlock[RoomSize.Area];
        var behaviors = new RoomBlockBehavior[terrain.Length];
        Position16<RoomBlocks> top = PhantoonDoorChangeDefinitions.Block;
        foreach ((Offset16<RoomBlocks> cell, RoomTerrainBlock block) in door)
        {
            int index = RoomSize.RowMajorIndex(top + cell);
            terrain[index] = block;
            behaviors[index] = cell.X == 0 ? RoomBlockBehavior.LinkingDoor(2) : cell.Y == 0 ? RoomBlockBehavior.None : RoomBlockBehavior.Extending(-1);
        }
        return CreateRoom(RoomSize, terrain, behaviors);
    }

    /// <summary>Runs Phantoon's door checks.</summary>
    [VerificationCommand("--phantoon-door-plms")]
    internal static int PhantoonDoorObjectsCommand()
    {
        Suite(nameof(VerifyCompiledPhantoonDoorObjects), () => VerifyCompiledPhantoonDoorObjects());
        return 0;
    }
}
