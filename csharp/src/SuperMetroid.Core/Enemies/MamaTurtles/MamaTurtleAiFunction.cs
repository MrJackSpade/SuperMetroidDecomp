namespace SuperMetroid.Core.Enemies.MamaTurtles;

/// <summary>
/// Indirect function pointers stored in Mama Turtle's ordinary enemy variable A. Keeping the
/// literal bank-$A2 offsets makes debugger watches directly comparable with the cartridge.
/// </summary>
public enum MamaTurtleAiFunction : ushort
{
    /// <summary>$A2:8DD8, links the four following Baby Turtle slots to their parent.</summary>
    Initial = 0x8dd8,
    /// <summary>$A2:8E09, inert state used after parent/child setup.</summary>
    Idle = 0x8e09,
    /// <summary>$A2:8E0A, waits asleep until the shared wake counter is cleared.</summary>
    Asleep = 0x8e0a,
    /// <summary>$A2:8EE0, moves Samus along the shell while Mama Turtle emerges.</summary>
    LeavingShell = 0x8ee0,
    /// <summary>$A2:8F3F, returns Mama Turtle and a carried Samus into the shell.</summary>
    EnteringShell = 0x8f3f,
    /// <summary>$A2:8F8D, rises from the shell toward the hovering height.</summary>
    RisingToHover = 0x8f8d,
    /// <summary>$A2:8FEB, hovers while updating shell collision and rider displacement.</summary>
    Hovering = 0x8feb,
    /// <summary>$A2:9083, accelerates upward toward the authored peak position.</summary>
    RisingToPeak = 0x9083,
    /// <summary>$A2:90CC, pauses at the peak before falling.</summary>
    HoveringAtPeak = 0x90cc,
    /// <summary>$A2:90E1, falls back toward the shell with a capped velocity.</summary>
    Falling = 0x90e1,
}
