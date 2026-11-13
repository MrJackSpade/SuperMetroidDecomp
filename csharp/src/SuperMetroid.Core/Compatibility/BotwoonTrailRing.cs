namespace SuperMetroid.Core.Compatibility;

/// <summary>
/// Botwoon's body trails his head through a ring of the head's last 256 moving positions (#1627): each update he moves
/// writes the head's position at the ring's next place, and each body segment stands where the head stood a whole number
/// of places back. A hole crossing is remembered by the place the head crossed at, and a segment toggles its hole state
/// on reaching that place; the place recurs every 256 moving updates, so a segment a spacing change carried past it meets
/// it again only when the ring comes round, and a segment placed before the head has moved that far stands where the
/// cleared ring leaves it.
/// </summary>
/// <remarks>
/// Consulted on every update Botwoon moves: as the head's position is written, as each segment is placed and as each
/// segment's place is compared with the remembered crossing. Evidence: the original keeps the head's positions in a table
/// of 256 entries whose cursor it advances and steps back within the table, cleared when the room's enemies load, and
/// compares the remembered crossing's cursor with each segment's with nothing counting the ring's turns, so the match
/// recurs with the ring's period.
/// </remarks>
[CompatibilityService("Botwoon's body trails his head through a ring of 256 moving positions, so a remembered hole crossing recurs at the same place every 256 moving updates")]
public static class BotwoonTrailRing
{
    /// <summary>The ring's places: the head's last 256 moving positions.</summary>
    public const int Places = 256;

    /// <summary>
    /// The ring place <paramref name="place"/> lands on: counting on past the ring's last place continues from its first,
    /// and counting back past its first continues from its last.
    /// </summary>
    /// <param name="place">A place counted on or back from the ring's first, any distance.</param>
    public static int PlaceAt(int place) => ((place % Places) + Places) % Places;
}
