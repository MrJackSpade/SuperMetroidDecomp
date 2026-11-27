using Engine.Actors;
using Engine.Collision;
using Engine.Content;
using Engine.Shared;
using Godot;

namespace Game.Enemies;

/// <summary>
/// An Alcoon: a fire-breather that hides below the floor. When Samus comes near, it leaps up out of its hiding place,
/// lands, and paces back and forth, stopping to breathe three fireballs at her when she is in front of it. Once it
/// has paced far enough from where it hides, it hops back down.
/// </summary>
/// <remarks>
/// Its timelines are "walking_left" and "walking_right", whose cue "move" takes one stride and "walk_cycle" counts a
/// cycle and takes one; "leap_left" and "leap_right" for its leap up, "airborne_left" and "airborne_right" for coming
/// down; and "fire_left" and "fire_right", whose cues "fire:up", "fire:level" and "fire:down" each breathe a fireball
/// and whose cue "start_walking" sets it pacing again. Its shot is "fireball".
/// </remarks>
public partial class Alcoon : Enemy
{
    /// <summary>How near Samus's height must be to where it lands, in pixels, for it to come out.</summary>
    [Export, DataValue("emerge_down_distance")]
    public int EmergeDownDistance { get; set; } = 32;

    /// <summary>How near across Samus must be, in pixels, for it to come out.</summary>
    [Export, DataValue("emerge_across_distance")]
    public int EmergeAcrossDistance { get; set; } = 80;

    /// <summary>How far across from its hiding place it paces, in pixels, before it hides again.</summary>
    [Export, DataValue("hide_distance")]
    public int HideDistance { get; set; } = 112;

    /// <summary>The upward speed its leap out starts with, as a number of pixels.</summary>
    [Export, DataValue("emerge_jump_speed")]
    public double EmergeJumpSpeed { get; set; } = 12;

    /// <summary>The upward speed its hop back into hiding starts with.</summary>
    [Export, DataValue("hide_jump_speed")]
    public double HideJumpSpeed { get; set; } = 4;

    /// <summary>The downward speed it gains each update in the air.</summary>
    [Export, DataValue("gravity")]
    public double Gravity { get; set; } = 0.5;

    /// <summary>How far one stride carries it, in pixels.</summary>
    [Export, DataValue("stride")]
    public int Stride { get; set; } = 2;

    /// <summary>Where a fireball leaves from when it faces right, across and down from its middle, in pixels.</summary>
    [Export, DataValue("muzzle_across")]
    public int MuzzleAcross { get; set; } = 16;

    /// <summary>How far above its middle a fireball leaves from, in pixels.</summary>
    [Export, DataValue("muzzle_up")]
    public int MuzzleUp { get; set; } = 12;

    /// <summary>The sound of it leaping out, by name.</summary>
    [Export, DataValue("emerge_sound")]
    public string EmergeSound { get; set; } = "";

    /// <summary>The sound of each fireball it breathes, by name: heard though the room has no place for the fireball.</summary>
    [Export, DataValue("fire_sound")]
    public string FireSound { get; set; } = "";

    /// <summary>
    /// How its moves are tried, as the cartridge moves an enemy: only where each would end, and sloped ground that holds
    /// things up by their middle met under its middle alone.
    /// </summary>
    private static readonly MoveOptions Moves =
        MoveOptions.AmongBlocks with { Tries = WayTried.WhereItEnds, Slopes = SlopeTreatment.ClimbUnderMiddle, RestsUnderSlopes = true };

    /// <summary>How a stride across is tried: as its other moves, and such sloped ground is no part of it.</summary>
    private static readonly MoveOptions StridesAcross = Moves with { Slopes = SlopeTreatment.IgnoreMiddleHeld };

    private enum Doing
    {
        Hidden,
        LeapingOut,
        ComingDown,
        Pacing,
        Breathing,
        HoppingBack,
        DroppingBack,
    }

    private Doing _doing;
    private int _hideX;
    private int _hideY;
    private int _landsAt;
    private int _cyclesLeft;
    private bool _walks;

    /// <inheritdoc/>
    protected override void OnEnterRoom()
    {
        (_hideX, _hideY) = (Position.X.Whole, Position.Y.Whole);
        Play("walking_left");

        // Where its leap out will land is found once, by making the leap: up freely, then down until the floor.
        Pixels speed = -Pixels.FromNumber(EmergeJumpSpeed), gravity = Pixels.FromNumber(Gravity);
        while (speed.IsNegative)
        {
            Position += new PixelOffset(Pixels.Zero, speed);
            speed += gravity;
        }

        for (int step = 0; !Drop(speed); step++)
        {
            if (step > 4096)
                throw new InvalidOperationException($"The Alcoon \"{Name}\" finds no floor to land on.");
            speed += gravity;
        }

        // Only its whole pixels go back: the part of a pixel its landing left it at stays, as on the cartridge.
        _landsAt = Position.Y.Whole;
        Position = Position with { Y = Position.Y.WithWhole(_hideY) };
    }

