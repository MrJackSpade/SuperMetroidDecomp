using Engine.Actors.Components;
using Engine.Shared;
using Engine.World;
using Godot;

namespace Game.Enemies;

/// <summary>
/// The flames of a swooper's tail (<see cref="Swooper"/>), a thing of the room in their own right as the cartridge
/// has them: placed right after their swooper, they take their turn among the enemies after it, and only within the
/// window enemies take their turns in, by where the flames themselves last stood. On a turn they draw a random
/// number nothing uses and then go to their swooper: unseen while it shows no flames, and otherwise put as far below
/// it as it says, playing the flames it says. Out of the window they stay where they were and draw nothing, though
/// their swooper flies on. Nothing hits them and they touch nothing.
/// </summary>
/// <remarks>
/// A frozen swooper takes them with it: on the turn they find it frozen they are frozen for as long as it then has
/// left, and count that time down themselves on every update, wherever they are, as the frozen do, drawing nothing.
/// The turn that finds their time up is held too. They do not find out that their swooper was frozen anew
/// meanwhile until then. They have no data of their own: their pictures are their swooper's, the timelines
/// "flames_down" and "flames_up" of its data file, drawn at the offset its own are.
/// </remarks>
public partial class SwooperFlames : RoomObject
{
    /// <summary>The swooper they are the flames of.</summary>
    [Export]
    public NodePath Leader { get; set; } = new();

    private Swooper? _swooper;
    private TimelinePlayer? _playing;
    private string _playingName = "";
    private bool _frozen;
    private Countdown _frozenLeft = Countdown.Over;

    /// <inheritdoc/>
    protected override void OnEnterRoom()
    {
        _swooper = GetNodeOrNull<Swooper>(Leader)
            ?? throw new InvalidOperationException($"The flames \"{Name}\" name no swooper they are the flames of.");

        // They are a second of their swooper's kind in the room, and as wide.
        TurnHalfWidth = _swooper.BodyHalfWidth;
        (_playing, _playingName, _frozen, _frozenLeft) = (null, "", false, Countdown.Over);
        foreach (ActorVisual visual in GetChildren().OfType<ActorVisual>())
        {
            visual.PictureOffset = _swooper.PictureOffset;
            visual.SetShown(false);
        }
    }

    /// <inheritdoc/>
    protected override void OnStep()
    {
        Swooper swooper = _swooper ?? throw new InvalidOperationException($"The flames \"{Name}\" are in no room.");
        if (_frozen)
        {
            if (_frozenLeft.IsOver)
                (_frozen, TurnsOnlyInView) = (false, true);
            else
                _frozenLeft = _frozenLeft.Stepped();
            return;
        }

        Room.Session.Random.Next();
        if (swooper.IsDead)
        {
            Room.Remove(this);
            return;
        }

        if (swooper.IsFrozen)
        {
            // The frozen take their turns wherever they are.
            (_frozen, _frozenLeft, TurnsOnlyInView) = (true, swooper.StatusLeft, false);
            Show(false);
            return;
        }

        if (swooper.FlamesShown is not { } flames)
        {
            Show(false);
            return;
        }

        if (_playing is null || _playingName != flames)
            (_playing, _playingName) = (new TimelinePlayer(swooper.Timelines.Get(flames)), flames);
        _playing.Step();
        Position = PixelPosition.FromWhole(swooper.Position.X.Whole, swooper.Position.Y.Whole + swooper.FlamesBelow);
        foreach (ActorVisual visual in GetChildren().OfType<ActorVisual>())
        {
            visual.SetShown(true);
            visual.Show(_playing.Shown);
        }
    }

    private void Show(bool shown)
    {
        foreach (ActorVisual visual in GetChildren().OfType<ActorVisual>())
            visual.SetShown(shown);
    }
}
