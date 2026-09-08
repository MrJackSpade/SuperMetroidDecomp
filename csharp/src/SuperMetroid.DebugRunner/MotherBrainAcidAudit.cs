using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Runtime;

/// <summary>Verifies both native LoadFxEntry calls through real room initialization and descent.</summary>
internal static class MotherBrainAcidAudit
{
    public static void Run(string romPath)
    {
        var bus = SuperMetroidAddressSpace.LoadRetailRom(romPath);
        var runtime = new SuperMetroidRuntime(bus, playerInvincibilityEnabled: true);
        runtime.InitializeHud(HudSnapshot.CeresDebug);
        runtime.InitializeStartingCeresRoom();
        runtime.InitializeCeresStartSamus();
        runtime.LoadCartridgeRoomForDebug(0xdd58);
        var heights = new HashSet<ushort>();
        for (int frame = 0; frame < 800; frame++)
        {
            runtime.StepFrame(0);
            heights.Add(runtime.RoomLayer3Fx.CurrentYPosition);
        }
        if (runtime.RoomLayer3Fx.CurrentYPosition != 184 || heights.Count != 49)
            throw new InvalidDataException("Mother Brain's initial FX entry failed to raise acid from 232 to 184.");
        runtime.System.SetEvent(EventNumber.MotherBrainGlassDestroyed);
        runtime.Enemies.MotherBrain!.Head!.Health = 0;
        runtime.Samus!.XPosition = 128;
        for (int frame = 0; frame < 400; frame++) runtime.StepFrame(0);
        if (runtime.RoomLayer3Fx.CurrentYPosition != 232)
            throw new InvalidDataException("Mother Brain's descent FX entry failed to lower acid back to 232.");
    }
}
