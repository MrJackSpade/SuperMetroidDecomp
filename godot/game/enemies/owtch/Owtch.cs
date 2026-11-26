using Engine.Actors;
using Engine.Collision;
using Engine.Content;
using Engine.Shared;
using Game.Data;
using Godot;

namespace Game.Enemies;

/// <summary>
/// An Owtch: a spiny shellfish that crawls back and forth along the sand, and now and then, by chance, burrows under
/// it for a while and comes up again heading either way. Its shell turns shots and bombs away except while it walks
/// left.
/// </summary>
/// <remarks>
/// Its timelines are "moving_left" and "moving_right", each of which starts with the cue "move:left" or "move:right"
/// that sets it walking that way.
/// </remarks>
public partial class Owtch : Enemy
{
    /// <summary>What an Owtch is doing.</summary>
    public enum Doing
    {
        /// <summary>Walking left.</summary>
        MovingLeft,

        /// <summary>Walking right.</summary>
        MovingRight,

        /// <summary>Waiting under the sand.</summary>
        Underground,

        /// <summary>Burrowing down.</summary>
        Sinking,

        /// <summary>Coming up.</summary>
        Rising,

        /// <summary>Turning round at the left end of its walk.</summary>
        TurningAtLeftEnd,
    }

    /// <summary>What it is doing when the room is entered: walking left or right, sinking, or underground.</summary>
    [Export]
    public Doing StartsBy { get; set; }

    /// <summary>How many updates it stays under the sand.</summary>
    [Export]
    public int UndergroundWait { get; set; } = 64;

    /// <summary>How far it walks each update, as a number of pixels.</summary>
    [Export]
    public double Speed { get; set; } = 0.5;

    /// <summary>How far to either side of where it is placed it walks, in pixels.</summary>
    [Export]
    public int TravelDistance { get; set; } = 32;

    /// <summary>How deep it burrows, in pixels, a pixel an update.</summary>
    [Export, DataValue("burial_depth")]
    public int BurialDepth { get; set; } = 16;

    /// <summary>Out of 256, how likely it is on any update of walking to start burrowing.</summary>
    [Export, DataValue("sink_chance")]
    public int SinkChance { get; set; } = 6;

    private Doing _doing;
    private Countdown _underground;
    private Countdown _burrow;
    private int _leftEnd;
    private int _rightEnd;

    /// <inheritdoc/>
    protected override void OnEnterRoom()
    {
        _doing = StartsBy;
        _underground = Countdown.Of(UndergroundWait);
        _leftEnd = Position.X.Whole - TravelDistance;
        _rightEnd = Position.X.Whole + TravelDistance;
        Play(StartsBy switch
        {
            Doing.MovingLeft or Doing.Underground => "moving_left",
            Doing.MovingRight or Doing.Sinking => "moving_right",
            _ => throw new InvalidOperationException($"The Owtch \"{Name}\" cannot start by {StartsBy}."),
        });
        if (_doing == Doing.Underground)
            Position += PixelOffset.FromWhole(0, BurialDepth);
        else if (_doing == Doing.Sinking)
            _burrow = Countdown.Of(BurialDepth);
    }

    /// <inheritdoc/>
    protected override void OnCue(string cue)
    {
        if (cue == "move:left")
            _doing = Doing.MovingLeft;
        else if (cue == "move:right")
            _doing = Doing.MovingRight;
    }

    /// <summary>
    /// While it does anything but walk left or turn at the left end of its walk, its shell takes in a beam, a missile,
    /// a super missile or a bomb with no harm and nothing shown, though a plasma beam flies on through it. A power bomb
    /// or Samus's body meets no shell, and is answered as its reactions say whatever it is doing.
    /// </summary>
    protected override Reaction ReactionTo(Hit hit, Collider part)
    {
        Reaction reaction = base.ReactionTo(hit, part);
        if (_doing is Doing.MovingLeft or Doing.TurningAtLeftEnd)
            return reaction;
        if (hit.Carries(new HitKind(HitKinds.Bomb)))
            return reaction with { Swallows = true };
        if (!hit.CarriesAny([new HitKind(HitKinds.Beam), new HitKind(HitKinds.Missile), new HitKind(HitKinds.SuperMissile)]))
            return reaction;
        return hit.Carries(new HitKind(HitKinds.Plasma)) ? reaction with { Passes = true } : reaction with { Swallows = true };
    }

    /// <inheritdoc/>
    protected override void Behave()
    {
        Pixels speed = Pixels.FromNumber(Speed);
        switch (_doing)
        {
            case Doing.MovingLeft:
                Position -= new PixelOffset(speed, Pixels.Zero);
                if (Position.X.Whole < _leftEnd)
                    _doing = Doing.TurningAtLeftEnd;
                MaybeSink();
                return;

            case Doing.MovingRight:
                Position += new PixelOffset(speed, Pixels.Zero);
                // At the right end it heads back without turning its picture round.
                if (Position.X.Whole >= _rightEnd)
                    _doing = Doing.MovingLeft;
                MaybeSink();
                return;

            case Doing.Underground:
                if (_underground.IsOver)
                    return;
                _underground = _underground.Stepped();
                if (_underground.IsRunning)
                    return;
                _doing = Doing.Rising;
                _burrow = Countdown.Of(BurialDepth);
                return;

            case Doing.Sinking:
                Position += PixelOffset.FromWhole(0, 1);
                _burrow = _burrow.Stepped();
                if (_burrow.IsRunning)
                    return;
                _doing = Doing.Underground;
                _underground = Countdown.Of(UndergroundWait);
                return;

            case Doing.Rising:
                Position -= PixelOffset.FromWhole(0, 1);
                _burrow = _burrow.Stepped();
                if (_burrow.IsRunning)
                    return;
                // Which way it comes up heading is read from the number last drawn.
                _doing = Random.Last.Part(8, 1) == 1 ? Doing.MovingRight : Doing.MovingLeft;
                return;

            case Doing.TurningAtLeftEnd:
                Play("moving_right", restart: true);
                return;

            default:
                throw new InvalidOperationException($"Undefined Owtch stage {_doing}.");
        }
    }

    /// <summary>By chance, a fresh number each update of walking, starts it burrowing.</summary>
    private void MaybeSink()
    {
        if (((Random.Draw().Part(0, 8) + Updates) & 0xFF) >= SinkChance)
            return;
        _doing = Doing.Sinking;
        _burrow = Countdown.Of(BurialDepth);
    }
}
