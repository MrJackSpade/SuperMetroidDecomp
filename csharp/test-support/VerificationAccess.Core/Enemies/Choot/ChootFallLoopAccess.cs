namespace SuperMetroid.Core.Enemies.Choot;

/// <summary>Builds the falling loops a check or a recorded state's import places partway through, which production only starts.</summary>
internal static class ChootFallLoopAccess
{
    /// <summary>A loop of <paramref name="fallingPath"/> that has counted <paramref name="age"/> updates.</summary>
    internal static ChootFallLoop AtAge(ChootFallingPath fallingPath, int age)
    {
        ChootFallLoop loop = ChootFallLoop.Starting(fallingPath);
        for (int counted = 0; counted < age; counted++)
            loop = loop is ChootFallLoop.AtSample at
                ? at.Counted()
                : throw new InvalidOperationException($"A loop of {fallingPath} has no update {age}.");
        return loop;
    }

    /// <summary>The update of the loop the next falling update takes: the loop's age, or its length once it is over.</summary>
    internal static int AgeOf(ChootFallLoop loop, ChootFallingPath fallingPath) => loop switch
    {
        ChootFallLoop.AtSample at => at.Age,
        ChootFallLoop.Over => ChootFallingPathDefinitions.Paths[fallingPath].Sum(sample => sample.Updates),
        _ => throw new InvalidOperationException($"Undefined {nameof(ChootFallLoop)} {loop}."),
    };

    /// <summary>The offset of the loop of <paramref name="fallingPath"/> at <paramref name="update"/>, or null past its end.</summary>
    internal static Offset16<RoomPixels>? OffsetAt(ChootFallingPath fallingPath, int update) =>
        AtAge(fallingPath, update) is ChootFallLoop.AtSample at ? at.Offset : null;
}
