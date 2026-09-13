using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Runtime;
using SuperMetroid.Core.Input;

/// <summary>Room-local setup exploration for #443; not a native parity assertion.</summary>
internal static class ZebetitePlayerSetupAudit
{
    public static int Run(string romPath)
    {
        RunCase(romPath, initializeOnscreen: false);
        RunCase(romPath, initializeOnscreen: true);
        if (!RunCase(romPath, true, 728, 641, 99, verbose: false) ||
            RunCase(romPath, true, 724, 641, 99, verbose: false) ||
            RunCase(romPath, true, 732, 641, 99, verbose: false) ||
            RunCase(romPath, true, 728, 641, 96, verbose: false))
            throw new InvalidDataException("Room-local single-hit suppression candidate or adjacent controls changed.");
        return 0;
    }

    public static int Search(string romPath)
    {
        int cases = 0, successes = 0;
        for (int x = 696; x <= 736; x += 4)
        for (int camera = 641; camera <= 655; camera += 2)
        for (int rightEnd = 87; rightEnd <= 105; rightEnd += 3)
        {
            if (RunCase(romPath, true, x, camera, rightEnd, verbose: false)) successes++;
            if (++cases % 40 == 0) Console.WriteLine($"SEARCH cases={cases} successes={successes}");
        }
        Console.WriteLine($"SEARCH complete cases={cases} successes={successes}; not native parity.");
        if (cases != 616 || successes != 24)
            throw new InvalidDataException("Exploratory exposure-search result changed; inspect the candidate trajectories.");
        return 0;
    }

    public static int Export(string romPath, string directory)
    {
        Directory.CreateDirectory(directory);
        foreach (int x in new[] { 724, 728, 732 })
            RunCase(romPath, true, x, 641, 99, verbose: false,
                exportPrefix: Path.Combine(directory, $"candidate-{x}"));
        Console.WriteLine("Exported three local candidate seeds/traces. Seeds contain room data; do not publish. Native import/parity remains unfinished.");
        return 0;
    }

