using System.Collections.Immutable;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Runtime;
using SuperMetroid.Core.Frontend.Ending;
using SuperMetroid.Core.Frontend.FileSelect;
using SuperMetroid.Core.Frontend.GameOver;
using SuperMetroid.Core.Frontend.Intro;
using SuperMetroid.Core.Frontend.Options;
using SuperMetroid.Core.Frontend.Pause;
using SuperMetroid.Core.Frontend.Title;
using SuperMetroid.Core.Frontend.CeresCinematics;

namespace SuperMetroid.Core.Frontend;

/// <summary>
/// The frontend's published display: one immutable render snapshot per frame, under the host's identity while it
/// captures and the display's own otherwise. Each scene publishes through here once per frame; brightness writes
/// append passes to the snapshot, and pixels are rendered from it on demand.
/// </summary>
internal sealed class FrontendDisplay
{
    /// <summary>How gameplay updates become the published display.</summary>
    private readonly IGameplayFramePublication gameplayFrames;
    /// <summary>The host's identity for the frame being captured; null outside a host capture.</summary>
    private RenderFrameIdentity? captureIdentity;
    /// <summary>Sequence of frames published outside a host capture.</summary>
    private long uncapturedSequence;
    /// <summary>The published frame; black until a scene publishes.</summary>
    private DisplayPacket capturedDisplay;

    /// <summary>Starts with a black frame.</summary>
    /// <param name="gameplayFrames">How gameplay updates become the published display.</param>
    internal FrontendDisplay(IGameplayFramePublication gameplayFrames)
    {
        this.gameplayFrames = Ensure.NotNull(gameplayFrames);
        capturedDisplay = new(NextIdentity(), Black);
    }

    /// <summary>The solid color a blank frame shows.</summary>
    private static Rgba32 Black => new(0, 0, 0, 255);

    /// <summary>The identity to publish under: the host's while it captures, otherwise the next own sequence number.</summary>
    private RenderFrameIdentity NextIdentity() => captureIdentity ?? new(++uncapturedSequence, 1, 0);

    /// <summary>Republishes the last frame under a new identity without recapturing.</summary>
    /// <param name="identity">The new identity.</param>
    internal DisplayPacket Retained(RenderFrameIdentity identity)
    {
        if (captureIdentity is not null) throw new InvalidOperationException("Cannot republish during display capture.");
        return Reframe(capturedDisplay, identity, capturedDisplay.Fades);
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
    internal DisplayPacket CompleteCapture(RenderFrameIdentity identity)
    {
        using var publicationTiming = RenderPublicationProfile.Measure();
        capturedDisplay = Reframe(capturedDisplay, identity, capturedDisplay.Fades);
        return capturedDisplay;
    }

    /// <summary>Ends a capture, whether or not the frame completed.</summary>
    internal void EndCapture() => captureIdentity = null;

    /// <summary>Publishes the title scene as a packet during capture or as legacy pixels otherwise.</summary>
    internal void PublishMenu(TitleSequenceState scene)
    {
        using var publicationTiming = RenderPublicationProfile.Measure();
        capturedDisplay = new(NextIdentity(), scene.CaptureRenderSnapshot());
    }

    /// <summary>Publishes the file-select menu using its current render snapshot when capturing.</summary>
    internal void PublishMenu(FileSelectMenuState scene)
    {
        using var publicationTiming = RenderPublicationProfile.Measure();
        capturedDisplay = new(NextIdentity(), scene.CaptureRenderSnapshot());
    }

    /// <summary>Publishes the game options menu using its current render snapshot when capturing.</summary>
    internal void PublishMenu(GameOptionsMenuState scene)
    {
        using var publicationTiming = RenderPublicationProfile.Measure();
        capturedDisplay = new(NextIdentity(), scene.CaptureRenderSnapshot());
    }

    /// <summary>Publishes the paused room and menu composition without advancing the scene.</summary>
    internal void PublishMenu(PauseMenuState scene)
    {
        using var publicationTiming = RenderPublicationProfile.Measure();
        capturedDisplay = new(NextIdentity(), scene.CaptureRenderSnapshot());
    }

    /// <summary>Publishes the current game-over menu composition.</summary>
    internal void PublishMenu(GameOverMenuState scene)
    {
        using var publicationTiming = RenderPublicationProfile.Measure();
        capturedDisplay = new(NextIdentity(), scene.CaptureRenderSnapshot());
    }

    /// <summary>Publishes the Ceres destruction cinematic's captured scene or software raster.</summary>
    internal void PublishCeresDestruction(CeresDestructionCinematicState scene)
    {
        using var publicationTiming = RenderPublicationProfile.Measure();
        capturedDisplay = new(NextIdentity(), scene.CaptureRenderSnapshot());
    }

    /// <summary>Publishes the ending credits presentation owned by the active scene.</summary>
    internal void PublishEnding(EndingCreditsState scene)
    {
        using var publicationTiming = RenderPublicationProfile.Measure();
        capturedDisplay = new(NextIdentity(), EndingCreditsPresentation.CaptureRenderSnapshot(scene));
    }

    /// <summary>Publishes the file-select map after its selection and animation state updates.</summary>
    internal void PublishFileMap(FileSelectMapMenuState scene)
    {
        using var publicationTiming = RenderPublicationProfile.Measure();
        capturedDisplay = new(NextIdentity(), scene.CaptureRenderSnapshot());
    }

    /// <summary>Requires a translated intro packet during capture and otherwise renders the intro raster.</summary>
    internal void PublishIntro(IntroCinematicState scene)
    {
        using var publicationTiming = RenderPublicationProfile.Measure();
        capturedDisplay = new(NextIdentity(), scene.CaptureTranslatedRenderSnapshot()
            ?? throw new InvalidOperationException("Intro publication omitted its render snapshot."));
    }

    /// <summary>Publishes a gameplay update through the session's gameplay-frame policy.</summary>
    internal void PublishGameplay(SuperMetroidRuntime activeRuntime)
    {
        using var publicationTiming = RenderPublicationProfile.Measure();
        capturedDisplay = gameplayFrames.Publish(activeRuntime, capturedDisplay, NextIdentity);
    }

    /// <summary>Replaces the published scene with opaque black under the current capture identity.</summary>
    internal void PublishBlack() => capturedDisplay = new(NextIdentity(), Black);

    /// <summary>Applies one write of the brightness register $51 to the published display.</summary>
    internal void ApplyBrightness(byte brightness)
    {
        using var publicationTiming = RenderPublicationProfile.Measure();
        DisplayPacket frame = capturedDisplay;
        capturedDisplay = Reframe(frame, frame.Identity, frame.Fades.Add(new FadeLevel(brightness)));
    }

    /// <summary>Reuses captured composition with a new host identity and brightness pass history.</summary>
    private static DisplayPacket Reframe(DisplayPacket frame, RenderFrameIdentity identity,
        ImmutableArray<FadeLevel> passes)
    {
        if (frame.Layers is { } layers) return new(identity, layers, passes);
        if (frame.Mode7 is { } mode7) return new(identity, mode7, passes);
        if (frame.SolidColor is { } color) return new(identity, color, passes);
        throw new InvalidDataException("Captured display has no composition.");
    }

}
