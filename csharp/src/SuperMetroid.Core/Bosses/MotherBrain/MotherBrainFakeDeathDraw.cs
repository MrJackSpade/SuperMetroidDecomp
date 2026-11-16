namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>
/// What Mother Brain's fake death draws in her room, other than the rows of the mural behind her tank, which are one draw
/// of their own (<see cref="MotherBrainFakeDeathDraws.MuralRow"/>).
/// </summary>
internal enum MotherBrainFakeDeathDraw
{
    /// <summary>Fill wall.</summary>
    FillWall,
    /// <summary>Escape door.</summary>
    EscapeDoor,
    /// <summary>Clear ceiling block.</summary>
    ClearCeilingBlock,
    /// <summary>Clear ceiling tube.</summary>
    ClearCeilingTube,
    /// <summary>Clear bottom middle side tube.</summary>
    ClearBottomMiddleSideTube,
    /// <summary>Clear bottom middle tubes.</summary>
    ClearBottomMiddleTubes,
    /// <summary>Clear bottom left tube.</summary>
    ClearBottomLeftTube,
    /// <summary>Clear bottom right tube.</summary>
    ClearBottomRightTube,
}
