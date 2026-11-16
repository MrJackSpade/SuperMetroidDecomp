namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>
/// The changes Mother Brain's fight and death make to her room: the wall, the escape door, the rows of the mural behind
/// her tank that the ascent redraws and the tubes the fake death collapses. These are the only spawns her room spawner performs.
/// </summary>
public enum MotherBrainRoomMutation
{
    /// <summary>Fills the wall.</summary>
    FillWall,
    /// <summary>Opens the escape door.</summary>
    EscapeDoor,
    /// <summary>
    /// Reveals the row of the mural behind her tank that lies at its block (<see cref="MotherBrainMuralDefinitions"/>).
    /// </summary>
    RevealMuralRow,
    /// <summary>Clears a ceiling block.</summary>
    ClearCeilingBlock,
    /// <summary>Clears a ceiling tube.</summary>
    ClearCeilingTube,
    /// <summary>Clears a lower middle side tube.</summary>
    ClearBottomMiddleSideTube,
    /// <summary>Clears the lower middle tubes.</summary>
    ClearBottomMiddleTubes,
    /// <summary>Clears the lower left tube.</summary>
    ClearBottomLeftTube,
    /// <summary>Clears the lower right tube.</summary>
    ClearBottomRightTube,
}
