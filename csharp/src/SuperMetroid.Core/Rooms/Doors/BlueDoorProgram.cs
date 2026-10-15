namespace SuperMetroid.Core.Rooms.Doors;

/// <summary>The blue door caps' PLM programs (#1296), three per facing, in place of their lists at $84:C489..C54C.</summary>
internal enum BlueDoorProgram
{
    /// <summary>A shot opens the left-facing cap.</summary>
    LeftOpening,
    /// <summary>The left-facing cap closes behind Samus, then runs into <see cref="LeftClosed"/>.</summary>
    LeftClosing,
    /// <summary>Restores the left-facing cap's shootable BTS, draws it and deletes.</summary>
    LeftClosed,
    /// <summary>A shot opens the right-facing cap.</summary>
    RightOpening,
    /// <summary>The right-facing cap closes behind Samus.</summary>
    RightClosing,
    /// <summary>Restores the right-facing cap.</summary>
    RightClosed,
    /// <summary>A shot opens the up-facing cap.</summary>
    UpOpening,
    /// <summary>The up-facing cap closes behind Samus.</summary>
    UpClosing,
    /// <summary>Restores the up-facing cap.</summary>
    UpClosed,
    /// <summary>A shot opens the down-facing cap.</summary>
    DownOpening,
    /// <summary>The down-facing cap closes behind Samus.</summary>
    DownClosing,
    /// <summary>Restores the down-facing cap.</summary>
    DownClosed,
}
