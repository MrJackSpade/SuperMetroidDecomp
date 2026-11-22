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

    /// <summary>
    /// The offset the loop along <paramref name="fallingPath"/> moves the Choot by <paramref name="frameIndex"/> updates
    /// after it started, or null on the update its path has run out.
    /// </summary>
    /// <exception cref="InvalidOperationException">The path ran out before <paramref name="frameIndex"/>.</exception>
    internal static Offset16<RoomPixels>? OffsetAt(ChootFallingPath fallingPath, int frameIndex)
    {
        ChootFallLoop loop = ChootFallLoop.Starting(fallingPath);
        for (int counted = 0; counted < Ensure.AtLeastZero(frameIndex); counted++)
            loop = loop is ChootFallLoop.AtSample at
                ? at.Counted()
                : throw new InvalidOperationException($"Choot falling path {fallingPath} ran out before update {frameIndex}.");
        return loop is ChootFallLoop.AtSample sample ? sample.Offset : null;
    }

    /// <summary>The offset from the falling origin <paramref name="loop"/> moves its Choot to next.</summary>
    /// <exception cref="InvalidOperationException">The loop is over.</exception>
    internal static Offset16<RoomPixels> NextOffsetOf(ChootFallLoop loop) => loop is ChootFallLoop.AtSample at
        ? at.Offset
        : throw new InvalidOperationException("A Choot's falling loop that is over moves it to no offset.");
}
