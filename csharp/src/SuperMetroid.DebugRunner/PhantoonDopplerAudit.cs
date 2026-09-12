using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;

/// <summary>Normal-input barrage search with an optional pinned original-CPU comparison.</summary>
internal static class PhantoonDopplerAudit
{
    public static int Run(string rom, string? nativePath = null)
    {
        if (nativePath is not null && Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(File.ReadAllBytes(nativePath))) !=
            "609DBAFF42F94CEEBC53B670C8ABDD91C0D97503ADAC6B81876069015BFA0BC5")
            throw new InvalidDataException("Use the accepted phantoon-doppler-native-01 capture.");
        var nativeRows = nativePath is null ? null : File.ReadLines(nativePath).Skip(1)
            .Select(line => line.Split(',').Select(int.Parse).ToArray()).Where(row => row[3] >= 0)
            .ToDictionary(row => (row[0], row[1], row[2], row[3]));
        int compared = 0;
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
                if (nativeRows is not null)
                {
                    int[] actual = [start, cadence, aerial ? 1 : 0, frame, input, runtime.TimeIsFrozen ? 1 : 0,
                        body.Health, body.InvincibilityTimer, body.FlashTimer, body.SpritemapPointer,
                        body.XPosition, body.YPosition, samus.XPosition, samus.YPosition,
                        samus.Pose, samus.SelectedHudItem, samus.Health, body.VariableF,
                        samus.Kinematics.XSubposition, samus.Kinematics.YSubposition];
                    actual = actual.Concat(runtime.Projectiles.Slots.SelectMany(p => p.Type != 0
                        ? new int[] { p.Type, p.XPosition, p.YPosition, p.XSubposition, p.YSubposition, p.Direction, p.Damage }
                        : new int[7])).Concat(runtime.Enemies.EnemyProjectiles.SelectMany(p => p.IsActive
                        ? new int[] { (ushort)p.Kind, p.XPosition, p.YPosition, p.XSubposition, p.YSubposition }
                        : new int[5])).Concat(new int[] { body.XSubposition, body.YSubposition,
                            runtime.Enemies.Phantoon!.Tentacles!.VariableC,
                            runtime.Enemies.Phantoon.Tentacles.VariableD, runtime.Enemies.Phantoon.Tentacles.VariableE }).ToArray();
                    int[] native = nativeRows[(start, cadence, aerial ? 1 : 0, frame)];
                    if (!actual.SequenceEqual(native))
                    {
                        int column = Enumerable.Range(0, Math.Min(actual.Length, native.Length))
                            .FirstOrDefault(i => actual[i] != native[i], -1);
                        throw new InvalidDataException($"Doppler mismatch: start={start}, cadence={cadence}, aerial={aerial}, frame={frame}, column={column}, port={(column < 0 ? actual.Length : actual[column])}, native={(column < 0 ? native.Length : native[column])}.");
                    }
                    compared++;
                }
                if (body.Health != health)
                    Console.WriteLine($"start={start}, cadence={cadence}, aerial={aerial}, hit={frame}, damage={health-body.Health}, health={body.Health}, phase={(PhantoonAiFunction)phase}->{(PhantoonAiFunction)body.VariableF}, eyeTimer={body.VariableE}, flash={body.FlashTimer}");
            }
        }
        if (nativeRows is not null && (compared != 55500 || compared != nativeRows.Count))
            throw new InvalidDataException($"Expected 55500 native barrage frames, compared {compared}.");
        Console.WriteLine($"Native barrage frames compared: {compared}.");
        return 0;
    }
}
