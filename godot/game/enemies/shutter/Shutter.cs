using Engine.Actors;
using Engine.Collision;
using Engine.Content;
using Engine.Shared;
using Engine.World;
using Game.Compatibility;
using Godot;

namespace Game.Enemies;

/// <summary>
/// A shutter: a solid slab that slides up or down a set way and stops, and Samus can ride. What sets it off is a
/// setting: time passing, Samus coming near, nothing at all, or being struck, once or every time. It may rest at
/// each end and go back. The shootable shutter, the Kamer and the shutter that can be destroyed are all this, with
/// their own settings and data; the one that can be destroyed has health like any enemy.
/// </summary>
/// <remarks>Its timeline is "idle".</remarks>
public partial class Shutter : Enemy
{
    /// <summary>What sets a shutter moving.</summary>
    public enum Triggers
    {
        /// <summary>A set time passing.</summary>
        Timer,

        /// <summary>Samus coming within a set distance across.</summary>
        SamusNear,

        /// <summary>Nothing: it moves at once.</summary>
        AtOnce,

        /// <summary>Being struck, the first time only.</summary>
        StruckOnce,

        /// <summary>Being struck, every time; each time it goes the other way.</summary>
        StruckEachTime,
    }

    /// <summary>What sets it moving.</summary>
    [Export]
    public Triggers Trigger { get; set; }

    /// <summary>For one on a timer, how many updates pass; for one that waits for Samus, how near she must come, in pixels across.</summary>
    [Export]
    public int TriggerAmount { get; set; }

    /// <summary>Whether it first moves up, not down.</summary>
    [Export]
    public bool MovesUpFirst { get; set; }

    /// <summary>How fast it moves, in pixels an update.</summary>
    [Export]
    public double Speed { get; set; } = 1;

    /// <summary>How far it travels, in pixels.</summary>
    [Export]
    public int Travel { get; set; } = 32;

    /// <summary>How many updates it rests after moving up before going back; negative to stay.</summary>
    [Export]
    public int RestAfterUp { get; set; } = -1;

    /// <summary>How many updates it rests after moving down before going back; negative to stay.</summary>
    [Export]
    public int RestAfterDown { get; set; } = -1;

    /// <summary>The sound of it setting off, by name.</summary>
    [Export, DataValue("activation_sound")]
    public string ActivationSound { get; set; } = "";

    private enum Doing
    {
        WaitForTimer,
        WaitForSamus,
        Activate,
        Still,
        MovingUp,
        MovingDown,
        StoppedAfterUp,
        StoppedAfterDown,
    }

    private Doing _doing;
    private int _timer;
    private int _rest;
    private int _top;
    private int _bottom;
    private bool _struckOnce;
    private bool _nextStrikeUp;

    /// <inheritdoc/>
    public override HitResult TakeHit(Hit hit, Collider part)
    {
        // One with health is hurt like any enemy; either way a strike may set it off.
        HitResult result = base.TakeHit(hit, part);
        if (!IsDead)
            React();
        return result;
    }

    /// <inheritdoc/>
    protected override void Touch(Collider part)
    {
        base.Touch(part);
        if (_doing is not (Doing.MovingUp or Doing.MovingDown) && Room.Player.BodyPowers.Count > 0)
            React();
    }

    /// <inheritdoc/>
    protected override void OnEnterRoom()
    {
        Play("idle");
        int home = Position.Y.Whole;
        (_top, _bottom) = MovesUpFirst ? (home - Travel, home) : (home, home + Travel);
        (_struckOnce, _nextStrikeUp, _timer) = (false, MovesUpFirst, TriggerAmount);
        _doing = Trigger switch
        {
            Triggers.Timer => Doing.WaitForTimer,
            Triggers.SamusNear => Doing.WaitForSamus,
            Triggers.AtOnce => Doing.Activate,
            Triggers.StruckOnce or Triggers.StruckEachTime => Doing.Still,
            _ => throw new InvalidOperationException($"A shutter has no trigger {Trigger}."),
        };
    }

    /// <inheritdoc/>
    protected override void Behave()
    {
        switch (_doing)
        {
            case Doing.WaitForTimer:
                if (--_timer > 0)
                    break;
                _timer = TriggerAmount;
                SetOff();
                break;

            case Doing.WaitForSamus:
                if (Math.Abs(Room.Player.Position.X.Whole - Position.X.Whole) < TriggerAmount)
                    SetOff();
                break;

            case Doing.Activate:
                SetOff();
                break;

            case Doing.Still:
                break;

            case Doing.MovingUp:
                Move(up: true);
                break;

            case Doing.MovingDown:
                Move(up: false);
                break;

            case Doing.StoppedAfterUp:
            case Doing.StoppedAfterDown:
                if (_rest > 0 && --_rest > 0)
                    break;
                bool wasUp = _doing == Doing.StoppedAfterUp;
                // One that waits for Samus waits again before each move in its first direction.
                _doing = Trigger == Triggers.SamusNear && MovesUpFirst != wasUp
                    ? Doing.WaitForSamus
                    : wasUp ? Doing.MovingDown : Doing.MovingUp;
                SoundIfInView();
                break;

            default:
                throw new InvalidOperationException($"A shutter has no step {_doing}.");
        }
    }

    private void SetOff()
    {
        _doing = MovesUpFirst ? Doing.MovingUp : Doing.MovingDown;
        SoundIfInView();
    }

    /// <summary>Slides it one update's way, carrying whoever stands on it, and stops it at the end of its travel.</summary>
    private void Move(bool up)
    {
        MoveCarrying(new PixelOffset(Pixels.Zero, Pixels.FromNumber(up ? -Speed : Speed)));
        bool arrived = up ? Position.Y.Whole <= _top : Position.Y.Whole >= _bottom;
        if (!arrived)
            return;
        int rest = up ? RestAfterUp : RestAfterDown;
        if (rest < 0)
        {
            _doing = Doing.Still;
            return;
        }

        _rest = rest + 1;
        _doing = up ? Doing.StoppedAfterUp : Doing.StoppedAfterDown;
    }

    /// <summary>What being struck does to it.</summary>
    private void React()
    {
        if (Trigger is not (Triggers.StruckOnce or Triggers.StruckEachTime))
        {
            SoundIfInView();
            return;
        }

        if (Trigger == Triggers.StruckOnce)
        {
            if (_struckOnce)
                return;
            _struckOnce = true;
        }

        if (_doing is Doing.MovingUp or Doing.MovingDown)
            return;
        _doing = _nextStrikeUp ? Doing.MovingUp : Doing.MovingDown;
        _nextStrikeUp = !_nextStrikeUp;
        SoundIfInView();
    }

    /// <summary>Sounds its start only while its middle is in the enemies' view.</summary>
    private void SoundIfInView()
    {
        if (EnemyView.Contains(Room, Position.X.Whole, Position.Y.Whole))
            Sound(ActivationSound);
    }
}
