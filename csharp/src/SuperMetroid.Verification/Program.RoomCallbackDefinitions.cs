using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

internal static partial class Program
{
    private static void VerifyCompiledRoomCallbackDefinitions()
    {
        SuperMetroidAddressSpace bus = SuperMetroidAddressSpace.LoadRetailRom(
            Path.GetFullPath("Super Metroid.smc"));
        string symbolPath = Path.GetFullPath(
            Path.Combine("upstream-sm", "assets", "names.txt"));
        ushort[] roomPointers = File.ReadLines(symbolPath)
            .Select(TryParseRoomHeaderPointer)
            .Where(pointer => pointer.HasValue)
            .Select(pointer => pointer!.Value)
            .Distinct()
            .Order()
            .ToArray();

        var states = new HashSet<ushort>();
        var mainCallbacks = new HashSet<RoomMainCallback>();
        var setupCallbacks = new HashSet<RoomSetupCallback>();
        foreach (ushort roomPointer in roomPointers)
        foreach (RoomStateSelectionContext context in BuildRoomStateAuditContexts())
        {
            CartridgeRoomState state = CartridgeRoomHeader.LoadUsingCompiledSelection(
                bus, roomPointer, context).State;
            if (!states.Add(state.Pointer))
                continue;

            RoomMainCallback main = state.MainCallback;
            RoomSetupCallback setup = state.SetupCallback;
            AssertEqual(state.MainCodePointer, RoomCallbackDefinitions.PointerOf(main),
                $"room state $8F:{state.Pointer:X4} main callback round trip");
            AssertEqual(state.SetupCodePointer, RoomCallbackDefinitions.PointerOf(setup),
                $"room state $8F:{state.Pointer:X4} setup callback round trip");
            mainCallbacks.Add(main);
            setupCallbacks.Add(setup);
        }

        AssertEqual(RoomStateDefinitions.RetailStateCount, states.Count,
            "callback audit reaches every compiled retail room state");
        AssertTrue(mainCallbacks.SetEquals(Enum.GetValues<RoomMainCallback>()),
            "every typed room-main callback occurs in the retail state graph");
        AssertTrue(setupCallbacks.SetEquals(Enum.GetValues<RoomSetupCallback>()),
            "every typed room-setup callback occurs in the retail state graph");
        AssertThrows<ArgumentOutOfRangeException>(
            () => RoomCallbackDefinitions.ResolveMain(0xffff),
            "room-main dispatch rejects unknown native pointers");
        AssertThrows<ArgumentOutOfRangeException>(
            () => RoomCallbackDefinitions.ResolveSetup(0xffff),
            "room-setup dispatch rejects unknown native pointers");
        AssertThrows<ArgumentOutOfRangeException>(
            () => RoomCallbackDefinitions.PointerOf((RoomMainCallback)byte.MaxValue),
            "room-main reverse mapping rejects unknown typed identities");
        AssertThrows<ArgumentOutOfRangeException>(
            () => RoomCallbackDefinitions.PointerOf((RoomSetupCallback)byte.MaxValue),
            "room-setup reverse mapping rejects unknown typed identities");
        AssertTrue(RoomCallbackDefinitions.SpawnsCeresHaze(
                RoomSetupCallback.TurnCeresDoorToSolidBlocksAndSpawnHaze),
            "Ceres solid-door callback retains its haze side effect");
        AssertTrue(!RoomCallbackDefinitions.SpawnsCeresHaze(
                RoomSetupCallback.SetMediumHorizontalRoomShaking),
            "unrelated callback does not acquire Ceres haze");

        Console.WriteLine(
            $"Room callbacks: all {states.Count} states map exactly to " +
            $"{mainCallbacks.Count} main and {setupCallbacks.Count} setup identities.");
    }
}
