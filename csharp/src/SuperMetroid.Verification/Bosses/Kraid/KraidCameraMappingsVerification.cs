using SuperMetroid.Core.Bosses.Kraid;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Hud;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Rooms.Scrolling;
using SuperMetroid.Core.Runtime;
using SuperMetroid.AssetExtraction.Cartridge;
using static SuperMetroid.Verification.Harness.RetailRuntimeFixtureVerification;

namespace SuperMetroid.Verification.Bosses.Kraid;

/// <summary>Compares Kraid's pre-growth and grown four-screen scroll mappings with immediate operands in bank $A7.</summary>
internal static class KraidCameraMappingsVerification
{
    /// <summary>
    /// Checks the two LDA immediate pairs that initialize Kraid's screen scrolls: each written cell, in order, is the screen
    /// the definition names at Kraid's room width, and takes the state the definition gives it.
    /// </summary>
    internal static void VerifyKraidInitialScrollMapping(CartridgeImportAddressSpace rom) =>
        VerifyKraidScrollMapping(rom, InitialOperands, KraidCameraDefinitions.InitialScrolls, "initial");

    /// <summary>Checks the grown-phase screen scroll bytes the same way.</summary>
    internal static void VerifyKraidGrownScrollMapping(CartridgeImportAddressSpace rom) =>
        VerifyKraidScrollMapping(rom, GrownOperands, KraidCameraDefinitions.GrownScrolls, "grown");

    /// <summary>The two LDA immediate operand pairs Kraid's setup stores to the room's first four cells, in cell order.</summary>
    private static readonly int[] InitialOperands = [0xa7a9eb, 0xa7a9ec, 0xa7a9f2, 0xa7a9f3];

    /// <summary>The two LDA immediate operand pairs Kraid's growth stores to the room's first four cells, in cell order.</summary>
    private static readonly int[] GrownOperands = [0xa7c0a8, 0xa7c0a9, 0xa7c0af, 0xa7c0b0];

    /// <summary>
    /// Kraid's production rise writes each definition's screen to the cell the original stores to (#1741): loading his
    /// room leaves its first four cells as the initial operands, and his growth's camera release leaves them as the grown
    /// operands.
    /// </summary>
    internal static void VerifyKraidRiseScrolls()
    {
        var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        SuperMetroidRuntime runtime = CreateRetailRuntimeFixture();
        runtime.HudSetup.InitializeHud(HudSnapshot.CeresDebug);
        runtime.RoomLoader.InitializeStartingCeresRoom();
        runtime.RoomLoader.InitializeCeresStartSamus();
        runtime.System.SetEvent(StoryEvent.ZebesAwake);
        runtime.LoadCartridgeRoomForDebug(RoomDefinitions.Kraid);
        RoomScrollGrid scrolls = runtime.Room.LoadedCamera.Scrolls;
        ExpectCells(rom, InitialOperands, scrolls, "Kraid's room setup");

        KraidEnemyState state = runtime.Enemies.KraidBehavior.Access.Kraid
            ?? throw new InvalidOperationException("Kraid's room loads Kraid.");
        RoomEnemySlot body = runtime.Enemies.SlotTable.Slots.First(slot => slot.DefinitionId == EnemyDefinitionId.Kraid);
        state.BodyState.Phase = KraidPhase.GrowReleaseCamera;
        runtime.Enemies.KraidBehavior.Rise.RunKraidGrowthPhase(body, state);
        ExpectCells(rom, GrownOperands, scrolls, "Kraid's growth");
    }

    /// <summary>Requires the room's first cells to hold the states the original's operands store to them, in order.</summary>
    private static void ExpectCells(ISnesAddressSpace rom, int[] operandAddresses, RoomScrollGrid scrolls, string change)
    {
        RoomScrollState[] kept = scrolls.KeptStates;
        for (int cell = 0; cell < operandAddresses.Length; cell++)
            AssertEqual(rom.ReadCartridgeByte(operandAddresses[cell]), kept[cell].ToStorageByte(),
                $"{change} leaves cell {cell} as the original stores it");
    }

    /// <summary>
    /// The native code stores its operands to the room's cells in row-major order, one byte per cell. Each definition, in
    /// order, names the screen at that cell in Kraid's room and the state of that operand; no definition names a screen
    /// outside the room.
    /// </summary>
    private static void VerifyKraidScrollMapping(ISnesAddressSpace rom, int[] operandAddresses, ScreenScrollChange[] changes, string layout)
    {
        Extent16<RoomScreens> room = RoomDefinitions.Kraid.Size;
        AssertEqual(room.Area, operandAddresses.Length, $"Kraid {layout} scrolls cover every screen of his room");
        AssertEqual(operandAddresses.Length, changes.Length, $"Kraid {layout} screen count");
        for (int cell = 0; cell < operandAddresses.Length; cell++)
        {
            AssertEqual(room.CellAtRowMajorIndex(cell), changes[cell].Screen, $"Kraid {layout} write {cell} names its cell's screen");
            AssertEqual(rom.ReadCartridgeByte(operandAddresses[cell]), (byte)changes[cell].State,
                $"Kraid {layout} scroll of screen {changes[cell].Screen}");
        }
    }
}
