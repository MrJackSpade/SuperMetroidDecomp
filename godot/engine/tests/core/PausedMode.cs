using Engine.Input;
using Engine.Core;

namespace Engine.Tests.Core;

/// <summary>
/// The game paused. Play is kept beneath it and does not run; the pause action hands control back. A game's own pause
/// screen, with its map and equipment, is a mode of the game's that does the same.
/// </summary>
public partial class PausedMode : GameMode
{
    private readonly PlayingMode _playing;

    /// <summary>A pause laid over <paramref name="playing"/>.</summary>
    public PausedMode(PlayingMode playing)
    {
        ArgumentNullException.ThrowIfNull(playing);
        _playing = playing;
    }

    /// <inheritdoc/>
    public override GameMode? Keeps => _playing;

    /// <inheritdoc/>
    public override GameMode Step(GameInput input)
    {
        if (!input.WasPressed(GameAction.Pause))
            return this;
        Session.EndPause();
        return _playing;
    }
}
