using Engine.Shared;

namespace Game.Enemies;

/// <summary>
/// How the giant Metroid flies: it is drawn toward a place, faster the farther off it is, and turned about sharply
/// when it is flying the wrong way, more sharply still when it has strayed well out of view.
/// </summary>
public static class BabyMetroidFlight
{
    private static readonly Pixels MostAcross = Pixels.FromWhole(8);
    private static readonly Pixels MostDown = Pixels.FromWhole(5);
    private static readonly Pixels Reversal = Pixels.FromNumber(1 / 32.0);
    private static readonly Pixels Finest = Pixels.FromNumber(1 / 256.0);

    /// <summary>How far past the view's edges it may stray, across and down, before it counts as out of view.</summary>
    public const int ViewMarginAcross = 16, ViewMarginDown = 96;

    /// <summary>The speed of a Metroid at <paramref name="at"/> flying at <paramref name="speed"/>, drawn toward <paramref name="target"/>.</summary>
    /// <param name="at">Where it is.</param>
    /// <param name="speed">Its speed.</param>
    /// <param name="target">Where it is drawn to.</param>
    /// <param name="divisor">How gently it is drawn: 1 the hardest, 16 the gentlest.</param>
    /// <param name="wrongWayKick">What turns it about when it is out of view and flying away across.</param>
    /// <param name="view">What the camera shows.</param>
    public static PixelVelocity Toward(PixelPosition at, PixelVelocity speed, PixelPosition target, int divisor, Pixels wrongWayKick, PixelRect view)
    {
        if (divisor is < 1 or > 16)
            throw new ArgumentOutOfRangeException(nameof(divisor), "The divisor is 16 down to 1.");
        bool outOfView = IsOutOfView(at, view);
        return new PixelVelocity(
            Seek(speed.X, at.X.Whole - target.X.Whole, divisor, MostAcross, outOfView, wrongWayKick),
            Seek(speed.Y, at.Y.Whole - target.Y.Whole, divisor, MostDown, false, Pixels.Zero));
    }

    /// <summary>Whether a Metroid at <paramref name="at"/> has strayed well out of view, or out of the room's top or left.</summary>
    public static bool IsOutOfView(PixelPosition at, PixelRect view)
    {
        int across = at.X.Whole, down = at.Y.Whole;
        if (across < 0 || down < 0)
            return true;
        int inViewDown = down + ViewMarginDown - view.Top, inViewAcross = across + ViewMarginAcross - view.Left;
        return inViewDown < 0 || inViewDown >= view.Height + ViewMarginDown * 2
            || inViewAcross < 0 || inViewAcross >= view.Width + ViewMarginAcross * 2;
    }

    private static Pixels Seek(Pixels speed, int fromTarget, int divisor, Pixels most, bool outOfView, Pixels kick)
    {
        if (fromTarget == 0)
            return speed;
        Pixels step = Pixels.FromNumber(Math.Max(1, Math.Abs(fromTarget) / divisor) / 256.0);
        if (fromTarget < 0)
        {
            if (speed.IsNegative)
            {
                if (outOfView)
                    speed += kick + Finest;
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
