using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Runtime;

/// <summary>#442 room-local exploration; a trace is not yet a cartridge parity assertion.</summary>
internal static class ZebetiteSkipAudit
{
    public static int Run(string romPath)
    {
        foreach (int stepBackFrames in new[] { 0, 8, 20 })
            RunCase(romPath, stepBackFrames);
        Console.WriteLine("Exploratory Ice-only traces completed. Native passage/control parity is NOT established.");
        return 0;
    }

    private static void RunCase(string romPath, int stepBackFrames)
    {
        var bus = SuperMetroidAddressSpace.LoadRetailRom(romPath);
        var runtime = new SuperMetroidRuntime(bus);
        runtime.InitializeHud(HudSnapshot.CeresDebug);
        runtime.InitializeStartingCeresRoom();
        runtime.InitializeCeresStartSamus();
        // No destroyed-Zebetite events: this must be the first barrier, not the
        // second-generation setup used by the ten-missile destruction fixture.
        runtime.LoadCartridgeRoomForDebug(0xdd58);
        var samus = runtime.Samus!;
        samus.XPosition = 900; samus.YPosition = 100;
        samus.Kinematics.XSubposition = samus.Kinematics.YSubposition = 0;
        samus.Pose = SamusPoseIds.FacingLeftNormalPose;
        samus.EquippedItems = samus.CollectedItems = (ushort)(SamusEquipmentFlags.MorphBall | SamusEquipmentFlags.GravitySuit);
        samus.EquippedBeams = samus.CollectedBeams = (ushort)SamusBeamFlags.Ice;
        samus.SelectedHudItem = 0;
        samus.Health = samus.MaxHealth = 399;
        samus.RefreshCollisionRadii(bus); samus.InitializeAnimation(bus);
        runtime.Camera!.SetPosition(768, 0);
        bool frozeSpawn = false;
        int jumpStart = 120 + stepBackFrames;
        Console.WriteLine($"CASE stepBackFrames={stepBackFrames}");
        for (int frame = 0; frame < 360; frame++)
        {
            ushort input = frame < 60 ? (ushort)SnesButton.Left :
                frame < 120 ? (ushort)(SnesButton.Up | SnesButton.X) :
                frame < jumpStart ? (ushort)SnesButton.Right :
                (ushort)(SnesButton.Left | ((frame - jumpStart) % 36 < 24 ? SnesButton.A : 0));
            runtime.StepFrame(input);
            frozeSpawn |= runtime.Enemies.Slots.Any(slot => slot.EnemyDefinitionPointer == 0xd23f &&
                slot.XPosition == 823 && slot.YPosition == 166 && slot.FrozenTimer != 0);
            if (frame % 12 == 0 || frame >= jumpStart && frame <= jumpStart + 24)
            {
                var actors = runtime.Enemies.Slots.Where(slot => slot.EnemyDefinitionPointer is 0xd23f or 0xe27f)
                    .Select(slot => $"{slot.NativeIndex}:{slot.EnemyDefinitionPointer:X4}@{slot.XPosition},{slot.YPosition}/f{slot.FrozenTimer}/p{slot.Properties}");
                Console.WriteLine($"SKIP frame={frame} input={input:X4} samus={samus.XPosition}.{samus.Kinematics.XSubposition:X4},{samus.YPosition}.{samus.Kinematics.YSubposition:X4} pose={samus.Pose:X2} hp={samus.Health} inv={samus.InvincibilityTimer} actors={string.Join(';', actors)}");
            }
        }
        if (!frozeSpawn)
            throw new InvalidDataException("Ice-only exploration failed to freeze the lower Rinka at its spawn point.");
        Console.WriteLine($"END stepBackFrames={stepBackFrames} x={samus.XPosition} y={samus.YPosition} pose={samus.Pose:X2} hp={samus.Health}; passage not asserted");
    }
}
