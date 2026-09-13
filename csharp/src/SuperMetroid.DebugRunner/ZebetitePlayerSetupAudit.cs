using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Runtime;
using SuperMetroid.Core.Input;

/// <summary>Room-local setup exploration for #443; not a native parity assertion.</summary>
internal static class ZebetitePlayerSetupAudit
{
    public static int Run(string romPath)
    {
        var bus = SuperMetroidAddressSpace.LoadRetailRom(romPath);
        var runtime = new SuperMetroidRuntime(bus);
        runtime.InitializeHud(HudSnapshot.CeresDebug);
        runtime.InitializeStartingCeresRoom();
        runtime.InitializeCeresStartSamus();
        runtime.System.SetEvent(EventNumber.ZebetiteDestroyedBit0);
        runtime.LoadCartridgeRoomForDebug(0xdd58);
        var samus = runtime.Samus!;
        samus.XPosition = 696; samus.YPosition = 100;
        samus.Kinematics.XSubposition = samus.Kinematics.YSubposition = 0;
        samus.Pose = SamusPoseIds.FacingRightNormalPose;
        samus.RefreshCollisionRadii(bus); samus.InitializeAnimation(bus);
        samus.Health = samus.MaxHealth = 999;
        samus.Missiles = samus.MaxMissiles = 10;
        samus.SelectedHudItem = 1;
        runtime.Camera!.SetPosition(641, 0);
        var level = runtime.LevelData!;
        for (int y = 0; y < 16; y++)
        {
            Console.Write($"row {y,2}: ");
            for (int x = 38; x <= 45; x++)
                Console.Write($"{level.ForegroundEntries.Span[y * level.WidthInBlocks + x] >> 12:X} ");
            Console.WriteLine();
        }
        for (int frame = 0; frame < 120; frame++)
        {
            SnesButton input = frame switch
            {
                60 or 61 => SnesButton.Down,
                >= 62 and < 71 => SnesButton.Left,
                71 => SnesButton.Left | SnesButton.X,
                72 => SnesButton.Left,
                73 => SnesButton.Left | SnesButton.A,
                >= 74 and < 80 => SnesButton.Right | SnesButton.A,
                _ => 0,
            };
            runtime.StepFrame((ushort)input);
            if (frame % 10 == 0 || frame is >= 60 and < 90)
                Console.WriteLine($"SETUP frame={frame} input={(ushort)input:X4} x={samus.XPosition} y={samus.YPosition} pose={samus.Pose:X2} camera={runtime.Camera.XPosition},{runtime.Camera.YPosition} health={samus.Health} missiles={samus.Missiles} barrier={string.Join('/', runtime.Enemies.Slots.Where(slot => slot.EnemyDefinitionPointer == 0xe27f).Select(slot => slot.Health))}");
        }
        return 0;
    }
}
