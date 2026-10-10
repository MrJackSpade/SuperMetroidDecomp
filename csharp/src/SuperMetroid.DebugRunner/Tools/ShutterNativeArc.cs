using System.Buffers.Binary;
using SuperMetroid.Core.Runtime;
using SuperMetroid.Core.Game;

internal static partial class AssetTools
{
    /// <summary>
    /// Regenerates issue #347's native comparison inputs from the reproduced case: the frame-117
    /// WRAM seed, every later frame's bomb slots, and the production trajectory the verifier pins.
    /// </summary>
    internal static int ExportShutterBombArc()
    {
        var bus = LoadRepositoryRom();
        var scenario = ShutterBombArcScenario.Reproduced(RepositoryInstallation.CreateRuntime(bus, playerInvincibilityEnabled: true), bus);
        var arc = new List<string>();
        string directory = ShutterBombArcScenario.FixtureDirectory;
        using var bombInputs = new BinaryWriter(File.Create(Path.Combine(directory, "bomb-arc.projectiles")));
        for (int frame = 0; frame < ShutterBombArcScenario.FrameCount; frame++)
        {
            if (frame == ShutterBombArcScenario.FirstArcFrame)
                ExportShutterBombArcSeed(scenario.Runtime, Path.Combine(directory, "bomb-arc.wram"));
            scenario.Runtime.StepFrame(scenario.Input(frame));
            if (frame < ShutterBombArcScenario.FirstArcFrame)
                continue;
            foreach (var bomb in scenario.Runtime.BombProjectiles.Slots)
            {
                bombInputs.Write(bomb.XPosition); bombInputs.Write(bomb.YPosition);
                bombInputs.Write(bomb.XRadius); bombInputs.Write(bomb.YRadius);
                bombInputs.Write(bomb.Direction); bombInputs.Write(bomb.Type);
                bombInputs.Write(bomb.Damage); bombInputs.Write(bomb.BombTimer);
            }
            arc.Add(scenario.ArcRow(frame));
        }
        File.WriteAllLines(Path.Combine(directory, "bomb-arc.csv"), arc);
        Console.WriteLine($"Exported {arc.Count} production bomb-ascent/landing frames for native comparison.");
        return 0;
    }

    /// <summary>Prints the reproduced case's per-frame Samus and shutter state for native comparison.</summary>
    internal static int TraceShutterBombArc()
    {
        var bus = LoadRepositoryRom();
        var scenario = ShutterBombArcScenario.Reproduced(RepositoryInstallation.CreateRuntime(bus, playerInvincibilityEnabled: true), bus);
        var shutter = scenario.Runtime.Enemies.VerticalShutterStates[0]!;
        for (int frame = 0; frame < ShutterBombArcScenario.FrameCount; frame++)
        {
            scenario.Runtime.StepFrame(scenario.Input(frame));
            if (frame >= ShutterBombArcScenario.FirstArcFrame)
                Console.WriteLine($"frame {frame}: {scenario.ArcRow(frame)} pose={scenario.Samus.Pose:X2} " +
                    $"shutter={(ushort)shutter.Function:X4} up={shutter.MovedUpRestTime:X4} down={shutter.MovedDownRestTime:X4} " +
                    $"rev={shutter.ReactionDirection:X4} act={(shutter.ShotActivated ? 1 : 0):X4}");
        }
        return 0;
    }

    /// <summary>
    /// Sweeps both shutters, start positions, bomb intervals and roll timings, reporting the
    /// smallest Samus/shutter gap. Diagnostic only; the verifier pins the reproduced case.
    /// </summary>
    internal static int SweepShutterMorphApproaches()
    {
        var bus = LoadRepositoryRom();
        int cases = 0, worstGap = 0;
        foreach (int slotIndex in new[] { 0, 1 })
        foreach (bool approach in new[] { false, true })
        foreach (int interval in new[] { 8, 20, 40 })
        foreach (int rollAt in new[] { 30, 45, 60, 75, 90, 105, 120, 150 })
        foreach (int duration in new[] { 4, 8, 16 })
        {
            var scenario = ShutterBombArcScenario.Create(RepositoryInstallation.CreateRuntime(bus, playerInvincibilityEnabled: true), bus, slotIndex, approach, interval, rollAt, duration);
            var samus = scenario.Samus;
            var platform = scenario.Platform;
            for (int frame = 0; frame < ShutterBombArcScenario.FrameCount; frame++)
            {
                scenario.Runtime.StepFrame(scenario.Input(frame));
                int gap = platform.YPosition - platform.YRadius - samus.YPosition - samus.Kinematics.YRadius;
                if (Math.Abs(samus.XPosition - platform.XPosition) < platform.XRadius + samus.Kinematics.XRadius &&
                    samus.YPosition < platform.YPosition && gap < worstGap)
                {
                    worstGap = gap;
                    Console.WriteLine($"Morph approach: slot={slotIndex} approach={approach} interval={interval} roll={rollAt}/{duration} frame={frame} gap={gap} Samus={samus.XPosition},{samus.YPosition}/{samus.Pose:X2} platformY={platform.YPosition}");
                }
                if (samus.Kinematics.YRadius != 7)
                    throw new InvalidOperationException(
                        $"slot={slotIndex} approach={approach} interval={interval} roll={rollAt}/{duration} frame={frame}: morph-only approach entered a standing/unmorph posture");
            }
            cases++;
        }
        Console.WriteLine($"{cases} morph-only bomb/roll/return sequences; minimum gap={worstGap}.");
        return 0;
    }

