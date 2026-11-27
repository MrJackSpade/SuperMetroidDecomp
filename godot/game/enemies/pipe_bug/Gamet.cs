using Engine.Actors;
using Engine.Content;
using Engine.Shared;
using Godot;

namespace Game.Enemies;

/// <summary>
/// A Gamet: a pipe bug that comes out five at a time. When Samus stands near and above their pipe all five rise to
/// her height, spread out above and below one another, and fly off at her one after another. One of the five leads:
/// it watches for Samus and sets the others off. Any that leaves the view is back in the pipe, and the five come out
/// again only once all are back.
/// </summary>
/// <remarks>Its timelines are "rising_left", "rising_right", "flying_left" and "flying_right".</remarks>
public partial class Gamet : Enemy
{
    /// <summary>The one that leads its five; empty for the leader itself.</summary>
    [Export]
    public NodePath Leader { get; set; } = new();

    /// <summary>For the leader, how near across Samus must be, in pixels, to bring the five out.</summary>
    [Export]
    public int TriggerWidth { get; set; } = 64;

    /// <summary>How far it flies each update, as a number of pixels.</summary>
    [Export]
    public double Speed { get; set; } = 2;

    /// <summary>How far below where it stops rising it spreads out to, in pixels; above when negative, and nowhere when nothing.</summary>
    [Export]
    public int SpreadDown { get; set; }

    /// <summary>How many updates after it stops rising it flies off.</summary>
    [Export]
    public int DepartureWait { get; set; } = 104;

    /// <summary>How far it rises, and spreads, each update, in pixels.</summary>
    [Export, DataValue("rise_speed")]
    public int RiseSpeed { get; set; } = 1;

    /// <summary>How many come out together, the leader with them.</summary>
    private const int FormationSize = 5;

    private enum Doing
    {
        InPipe,
        WaitingForSamus,
        Rising,
        Spreading,
        WaitingToFly,
        Flying,
    }

    private Doing _doing;
    private PixelPosition _pipe;
    private int _roseTo;
    private int _departsIn;

    /// <summary>Whether it is back in its pipe, ready to come out with the others.</summary>
    public bool IsInPipe => _doing == Doing.InPipe && !IsDead;

    /// <inheritdoc/>
    protected override void OnEnterRoom()
    {
        _pipe = PixelPosition.FromWhole(Position.X.Whole, Position.Y.Whole);
        _departsIn = DepartureWait;
        Play("rising_left");
        Visual?.SetShown(false);
    }

    /// <inheritdoc/>
    protected override void Behave()
    {
        switch (_doing)
        {
            case Doing.InPipe:
                // Only the leader moves the five on, and only once every one of them is back in the pipe: one killed
                // holds the rest back until it is there again.
                List<Gamet> five = [.. Five()];
                if (Leader.IsEmpty && five.Count == FormationSize && five.All(member => member.IsInPipe))
                    _doing = Doing.WaitingForSamus;
                break;

            case Doing.WaitingForSamus:
                if (Math.Abs(Position.X.Whole - Target.X.Whole) < TriggerWidth && Position.Y.Whole >= Target.Y.Whole)
                {
                    // One killed since they were all back has nothing to come out.
                    bool right = Target.X.Whole >= Position.X.Whole;
                    foreach (Gamet member in Five().Where(member => !member.IsDead))
                        member.ComeOut(right);
                    Tick();
                }

                break;

            case Doing.Rising:
                Visual?.SetShown(true);
                Position -= PixelOffset.FromWhole(0, RiseSpeed);
                if (Position.Y.Whole < Target.Y.Whole)
                {
                    _doing = Doing.Spreading;
                    _roseTo = Position.Y.Whole;
                    Play(Target.X.Whole >= Position.X.Whole ? "rising_right" : "rising_left", restart: true);
                }

                break;

            case Doing.Spreading:
                Tick();
                if (SpreadDown == 0)
                {
                    _doing = Doing.WaitingToFly;
                    break;
                }

                Position += PixelOffset.FromWhole(0, SpreadDown < 0 ? -RiseSpeed : RiseSpeed);
                int target = _roseTo + SpreadDown;
                if (SpreadDown < 0 ? Position.Y.Whole < target : Position.Y.Whole >= target)
                {
                    Position = new PixelPosition(Position.X, Pixels.FromWhole(target));
                    _doing = Doing.WaitingToFly;
                }

                break;

            case Doing.WaitingToFly:
                Tick();
                if (_departsIn > 0)
                    break;
                _departsIn = DepartureWait;
                _doing = Doing.Flying;
                Facing = Target.X.Whole >= Position.X.Whole ? Facing.Right : Facing.Left;
                Play(Facing == Facing.Right ? "flying_right" : "flying_left", restart: true);
                break;

            case Doing.Flying:
                Position += new PixelOffset(Facing.Toward(Pixels.FromNumber(Speed)), Pixels.Zero);
                break;

            default:
                throw new InvalidOperationException($"Undefined Gamet stage {_doing}.");
        }

        // Out of view, it is back in the pipe, keeping its parts of a pixel and how near it had come to flying off.
        if (IsInView(0))
            return;
        Visual?.SetShown(false);
        _doing = Doing.InPipe;
        Position = new PixelPosition(Pixels.FromParts(_pipe.X.Whole, Position.X.Fraction), Pixels.FromParts(_pipe.Y.Whole, Position.Y.Fraction));
    }

    /// <summary>Sets it rising out of the pipe, facing right or left.</summary>
    private void ComeOut(bool right)
    {
        Play(right ? "rising_right" : "rising_left", restart: true);
        _doing = Doing.Rising;
    }

    /// <summary>Counts one update toward flying off.</summary>
    private void Tick()
    {
        if (_departsIn > 0)
            _departsIn--;
    }

    /// <summary>The leader's five: itself and every Gamet placed beside it that names it as leader.</summary>
    private IEnumerable<Gamet> Five() =>
        GetParent().GetChildren().OfType<Gamet>().Where(member => member == this || (!member.Leader.IsEmpty && member.GetNodeOrNull(member.Leader) == this));
}
