using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Death;
using SuperMetroid.Core.Gameplay;
using SuperMetroid.Core.World;

namespace SuperMetroid.Core.Runtime;

/// <summary>
/// Samus's death sequence: the post-music-wait start, the fatal frame, and each death-sequence frame.
/// </summary>
internal sealed partial class DeathSequenceRunner
{
    /// <summary>Supplies the active Samus turning-floor transform during death drawing.</summary>
    private readonly GameplayDisplayState _display;
    /// <summary>Processes one frame boundary before each isolated death-state frame.</summary>
    private readonly AcceptedFrameBoundary _frameBoundary;
    /// <summary>Sprite frame begun and finalized around each drawn frame.</summary>
    private readonly SpriteFrameBuffer _sprites;
    /// <summary>Owns Samus and records the last death-sequence step result.</summary>
    private readonly SamusFrameState _player;

    /// <summary>Binds the display, frame boundary, sprites, Samus and room the death sequence runs with.</summary>
    internal DeathSequenceRunner(DeathSequenceRunnerDependencies dependencies)
    {
        _display = dependencies.Display;
        _frameBoundary = dependencies.FrameBoundary;
        _sprites = dependencies.Sprites;
        _player = dependencies.Player;
    }

    /// <summary>
    /// Enters the death animation once the death game state has waited for its music. Detecting the fatal damage,
    /// the blackout and waiting for the music are the death game state's.
    /// </summary>
    public void BeginDeathSequenceAfterMusicWait(RoomScope room, SamusState samus)
    {
        if (!_player.GroundedSamusMovementEnabled)
            throw new InvalidOperationException("Death sequence requires Samus's gameplay movement.");

        samus.DeathSequence.Begin(
            samus,
            room.Camera.PixelPosition);
    }

    /// <summary>
    /// Runs one fatal-display frame of the death sequence without advancing enemies, room objects, scrolling,
    /// room main, or the gameplay clock. It draws Samus alone while she is neither animating nor dying,
    /// not a shortened main-gameplay update.
    /// </summary>
    public void DrawFatalSamusFrame(ControllerButton controllerInput, RoomScope room, SamusState samus)
    {
        _frameBoundary.BeginAcceptedFrame(controllerInput);
        _sprites.BeginFrame();
        samus.Presentation.DrawWithoutFlicker(samus,
            _sprites,
            room.Camera.PixelPosition,
            _display.ActiveTurningFloor);
        _sprites.FinalizeFrame();
    }

    /// <summary>
    /// Runs one isolated death-animation frame for the death-animation game states.
    /// The selected pose or explosion frame becomes visible through the ordinary frame
    /// boundary, while every gameplay actor remains stopped.
    /// </summary>
    public SamusDeathSequenceStepResult StepDeathSequenceFrame(ControllerButton controllerInput, RoomScope room, SamusState samus)
    {
        if (!samus.DeathSequence.IsActive)
            throw new InvalidOperationException("The death sequence is not active.");

        _frameBoundary.BeginAcceptedFrame(controllerInput);
        _sprites.BeginFrame();
        SamusDeathSequenceStepResult step = samus.DeathSequence.Step(
            samus,
            _player.Colours,
            samus.Artwork.Body.DeathSteps);
        samus.DeathSequence.Draw(step.Draws, samus, _sprites, room.Camera.PixelPosition, _display.ActiveTurningFloor);
        _sprites.FinalizeFrame();
        return step;
    }
}
