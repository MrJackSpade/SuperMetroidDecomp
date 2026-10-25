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
internal sealed class DeathSequenceRunner
{
    /// <summary>Provides installed death palettes and explosion sprite compositions.</summary>
    private readonly RuntimeArtwork _artwork;
    /// <summary>Receives the death sequence's palette transitions.</summary>
    private readonly GameplayColours _cgram;
    /// <summary>Supplies the active Samus Mode-7 transform during death drawing.</summary>
    private readonly GameplayDisplayState _display;
    /// <summary>Processes one accepted NMI before each isolated death-state frame.</summary>
    private readonly AcceptedFrameBoundary _frameBoundary;
    /// <summary>Sprite frame begun and finalized around each drawn frame.</summary>
    private readonly SpriteFrameBuffer _sprites;
    /// <summary>Owns Samus and records the last death-sequence step result.</summary>
    private readonly SamusFrameState _player;
    /// <summary>Supplies the active layer-1 camera for death drawing.</summary>
    private readonly ActiveRoomState _room;

    /// <summary>Binds the death-state renderer and runtime services supplied by the gameplay host.</summary>
    internal DeathSequenceRunner(DeathSequenceRunnerDependencies dependencies)
    {
        _artwork = dependencies.Artwork;
        _cgram = dependencies.Cgram;
        _display = dependencies.Display;
        _frameBoundary = dependencies.FrameBoundary;
        _sprites = dependencies.Sprites;
        _player = dependencies.Player;
        _room = dependencies.Room;
    }

    /// <summary>
    /// Enters `$9B:B3A7` after the outer death game state has completed its music wait.
    /// Fatal-damage detection, blackout, and queued-music polling are deliberately not
    /// fabricated by this narrow runtime seam.
    /// </summary>
    public SamusDeathSequenceStartResult BeginDeathSequenceAfterMusicWait()
    {
        if (_player.Samus is null || _room.Camera is null || !_player.GroundedSamusMovementEnabled)
        {
            throw new InvalidOperationException(
                "Death sequence requires initialized gameplay Samus and a layer-1 camera.");
        }

        SamusDeathSequenceStartResult result = _player.Samus.DeathSequence.Begin(
            _player.Samus,
            _room.Camera.PixelPosition);

        // The next accepted NMI must see pose `$D7/$D8` definitions. Native game state 15
        // draws once before setup and state 16 draws the new pose on its following call;
        // priming here preserves that same first-visible-frame contract in this thin host.
        _player.Samus.Presentation.PrimeGraphics(_player.Samus);
        return result;
    }

    /// <summary>
    /// Runs one state-$15 fatal-display frame without advancing enemies, PLMs, scrolling,
    /// room main, or the gameplay clock. This is <c>Samus_DrawWhenNotAnimatingOrDying</c>,
    /// not a shortened state-eight call.
    /// </summary>
    public void DrawFatalSamusFrame(ControllerButton controllerInput)
    {
        if (_player.Samus is null || _room.Camera is null)
            throw new InvalidOperationException("Fatal Samus drawing requires an active room.");

        _frameBoundary.BeginAcceptedFrame(controllerInput);
        _sprites.BeginFrame();
        _player.Samus.Presentation.Draw(_player.Samus, 
            _sprites,
            _room.Camera.PixelPosition,
            mode7Transform: _display.ActiveSamusMode7Transform);
        _sprites.FinalizeFrame();
    }

    /// <summary>
    /// Runs one isolated bank-$9B death-animation frame for game states <c>$16-$18</c>.
    /// The selected pose or explosion frame becomes visible through the ordinary accepted-
    /// NMI pipeline, while every state-eight actor remains stopped as on the cartridge.
    /// </summary>
    public SamusDeathSequenceStepResult StepDeathSequenceFrame(ControllerButton controllerInput)
    {
        if (_player.Samus is null || _room.Camera is null || !_player.Samus.DeathSequence.IsActive)
            throw new InvalidOperationException("The bank-$9B death sequence is not active.");

        _frameBoundary.BeginAcceptedFrame(controllerInput);
        _sprites.BeginFrame();
        _player.LastDeathSequenceStep = _player.Samus.DeathSequence.Step(
            _player.Samus,
            _cgram,
            _player.Colours,
            _artwork.SamusBodyArt?.DeathPalettes);
        if (_player.LastDeathSequenceStep.Value.DrawPose)
        {
            _player.Samus.Presentation.Draw(_player.Samus, 
                _sprites,
                _room.Camera.PixelPosition,
                mode7Transform: _display.ActiveSamusMode7Transform);
        }
        else if (_player.LastDeathSequenceStep.Value.DrawExplosion)
        {
            _player.Samus.DeathSequence.DrawExplosion(_sprites, _player.Samus.Presentation.BodyFrames.Sheets);
        }
        _sprites.FinalizeFrame();
        return _player.LastDeathSequenceStep.Value;
    }
}
