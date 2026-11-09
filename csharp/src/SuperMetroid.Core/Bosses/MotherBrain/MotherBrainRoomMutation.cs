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
    /// <summary>Draws the first row of the mural behind her tank.</summary>
    MuralRow1,
    /// <summary>Draws the second row of the mural behind her tank.</summary>
    MuralRow2,
    /// <summary>Draws the third row of the mural behind her tank.</summary>
    MuralRow3,
    /// <summary>Draws the fourth row of the mural behind her tank.</summary>
    MuralRow4,
    /// <summary>Draws the fifth row of the mural behind her tank.</summary>
    MuralRow5,
    /// <summary>Draws the sixth row of the mural behind her tank.</summary>
    MuralRow6,
    /// <summary>Draws the seventh row of the mural behind her tank.</summary>
    MuralRow7,
    /// <summary>Draws the eighth row of the mural behind her tank.</summary>
    MuralRow8,
    /// <summary>Draws the ninth row of the mural behind her tank.</summary>
    MuralRow9,
    /// <summary>Draws the tenth row of the mural behind her tank.</summary>
    MuralRow10,
    /// <summary>Draws the eleventh row of the mural behind her tank.</summary>
    MuralRow11,
    /// <summary>Draws the twelfth row of the mural behind her tank.</summary>
    MuralRow12,
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
