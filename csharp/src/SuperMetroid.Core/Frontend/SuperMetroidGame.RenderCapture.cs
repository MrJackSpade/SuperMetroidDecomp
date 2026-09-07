using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Runtime;

namespace SuperMetroid.Core.Frontend;

public sealed partial class SuperMetroidGame
{
    private RenderFrameIdentity? captureIdentity;
    private RenderFrameSnapshot? capturedDisplay;

    /// <summary>
    /// Advances one frame, capturing extracted scenes instead of rasterizing them.
    /// Unconverted scenes are explicitly reported as legacy pixel output. The host
    /// supplies sequence/generation outside the saved game graph, so restoring game
    /// state must not rewind presentation identity. This is a staged migration API.
    /// </summary>
    public CapturedFrontendFrame StepCaptured(ushort controllerInput, long hostSequence, long hostGeneration)
    {
        if (captureIdentity is not null) throw new InvalidOperationException("Reentrant display capture.");
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(hostSequence);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(hostGeneration);
        var identity = new RenderFrameIdentity(hostSequence, hostGeneration, unchecked((ushort)(FrameNumber + 1)));
        captureIdentity = identity;
        try
        {
            FrontendFrame frame = Step(controllerInput);
            // A dispatcher state may deliberately retain its prior display. Give that
            // immutable content this host publication's identity without recapturing a
            // scene that has already advanced into the next state.
            if (capturedDisplay is not null)
                capturedDisplay = Reframe(capturedDisplay, identity, capturedDisplay.BrightnessPasses);
            return new(frame, capturedDisplay);
        }
        finally { captureIdentity = null; }
    }

    private void PublishMenu(TitleSequenceState scene)
    {
        if (captureIdentity is { } identity) capturedDisplay = new(identity, scene.CaptureRenderSnapshot());
        else lastPixels = scene.Render();
    }

    private void PublishMenu(FileSelectMenuState scene)
    {
        if (captureIdentity is { } identity) capturedDisplay = new(identity, scene.CaptureRenderSnapshot());
        else lastPixels = scene.Render();
    }

    private void PublishMenu(GameOptionsMenuState scene)
    {
        if (captureIdentity is { } identity) capturedDisplay = new(identity, scene.CaptureRenderSnapshot());
        else lastPixels = scene.Render();
    }

    private void PublishMenu(PauseMenuState scene)
    {
        if (captureIdentity is { } identity) capturedDisplay = new(identity, scene.CaptureRenderSnapshot());
        else lastPixels = scene.Render();
    }

    private void ApplyDisplayBrightness(byte brightness)
    {
        if (captureIdentity is not null && capturedDisplay is { } frame)
        {
            byte[] passes = [.. frame.BrightnessPasses, brightness];
            capturedDisplay = Reframe(frame, frame.Identity, passes);
        }
        else
        {
            Rgba32[] pixels = lastPixels;
            MasterBrightnessFilter.Apply(pixels, brightness);
            lastPixels = pixels;
        }
    }

    private void PublishMenu(GameOverMenuState scene)
    {
        if (captureIdentity is { } identity) capturedDisplay = new(identity, scene.CaptureRenderSnapshot());
        else lastPixels = scene.Render();
    }

    private void PublishCeresDestruction(CeresDestructionCinematicState scene)
    {
        if (captureIdentity is { } identity) capturedDisplay = new(identity, scene.CaptureRenderSnapshot());
        else lastPixels = scene.Render();
    }

    private void PublishIntro(IntroCinematicState scene)
    {
        if (captureIdentity is { } identity && scene.CaptureTranslatedRenderSnapshot() is { } snapshot)
            capturedDisplay = new(identity, snapshot);
        else lastPixels = scene.Render();
    }

    private void PublishGameplay(SuperMetroidRuntime activeRuntime)
    {
        // The diagnostic no-render mode deliberately retains the prior display. Do not
        // materialize a stored packet simply because this simulation-only caller steps.
        if (!renderGameplayFrames) return;
        if (captureIdentity is { } identity && GameplayDisplayCapture.TryCaptureFrame(activeRuntime) is { } packet)
            capturedDisplay = new(identity, packet);
        else lastPixels = SuperMetroidRuntimeFrameRenderer.Render(activeRuntime);
    }

    private void PublishBlack()
    {
        if (captureIdentity is { } identity) capturedDisplay = new(identity, new Rgba32(0, 0, 0, 255));
        else lastPixels = CreateBlackFrame();
    }

    private static RenderFrameSnapshot Reframe(RenderFrameSnapshot frame, RenderFrameIdentity identity,
        ReadOnlySpan<byte> passes)
    {
        if (frame.Layers is { } layers) return new(identity, layers, passes);
        if (frame.Mode7 is { } mode7) return new(identity, mode7, passes);
        if (frame.SolidColor is { } color) return new(identity, color, passes);
        throw new InvalidDataException("Captured display has no composition.");
    }
}

/// <summary>
/// Explicit staged output: Snapshot is GPU-ready data for extracted scenes; otherwise
/// Frame.Pixels contains legacy raster output. This distinction must remain visible in
/// backend coverage/telemetry. No normal host has been switched to this API yet.
/// </summary>
public readonly record struct CapturedFrontendFrame(FrontendFrame Frame, RenderFrameSnapshot? Snapshot)
{
    public bool UsedLegacyRaster => Snapshot is null;
}
