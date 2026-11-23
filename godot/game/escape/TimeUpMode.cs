using Engine.Core;
using Engine.Input;
using Engine.Ui;
using Godot;

namespace Game;

/// <summary>
/// Time is up. The screen goes white over what was being played, then black, and what follows a death follows.
/// </summary>
public partial class TimeUpMode : GameMode
{
    private readonly PlayingMode _playing;
    private ScreenFade? _white;
    private ScreenFade? _black;

    /// <summary>Time running out on <paramref name="playing"/>.</summary>
    public TimeUpMode(PlayingMode playing)
    {
        ArgumentNullException.ThrowIfNull(playing);
        _playing = playing;
    }

    /// <summary>How many updates the screen takes to go white.</summary>
    public int WhiteOutUpdates { get; set; } = 32;

    /// <summary>How many updates it then takes to go black.</summary>
    public int BlackOutUpdates { get; set; } = 32;

    /// <inheritdoc/>
    public override GameMode? Keeps => _playing;

    /// <inheritdoc/>
    protected override void Enter()
    {
        _white = new ScreenFade { Name = "WhiteOut", Colour = Colors.White };
        AddChild(_white);
        _white.FadeOut(WhiteOutUpdates);
        Session.Sounds.RequestMusic("");
    }

    /// <inheritdoc/>
    public override GameMode Step(GameInput input)
    {
        if (_white is { IsDone: false })
        {
            _white.Step();
            return this;
        }

        if (_black is null)
        {
            _black = new ScreenFade { Name = "BlackOut", Colour = Colors.Black };
            AddChild(_black);
            _black.FadeOut(BlackOutUpdates);
            return this;
        }

        if (!_black.IsDone)
        {
            _black.Step();
            return this;
        }

        return Session.Definition.CreateModeAfterDeath();
    }
}
