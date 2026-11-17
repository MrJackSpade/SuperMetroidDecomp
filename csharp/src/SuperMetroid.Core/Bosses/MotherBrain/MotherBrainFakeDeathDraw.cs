namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>
/// What Mother Brain's fake death draws in her room, other than the rows of the mural behind her tank, which are one draw
/// of their own (<see cref="MotherBrainFakeDeathDraws.MuralRow"/>).
/// </summary>
internal enum MotherBrainFakeDeathDraw
{
    /// <summary>A column of three solid blocks that fills a gap in the wall.</summary>
    FillWall,
    /// <summary>The opened escape door.</summary>
    EscapeDoor,
    /// <summary>A ceiling block cleared away.</summary>
    ClearCeilingBlock,
    /// <summary>A ceiling tube cleared away.</summary>
    ClearCeilingTube,
    /// <summary>A lower middle side tube cleared away.</summary>
    ClearBottomMiddleSideTube,
    /// <summary>The lower middle tubes cleared away.</summary>
    ClearBottomMiddleTubes,
    /// <summary>The lower left tube cleared away.</summary>
    ClearBottomLeftTube,
    /// <summary>The lower right tube cleared away.</summary>
    ClearBottomRightTube,
}
