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

using SuperMetroid.Core.Assets;
namespace SuperMetroid.Core.Frontend;

/// <summary>
/// The frontend's published display: one immutable frame at a time, under the host's identity while it captures and
/// the display's own otherwise. Each scene publishes its composed frame through here once per update; the session may
/// then set the screen's brightness, which dims that frame until the next scene publishes, and pixels are drawn from
/// the published frame on demand.
/// </summary>
internal sealed class FrontendDisplay
{
    /// <summary>How gameplay updates become the published display.</summary>
    private readonly IGameplayFramePublication gameplayFrames;
    /// <summary>The host's identity for the frame being captured; null outside a host capture.</summary>
    private RenderFrameIdentity? captureIdentity;
    /// <summary>Sequence of frames published outside a host capture.</summary>
    private long uncapturedSequence;
    /// <summary>The frame as its scene composed it, at the scene's own brightness; black until a scene publishes.</summary>
    private DisplayPacket composedDisplay;
    /// <summary>The screen brightness the session set on <see cref="composedDisplay"/>, or null while it has set none.</summary>
    private int? screenBrightness;
    /// <summary>The published frame: <see cref="composedDisplay"/> shown at the brightness the session set.</summary>
    private DisplayPacket publishedDisplay;

    /// <summary>Starts with a black frame.</summary>
    /// <param name="gameplayFrames">How gameplay updates become the published display.</param>
    internal FrontendDisplay(IGameplayFramePublication gameplayFrames)
    {
        this.gameplayFrames = Ensure.NotNull(gameplayFrames);
        composedDisplay = publishedDisplay = new(NextIdentity(), Black);
    }

    /// <summary>The solid color a blank frame shows.</summary>
    private static RgbaPixel Black => new(0, 0, 0, 255);

    /// <summary>The identity to publish under: the host's while it captures, otherwise the next own sequence number.</summary>
    private RenderFrameIdentity NextIdentity() => captureIdentity ?? new(++uncapturedSequence, 1, 0);

    /// <summary>Republishes the last frame under a new identity without recapturing.</summary>
    /// <param name="identity">The new identity.</param>
    internal DisplayPacket Retained(RenderFrameIdentity identity)
    {
        if (captureIdentity is not null) throw new InvalidOperationException("Cannot republish during display capture.");
        return Shown(identity);
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
        publishedDisplay = Shown(identity);
        return publishedDisplay;
    }

    /// <summary>Ends a capture, whether or not the frame completed.</summary>
    internal void EndCapture() => captureIdentity = null;

    /// <summary>Publishes the title scene's composed frame.</summary>
    internal void PublishMenu(TitleSequenceState scene) => Compose(new(NextIdentity(), scene.CaptureRenderSnapshot()));

    /// <summary>Publishes the file-select menu's composed frame.</summary>
    internal void PublishMenu(FileSelectMenuState scene) => Compose(new(NextIdentity(), scene.CaptureRenderSnapshot()));

    /// <summary>Publishes the game options menu's composed frame.</summary>
    internal void PublishMenu(GameOptionsMenuState scene) => Compose(new(NextIdentity(), scene.CaptureRenderSnapshot()));

    /// <summary>Publishes the paused room and menu composition without advancing the scene.</summary>
    internal void PublishMenu(PauseMenuState scene) => Compose(new(NextIdentity(), scene.CaptureRenderSnapshot()));

    /// <summary>Publishes the current game-over menu composition.</summary>
    internal void PublishMenu(GameOverMenuState scene) => Compose(new(NextIdentity(), scene.CaptureRenderSnapshot()));

    /// <summary>Publishes the Ceres destruction cinematic's composed frame.</summary>
    internal void PublishCeresDestruction(CeresDestructionCinematicState scene) => Compose(new(NextIdentity(), scene.CaptureRenderSnapshot()));

    /// <summary>Publishes the ending credits presentation owned by the active scene.</summary>
    internal void PublishEnding(EndingCreditsState scene) => Compose(new(NextIdentity(), EndingCreditsPresentation.CaptureRenderSnapshot(scene)));

    /// <summary>Publishes the file-select map after its selection and animation state updates.</summary>
    internal void PublishFileMap(FileSelectMapMenuState scene) => Compose(new(NextIdentity(), scene.CaptureRenderSnapshot()));

    /// <summary>Publishes the intro cinematic's composed frame, which every intro update must provide.</summary>
    internal void PublishIntro(IntroCinematicState scene) => Compose(new(NextIdentity(), scene.CaptureTranslatedRenderSnapshot()
        ?? throw new InvalidOperationException("Intro publication omitted its render snapshot.")));

    /// <summary>
    /// Publishes a gameplay update through the session's gameplay-frame policy. A policy that keeps the composed frame
    /// keeps the brightness set on it as well.
    /// </summary>
    internal void PublishGameplay(SuperMetroidRuntime activeRuntime)
    {
        DisplayPacket next = gameplayFrames.Publish(activeRuntime, composedDisplay, NextIdentity);
        if (!ReferenceEquals(next, composedDisplay)) Compose(next);
    }

    /// <summary>Replaces the published scene with opaque black under the current capture identity.</summary>
    internal void PublishBlack() => Compose(new(NextIdentity(), Black));

    /// <summary>
    /// Sets the screen's brightness for the published frame, replacing any level set on it before. The frame is shown at
    /// its scene's own brightness combined with this one (<see cref="DisplayBrightness.Combine"/>), until the next scene
    /// publishes.
    /// </summary>
    /// <param name="brightness">The screen brightness, from black (zero) to <see cref="DisplayBrightness.Full"/>.</param>
    /// <exception cref="ArgumentOutOfRangeException">The brightness is below black or above full.</exception>
    internal void SetBrightness(int brightness)
    {
        screenBrightness = DisplayBrightness.Require(brightness);
        publishedDisplay = Shown(publishedDisplay.Identity);
    }

    /// <summary>Publishes a newly composed frame at its own brightness; a brightness set on the previous frame no longer applies.</summary>
    private void Compose(DisplayPacket composed)
    {
        composedDisplay = publishedDisplay = composed;
        screenBrightness = null;
    }

    /// <summary>The composed frame under <paramref name="identity"/>, at the brightness the session set on it.</summary>
    private DisplayPacket Shown(RenderFrameIdentity identity)
    {
        DisplayPacket frame = composedDisplay;
        int brightness = screenBrightness is { } level ? DisplayBrightness.Combine(frame.Brightness, level) : frame.Brightness;
        if (frame.Layers is { } layers)
            return new(identity, brightness == layers.Brightness ? layers : new LayeredScene(layers.Paint, layers.Layers, brightness));
        if (frame.Floor is { } floor)
            return new(identity, brightness == floor.Brightness ? floor : new FloorScene(floor.Paint, floor.Background, brightness, floor.Gradient, floor.Floor));
        if (frame.SolidColor is { } color) return new(identity, color, brightness);
        throw new InvalidDataException("Captured display has no composition.");
    }
}
