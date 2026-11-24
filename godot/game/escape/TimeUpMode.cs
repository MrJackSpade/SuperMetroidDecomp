using Engine.Core;
using Engine.Input;
using Engine.Ui;
using Game.Cinematics;
using Game.Data;
using Godot;

namespace Game;

/// <summary>
/// Time is up. The screen goes white over what was being played, then black. On Zebes what follows a death follows;
/// on Ceres the station is first seen blowing up with Samus still aboard.
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
    public int WhiteOutUpdates { get; set; } = 10;

    /// <summary>How many updates it then takes to go black.</summary>
    public int BlackOutUpdates { get; set; } = 15;

    /// <inheritdoc/>
    public override GameMode? Keeps => _playing;

    /// <inheritdoc/>
    protected override void Enter()
    {
        _white = new ScreenFade { Name = "WhiteOut", Colour = Colors.White };
        AddChild(_white);
        _white.FadeOut(WhiteOutUpdates);
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

            // Once black, every library is told to stop, and no number of sounds waiting turns that away.
            if (_black.IsDone)
                Session.Sounds.StopAll(SoundPriority.Essential);
            return this;
        }

        return Session.Progress.Has(StoryEvents.EscapeStarted)
            ? Session.Definition.CreateModeAfterDeath()
            : new CeresDestruction { SamusAboard = true };
    }
}
