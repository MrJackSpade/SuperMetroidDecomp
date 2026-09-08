using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Runtime;

internal static partial class MorphBallEyeAudit
{
    private static void CaptureReportedBlueBrinstarEye(SuperMetroidAddressSpace bus)
    {
        var runtime = new SuperMetroidRuntime(bus);
        runtime.InitializeHud(HudSnapshot.CeresDebug);
        runtime.RunNmi(0, true);
        runtime.InitializeStartingCeresRoom();
        runtime.InitializeCeresStartSamus();
        runtime.LoadCartridgeRoomForDebug(BlueBrinstarETankRoom, cameraX: 384, cameraY: 512);
        var body = runtime.Enemies.Slots[1];
        var samus = runtime.Samus!;
        samus.CollectedItems = (ushort)SamusEquipmentFlags.MorphBall;
        samus.InputLocked = true;
        samus.XPosition = (ushort)(body.XPosition - 64);
        samus.YPosition = body.YPosition;
        string directory = Path.GetFullPath("csharp/test-temp/issue-51-reported-eye");
        Directory.CreateDirectory(directory);
        bool first = false;
        for (int frame = 0; frame < 80; frame++)
        {
            runtime.StepFrame(0);
            if (!first && runtime.DisplayedMorphBallEyeBeam is { Phase: MorphBallEyeBeamPhase.Widening })
            {
                Capture("first-beam");
                first = true;
            }
        }
        if (!first || runtime.DisplayedMorphBallEyeBeam is not { Phase: MorphBallEyeBeamPhase.Full })
            throw new InvalidDataException("Reported room 01/10 eye did not activate its first and full beam.");
        foreach (int dy in new[] { -64, -32, 0, 32, 64 })
        {
            samus.YPosition = (ushort)(body.YPosition + dy);
            for (int frame = 0; frame < 4; frame++) runtime.StepFrame(0);
            Capture($"tracking-{dy}");
        }

        void Capture(string name)
        {
            var ppu = runtime.DisplayedGameplayPpu;
            var beam = runtime.DisplayedMorphBallEyeBeam
                ?? throw new InvalidDataException("Reported eye frame omitted the displayed beam.");
            var pixels = SuperMetroidRuntimeFrameRenderer.Render(runtime);
            PngWriter.WriteRgba(Path.Combine(directory, name + ".png"),
                FrontendFrame.Width, FrontendFrame.Height, pixels);
            Console.WriteLine($"01/10 {name}: body={body.XPosition},{body.YPosition}, " +
                $"map={body.SpritemapPointer:X4}, palette={body.PaletteIndex:X4}, camera={ppu.Layer1XPosition},{ppu.Layer1YPosition}, " +
                $"beam={beam.WorldX},{beam.WorldY} angle={beam.Angle.TableIndex} width={beam.AngularWidth}");
        }
    }
}
