using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Runtime;

/// <summary>Constructed geometry with the complete gameplay dispatcher, but no arrival coroutine.</summary>
internal static class FlatFloorMovementFixture
{
    public static SuperMetroidRuntime Create(SuperMetroidAddressSpace bus, bool water)
    {
        var runtime = new SuperMetroidRuntime(bus);
        runtime.InitializeHud(HudSnapshot.CeresDebug);
        runtime.RunNmi(0, true);
        runtime.InitializeStartingCeresRoom();
        runtime.InitializeCeresStartSamus();
        // Ordinary reload clears the fresh-game elevator arrival sequence. Otherwise
        // the fixture would teleport Samus after a second instead of testing movement.
        runtime.LoadCartridgeRoomForDebug(runtime.ActiveRoom!.Pointer, 0, 0);
        var level = runtime.LevelData!;
        for (int y = 0; y <= 16; y++)
            for (int x = 0; x < level.WidthInBlocks; x++)
                level.SetForegroundEntry(y * level.WidthInBlocks + x,
                    y == 16 ? (ushort)0x8000 : (ushort)0);
        runtime.InitializeDebugGroundedSamus(200, 166, 16);
        runtime.Samus!.InputLocked = false;
        if (water) runtime.Samus.LiquidPhysics.ConfigureWater(8, 0x80);
        return runtime;
    }
}
