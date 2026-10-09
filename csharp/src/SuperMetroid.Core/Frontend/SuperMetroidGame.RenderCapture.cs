using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Runtime;

namespace SuperMetroid.Core.Frontend;

public sealed partial class SuperMetroidGame
{
    /// <summary>Host identity assigned while an extracted scene is being captured.</summary>
    private RenderFrameIdentity? captureIdentity;
    /// <summary>Most recently captured immutable display, retained for paused publication.</summary>
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

    /// <summary>Publishes title-sequence output as a snapshot during capture or as legacy pixels otherwise.</summary>
    /// <param name="scene">Title scene whose current display is published.</param>
    private void PublishMenu(TitleSequenceState scene)
    {
        using var publicationTiming = RenderPublicationProfile.Measure();
        if (captureIdentity is { } identity) capturedDisplay = new(identity, scene.CaptureRenderSnapshot());
        else lastPixels = scene.Render();
    }

    /// <summary>Publishes file-selection output as a snapshot during capture or as legacy pixels otherwise.</summary>
    /// <param name="scene">File-selection menu whose current display is published.</param>
    private void PublishMenu(FileSelectMenuState scene)
    {
        using var publicationTiming = RenderPublicationProfile.Measure();
        if (captureIdentity is { } identity) capturedDisplay = new(identity, scene.CaptureRenderSnapshot());
        else lastPixels = scene.Render();
    }

    /// <summary>Publishes options-menu output as a snapshot during capture or as legacy pixels otherwise.</summary>
    /// <param name="scene">Options menu whose current display is published.</param>
    private void PublishMenu(GameOptionsMenuState scene)
    {
        using var publicationTiming = RenderPublicationProfile.Measure();
        if (captureIdentity is { } identity) capturedDisplay = new(identity, scene.CaptureRenderSnapshot());
        else lastPixels = scene.Render();
    }

    /// <summary>Publishes pause-menu output as a snapshot during capture or as legacy pixels otherwise.</summary>
    /// <param name="scene">Pause menu whose current display is published.</param>
    private void PublishMenu(PauseMenuState scene)
    {
        using var publicationTiming = RenderPublicationProfile.Measure();
        if (captureIdentity is { } identity) capturedDisplay = new(identity, scene.CaptureRenderSnapshot());
        else lastPixels = scene.Render();
    }

    /// <summary>Applies one master-brightness step to the retained snapshot or legacy raster.</summary>
    /// <param name="brightness">Brightness-register value appended to snapshot passes or applied to pixels.</param>
    private void ApplyDisplayBrightness(byte brightness)
    {
        // Every published level is a write of the brightness register $51; pause entry
        // fades from the last one.
        pauseBrightness = brightness;
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

    /// <summary>Publishes game-over output as a snapshot during capture or as legacy pixels otherwise.</summary>
    /// <param name="scene">Game-over menu whose current display is published.</param>
    private void PublishMenu(GameOverMenuState scene)
    {
        using var publicationTiming = RenderPublicationProfile.Measure();
        if (captureIdentity is { } identity) capturedDisplay = new(identity, scene.CaptureRenderSnapshot());
        else lastPixels = scene.Render();
    }

    /// <summary>Publishes Ceres-destruction output as a snapshot during capture or as legacy pixels otherwise.</summary>
    /// <param name="scene">Cinematic whose current display is published.</param>
    private void PublishCeresDestruction(CeresDestructionCinematicState scene)
    {
        using var publicationTiming = RenderPublicationProfile.Measure();
        if (captureIdentity is { } identity) capturedDisplay = new(identity, scene.CaptureRenderSnapshot());
        else lastPixels = scene.Render();
    }

    /// <summary>Publishes ending-credit output as a snapshot during capture or as legacy pixels otherwise.</summary>
    /// <param name="scene">Ending-credit state whose current display is published.</param>
    private void PublishEnding(EndingCreditsState scene)
    {
        using var publicationTiming = RenderPublicationProfile.Measure();
        if (captureIdentity is { } identity) capturedDisplay = new(identity, scene.CaptureRenderSnapshot());
        else lastPixels = scene.Render();
    }

    /// <summary>Publishes file-map output as a snapshot during capture or as legacy pixels otherwise.</summary>
    /// <param name="scene">File-map menu whose current display is published.</param>
    private void PublishFileMap(FileSelectMapMenuState scene)
    {
        using var publicationTiming = RenderPublicationProfile.Measure();
        if (captureIdentity is { } identity) capturedDisplay = new(identity, scene.CaptureRenderSnapshot());
        else lastPixels = scene.Render();
    }

    /// <summary>Publishes translated intro output as a snapshot during capture or as legacy pixels otherwise.</summary>
    /// <param name="scene">Intro cinematic whose current display is published.</param>
    private void PublishIntro(IntroCinematicState scene)
    {
        using var publicationTiming = RenderPublicationProfile.Measure();
        if (captureIdentity is { } identity)
            capturedDisplay = new(identity, scene.CaptureTranslatedRenderSnapshot()
                ?? throw new InvalidOperationException("Intro publication omitted its render snapshot."));
        else lastPixels = scene.Render();
    }

    /// <summary>Publishes gameplay output unless diagnostic no-render mode retains the prior display.</summary>
    /// <param name="activeRuntime">Runtime whose current extracted display is captured or rasterized.</param>
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

    /// <summary>Publishes opaque black for a state with no scene output.</summary>
    private void PublishBlack()
    {
        if (captureIdentity is { } identity) capturedDisplay = new(identity, new Rgba32(0, 0, 0, 255));
        else lastPixels = CreateBlackFrame();
    }

    /// <summary>Creates a snapshot with a new host identity while preserving its composition and brightness passes.</summary>
    /// <param name="frame">Previously captured composition to retain.</param>
    /// <param name="identity">Host publication identity assigned to the retained display.</param>
    /// <param name="passes">Brightness-register writes to apply when the snapshot is composed.</param>
    /// <returns>A snapshot containing the original display content under the supplied identity.</returns>
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
/// <param name="Frame">Legacy frontend frame, including raster pixels when the state has no extracted snapshot.</param>
/// <param name="Snapshot">GPU-ready immutable composition for scenes supported by the capture path; null for legacy-only output.</param>
public readonly record struct CapturedFrontendFrame(FrontendFrame Frame, RenderFrameSnapshot? Snapshot)
{
}
