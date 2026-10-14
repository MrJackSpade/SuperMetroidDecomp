namespace SuperMetroid.Core.Game;

/// <summary>
/// Indexes into Ridley's tail-controller table at $A6:CB21 that the translated AI installs.
/// The native table's two trailing entries (7 and 8) have no translated producer.
/// </summary>
public enum RidleyTailFunction : ushort
{
    /// <summary>$A6:CB21 -> CB20, an RTS: the tail retains its offsets.</summary>
    None = 0,
    /// <summary>$A6:CB23 -> CBC0, neutral tail.</summary>
    Neutral = 1,
    /// <summary>$A6:CB25 -> CB33, immediate pogo setup.</summary>
    PogoSetup = 2,
    /// <summary>$A6:CB27 -> CB45, point tail down.</summary>
    PointDown = 3,
    /// <summary>$A6:CB29 -> CBC7, normal pogo tail.</summary>
    Pogo = 4,
    /// <summary>$A6:CB2B -> CBCE, descending stab setup.</summary>
    StabSetup = 5,
    /// <summary>$A6:CB2D -> CB4E, descending stab.</summary>
    Stab = 6,
}