    private static bool RunCase(string romPath, bool initializeOnscreen,
        int startX = 696, int startCamera = 641, int rightEnd = 87, bool verbose = true,
        string? exportPrefix = null)
    {
        if (verbose) Console.WriteLine($"CASE initialized={initializeOnscreen}");
        var bus = SuperMetroidAddressSpace.LoadRetailRom(romPath);
        var runtime = new SuperMetroidRuntime(bus);
        runtime.InitializeHud(HudSnapshot.CeresDebug);
        runtime.InitializeStartingCeresRoom();
        runtime.InitializeCeresStartSamus();
        runtime.System.SetEvent(EventNumber.ZebetiteDestroyedBit0);
        runtime.LoadCartridgeRoomForDebug(0xdd58);
        var samus = runtime.Samus!;
        samus.XPosition = (ushort)startX; samus.YPosition = 100;
        samus.Kinematics.XSubposition = samus.Kinematics.YSubposition = 0;
        samus.Pose = SamusPoseIds.FacingRightNormalPose;
        samus.RefreshCollisionRadii(bus); samus.InitializeAnimation(bus);
        samus.Health = samus.MaxHealth = 999;
        samus.Missiles = samus.MaxMissiles = 10;
        samus.SelectedHudItem = 1;
        if (initializeOnscreen)
        {
            runtime.Camera!.SetPosition(600, 0);
            runtime.StepFrame(0);
            runtime.StepFrame(0);
        }
        runtime.Camera!.SetPosition(startCamera, 0);
        var level = runtime.LevelData!;
        for (int y = 0; verbose && y < 16; y++)
        {
            Console.Write($"row {y,2}: ");
            for (int x = 38; x <= 45; x++)
                Console.Write($"{level.ForegroundEntries.Span[y * level.WidthInBlocks + x] >> 12:X} ");
            Console.WriteLine();
        }
        bool hitBarrier = false;
        using var trace = exportPrefix is null ? null : new StreamWriter(exportPrefix + ".jsonl");
        for (int frame = 0; frame < 120; frame++)
        {
            if (frame == 60 && exportPrefix is not null)
            {
                RoomMovementSeedExporter.Write(runtime, exportPrefix + ".movement-seed");
                // Metadata supplements, but does not pretend to extend, MOV1's
                // collision-only native consumer. A dedicated importer is required.
                File.WriteAllText(exportPrefix + ".metadata.json", System.Text.Json.JsonSerializer.Serialize(new
                {
                    Format = "zebetite-candidate-v1", StartFrame = frame,
                    NativeParityEstablished = false,
                    samus.Health, samus.MaxHealth, samus.Missiles, samus.SelectedHudItem,
                    samus.PoseHistory.PreviousPose, samus.PoseHistory.PreviousDirectionAndMovement,
                    samus.PoseHistory.LastDifferentPose, samus.PoseHistory.LastDifferentDirectionAndMovement,
                    CameraX = runtime.Camera.XPosition, CameraY = runtime.Camera.YPosition,
                    Enemies = runtime.Enemies.Slots.Where(slot => slot.EnemyDefinitionPointer != 0).Select(slot => new
                    {
                        slot.NativeIndex, slot.EnemyDefinitionPointer, slot.XPosition, slot.YPosition,
                        slot.Health, slot.FlashTimer, slot.AiHandlerBits, slot.Parameter1, slot.Parameter2,
                        slot.VariableA, slot.VariableB, slot.VariableC, slot.VariableD, slot.VariableE, slot.VariableF,
                    }).ToArray(),
                }, new System.Text.Json.JsonSerializerOptions { WriteIndented = true }));
            }
            SnesButton input = frame switch
            {
                60 or 61 => SnesButton.Down,
                66 => SnesButton.Left,
                78 => SnesButton.X,
                80 => SnesButton.Left | SnesButton.A,
                _ when frame >= 81 && frame < rightEnd => SnesButton.Right | SnesButton.A,
                _ => 0,
            };
            runtime.StepFrame((ushort)input);
            if (frame >= 60 && trace is not null)
                trace.WriteLine(System.Text.Json.JsonSerializer.Serialize(new
                {
                    Frame = frame, Input = (ushort)input,
                    samus.Kinematics.XFixed, samus.Kinematics.YFixed, samus.Pose,
                    samus.AnimationFrame, samus.AnimationFrameTimer,
                    CameraX = runtime.Camera.XPosition, CameraY = runtime.Camera.YPosition,
                    CameraXSub = runtime.Camera.XSubposition, CameraYSub = runtime.Camera.YSubposition,
                    samus.Missiles,
                    Shots = runtime.Projectiles.Slots.Select(shot => new
                    {
                        shot.Type, shot.Damage, shot.XPosition, shot.YPosition, shot.XSubposition, shot.YSubposition,
                        shot.XVelocity, shot.YVelocity, shot.Direction, shot.InstructionPointer, shot.InstructionTimer,
                        shot.Variable,
                    }).ToArray(),
                    Barriers = runtime.Enemies.Slots.Where(slot => slot.EnemyDefinitionPointer == 0xe27f)
                        .Select(slot => new { slot.NativeIndex, slot.Health, slot.FlashTimer, slot.AiHandlerBits }).ToArray(),
                }));
            hitBarrier |= runtime.Enemies.Slots.Any(slot => slot.EnemyDefinitionPointer == 0xe27f && slot.Health < 1000);
            if (verbose && (frame % 10 == 0 || frame is >= 60 and < 90))
                Console.WriteLine($"SETUP frame={frame} input={(ushort)input:X4} x={samus.XPosition} y={samus.YPosition} pose={samus.Pose:X2} camera={runtime.Camera.XPosition},{runtime.Camera.YPosition} health={samus.Health} missiles={samus.Missiles} barrier={string.Join('/', runtime.Enemies.Slots.Where(slot => slot.EnemyDefinitionPointer == 0xe27f).Select(slot => slot.Health))}");
            if (verbose && frame is >= 70 and < 90)
                foreach (var shot in runtime.Projectiles.Slots.Where(shot => shot.Type != 0))
                    Console.WriteLine($"SHOT frame={frame} type={shot.Type:X4} x={shot.XPosition} y={shot.YPosition} direction={shot.Direction:X4}");
        }
        if (verbose && (!hitBarrier || samus.Missiles != 9))
            throw new InvalidDataException("Exploratory room setup no longer delivers its single missile hit.");
        var lower = runtime.Enemies.Slots.FirstOrDefault(slot => slot.EnemyDefinitionPointer == 0xe27f && slot.Parameter1 != 0);
        bool success = hitBarrier && samus.Missiles == 9 && lower?.Health == 900 && runtime.Camera.XPosition > 640;
        if (success) Console.WriteLine($"CANDIDATE x={startX} camera={startCamera} rightEnd={rightEnd} finalX={samus.XPosition} finalCamera={runtime.Camera.XPosition} lowerHealth={lower!.Health}");
        return success;
    }
}
