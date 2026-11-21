namespace SuperMetroid.Core.Enemies.Choot;

/// <summary>A Choot's falling loop a recording names by its age (#2554).</summary>
internal static class ChootFallLoopAccess
{
    /// <summary>
    /// The loop along <paramref name="fallingPath"/> <paramref name="age"/> updates after it started, counted as the Choot
    /// counts it: over once its path has run out.
    /// </summary>
    /// <param name="fallingPath">The Choot's falling path.</param>
    /// <param name="age">The updates since the loop started.</param>
    internal static ChootFallLoop AtAge(ChootFallingPath fallingPath, int age)
    {
        ChootFallLoop loop = ChootFallLoop.Starting(fallingPath);
        for (int counted = 0; counted < Ensure.AtLeastZero(age) && loop is ChootFallLoop.AtSample at; counted++)
            loop = at.Counted();
        return loop;
    }
}
