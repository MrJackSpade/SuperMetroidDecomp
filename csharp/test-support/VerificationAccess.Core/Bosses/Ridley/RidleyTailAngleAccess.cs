namespace SuperMetroid.Core.Bosses.Ridley;

/// <summary>Builds tail angles for checks that state them by their distance from straight down.</summary>
internal static class RidleyTailAngleAccess
{
    extension(RidleyTailAngle)
    {
        /// <summary>The angle <paramref name="steps"/> 256ths of a turn counterclockwise of straight down, clockwise when negative.</summary>
        internal static RidleyTailAngle FromStepsFromStraightDown(int steps) => RidleyTailAngle.StraightDown.TurnedCounterclockwise(steps);
    }
}
