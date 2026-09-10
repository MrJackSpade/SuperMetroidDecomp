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
}
