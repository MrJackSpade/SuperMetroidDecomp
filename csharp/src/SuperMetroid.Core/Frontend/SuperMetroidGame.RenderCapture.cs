using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Runtime;

namespace SuperMetroid.Core.Frontend;

public sealed partial class SuperMetroidGame
{
    private RenderFrameIdentity? captureIdentity;
    private RenderFrameSnapshot? capturedDisplay;

    /// <summary>
    /// Republishes the retained immutable display under a fresh host identity without
    /// advancing simulation, audio, or draw-owned state. Used after restoring a captured
    /// debugger state and when attaching a presenter to an already paused game.
    /// </summary>
    /// <remarks>
    /// Returns null when this game has only produced legacy pixels. The host must handle
    /// that explicitly; this method never fabricates a GPU packet from a CPU raster or
    /// calls a scene draw routine to reconstruct an earlier display.
    /// </remarks>
    public RenderFrameSnapshot? GetRetainedDisplay(long hostSequence, long hostGeneration)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(hostSequence);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(hostGeneration);
        if (captureIdentity is not null) throw new InvalidOperationException("Cannot republish during display capture.");
        return capturedDisplay is { } display
            ? Reframe(display, new(hostSequence, hostGeneration, FrameNumber), display.BrightnessPasses)
            : null;
    }

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
            {
                using var publicationTiming = RenderPublicationProfile.Measure();
                capturedDisplay = Reframe(capturedDisplay, identity, capturedDisplay.BrightnessPasses);
            }
            return new(frame, capturedDisplay);
        }
        finally { captureIdentity = null; }
    }

    private void PublishMenu(TitleSequenceState scene)
    {
        using var publicationTiming = RenderPublicationProfile.Measure();
        if (captureIdentity is { } identity) capturedDisplay = new(identity, scene.CaptureRenderSnapshot());
        else lastPixels = scene.Render();
    }

    private void PublishMenu(FileSelectMenuState scene)
    {
        using var publicationTiming = RenderPublicationProfile.Measure();
        if (captureIdentity is { } identity) capturedDisplay = new(identity, scene.CaptureRenderSnapshot());
        else lastPixels = scene.Render();
    }

    private void PublishMenu(GameOptionsMenuState scene)
    {
        using var publicationTiming = RenderPublicationProfile.Measure();
        if (captureIdentity is { } identity) capturedDisplay = new(identity, scene.CaptureRenderSnapshot());
        else lastPixels = scene.Render();
    }

    private void PublishMenu(PauseMenuState scene)
    {
        using var publicationTiming = RenderPublicationProfile.Measure();
        if (captureIdentity is { } identity) capturedDisplay = new(identity, scene.CaptureRenderSnapshot());
        else lastPixels = scene.Render();
    }

    private void ApplyDisplayBrightness(byte brightness)
    {
        using var publicationTiming = RenderPublicationProfile.Measure();
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
        using var publicationTiming = RenderPublicationProfile.Measure();
        if (captureIdentity is { } identity) capturedDisplay = new(identity, scene.CaptureRenderSnapshot());
        else lastPixels = scene.Render();
    }

    private void PublishCeresDestruction(CeresDestructionCinematicState scene)
    {
        using var publicationTiming = RenderPublicationProfile.Measure();
        if (captureIdentity is { } identity) capturedDisplay = new(identity, scene.CaptureRenderSnapshot());
        else lastPixels = scene.Render();
    }

    private void PublishEnding(EndingCreditsState scene)
    {
        using var publicationTiming = RenderPublicationProfile.Measure();
        if (captureIdentity is { } identity) capturedDisplay = new(identity, scene.CaptureRenderSnapshot());
        else lastPixels = scene.Render();
    }

    private void PublishFileMap(FileSelectMapMenuState scene)
    {
        using var publicationTiming = RenderPublicationProfile.Measure();
        if (captureIdentity is { } identity) capturedDisplay = new(identity, scene.CaptureRenderSnapshot());
        else lastPixels = scene.Render();
    }

    private void PublishIntro(IntroCinematicState scene)
    {
        using var publicationTiming = RenderPublicationProfile.Measure();
        if (captureIdentity is { } identity)
            capturedDisplay = new(identity, scene.CaptureTranslatedRenderSnapshot()
                ?? throw new InvalidOperationException("Intro publication omitted its render snapshot."));
        else lastPixels = scene.Render();
    }

    private void PublishGameplay(SuperMetroidRuntime activeRuntime)
    {
        using var publicationTiming = RenderPublicationProfile.Measure();
        // The diagnostic no-render mode deliberately retains the prior display. Do not
        // materialize a stored packet simply because this simulation-only caller steps.
        if (!renderGameplayFrames) return;
        if (captureIdentity is { } identity)
            capturedDisplay = new(identity, GameplayDisplayCapture.TryCaptureFrame(activeRuntime)
                ?? throw new InvalidOperationException("Gameplay publication omitted its render snapshot."));
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
/// backend coverage/telemetry. The normal desktop uses captured output; legacy
/// callers can still explicitly request raster output through Step.
/// </summary>
public readonly record struct CapturedFrontendFrame(FrontendFrame Frame, RenderFrameSnapshot? Snapshot)
{
    public bool UsedLegacyRaster => Snapshot is null;
}
