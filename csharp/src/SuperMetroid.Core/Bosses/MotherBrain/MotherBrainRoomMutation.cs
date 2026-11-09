namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>
/// The changes Mother Brain's fight and death make to her room: the wall, the escape door, the background rows the
/// ascent redraws and the tubes the fake death collapses. These are the only spawns her room spawner performs.
/// </summary>
public enum MotherBrainRoomMutation
{
    /// <summary>Fills the wall.</summary>
    FillWall,
    /// <summary>Opens the escape door.</summary>
    EscapeDoor,
    /// <summary>Installs background row two.</summary>
    BackgroundRow2,
    /// <summary>Installs background row three.</summary>
    BackgroundRow3,
    /// <summary>Installs background row four.</summary>
    BackgroundRow4,
    /// <summary>Installs background row five.</summary>
    BackgroundRow5,
    /// <summary>Installs background row six.</summary>
    BackgroundRow6,
    /// <summary>Installs background row seven.</summary>
    BackgroundRow7,
    /// <summary>Installs background row eight.</summary>
    BackgroundRow8,
    /// <summary>Installs background row nine.</summary>
    BackgroundRow9,
    /// <summary>Installs background row ten.</summary>
    BackgroundRow10,
    /// <summary>Installs background row eleven.</summary>
    BackgroundRow11,
    /// <summary>Installs background row twelve.</summary>
    BackgroundRow12,
    /// <summary>Installs background row thirteen.</summary>
    BackgroundRow13,
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
