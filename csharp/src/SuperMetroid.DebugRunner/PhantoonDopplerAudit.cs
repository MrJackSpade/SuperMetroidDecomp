using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;

/// <summary>Normal-input barrage search with an optional pinned original-CPU comparison.</summary>
internal static class PhantoonDopplerAudit
{
    public static int Run(string rom, string? nativePath = null, bool expandedSearch = false, bool windowControls = false, bool patternSearch = false, bool movingControls = false)
    {
        string expectedHash = movingControls ? "E6CA849236B495FB2B7E1971E4998F118022F7BACE5CB6DCF8C34AB4283519EA"
            : patternSearch ? "70F2F64F7A6D4169D2F4382CB6C617D9A6440DEE3A37983CE9F4D34105BDF928"
            : windowControls ? "15A5E22F7BBCC381D98EE42B648489818A59F5733D002297C4AE40FF0F83DF1A"
            : "609DBAFF42F94CEEBC53B670C8ABDD91C0D97503ADAC6B81876069015BFA0BC5";
        if (nativePath is not null && Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(File.ReadAllBytes(nativePath))) != expectedHash)
            throw new InvalidDataException("Use the accepted phantoon-doppler-native-01 capture.");
        var nativeRows = nativePath is null ? null : File.ReadLines(nativePath).Skip(1)
            .Select(line => line.Split(',').Select(int.Parse).ToArray()).Where(row => row[3] >= 0)
            .ToDictionary(row => (row[0], row[1], row[2], row[3]));
        int compared = 0;
        foreach (var (start, cadence, aerial, movement) in Cases(expandedSearch, windowControls, patternSearch, movingControls))
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
            var hitFrames = new List<int>();
            var hitY = new List<ushort>();
            var hitX = new List<ushort>();
            int firstClosure = -1;
            for (int frame = 0; frame < 1850; frame++)
            {
                ushort input = (ushort)SnesButton.Up;
                if (frame is >= 1460 and < 1480) input = (ushort)SnesButton.Right;
                if (frame is >= 1548 and < 1566) input |= runtime.ControllerBindings.Jump;
                if (frame is 1565 or 1575) input |= runtime.ControllerBindings.Shoot;
                if (frame >= start)
                {
                    input = (ushort)SnesButton.Left;
                    if (movement == 1) input |= runtime.ControllerBindings.Dash;
                    if (movement == 2 && frame > start) input = 0;
                    // Short release/repress movements between shots search the
                    // stuttered technique without forcing Samus's coordinates.
                    if (movement is >= 7 and <= 10 && (frame - start) % 10 >= movement - 6) input = 0;
                    if (movement >= 11 && frame > start && frame - start < 20 + (movement - 11) * 5) input = 0;
                    if ((frame - start) % cadence == 0) input |= runtime.ControllerBindings.Shoot;
                    if (patternSearch)
                    {
                        // Search two-shot groups separated by a quiet interval,
                        // followed by a barrage. Vary physical movement only;
                        // never reposition either actor after encounter setup.
                        int elapsed = frame - start;
                        bool shot = elapsed is 0 or 10 or 40 or 50 || elapsed >= 80 && (elapsed - 80) % cadence == 0;
                        bool walk = movement == 3 || movement == 4 && elapsed >= 80 ||
                            movement == 5 && elapsed % 20 < 10;
                        input = walk || elapsed == 0 ? (ushort)SnesButton.Left : (ushort)0;
                        if (shot) input |= runtime.ControllerBindings.Shoot;
                    }
                    if (aerial && frame < start + 20) input |= runtime.ControllerBindings.Jump;
                }
                ushort health = body.Health, phase = body.VariableF;
                runtime.StepFrame(input);
                if (nativeRows is not null)
                {
                    int[] actual = [start, cadence, patternSearch || movingControls ? movement : aerial ? 1 : 0, frame, input, runtime.TimeIsFrozen ? 1 : 0,
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
                    if (windowControls || patternSearch || movingControls)
                        actual = actual.Concat(new int[] { body.VariableE, body.CurrentInstruction, body.InstructionTimer,
                            runtime.Enemies.Phantoon.Eye!.CurrentInstruction, runtime.Enemies.Phantoon.Eye.InstructionTimer,
                            runtime.Enemies.Phantoon.Tentacles.VariableB, samus.PreviousDrawNewInput, runtime.BombProjectiles.CooldownTimer }).ToArray();
                    int[] native = nativeRows[(start, cadence, patternSearch || movingControls ? movement : aerial ? 1 : 0, frame)];
                    if (!actual.SequenceEqual(native))
                    {
                        int column = Enumerable.Range(0, Math.Min(actual.Length, native.Length))
                            .FirstOrDefault(i => actual[i] != native[i], -1);
                        throw new InvalidDataException($"Doppler mismatch: start={start}, cadence={cadence}, aerial={aerial}, frame={frame}, column={column}, port={(column < 0 ? actual.Length : actual[column])}, native={(column < 0 ? native.Length : native[column])}.");
                    }
                    compared++;
                }
                if (body.Health != health)
                {
                    hitFrames.Add(frame);
                    hitY.Add(samus.YPosition);
                    hitX.Add(samus.XPosition);
                    Console.WriteLine($"start={start}, cadence={cadence}, aerial={aerial}, movement={movement}, hit={frame}, damage={health-body.Health}, health={body.Health}, phase={(PhantoonAiFunction)phase}->{(PhantoonAiFunction)body.VariableF}, eyeTimer={body.VariableE}, flash={body.FlashTimer}, samus={samus.XPosition}/{samus.YPosition}");
                }
                if (firstClosure < 0 && body.VariableF == (ushort)PhantoonAiFunction.FadeOutWhileSwooping)
                    firstClosure = frame;
            }
            if (windowControls)
            {
                int[] expectedHits = (start, cadence) switch
                {
                    (1680, 10) => [1565, 1693, 1701, 1711, 1720],
                    (1680, 11) => [1565, 1693, 1703, 1713],
                    (1710, 9) => [1565, 1710, 1720, 1747, 1756],
                    (1710, 10) => [1565, 1710, 1720, 1837],
                    _ => throw new InvalidDataException("Unknown window control.")
                };
                int expectedClosure = expectedHits[^1] + 9;
                if (!hitFrames.SequenceEqual(expectedHits) || firstClosure != expectedClosure ||
                    body.Health != 2500 - 100 * expectedHits.Length ||
                    start == 1710 && cadence == 9 && (hitY[^2] != 106 || hitY[^1] != 122))
                    throw new InvalidDataException($"Window control failed: hits={string.Join(',', hitFrames)}, closure={firstClosure}, health={body.Health}.");
            }
            if (patternSearch)
                Console.WriteLine($"pattern start={start}, cadence={cadence}, movement={movement}, hits={string.Join(',', hitFrames)}, closure={firstClosure}, samus={samus.XPosition}/{samus.YPosition}, boss={body.XPosition}/{body.YPosition}");
            if (movingControls)
            {
                int[] expected = (movement, cadence) switch
                {
                    (12, 10) => [1565, 1693, 1701, 1710],
                    (12, 11) => [1565, 1693, 1703, 1713],
                    (13, 10) => [1565, 1693, 1701, 1711, 1720],
                    (13, 11) => [1565, 1693, 1703, 1713],
                    _ => throw new InvalidDataException("Unknown moving control.")
                };
                if (!hitFrames.SequenceEqual(expected) || firstClosure != expected[^1] + 9 || body.Health != 2500 - 100 * expected.Length)
                    throw new InvalidDataException("Moving barrage hit/closure boundary differs.");
                if (movement == 13 && cadence == 10 && (hitX[^2] != 116 || hitX[^1] != 105 || hitY[^1] != 187))
                    throw new InvalidDataException("Successful barrage must move left on the ground during its final two hits.");
            }
        }
        int expectedFrames = patternSearch ? 103600 : windowControls || movingControls ? 7400 : 55500;
        if (nativeRows is not null && (compared != expectedFrames || compared != nativeRows.Count))
            throw new InvalidDataException($"Expected {expectedFrames} native barrage frames, compared {compared}.");
        Console.WriteLine($"Native barrage frames compared: {compared}.");
        return 0;
    }

    private static IEnumerable<(int Start, int Cadence, bool Aerial, int Movement)> Cases(bool expanded, bool windows, bool patterns, bool moving)
    {
        if (moving)
        {
            foreach (int movement in new[] { 12, 13 })
            foreach (int cadence in new[] { 10, 11 })
                yield return (1680, cadence, false, movement);
            yield break;
        }
        if (patterns)
        {
            foreach (int start in new[] { 1640, 1650, 1660, 1670, 1680, 1690, 1700 })
            foreach (int cadence in new[] { 10, 11 })
            foreach (int movement in new[] { 3, 4, 5, 6 })
                yield return (start, cadence, false, movement);
            yield break;
        }
        if (windows)
        {
            yield return (1680, 10, false, 2);
            yield return (1680, 11, false, 2);
            yield return (1710, 9, true, 2);
            yield return (1710, 10, true, 2);
            yield break;
        }
        if (expanded)
            foreach (int movement in Enumerable.Range(11, 9))
            foreach (int cadence in new[] { 10, 11 })
                yield return (1680, cadence, false, movement);
        foreach (int start in expanded ? new[] { 1680, 1690, 1700, 1710, 1720, 1730, 1740, 1750, 1760 } : new[] { 1640, 1650, 1660 })
        foreach (int cadence in new[] { 8, 9, 10, 11, 12 })
        foreach (bool aerial in new[] { false, true })
        foreach (int movement in expanded ? new[] { 0, 1, 2, 7, 8, 9, 10 } : new[] { 0 })
            yield return (start, cadence, aerial, movement);
    }
}
