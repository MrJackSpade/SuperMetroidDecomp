using Engine.Input;
using Engine.Ui;
using Engine.World;

namespace Engine.Core;

/// <summary>
/// The change from one room to the next. Play is kept beneath it and does not run. Halfway through, the room being
/// left is taken down and the next is set up with the player arriving at the exit the old one led to; no update of
/// either room runs until the change is over.
/// </summary>
public partial class RoomTransitionMode : GameMode
{
    private readonly PlayingMode _playing;
    private readonly string _toRoom;
    private readonly string _toExit;
    private readonly TransitionStyle _style;
    private ScreenFade? _fade;
    private int _elapsed;
    private bool _changed;

    /// <summary>The change of rooms that follows the player entering <paramref name="exit"/> during <paramref name="playing"/>.</summary>
    public RoomTransitionMode(PlayingMode playing, RoomExit exit)
    {
        ArgumentNullException.ThrowIfNull(playing);
        ArgumentNullException.ThrowIfNull(exit);
        if (exit.LeadsToRoom.Length == 0 || exit.LeadsToExit.Length == 0)
            throw new InvalidOperationException($"The exit \"{exit.Name}\" does not say which room and exit it leads to.");
        _playing = playing;
        _toRoom = exit.LeadsToRoom;
        _toExit = exit.LeadsToExit;
        _style = exit.Style;
    }

    /// <inheritdoc/>
    public override GameMode? Keeps => _playing;

    /// <summary>How the change looks.</summary>
    public TransitionStyle Style => _style;

    /// <inheritdoc/>
    protected override void Enter()
    {
        _fade = new ScreenFade { Name = "Fade" };
        AddChild(_fade);
        _fade.FadeOut(Math.Max(1, Session.Definition.TransitionUpdates / 2));
    }

    /// <inheritdoc/>
    public override GameMode Step(GameInput input)
    {
        int length = Math.Max(2, Session.Definition.TransitionUpdates);
        _fade?.Step();
        _elapsed++;
        if (!_changed && _elapsed >= length / 2)
        {
            _changed = true;
            var entry = new RoomEntry(_toExit, Session.Player.Velocity, Session.Player.Facing);
            _playing.ChangeRoom(_toRoom, entry);
            _fade?.FadeIn(length - length / 2);
        }

        return _elapsed >= length ? _playing : this;
    }
}
