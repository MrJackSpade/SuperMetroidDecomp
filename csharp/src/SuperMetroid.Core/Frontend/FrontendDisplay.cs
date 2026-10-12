using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Runtime;

namespace SuperMetroid.Core.Frontend;

/// <summary>
/// The frontend's published display: an immutable captured snapshot while a host captures frames, otherwise the
/// legacy software raster. Each scene publishes through here once per frame; brightness writes append passes to
/// the captured snapshot or filter the raster.
/// </summary>
internal sealed class FrontendDisplay
{
    /// <summary>Stores render gameplay frames.</summary>
    private readonly bool renderGameplayFrames;
    /// <summary>Stores capture identity.</summary>
    private RenderFrameIdentity? captureIdentity;
    /// <summary>Stores captured display.</summary>
    private RenderFrameSnapshot? capturedDisplay;

    // Software raster of a captured display, reused by every read of the frame below.
    /// <summary>Stores captured display raster.</summary>
    [NonSerialized] private Rgba32[]? capturedDisplayRaster;

    /// <param name="renderGameplayFrames">False for the diagnostic no-render mode, which keeps the prior display through gameplay.</param>
    internal FrontendDisplay(bool renderGameplayFrames) => this.renderGameplayFrames = renderGameplayFrames;

    /// <summary>Gets or sets pixels.</summary>
    private Rgba32[] Pixels
    {
        get => capturedDisplay is null ? field : SoftwareFrameSnapshotRenderer.Render(capturedDisplay,
            capturedDisplayRaster ??= new Rgba32[FrontendFrame.Width * FrontendFrame.Height]);
        set { field = value; capturedDisplay = null; }
    } = CreateBlackFrame();

    /// <summary>
    /// The current image for a status read: empty when a display capture already owns it, so the host presents
    /// that capture instead.
    /// </summary>
    internal Rgba32[] CurrentPixels => captureIdentity is not null && capturedDisplay is not null ? Array.Empty<Rgba32>() : Pixels;

    /// <summary>The retained display under a fresh host identity, or null when only legacy pixels exist.</summary>
    internal RenderFrameSnapshot? Retained(RenderFrameIdentity identity)
    {
        if (captureIdentity is not null) throw new InvalidOperationException("Cannot republish during display capture.");
        return capturedDisplay is { } display ? Reframe(display, identity, display.BrightnessPasses) : null;
    }

    /// <summary>Starts capturing the frame published under <paramref name="identity"/>.</summary>
    internal void BeginCapture(RenderFrameIdentity identity)
    {
        if (captureIdentity is not null) throw new InvalidOperationException("Reentrant display capture.");
        captureIdentity = identity;
    }

    /// <summary>
    /// The captured display for this publication. A dispatcher state may deliberately retain its prior display;
    /// that immutable content takes this publication's identity without recapturing a scene that has already
    /// advanced into the next state.
    /// </summary>
    internal RenderFrameSnapshot? CompleteCapture(RenderFrameIdentity identity)
    {
        if (capturedDisplay is not null)
        {
            using var publicationTiming = RenderPublicationProfile.Measure();
            capturedDisplay = Reframe(capturedDisplay, identity, capturedDisplay.BrightnessPasses);
        }
        return capturedDisplay;
    }

    /// <summary>Ends a capture, whether or not the frame completed.</summary>
    internal void EndCapture() => captureIdentity = null;

    /// <summary>Publishes menu.</summary>
    internal void PublishMenu(TitleSequenceState scene)
    {
        using var publicationTiming = RenderPublicationProfile.Measure();
        if (captureIdentity is { } identity) capturedDisplay = new(identity, scene.CaptureRenderSnapshot());
        else Pixels = scene.Render();
    }

    /// <summary>Publishes menu.</summary>
    internal void PublishMenu(FileSelectMenuState scene)
    {
        using var publicationTiming = RenderPublicationProfile.Measure();
        if (captureIdentity is { } identity) capturedDisplay = new(identity, scene.CaptureRenderSnapshot());
        else Pixels = scene.Render();
    }

    /// <summary>Publishes menu.</summary>
    internal void PublishMenu(GameOptionsMenuState scene)
    {
        using var publicationTiming = RenderPublicationProfile.Measure();
        if (captureIdentity is { } identity) capturedDisplay = new(identity, scene.CaptureRenderSnapshot());
        else Pixels = scene.Render();
    }

