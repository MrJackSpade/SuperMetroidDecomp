using Engine.Actors;
using Engine.Content;
using Engine.Shared;
using Engine.World;
using Godot;

namespace Game.Enemies;

/// <summary>
/// An orb a Torizo spews. Bomb Torizo's arcs down and bursts on the first wall or floor it meets. Golden Torizo's
/// bounces off walls and along the floor, each bounce half the last, and bursts when it has all but stopped. Shot
/// down, it may leave a pickup, as its settings say.
/// </summary>
/// <remarks>Its timelines are "wall_impact" and "floor_impact".</remarks>
public partial class TorizoOrb : CuedProjectile
{
    /// <summary>The downward speed it gains each update, as a number of pixels.</summary>
    [Export, DataValue("gravity")]
    public double Gravity { get; set; } = 0.0703125;

    /// <summary>Whether it bounces rather than bursting at once.</summary>
    [Export, DataValue("bounces")]
    public bool Bounces { get; set; }

    /// <summary>The speed across a bounce on the floor costs it.</summary>
    [Export, DataValue("bounce_drag")]
    public double BounceDrag { get; set; } = 0.25;

    /// <inheritdoc/>
    protected override void Move(Room room)
    {
        if (MoveAcrossMeetingTerrain(room, Speed.X))
        {
            if (!Bounces)
            {
                Burst("wall_impact");
                return;
            }

            Speed = Speed.WithX(-Speed.X);
        }

        bool landed = MoveDownMeetingTerrain(room, Speed.Y) && !Speed.Y.IsNegative;
        if (landed && Bounces)
        {
            // Half its fall comes back as rise.
            Pixels rise = -Speed.Y.Share(1, 2);
            Speed = new PixelVelocity(Dragged(Speed.X, Pixels.FromNumber(BounceDrag)), rise);
            landed = rise.IsNegative && rise >= Pixels.FromNumber(-0.5);
        }

        if (landed)
        {
            Position = PixelPosition.FromWhole(Position.X.Whole, Position.Y.Whole / 16 * 16 + 6);
            Burst("floor_impact");
            return;
        }

        Speed = Speed.WithY(Speed.Y + Pixels.FromNumber(Gravity));
        if (!Bounces && IsRunaway(Speed.Y))
            Leave(room);
    }
}
