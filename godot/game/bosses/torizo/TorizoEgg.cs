using Engine.Actors;
using Engine.Content;
using Engine.Shared;
using Engine.World;
using Godot;

namespace Game.Enemies;

/// <summary>
/// An egg from Golden Torizo's chest. It bounces about, turning at walls, until it hatches; the hatchling charges
/// level, faster and faster, the way the egg was last going, drops where a wall stops it, and breaks on the floor.
/// </summary>
/// <remarks>
/// Its timelines are "hatching_left", "hatching_right", "hatched_left", "hatched_right", "break_left" and
/// "break_right". The cue "hatch" of a hatching timeline brings the hatchling out, and "charge" sets it off.
/// </remarks>
public partial class TorizoEgg : CuedProjectile
{
    /// <summary>How many updates it bounces before it hatches.</summary>
    [Export, DataValue("hatch_updates")]
    public int HatchUpdates { get; set; } = 67;

    /// <summary>The speed it gains each update, falling or charging, as a number of pixels.</summary>
    [Export, DataValue("quickening")]
    public double Quickening { get; set; } = 0.1875;

    /// <summary>The speed across a bounce on the floor costs it.</summary>
    [Export, DataValue("bounce_drag")]
    public double BounceDrag { get; set; } = 0.125;

    private enum Doing
    {
        Bouncing,
        Charging,
        Dropping,
    }

    private Doing _doing;
    private Facing _laid = Facing.Left;
    private Facing _facing = Facing.Left;
    private int _age;

    /// <summary>Tells it which way the Torizo faced as it was laid.</summary>
    public void Laid(Facing facing) => (_laid, _facing) = (facing, facing);

    /// <inheritdoc/>
    protected override void OnOwnCue(Room room, string[] cue)
    {
        switch (cue[0])
        {
            case "hatch":
                ShowTimeline(_facing == Facing.Right ? "hatched_right" : "hatched_left");
                break;
            case "charge":
                _doing = Doing.Charging;
                break;
            default:
                base.OnOwnCue(room, cue);
                break;
        }
    }

    /// <inheritdoc/>
    protected override void Move(Room room)
    {
        Pixels gain = Pixels.FromNumber(Quickening);
        switch (_doing)
        {
            case Doing.Bouncing:
                if (++_age == HatchUpdates)
                {
                    ShowTimeline(_laid == Facing.Right ? "hatching_right" : "hatching_left");
                    Speed = Speed.WithX(_facing.Toward(Pixels.FromWhole(1)));
                    return;
                }

                if (MoveAcrossMeetingTerrain(room, Speed.X))
                {
                    Speed = Speed.WithX(-Speed.X);
                    _facing = _facing.Opposite();
                }

                if (MoveDownMeetingTerrain(room, Speed.Y) && !Speed.Y.IsNegative)
                    Speed = new PixelVelocity(Dragged(Speed.X, Pixels.FromNumber(BounceDrag)), -Speed.Y);
                Speed = Speed.WithY(Speed.Y + gain);
                if (IsRunaway(Speed.Y))
                    Leave(room);
                break;

            case Doing.Charging:
                if (MoveAcrossMeetingTerrain(room, Speed.X))
                {
                    _doing = Doing.Dropping;
                    Speed = Speed.WithY(Pixels.Zero);
                    break;
                }

                Speed = Speed.WithX(_facing == Facing.Right ? Speed.X + gain : Speed.X - gain);
                break;

            case Doing.Dropping:
                if (MoveDownMeetingTerrain(room, Speed.Y))
                {
                    Moving = false;
                    ShowTimeline(_facing == Facing.Right ? "break_right" : "break_left");
                    break;
                }

                Speed = Speed.WithY(Speed.Y + gain);
                break;

            default:
                throw new InvalidOperationException($"A Torizo's egg has nothing to do called {_doing}.");
        }
    }
}
