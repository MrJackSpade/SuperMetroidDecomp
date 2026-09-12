using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;

/// <summary>Exploratory normal-input barrage search; not a native parity assertion.</summary>
internal static class PhantoonDopplerAudit
{
    public static int Run(string rom)
    {
        foreach (int start in new[] { 1640, 1650, 1660 })
        foreach (int cadence in new[] { 8, 9, 10, 11, 12 })
        foreach (bool aerial in new[] { false, true })
        {
            var runtime = new SuperMetroidRuntime(SuperMetroidAddressSpace.LoadRetailRom(rom));
            runtime.InitializeHud(HudSnapshot.CeresDebug);
            runtime.InitializeStartingCeresRoom();
            runtime.InitializeCeresStartSamus();
            runtime.LoadCartridgeRoomForDebug(RoomHeaderPointers.Phantoon);
            runtime.InitializeDebugGroundedSamus(128, 166, 8);
            var samus = runtime.Samus!;
            samus.InputLocked = false;
            samus.Health = samus.MaxHealth = 999;
            samus.EquippedItems = (ushort)SamusEquipmentFlags.VariaSuit;
            samus.EquippedBeams = 0;
            samus.Missiles = samus.MaxMissiles = 100;
            samus.SuperMissiles = samus.MaxSuperMissiles = samus.PowerBombs = samus.MaxPowerBombs = 0;
            runtime.StepFrame(runtime.ControllerBindings.ItemSelect);
            runtime.StepFrame(0);
            var body = runtime.Enemies.Phantoon!.Body;
            for (int frame = 0; frame < 1850; frame++)
            {
                ushort input = (ushort)SnesButton.Up;
                if (frame is >= 1460 and < 1480) input = (ushort)SnesButton.Right;
                if (frame is >= 1548 and < 1566) input |= runtime.ControllerBindings.Jump;
                if (frame is 1565 or 1575) input |= runtime.ControllerBindings.Shoot;
                if (frame >= start)
                {
                    input = (ushort)SnesButton.Left;
                    if ((frame - start) % cadence == 0) input |= runtime.ControllerBindings.Shoot;
                    if (aerial && frame < start + 20) input |= runtime.ControllerBindings.Jump;
                }
                ushort health = body.Health, phase = body.VariableF;
                runtime.StepFrame(input);
                if (body.Health != health)
                    Console.WriteLine($"start={start}, cadence={cadence}, aerial={aerial}, hit={frame}, damage={health-body.Health}, health={body.Health}, phase={(PhantoonAiFunction)phase}->{(PhantoonAiFunction)body.VariableF}, eyeTimer={body.VariableE}, flash={body.FlashTimer}");
            }
        }
        return 0;
    }
}