    /// <summary>Publishes menu.</summary>
    internal void PublishMenu(PauseMenuState scene)
    {
        using var publicationTiming = RenderPublicationProfile.Measure();
        if (captureIdentity is { } identity) capturedDisplay = new(identity, scene.CaptureRenderSnapshot());
        else Pixels = scene.Render();
    }

    /// <summary>Publishes menu.</summary>
    internal void PublishMenu(GameOverMenuState scene)
    {
        using var publicationTiming = RenderPublicationProfile.Measure();
        if (captureIdentity is { } identity) capturedDisplay = new(identity, scene.CaptureRenderSnapshot());
        else Pixels = scene.Render();
    }

    /// <summary>Publishes ceres destruction.</summary>
    internal void PublishCeresDestruction(CeresDestructionCinematicState scene)
    {
        using var publicationTiming = RenderPublicationProfile.Measure();
        if (captureIdentity is { } identity) capturedDisplay = new(identity, scene.CaptureRenderSnapshot());
        else Pixels = scene.Render();
    }

    /// <summary>Publishes ending.</summary>
    internal void PublishEnding(EndingCreditsState scene)
    {
        using var publicationTiming = RenderPublicationProfile.Measure();
        if (captureIdentity is { } identity) capturedDisplay = new(identity, scene.Presentation.CaptureRenderSnapshot());
        else Pixels = scene.Presentation.Render();
    }

    /// <summary>Publishes file map.</summary>
    internal void PublishFileMap(FileSelectMapMenuState scene)
    {
        using var publicationTiming = RenderPublicationProfile.Measure();
        if (captureIdentity is { } identity) capturedDisplay = new(identity, scene.CaptureRenderSnapshot());
        else Pixels = scene.Render();
    }

    /// <summary>Publishes intro.</summary>
    internal void PublishIntro(IntroCinematicState scene)
    {
        using var publicationTiming = RenderPublicationProfile.Measure();
        if (captureIdentity is { } identity)
            capturedDisplay = new(identity, scene.CaptureTranslatedRenderSnapshot()
                ?? throw new InvalidOperationException("Intro publication omitted its render snapshot."));
        else Pixels = scene.Render();
    }

    /// <summary>Publishes gameplay.</summary>
    internal void PublishGameplay(SuperMetroidRuntime activeRuntime)
    {
        using var publicationTiming = RenderPublicationProfile.Measure();
        // The diagnostic no-render mode deliberately retains the prior display. Do not
        // materialize a stored packet simply because this simulation-only caller steps.
        if (!renderGameplayFrames) return;
        if (captureIdentity is { } identity)
            capturedDisplay = new(identity, GameplayDisplayCapture.TryCaptureFrame(activeRuntime)
                ?? throw new InvalidOperationException("Gameplay publication omitted its render snapshot."));
        else Pixels = SuperMetroidRuntimeFrameRenderer.Render(activeRuntime);
    }

    /// <summary>Publishes black.</summary>
    internal void PublishBlack()
    {
        if (captureIdentity is { } identity) capturedDisplay = new(identity, new Rgba32(0, 0, 0, 255));
        else Pixels = CreateBlackFrame();
    }

    /// <summary>Applies one write of the brightness register $51 to the published display.</summary>
    internal void ApplyBrightness(byte brightness)
    {
        using var publicationTiming = RenderPublicationProfile.Measure();
        if (captureIdentity is not null && capturedDisplay is { } frame)
        {
            byte[] passes = [.. frame.BrightnessPasses, brightness];
            capturedDisplay = Reframe(frame, frame.Identity, passes);
        }
        else
        {
            Rgba32[] pixels = Pixels;
            MasterBrightnessFilter.Apply(pixels, brightness);
            Pixels = pixels;
        }
    }

    /// <summary>Reframe.</summary>
    private static RenderFrameSnapshot Reframe(RenderFrameSnapshot frame, RenderFrameIdentity identity,
        ReadOnlySpan<byte> passes)
    {
        if (frame.Layers is { } layers) return new(identity, layers, passes);
        if (frame.Mode7 is { } mode7) return new(identity, mode7, passes);
        if (frame.SolidColor is { } color) return new(identity, color, passes);
        throw new InvalidDataException("Captured display has no composition.");
    }

    /// <summary>Creates black frame.</summary>
    private static Rgba32[] CreateBlackFrame()
    {
        var pixels = new Rgba32[FrontendFrame.Width * FrontendFrame.Height];
        Array.Fill(pixels, new Rgba32(0, 0, 0, 255));
        return pixels;
    }
}
