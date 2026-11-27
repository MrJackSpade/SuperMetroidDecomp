using Engine.Actors;
using Engine.Content;
using Engine.Shared;
using Game.Compatibility;
using Godot;

namespace Game.Enemies;

/// <summary>
/// A Zoa: it lies hidden until Samus comes near across, rises until it is level with her, and then shoots across the
/// room the way she was, faster and faster. Once its middle has left the enemies' view it is back where it started,
/// hidden again.
/// Nothing in the room stops it.
/// </summary>
/// <remarks>
/// Its timelines are "rising_left", "rising_right", "shooting_left" and "shooting_right". A cue of the shooting
/// timeline, "speed:" and a number of pixels, sets how fast it shoots across from then on.
/// </remarks>
public partial class Zoa : Enemy
{
    /// <summary>How near across Samus must come, in pixels, to set it off.</summary>
    [Export, DataValue("reach")]
    public int Reach { get; set; } = 128;

    /// <summary>How far it rises each update, as a number of pixels.</summary>
    [Export, DataValue("rise_speed")]
    public double RiseSpeed { get; set; } = 0.5;

    private enum Doing
    {
        Hidden,
        Rising,
        Shooting,
    }

    private Doing _doing;
    private PixelPosition _home;
    private Pixels _speed;

    /// <inheritdoc/>
    protected override void OnEnterRoom()
    {
        _home = Position;
        Facing = Facing.Left;
        Play("shooting_left");
        Visual?.SetShown(false);
    }

    /// <inheritdoc/>
    protected override void OnCue(string cue)
    {
        if (cue.StartsWith("speed:", StringComparison.Ordinal))
            _speed = Pixels.FromNumber(double.Parse(cue["speed:".Length..], System.Globalization.CultureInfo.InvariantCulture));
    }

    /// <inheritdoc/>
    protected override void Behave()
    {
        switch (_doing)
        {
            case Doing.Hidden:
                if (Math.Abs(Target.X.Whole - Position.X.Whole) >= Reach)
                    return;
                Facing = Target.X.Whole < Position.X.Whole ? Facing.Left : Facing.Right;
                Play(Facing == Facing.Left ? "rising_left" : "rising_right");
                _doing = Doing.Rising;
                return;

            case Doing.Rising:
                Visual?.SetShown(true);
                if (Target.Y.Whole < Position.Y.Whole)
                {
                    Position -= new PixelOffset(Pixels.Zero, Pixels.FromNumber(RiseSpeed));
                    return;
                }

                Play(Facing == Facing.Left ? "shooting_left" : "shooting_right");
                _doing = Doing.Shooting;
                return;

            case Doing.Shooting:
                Position += new PixelOffset(Facing.Toward(_speed), Pixels.Zero);
                if (EnemyView.Contains(Room, Position.X.Whole, Position.Y.Whole))
                    return;
                // Back home it keeps where it lay within a pixel, the speed its timeline last gave it, and that timeline
                // if it shot left: it plays on unseen.
                Visual?.SetShown(false);
                Position = new PixelPosition(Pixels.FromParts(_home.X.Whole, Position.X.Fraction), Pixels.FromParts(_home.Y.Whole, Position.Y.Fraction));
                Facing = Facing.Left;
                Play("shooting_left");
                _doing = Doing.Hidden;
                return;

            default:
                throw new InvalidOperationException($"Undefined Zoa stage {_doing}.");
        }
    }
}
