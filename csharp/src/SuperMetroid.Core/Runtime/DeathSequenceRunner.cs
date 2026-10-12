using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Runtime;

/// <summary>
/// Samus's death sequence: the post-music-wait start, the fatal frame, and each death-sequence frame.
/// </summary>
internal sealed class DeathSequenceRunner
{
    /// <summary>Stores actor drawing.</summary>
    private readonly GameplayActorDrawing _actorDrawing;
    /// <summary>Stores artwork.</summary>
    private readonly RuntimeArtwork _artwork;
    /// <summary>Stores cgram.</summary>
    private readonly SnesCgram _cgram;
    /// <summary>Stores display.</summary>
    private readonly GameplayDisplayState _display;
    /// <summary>Stores nmi handler.</summary>
    private readonly NmiHandler _nmiHandler;
    /// <summary>Stores oam.</summary>
    private readonly OamBuffer _oam;
    /// <summary>Stores player.</summary>
    private readonly SamusFrameState _player;
    /// <summary>Stores room.</summary>
    private readonly ActiveRoomState _room;
    /// <summary>Stores vram writes.</summary>
    private readonly VramWriteQueue _vramWrites;
    /// <summary>Stores address space.</summary>
    private readonly ISnesAddressSpace _addressSpace;

    /// <summary>Initializes a new death sequence runner instance.</summary>
    internal DeathSequenceRunner(DeathSequenceRunnerDependencies dependencies)
    {
        _actorDrawing = dependencies.ActorDrawing;
        _artwork = dependencies.Artwork;
        _cgram = dependencies.Cgram;
        _display = dependencies.Display;
        _nmiHandler = dependencies.NmiHandler;
        _oam = dependencies.Oam;
        _player = dependencies.Player;
        _room = dependencies.Room;
        _vramWrites = dependencies.VramWrites;
        _addressSpace = dependencies.AddressSpace;
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
            _addressSpace,
            _player.Samus,
            _room.Camera.PixelPosition);

        // The next accepted NMI must see pose `$D7/$D8` definitions. Native game state 15
        // draws once before setup and state 16 draws the new pose on its following call;
        // priming here preserves that same first-visible-frame contract in this thin host.
        _player.Samus.Presentation.PrimeGraphics(_addressSpace);
        return result;
    }

    /// <summary>
    /// Runs one state-$15 fatal-display frame without advancing enemies, PLMs, scrolling,
    /// room main, or the gameplay clock. This is <c>Samus_DrawWhenNotAnimatingOrDying</c>,
    /// not a shortened state-eight call.
    /// </summary>
    public void DrawFatalSamusFrame(ushort controllerInput)
    {
        if (_player.Samus is null || _room.Camera is null)
            throw new InvalidOperationException("Fatal Samus drawing requires an active room.");

        _nmiHandler.RunNmi(controllerInput, mainLoopRequestedNmi: true);
        _oam.BeginFrame();
        _actorDrawing.LastSamusBodyDrawn = _player.Samus.Presentation.Draw(
            _addressSpace,
            _oam,
            _room.Camera.PixelPosition,
            mode7Transform: _display.ActiveSamusMode7Transform);
        _oam.FinalizeFrame();
    }

    /// <summary>
    /// Runs one isolated bank-$9B death-animation frame for game states <c>$16-$18</c>.
    /// The resulting OAM and queued graphics become visible through the ordinary accepted-
    /// NMI pipeline, while every state-eight actor remains stopped as on the cartridge.
    /// </summary>
    public SamusDeathSequenceStepResult StepDeathSequenceFrame(ushort controllerInput)
    {
        if (_player.Samus is null || _room.Camera is null || !_player.Samus.DeathSequence.IsActive)
            throw new InvalidOperationException("The bank-$9B death sequence is not active.");

        _nmiHandler.RunNmi(controllerInput, mainLoopRequestedNmi: true);
        _oam.BeginFrame();
        _actorDrawing.LastSamusBodyDrawn = false;
        _player.LastDeathSequenceStep = _player.Samus.DeathSequence.Step(
            _addressSpace,
            _player.Samus,
            _cgram,
            _vramWrites,
            _artwork.SamusBodyArt?.DeathPalettes);
        if (_player.LastDeathSequenceStep.Value.DrawPose)
        {
            _actorDrawing.LastSamusBodyDrawn = _player.Samus.Presentation.Draw(
                _addressSpace,
                _oam,
                _room.Camera.PixelPosition,
                mode7Transform: _display.ActiveSamusMode7Transform);
        }
        else if (_player.LastDeathSequenceStep.Value.DrawExplosion)
        {
            _player.Samus.DeathSequence.DrawExplosion(_addressSpace, _oam, _artwork.SamusBodyArt?.Spritemaps);
        }
        _oam.FinalizeFrame();
        return _player.LastDeathSequenceStep.Value;
    }
}
