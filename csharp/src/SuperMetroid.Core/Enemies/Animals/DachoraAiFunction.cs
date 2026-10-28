namespace SuperMetroid.Core.Enemies.Animals;

/// <summary>
/// The routine Dachora's main AI runs each frame.
/// Its values are arbitrary; verification's native view names each member's cartridge routine (#1412).
/// </summary>
public enum DachoraAiFunction
{
    /// <summary><c>$A7:F570</c>: probes the floor and waits for Samus to enter the activation rectangle.</summary>
    WaitingForSamus = 1,

    /// <summary><c>$A7:F5BC</c>: runs the 120-update warning countdown before installing the first run list.</summary>
    BlinkingBeforeRun,

    /// <summary><c>$A7:F5ED</c>: accelerates left along the floor until the turn coordinate or a wall.</summary>
    RunningLeft,

    /// <summary><c>$A7:F65E</c>: accelerates right until the tutorial launch coordinate or a wall.</summary>
    RunningRight,

    /// <summary><c>$A7:F78F</c>: cycles the shine palette and counts down to the vertical launch.</summary>
    ChargingShinespark,

    /// <summary><c>$A7:F806</c>: accelerates upward, publishes echo positions, and transitions on ceiling collision.</summary>
    Shinesparking,

    /// <summary><c>$A7:F935</c>: applies capped gravity until floor collision resumes the running phase.</summary>
    Falling,

    /// <summary><c>$A7:F98C</c>: alternates one physical echo slot's visibility for its eight-update lifetime.</summary>
    Echo,
}
