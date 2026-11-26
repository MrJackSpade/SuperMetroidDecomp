using Engine.Shared;

namespace Game.Compatibility;

/// <summary>
/// The flight of the giant Metroid, in Tourian and in Mother Brain's fight: drawn toward a place, the farther off the
/// harder. Going the wrong way it is turned about sharply, and more sharply still across when it has strayed well out
/// of view. It only works out the speed; the flier moves itself. Each way its speed changes by the cartridge's steps,
/// one at a time (<see cref="SeekingSteps"/>): the push it takes out of view and the check of a wrong way each start
/// afresh, dropping any unit owed before them.
/// </summary>
public static class GradualFlight
{
    /// <summary>The top speeds across and up or down.</summary>
    private static readonly Pixels TopSpeedAcross = Pixels.FromWhole(8), TopSpeedDown = Pixels.FromWhole(5);

    /// <summary>The speed taken off a flier going the wrong way, on top of the pull.</summary>
    private static readonly Pixels Reversal = Pixels.FromParts(0, Pixels.FractionsPerPixel / 32);

    /// <summary>How far past the view's side edges, and past its top and bottom, the flier may stray, in pixels, before it counts as out of view.</summary>
    private const int ViewMarginAcross = 16, ViewMarginDown = 96;

    /// <summary>The speed of a flier at <paramref name="at"/> going at <paramref name="speed"/>, drawn toward <paramref name="target"/>.</summary>
    /// <param name="at">Where it is.</param>
    /// <param name="speed">Its speed.</param>
    /// <param name="target">Where it is drawn to.</param>
    /// <param name="gentleness">How gently it is drawn: 1 the hardest, 16 the gentlest.</param>
    /// <param name="wrongWayKick">What more turns it about when it is out of view and flying away across.</param>
    /// <param name="view">What the camera shows.</param>
    public static PixelVelocity Toward(PixelPosition at, PixelVelocity speed, PixelPosition target, int gentleness, Pixels wrongWayKick, PixelRect view)
    {
        if (gentleness is < 1 or > 16)
            throw new ArgumentOutOfRangeException(nameof(gentleness), "The gentleness is 16 down to 1.");
        return new PixelVelocity(
            Seek(speed.X, at.X.Whole, target.X.Whole, gentleness, TopSpeedAcross, () => IsOutOfView(at, view), wrongWayKick),
            Seek(speed.Y, at.Y.Whole, target.Y.Whole, gentleness, TopSpeedDown, static () => false, Pixels.Zero));
    }

    /// <summary>Whether a flier at <paramref name="at"/> has strayed well out of view, or out of the room's top or left.</summary>
    public static bool IsOutOfView(PixelPosition at, PixelRect view)
    {
        int across = at.X.Whole, down = at.Y.Whole;
        if (across < 0 || down < 0)
            return true;
        int inViewDown = down + ViewMarginDown - view.Top, inViewAcross = across + ViewMarginAcross - view.Left;
        return inViewDown < 0 || inViewDown >= view.Height + ViewMarginDown * 2
            || inViewAcross < 0 || inViewAcross >= view.Width + ViewMarginAcross * 2;
    }

    /// <summary>
    /// One way's speed after this update's steps. Whether it is out of view is asked only when it is going the wrong
    /// way, as the cartridge asks it; out of view, the raise is a 256th of a pixel more than the matching lowering.
    /// </summary>
    private static Pixels Seek(Pixels speed, int at, int target, int gentleness, Pixels most, Func<bool> outOfView, Pixels kick)
    {
        int fromTarget = SeekingSteps.ShortWay(at, target);
        if (fromTarget == 0)
            return speed;
        Pixels step = SeekingSteps.StepFor(Math.Max(1, Math.Abs(fromTarget) / gentleness));
        bool owed = SeekingSteps.WrapReversesOrder(at, target);
        if (fromTarget < 0)
        {
            if (speed.IsNegative)
            {
                if (outOfView())
                {
                    owed = false;
                    SeekingSteps.Raise(ref speed, ref owed, kick + SeekingSteps.FinestStep);
                }

                owed = false;
                SeekingSteps.Raise(ref speed, ref owed, Reversal);
                SeekingSteps.Raise(ref speed, ref owed, step);
            }

            SeekingSteps.Raise(ref speed, ref owed, step);
            return speed > most ? most : speed;
        }

        if (!speed.IsNegative)
        {
            if (outOfView())
            {
                owed = false;
                SeekingSteps.Lower(ref speed, ref owed, kick);
            }

            owed = false;
            SeekingSteps.Lower(ref speed, ref owed, Reversal);
            SeekingSteps.Lower(ref speed, ref owed, step);
        }

        SeekingSteps.Lower(ref speed, ref owed, step);
        return speed < -most ? -most : speed;
    }
}
