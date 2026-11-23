using Engine.Shared;

namespace Engine.Actors.Movement;

/// <summary>
/// Circling a point at a set distance and speed. The mover is placed on its circle each update; nothing stops it, since
/// a thing that circles is carried round by what it circles, not by its own feet.
/// </summary>
public sealed class Orbit
{
    /// <summary>How far from the centre it circles.</summary>
    public Pixels Radius { get; set; } = Pixels.FromWhole(24);

    /// <summary>How many angle steps, of 256 to a full circle, it advances each update; negative circles the other way.</summary>
    public int AngleStep { get; set; } = 2;

    /// <summary>Where on the circle it is, in angle steps from the rightmost point, turning toward the bottom.</summary>
    public int Angle { get; set; }

    /// <summary>Moves <paramref name="mover"/> for one update around <paramref name="centre"/>.</summary>
    public MoveReport Step(IMoves mover, PixelPosition centre)
    {
        Angle = ((Angle + AngleStep) % Turns.PerTurn + Turns.PerTurn) % Turns.PerTurn;
        PixelPosition before = mover.Position;
        mover.Position = centre + new PixelOffset(Turns.Cosine(Angle, Radius), Turns.Sine(Angle, Radius));
        PixelOffset moved = mover.Position - before;
        mover.Velocity = new PixelVelocity(moved.X, moved.Y);
        return new MoveReport(false, false, false, false, Angle < Math.Abs(AngleStep) && AngleStep != 0);
    }
}
