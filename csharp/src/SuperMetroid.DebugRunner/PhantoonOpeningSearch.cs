using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;

/// <summary>
/// Finds an input-only two-hit eye opening before constructing the spaced
/// Doppler barrage. Exploratory output is not an original-cartridge oracle.
/// </summary>
internal static class PhantoonOpeningSearch
{
    public static int Run(string rom, bool barrage = false, string? nativePath = null)
    {
        if (nativePath is not null && (!barrage || Convert.ToHexString(
            System.Security.Cryptography.SHA256.HashData(File.ReadAllBytes(nativePath))) !=
            "00DC7756E95692D4C969A15D83BF60B2490E80328F05B48CD7F379B4D771EA7C"))
            throw new InvalidDataException("Use the pinned original-CPU spaced-Doppler capture.");
        var nativeRows = nativePath is null ? null : File.ReadLines(nativePath).Skip(1)
            .Select(line => line.Split(',').Select(int.Parse).ToArray()).Where(row => row[3] >= 0)
            .ToDictionary(row => (row[0], row[1], row[2], row[3]));
        int compared = 0;
        foreach (bool clearFlames in barrage ? new[] { true } : new[] { false, true })
        foreach (int jumpStart in barrage ? new[] { 1530 } : new[] { 1520, 1530, 1540 })
        foreach (int firstShot in barrage ? new[] { 1540 } : new[] { 1540, 1550, 1560 })
        foreach (int barrageStart in barrage ? new[] { 1630, 1640, 1650 } : new[] { 0 })
        foreach (int movement in barrage ? Enumerable.Range(0, 13) : new[] { 0 })
        foreach (int cadence in barrage ? new[] { 9, 10, 11 } : new[] { 10 })
        {
            if (nativeRows is not null && (barrageStart != 1640 ||
                !((movement is 0 or 2 && cadence == 10) || movement == 5))) continue;
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
            // Match the two setup frames used by the existing native probes.
            // Beam-clear controls defer selecting missiles until after clearing.
            runtime.StepFrame(clearFlames ? (ushort)0 : runtime.ControllerBindings.ItemSelect);
            runtime.StepFrame(0);
            var body = runtime.Enemies.Phantoon!.Body;
            int openingHits = 0;
            int firstClosure = -1;
            var hits = new List<int>();
            for (int frame = 0; frame < (barrage ? 1950 : 1620); frame++)
            {
                ushort input = (ushort)SnesButton.Up;
                if (frame is >= 1460 and < 1480) input = (ushort)SnesButton.Right;
                if (clearFlames && frame is >= 1400 and < 1510 && frame % 12 == 0)
                    input |= runtime.ControllerBindings.Shoot;
                if (clearFlames && frame == 1512) input |= runtime.ControllerBindings.ItemSelect;
                if (frame >= jumpStart && frame < jumpStart + 40) input |= runtime.ControllerBindings.Jump;
                if (frame == firstShot || frame == firstShot + 10) input |= runtime.ControllerBindings.Shoot;
                if (barrage && frame >= barrageStart)
                {
                    int elapsed = frame - barrageStart;
                    bool walk = movement == 1 || movement == 2 && elapsed >= 60 || movement == 3 && elapsed % 20 < 10;
                    if (movement >= 4) walk = elapsed >= 60 && (elapsed - 60) % 10 < movement - 3;
                    input = elapsed == 0 || walk ? (ushort)SnesButton.Left : (ushort)0;
                    if (elapsed is 0 or 10 || elapsed >= 60 && (elapsed - 60) % cadence == 0)
                        input |= runtime.ControllerBindings.Shoot;
                }
                ushort health = body.Health;
                var phase = (PhantoonAiFunction)body.VariableF;
                runtime.StepFrame(input);
                if (nativeRows is not null)
                {
                    int[] actual = PhantoonFrameTrace.Capture(runtime, barrageStart, cadence, movement, frame, input);
                    int[] native = nativeRows[(barrageStart, cadence, movement, frame)];
                    if (!actual.SequenceEqual(native))
                    {
                        int column = Enumerable.Range(0, Math.Min(actual.Length, native.Length))
                            .FirstOrDefault(i => actual[i] != native[i], -1);
                        throw new InvalidDataException($"Spaced Doppler mismatch: movement={movement}, cadence={cadence}, frame={frame}, column={column}, port={(column < 0 ? actual.Length : actual[column])}, native={(column < 0 ? native.Length : native[column])}.");
                    }
                    compared++;
                }
                if (firstClosure < 0 && body.VariableF == (ushort)PhantoonAiFunction.FadeOutWhileSwooping)
                    firstClosure = frame;
                if (body.Health == health) continue;
                hits.Add(frame);
                if (phase == PhantoonAiFunction.EyeTracksSamus) openingHits++;
                Console.WriteLine($"clear={clearFlames}, jump={jumpStart}, shot={firstShot}, start={barrageStart}, movement={movement}, cadence={cadence}, hit={frame}, damage={health-body.Health}, phase={phase}, samus={samus.XPosition}/{samus.YPosition}, boss={body.XPosition}/{body.YPosition}");
            }
            Console.WriteLine($"RESULT clear={clearFlames}, jump={jumpStart}, shot={firstShot}, start={barrageStart}, movement={movement}, cadence={cadence}, openingHits={openingHits}, health={body.Health}, closure={firstClosure}, hits={string.Join(',', hits)}");
            if (nativeRows is not null)
            {
                int[] expectedHits = (movement, cadence) switch
                {
                    (0, 10) => [1546, 1550, 1645, 1658, 1709],
                    (2, 10) => [1546, 1550, 1645, 1658, 1709, 1713, 1720],
                    (5, 9) => [1546, 1550, 1645, 1658, 1709, 1718],
                    (5, 10) => [1546, 1550, 1645, 1658, 1709, 1718, 1727, 1735, 1744, 1752, 1760],
                    (5, 11) => [1546, 1550, 1645, 1658, 1709, 1718, 1726, 1735, 1744],
                    _ => throw new InvalidDataException("Unknown spaced-Doppler control.")
                };
                if (openingHits != 2 || !hits.SequenceEqual(expectedHits) ||
                    firstClosure != expectedHits[^1] + 9 || body.Health != 2500 - 100 * expectedHits.Length)
                    throw new InvalidDataException("Spaced opening, accepted barrage hits, health, or closure timing differs.");
            }
        }
        if (nativeRows is not null && (compared != 9750 || compared != nativeRows.Count))
            throw new InvalidDataException($"Expected 9,750 original-CPU spaced frames; compared {compared}.");
        Console.WriteLine($"Native spaced frames compared: {compared}.");
        return 0;
    }
}
