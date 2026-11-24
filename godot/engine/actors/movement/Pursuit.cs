using Engine.Shared;

namespace Engine.Actors.Movement;

/// <summary>
/// Flying drawn toward a place: the farther off the place, the harder the pull. A flier going the wrong way is
/// turned about sharply, and more sharply still when it has strayed well out of view. It only works out the speed;
/// the flier moves itself.
/// </summary>
public sealed class Pursuit
{
    /// <summary>The top speed across.</summary>
    public Pixels TopSpeedAcross { get; set; } = Pixels.FromWhole(8);

    /// <summary>The top speed up or down.</summary>
    public Pixels TopSpeedDown { get; set; } = Pixels.FromWhole(5);

    /// <summary>The speed taken off a flier going the wrong way, on top of the pull.</summary>
    public Pixels Reversal { get; set; } = Pixels.FromParts(0, Pixels.FractionsPerPixel / 32);

    /// <summary>How far past the view's side edges the flier may stray, in pixels, before it counts as out of view.</summary>
    public int ViewMarginAcross { get; set; } = 16;

    /// <summary>How far past the view's top and bottom the flier may stray, in pixels, before it counts as out of view.</summary>
    public int ViewMarginDown { get; set; } = 96;

    /// <summary>The speed of a flier at <paramref name="at"/> going at <paramref name="speed"/>, drawn toward <paramref name="target"/>.</summary>
    /// <param name="at">Where it is.</param>
    /// <param name="speed">Its speed.</param>
    /// <param name="target">Where it is drawn to.</param>
    /// <param name="gentleness">How gently it is drawn: 1 the hardest, 16 the gentlest.</param>
    /// <param name="wrongWayKick">What more turns it about when it is out of view and flying away across.</param>
    /// <param name="view">What the camera shows.</param>
    public PixelVelocity Toward(PixelPosition at, PixelVelocity speed, PixelPosition target, int gentleness, Pixels wrongWayKick, PixelRect view)
    {
        if (gentleness is < 1 or > 16)
            throw new ArgumentOutOfRangeException(nameof(gentleness), "The gentleness is 16 down to 1.");
        bool outOfView = IsOutOfView(at, view);
        return new PixelVelocity(
            Seek(speed.X, at.X.Whole - target.X.Whole, gentleness, TopSpeedAcross, outOfView, wrongWayKick),
            Seek(speed.Y, at.Y.Whole - target.Y.Whole, gentleness, TopSpeedDown, false, Pixels.Zero));
    }

    /// <summary>Whether a flier at <paramref name="at"/> has strayed well out of view, or out of the room's top or left.</summary>
    public bool IsOutOfView(PixelPosition at, PixelRect view)
    {
        int across = at.X.Whole, down = at.Y.Whole;
        if (across < 0 || down < 0)
            return true;
        int inViewDown = down + ViewMarginDown - view.Top, inViewAcross = across + ViewMarginAcross - view.Left;
        return inViewDown < 0 || inViewDown >= view.Height + ViewMarginDown * 2
            || inViewAcross < 0 || inViewAcross >= view.Width + ViewMarginAcross * 2;
    }

    private Pixels Seek(Pixels speed, int fromTarget, int gentleness, Pixels most, bool outOfView, Pixels kick)
    {
        if (fromTarget == 0)
            return speed;
        Pixels finest = Pixels.FromParts(0, Pixels.FractionsPerPixel / 256);
        int pull = Math.Max(1, Math.Abs(fromTarget) / gentleness);
        Pixels step = Pixels.FromParts(pull / 256, pull % 256 * (Pixels.FractionsPerPixel / 256));
        if (fromTarget < 0)
        {
            if (speed.IsNegative)
            {
                if (outOfView)
                    speed += kick + finest;
                speed += Reversal + step;
            }

            speed += step;
            return speed > most ? most : speed;
        }

        if (!speed.IsNegative)
        {
            if (outOfView)
                speed -= kick;
            speed -= Reversal + step;
        }

        speed -= step;
        return speed < -most ? -most : speed;
    }
}
