namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>Mother Brain's twenty-two fake-death room mutation programs (#1296), in authored order.</summary>
internal enum MotherBrainFakeDeathProgram
{
    /// <summary>Fills the wall (header $B673).</summary>
    FillWall,
    /// <summary>Draws the opened escape door (header $B677).</summary>
    EscapeDoor,
    /// <summary>Installs background row two's thirteen-block layout.</summary>
    BackgroundRow2,
    /// <summary>Installs background row three's layout.</summary>
    BackgroundRow3,
    /// <summary>Installs background row four's layout.</summary>
    BackgroundRow4,
    /// <summary>Installs background row five's layout.</summary>
    BackgroundRow5,
    /// <summary>Installs background row six's layout.</summary>
    BackgroundRow6,
    /// <summary>Installs background row seven's layout.</summary>
    BackgroundRow7,
    /// <summary>Installs background row eight's layout.</summary>
    BackgroundRow8,
    /// <summary>Installs background row nine's layout.</summary>
    BackgroundRow9,
    /// <summary>Installs background row ten's layout.</summary>
    BackgroundRowA,
    /// <summary>Installs background row eleven's layout.</summary>
    BackgroundRowB,
    /// <summary>Installs background row twelve's layout.</summary>
    BackgroundRowC,
    /// <summary>Installs background row thirteen's layout.</summary>
    BackgroundRowD,
    /// <summary>The cartridge-unused background row fourteen.</summary>
    BackgroundRowE,
    /// <summary>The cartridge-unused background row fifteen.</summary>
    BackgroundRowF,
    /// <summary>Clears the ceiling block.</summary>
    ClearCeilingBlock,
    /// <summary>Clears the ceiling tube.</summary>
    ClearCeilingTube,
    /// <summary>Clears the lower middle side tube.</summary>
    ClearBottomMiddleSideTube,
    /// <summary>Clears the lower middle tubes.</summary>
    ClearBottomMiddleTubes,
    /// <summary>Clears the lower left tube.</summary>
    ClearBottomLeftTube,
    /// <summary>Clears the lower right tube.</summary>
    ClearBottomRightTube,
}
