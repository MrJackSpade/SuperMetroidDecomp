using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Runtime;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Hardware;

/// <summary>
/// Captures the actual runtime-rendered defeat and independent arm state for #520.
/// No gameplay state is changed by this observer. The encounter setup and genuine
/// projectile-hit seam are owned by KraidAudit, including its explicitly seeded lethal HP.
/// </summary>
internal sealed class KraidDeathCapture : IDisposable
{
    private readonly string _directory;
    private readonly StreamWriter _trace;
    private ushort? _lastPhase;
    private Rgba32[]? _floorBefore;

    public KraidDeathCapture(string directory)
    {
        _directory = directory;
        Directory.CreateDirectory(directory);
        _trace = new StreamWriter(Path.Combine(directory, "arm.csv"), append: false);
        _trace.WriteLine("frame,phase,bodyx,bodyy,camerax,cameray,armx,army,properties,instruction,map,timer");
    }

    public void Capture(SuperMetroidRuntime runtime, int frame)
    {
        var body = runtime.Enemies.Slots[0];
        var arm = runtime.Enemies.Slots[1];
        var camera = runtime.Camera ?? throw new InvalidDataException("Kraid capture requires a room camera.");
        _trace.WriteLine($"{frame},{body.VariableA:X4},{body.XPosition},{body.YPosition}," +
            $"{camera.XPosition},{camera.YPosition},{arm.XPosition},{arm.YPosition}," +
            $"{(ushort)arm.Properties:X4},{arm.CurrentInstruction:X4},{arm.SpritemapPointer:X4},{arm.InstructionTimer}");
        if (frame % 8 == 0 || _lastPhase != body.VariableA)
            PngWriter.WriteRgba(Path.Combine(_directory, $"death-{frame + 1:D4}-{body.VariableA:X4}.png"),
                FrontendFrame.Width, FrontendFrame.Height,
                SoftwareLayeredSnapshotRenderer.Render(GameplayDisplayCapture.TryCaptureFrame(runtime)
                    ?? throw new InvalidDataException("Kraid capture did not produce a gameplay packet.")));
        _lastPhase = body.VariableA;
        if (frame == -1 || runtime.Enemies.Kraid?.DeathSequenceComplete == true)
        {
            var ppu = runtime.DisplayedGameplayPpu;
            var bg1 = SnesBgTilemapRenderer.Render4BppViewport(runtime.Vram, runtime.Cgram,
                    SnesPpuLayout.GameplayBg1TilemapWord, 0,
                    ppu.Bg1HorizontalScroll, ppu.Bg1VerticalScroll,
                    FrontendFrame.Width, FrontendFrame.Height);
            PngWriter.WriteRgba(Path.Combine(_directory, $"bg1-{frame + 1:D4}.png"),
                FrontendFrame.Width, FrontendFrame.Height, bg1);
            // This rectangle is the visible part of the hazard row only in the
            // bottom-left observer. Do not silently apply it to the arm viewport.
            if (camera.XPosition == 0 && camera.YPosition == 256)
            {
                if (frame == -1) _floorBefore = bg1;
                else if (_floorBefore is not null)
                {
                    int same = 0, total = 0;
                    for (int y = 176; y < 192; y++)
                    for (int x = 80; x < 256; x++)
                    {
                        int pixel = y * FrontendFrame.Width + x;
                        if (bg1[pixel] == _floorBefore[pixel]) same++;
                        total++;
                    }
                    Console.WriteLine($"Live spike-strip pixels unchanged after death: {same}/{total} (diagnostic, not expected cartridge behavior).");
                }
            }
            Console.WriteLine($"Kraid floor frame={frame}: camera=({camera.XPosition},{camera.YPosition}), BG1 scroll=({ppu.Bg1HorizontalScroll},{ppu.Bg1VerticalScroll}).");
            var level = runtime.LevelData ?? throw new InvalidDataException("Floor capture requires level data.");
            using var hazards = new StreamWriter(Path.Combine(_directory, $"hazards-{frame + 1:D4}.csv"));
            hazards.WriteLine("x,y,levelword,bts");
            for (int y = 0; y < level.HeightInBlocks; y++)
            for (int x = 0; x < level.WidthInBlocks; x++)
            {
                var block = level.GetCollisionBlock(x, y);
                if (block.CollisionType is RoomCollisionType.SpikeAir or RoomCollisionType.SpikeBlock)
                {
                    hazards.WriteLine($"{x},{y},{block.LevelWord:X4},{block.Behavior:X2}");
                    if (x == 5 && y == 27)
                        Console.WriteLine($"Spike block expands to {LevelBlockTilemapExpander.Expand(block.LevelWord, level.BlockDefinitions.Span)}");
                }
            }
        }
    }

    public void Dispose() => _trace.Dispose();

    /// <summary>
    /// Checks the live runtime's capture against original-CPU arm AI/interpreter output.
    /// Body/camera inputs and list activation frames are fixed diagnostic stimuli;
    /// this proves the arm state sequence, not native whole-encounter timing or pixels.
    /// </summary>
    public static void VerifyNativeArmTrace(string directory, string nativeCsv)
    {
        byte[] bytes = File.ReadAllBytes(nativeCsv);
        if (Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(bytes)) !=
            KraidAuditDefinitions.NativeArmTraceSha256)
            throw new InvalidDataException("Unrecognized original-CPU Kraid arm trace.");
        string[] expected = File.ReadAllLines(nativeCsv).Skip(1).ToArray();
        if (expected.Length != KraidAuditDefinitions.NativeArmDeathFrames)
            throw new InvalidDataException("Original-CPU Kraid arm trace is incomplete.");
        var actual = File.ReadAllLines(Path.Combine(directory, "arm.csv")).Skip(1)
            .ToDictionary(line => int.Parse(line.AsSpan(0, line.IndexOf(','))));
        for (int frame = 0; frame < expected.Length; frame++)
            if (!actual.TryGetValue(frame, out string? row) || row != expected[frame])
                throw new InvalidDataException($"Kraid arm differs from original CPU at frame {frame}: " +
                    $"expected '{expected[frame]}', actual '{row}'.");
        Console.WriteLine($"Kraid arm matches original CPU across {expected.Length} live death frames.");
    }
}
