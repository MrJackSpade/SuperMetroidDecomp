using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Runtime;

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
