using SuperMetroid.Core.Game;
using SuperMetroid.Core.Rooms;

internal static partial class Program
{
    // #1269: command six ($90:F1AA) installs the bare RTL beta $90:E8D6, which neither moves
    // nor animates Samus. Underwater, the 100% movie's Maridia map station therefore spawns
    // no air bubble (and draws no RNG) while the following pause fade runs gameplay.
    private static void VerifyRefillStationLock()
    {
        var bus = SuperMetroid.AssetExtraction.CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        var runtime = CreateRetailRuntimeFixture(bus);
        runtime.InitializeHud(HudSnapshot.CeresDebug);
        runtime.InitializeStartingCeresRoom();
        runtime.InitializeCeresStartSamus();
        runtime.LoadCartridgeRoomForDebug(RoomHeaderPointers.LandingSite);
        var samus = runtime.Samus!;
        samus.InputLocked = false;
        for (int frame = 0; frame < 8; frame++)
            runtime.StepFrame(0);

        (ushort Frame, ushort Timer) Animation() => (samus.AnimationFrame, samus.AnimationFrameTimer);
        var unlocked = Animation();
        runtime.StepFrame(0);
        AssertTrue(Animation() != unlocked, "the normal beta advances Samus's animation");

        samus.LockIntoRefillStation();
        AssertTrue(samus.RefillStationLocked && samus.InputLocked && !samus.StationaryScriptControlLocked,
            "command six installs the station pair, not command zero's");
        var locked = Animation();
        for (int frame = 0; frame < 4; frame++)
            runtime.StepFrame(0);
        AssertEqual(locked, Animation(), "the station beta does not animate Samus");

        samus.InputLocked = false;
        AssertTrue(!samus.RefillStationLocked, "restoring the normal handlers releases the station beta");
        Console.WriteLine("Refill-station lock: command six's RTL beta holds Samus's animation until release.");
    }
}
