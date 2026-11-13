namespace SuperMetroid.Core.Bosses.Ridley;

/// <summary>
/// The tail controllers Ridley's AI installs.
/// The table's two trailing entries (7 and 8) have no producer.
/// </summary>
public enum RidleyTailFunction
{
    /// <summary>An empty function: the tail retains its offsets.</summary>
    None = 0,
    /// <summary>The neutral tail.</summary>
    Neutral = 1,
    /// <summary>The immediate pogo setup.</summary>
    PogoSetup = 2,
    /// <summary>Points the tail down.</summary>
    PointDown = 3,
    /// <summary>The normal pogo tail.</summary>
    Pogo = 4,
    /// <summary>The descending stab setup.</summary>
    StabSetup = 5,
    /// <summary>The descending stab.</summary>
    Stab = 6,
}
