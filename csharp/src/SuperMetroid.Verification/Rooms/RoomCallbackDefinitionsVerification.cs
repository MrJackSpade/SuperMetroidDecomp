using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Rooms;
using static SuperMetroid.Verification.Rooms.RoomMainCodeCatalogVerification;
namespace SuperMetroid.Verification.Rooms;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.World.RoomStates;
using SuperMetroid.AssetExtraction.World.RoomStates;
using SuperMetroid.AssetExtraction.Rooms;

/// <summary>Checks typed room setup and main callbacks against every compiled retail room state.</summary>
internal static class RoomCallbackDefinitionsVerification
{
    /// <summary>
    /// Traverses selectable room states and checks each compiled callback is the one the state's cartridge pointer decodes
    /// to, that every typed callback occurs, and that unknown pointers are refused.
    /// </summary>
    internal static void VerifyCompiledRoomCallbackDefinitions()
    {
        CartridgeImportAddressSpace bus = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(
            Path.GetFullPath("Super Metroid.smc"));
        string symbolPath = Path.GetFullPath(
            Path.Combine("upstream-sm", "assets", "names.txt"));
        ushort[] roomPointers = File.ReadLines(symbolPath)
            .Select(TryParseRoomHeaderPointer)
            .Where(pointer => pointer.HasValue)
            .Select(pointer => (pointer ?? throw new InvalidOperationException("Verification requires pointer.")))
            .Distinct()
            .Order()
            .ToArray();

        var states = new HashSet<ushort>();
        var mainCallbacks = new HashSet<RoomMainBehavior>();
        var setupCallbacks = new HashSet<RoomSetupBehavior>();
        foreach (ushort roomPointer in roomPointers)
        foreach (RoomStateSelectionContext context in BuildRoomStateAuditContexts())
        {
            RoomStateSettings state = SelectedRoom.LoadUsingCompiledSelection(
                RoomDefinitions.Get(roomPointer), context).State;
            if (!states.Add(state.Pointer))
                continue;

            RoomMainBehavior main = state.MainBehavior;
            RoomSetupBehavior setup = state.SetupBehavior;
            AssertEqual(RoomBehaviorDefinitions.ResolveMain(CartridgeRoomStateImporter.MainCodePointer(bus, state.Pointer)), main,
                $"room state $8F:{state.Pointer:X4} main callback is the one its cartridge pointer decodes to");
            AssertEqual(RoomBehaviorDefinitions.ResolveSetup(CartridgeRoomStateImporter.SetupCodePointer(bus, state.Pointer)), setup,
                $"room state $8F:{state.Pointer:X4} setup callback is the one its cartridge pointer decodes to");
            mainCallbacks.Add(main);
            setupCallbacks.Add(setup);
        }

        AssertEqual(RoomStateDefinitions.RetailStateCount, states.Count,
            "callback audit reaches every compiled retail room state");
        AssertTrue(mainCallbacks.SetEquals(Enum.GetValues<RoomMainBehavior>()),
            "every typed room-main callback occurs in the retail state graph");
        AssertTrue(setupCallbacks.SetEquals(Enum.GetValues<RoomSetupBehavior>()),
            "every typed room-setup callback occurs in the retail state graph");
        AssertThrows<ArgumentOutOfRangeException>(
            () => RoomBehaviorDefinitions.ResolveMain(0xffff),
            "room-main dispatch rejects unknown native pointers");
        AssertThrows<ArgumentOutOfRangeException>(
            () => RoomBehaviorDefinitions.ResolveSetup(0xffff),
            "room-setup dispatch rejects unknown native pointers");
        AssertTrue(RoomBehaviorDefinitions.CeresHazeSetups.Contains(
                RoomSetupBehavior.TurnCeresDoorToSolidBlocksAndSpawnHaze),
            "Ceres solid-door callback retains its haze side effect");
        AssertTrue(!RoomBehaviorDefinitions.CeresHazeSetups.Contains(
                RoomSetupBehavior.SetMediumHorizontalRoomShaking),
            "unrelated callback does not acquire Ceres haze");

        Console.WriteLine(
            $"Room callbacks: all {states.Count} states map exactly to " +
            $"{mainCallbacks.Count} main and {setupCallbacks.Count} setup identities.");
    }

    /// <summary>Runs the focused room callback catalog check.</summary>
    [VerificationCommand("--room-callback-definitions")]
    internal static int RoomCallbackDefinitionsCommand()
    {
        Suite(nameof(VerifyCompiledRoomCallbackDefinitions), () => VerifyCompiledRoomCallbackDefinitions());
        return 0;
    }
}