    /// <summary>Minimal native WRAM seed for the actual room's frame-117 bomb ascent.</summary>
    private static void ExportShutterBombArcSeed(SuperMetroidRuntime runtime, string path)
    {
        var ram = new byte[0x20000];
        var samus = runtime.Samus!;
        var k = samus.Kinematics;
        var speed = samus.HorizontalSpeed;
        var level = runtime.LevelData!;
        Word(0x7A5, level.WidthInBlocks);
        Word(0x7A7, level.HeightInBlocks);
        for (int i = 0; i < level.WidthInBlocks * level.HeightInBlocks; i++)
        {
            var block = level.GetCollisionBlockByIndex(i);
            Word(0x10002 + i * 2, block.LevelWord);
            ram[0x16402 + i] = block.Behavior;
        }
        Word(0xAF6, k.XPosition); Word(0xAF8, k.XSubposition);
        Word(0xAFA, k.YPosition); Word(0xAFC, k.YSubposition);
        Word(0xAFE, k.XRadius); Word(0xB00, k.YRadius);
        Word(0xA1C, samus.Pose);
        // Native pose record carries direction/movement type in its first two bytes.
        int pose = 0x91B629 + samus.Pose * 8;
        ram[0xA1E] = runtime.AddressSpace.ReadByte(pose);
        ram[0xA1F] = runtime.AddressSpace.ReadByte(pose + 1);
        Word(0xA56, samus.BombJumpDirection); Word(0xA58, 0xE032); Word(0xA60, 0xE90E);
        Word(0xB2C, k.YSubspeed); Word(0xB2E, k.YSpeed);
        Word(0xB32, k.YSubacceleration); Word(0xB34, k.YAcceleration); Word(0xB36, k.YDirection);
        Word(0xB42, speed.ExtraRunSpeed); Word(0xB44, speed.ExtraRunSubspeed);
        Word(0xB46, speed.BaseSpeed); Word(0xB48, speed.BaseSubspeed); Word(0xB4A, speed.AccelerationMode);
        Word(0x195E, 0xFFFF); Word(0x1962, 0xFFFF);
        Word(0x5B6, runtime.NmiFrameCounter);
        // Retain both solid platforms, even though only slot zero is moving here.
        Word(0x17A6, 4); Word(0x17EC, 0); Word(0x17EE, 64); Word(0x17F0, 0xFFFF);
        for (int i = 0; i < 2; i++)
        {
            int offset = i * 64;
            var slot = runtime.Enemies.Slots[i];
            Word(0xF78 + offset, (ushort)slot.EnemyDefinitionPointer);
            Word(0xF8E + offset, slot.SpritemapPointer);
            ram[0xFA6 + offset] = slot.Definition.Bank;
            Word(0xF7A + offset, slot.XPosition); Word(0xF7E + offset, slot.YPosition);
            Word(0xF80 + offset, slot.YSubposition); Word(0xF82 + offset, slot.XRadius);
            Word(0xF84 + offset, slot.YRadius); Word(0xF86 + offset, 0x8000);
        }
        var shutter = runtime.Enemies.VerticalShutterStates[0]!;
        Word(0xFA8, (ushort)shutter.Function);
        Word(0xFB0, shutter.UpSubvelocity); Word(0xFB2, unchecked((ushort)shutter.UpVelocity));
        Word(0xFAC, shutter.DownSubvelocity); Word(0xFAE, unchecked((ushort)shutter.DownVelocity));
        Word(0x780E, shutter.InitialFunctionTableOffset); Word(0x7818, shutter.ShotActivated ? 1 : 0);
        Word(0x7812, shutter.MovedDownRestTime); Word(0x7820, shutter.MaximumYPosition);
        Word(0x8000, shutter.ReactionDirection);
        Word(0x781E, shutter.MinimumYPosition); Word(0x7810, shutter.MovedUpRestTime);
        File.WriteAllBytes(path, ram);

        void Word(int address, int value) => BinaryPrimitives.WriteUInt16LittleEndian(ram.AsSpan(address, 2), unchecked((ushort)value));
    }
}
