using Engine.Actors;
using Engine.Core;
using Engine.Input;
using Engine.Ui;
using Game.Cinematics;
using Game.Data;
using Godot;

namespace Game;

/// <summary>
/// Time is up. Play goes on while the screen goes white over it, then stops while the white screen goes black. On
/// Zebes what follows a death follows; on Ceres the station is first seen blowing up with Samus still aboard.
/// </summary>
/// <remarks>
/// The white-out is the countdown's own (<see cref="Escape.StepWhiteOut"/>), shown by what is shown over play, so
/// that whatever takes play from beneath this mode, a door or a refill from her reserves, leaves it where it stood,
/// and the time up that comes back goes on with it.
/// </remarks>
public partial class TimeUpMode : GameMode
{
    /// <summary>How many levels the white screen goes black in, one each update.</summary>
    private const int BlackOutLevels = 15;

    /// <summary>
    /// The terms play goes on beneath the white-out on, as the cartridge has it: Samus with no energy left does not
    /// begin to die, though her reserves may still save her; the pause is refused; and nothing of the escape's takes
    /// play again, the time up least of all.
    /// </summary>
    private static readonly PlayTerms BeneathTheWhiteOut = new() { PlayerMayDie = false, MayPause = false, ServicesMayTakeOver = false };

    private readonly PlayingMode _playing;
    private ScreenFade? _black;
    private int _dark;

    /// <summary>Time running out on <paramref name="playing"/>.</summary>
    public TimeUpMode(PlayingMode playing)
    {
        ArgumentNullException.ThrowIfNull(playing);
        _playing = playing;
    }

    /// <inheritdoc/>
    public override GameMode? Keeps => _playing;

    /// <inheritdoc/>
    public override GameMode Step(GameInput input)
    {
        Escape escape = Session.Service<Escape>();
        if (_black is null)
        {
            // A whole update of play, and then the white-out's step, which it takes whatever the update led to.
            GameMode? next = _playing.PlayBeneath(input, BeneathTheWhiteOut);
            bool whitened = escape.StepWhiteOut();
            _playing.Hud?.Refresh(Session, _playing.Room);
            if (next is not null)
                return next;

            // Her reserves saving her take the update from the time up: play is its own again, and on Zebes the
            // time up comes back after its next update.
            if (_playing.Room.EnergySettled == EmptyEnergy.Saved)
                return _playing;
            if (whitened)
            {
                _black = new ScreenFade { Name = "BlackOut", Colour = Colors.Black, Layer = 20 };
                AddChild(_black);
                _black.Cover(0, BlackOutLevels);
            }

            return this;
        }

        // The fade to black plays nothing: on each of its updates the liquid and the statues still move, and then
        // the update's random number is drawn.
        _playing.Room.OpenUpdate(movesLiquid: true, opensProjectiles: false);
        _dark++;
        _black.Cover(_dark, BlackOutLevels);
        if (_dark < BlackOutLevels)
            return this;

        // On the update it reaches black the countdown is stopped, every library is told to stop, which no number of
        // sounds waiting turns away, and what follows begins.
        escape.Stop();
        Session.Sounds.StopAll(SoundPriority.Essential);
        return Session.Progress.Has(StoryEvents.EscapeStarted)
            ? Session.Definition.CreateModeAfterDeath()
            : new CeresDestruction { SamusAboard = true };
    }
}