    /// <inheritdoc/>
    protected override void OnCue(string cue)
    {
        switch (cue)
        {
            case "move":
                TakeStride();
                break;
            case "walk_cycle":
                if (_cyclesLeft > 0)
                    _cyclesLeft--;
                TakeStride();
                break;
            case "start_walking":
                _doing = Doing.Pacing;
                // How many cycles it paces before its next volley is picked by the number last drawn.
                _cyclesLeft = Random.Last.Part(0, 2) switch { 0 => 2, 1 => 1, 2 => 2, _ => 3 };
                Play(Facing == Facing.Left ? "walking_left" : "walking_right", restart: true);
                break;
            case "fire:up" or "fire:level" or "fire:down":
                Pixels down = cue == "fire:up" ? Pixels.FromWhole(-1) : cue == "fire:down" ? Pixels.FromWhole(1) : Pixels.Zero;
                // A fireball leaves from whole pixels, whatever part of a pixel the Alcoon is at.
                PixelPosition muzzle = PixelPosition.FromWhole(
                    Position.X.Whole + (Facing == Facing.Left ? -MuzzleAcross : MuzzleAcross), Position.Y.Whole - MuzzleUp);
                Fire("fireball", muzzle, new PixelVelocity(Facing.Toward(Shot("fireball").Speed), down));
                Sound(FireSound);
                break;
        }
    }

    /// <inheritdoc/>
    protected override void Behave()
    {
        Pixels gravity = Pixels.FromNumber(Gravity);
        switch (_doing)
        {
            case Doing.Hidden:
                if (Math.Abs(_landsAt - Target.Y.Whole) >= EmergeDownDistance)
                    return;
                int across = Target.X.Whole - Position.X.Whole;
                if (Math.Abs(across) >= EmergeAcrossDistance)
                    return;
                Velocity = new PixelVelocity(Pixels.Zero, -Pixels.FromNumber(EmergeJumpSpeed));
                Facing = across < 0 ? Facing.Left : Facing.Right;
                _walks = false;
                Play(Facing == Facing.Left ? "leap_left" : "leap_right", restart: true);
                _doing = Doing.LeapingOut;
                Sound(EmergeSound);
                return;

            case Doing.LeapingOut:
                Position += new PixelOffset(Pixels.Zero, Velocity.Y);
                Velocity = Velocity.WithY(Velocity.Y + gravity);
                if (Velocity.Y.IsNegative)
                    return;
                _doing = Doing.ComingDown;
                Play(Facing == Facing.Left ? "airborne_left" : "airborne_right", restart: true);
                return;

            case Doing.ComingDown:
                if (!Drop(Velocity.Y))
                {
                    Velocity = Velocity.WithY(Velocity.Y + gravity);
                    return;
                }

                Facing = TowardSamus;
                _walks = true;
                Play(Facing == Facing.Left ? "walking_left" : "walking_right", restart: true);
                _doing = Doing.Pacing;
                _cyclesLeft = 1;
                return;

            case Doing.Pacing:
                Drop(Pixels.FromWhole(2));
                if (Math.Abs(_hideX - Position.X.Whole) >= HideDistance)
                {
                    _doing = Doing.HoppingBack;
                    _walks = false;
                    Velocity = new PixelVelocity(Pixels.Zero, -Pixels.FromNumber(HideJumpSpeed));
                    Play(Facing == Facing.Left ? "airborne_left" : "airborne_right", restart: true);
                    return;
                }

                // It breathes fire only once its cycles are paced out and Samus is on the side it faces.
                if (_cyclesLeft > 0 || TowardSamus != Facing)
                    return;
                Play(Facing == Facing.Left ? "fire_left" : "fire_right", restart: true);
                _doing = Doing.Breathing;
                return;

            case Doing.Breathing:
                return;

            case Doing.HoppingBack:
                Position += new PixelOffset(Pixels.Zero, Velocity.Y);
                Velocity = Velocity.WithY(Velocity.Y + gravity);
                if (!Velocity.Y.IsNegative)
                    _doing = Doing.DroppingBack;
                return;

            case Doing.DroppingBack:
                Position += new PixelOffset(Pixels.Zero, Velocity.Y);
                if (Position.Y.Whole < _hideY)
                {
                    Velocity = Velocity.WithY(Velocity.Y + gravity);
                    return;
                }

                // Its whole pixels go back to where it hides; the part of a pixel it is at stays.
                Position = new PixelPosition(Position.X.WithWhole(_hideX), Position.Y.WithWhole(_hideY));
                _doing = Doing.Hidden;
                return;

            default:
                throw new InvalidOperationException($"Undefined Alcoon stage {_doing}.");
        }
    }

    /// <summary>
    /// Takes one stride the way it faces, then is put where sloped ground has it under its middle; a wall turns it
    /// round and starts its pacing over. Before it has first come out it strides nowhere, and is only put so.
    /// </summary>
    private void TakeStride()
    {
        if (_walks && Room.Mover.MoveAcross(MovingBody(), Facing.Toward(Pixels.FromWhole(Stride)), StridesAcross).Blocked)
        {
            // The other walk is taken up at its first stride, past the cue that would stride again at once.
            Facing = Facing.Opposite();
            ResumeTimeline(new TimelineMark(Facing == Facing.Left ? "walking_left" : "walking_right", 0, 1));
            return;
        }

        // A move across of no distance meets nothing, and ends as every move across that climbs such ground does.
        Room.Mover.MoveAcross(MovingBody(), Pixels.Zero, Moves);
    }

    /// <summary>Moves it down by <paramref name="distance"/>, upward when less than nothing; returns whether something stopped it.</summary>
    private bool Drop(Pixels distance) => Room.Mover.MoveDown(MovingBody(), distance, Moves).Blocked;

    /// <summary>The collider it moves by.</summary>
    private Collider MovingBody() => Body ?? throw new InvalidOperationException($"The Alcoon \"{Name}\" has no body collider.");

    /// <summary>The side Samus is on, by whole pixels alone: in the same pixel column she counts as on its right.</summary>
    private Facing TowardSamus => Target.X.Whole < Position.X.Whole ? Facing.Left : Facing.Right;
}
