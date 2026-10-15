namespace SuperMetroid.Core.Enemies.Animals;

/// <summary>
/// Bank-$A7 function words stored in Dachora variable F. These are native addresses, not
/// host-only state IDs, so debugger watches can be compared directly with the cartridge.
/// </summary>
public enum DachoraAiFunction : ushort
{
    /// <summary><c>$A7:F570</c>: probes the floor and waits for Samus to enter the activation rectangle.</summary>
    WaitingForSamus = 0xf570,

    /// <summary><c>$A7:F5BC</c>: runs the 120-update warning countdown before installing the first run list.</summary>
    BlinkingBeforeRun = 0xf5bc,

    /// <summary><c>$A7:F5ED</c>: accelerates left along the floor until the turn coordinate or a wall.</summary>
    RunningLeft = 0xf5ed,

    /// <summary><c>$A7:F65E</c>: accelerates right until the tutorial launch coordinate or a wall.</summary>
    RunningRight = 0xf65e,

    /// <summary><c>$A7:F78F</c>: cycles the shine palette and counts down to the vertical launch.</summary>
    ChargingShinespark = 0xf78f,

    /// <summary><c>$A7:F806</c>: accelerates upward, publishes echo positions, and transitions on ceiling collision.</summary>
    Shinesparking = 0xf806,

    /// <summary><c>$A7:F935</c>: applies capped gravity until floor collision resumes the running phase.</summary>
    Falling = 0xf935,

    /// <summary><c>$A7:F98C</c>: alternates one physical echo slot's visibility for its eight-update lifetime.</summary>
    Echo = 0xf98c,
}
