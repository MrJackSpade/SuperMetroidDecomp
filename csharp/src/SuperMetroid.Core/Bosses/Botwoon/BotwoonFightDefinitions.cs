namespace SuperMetroid.Core.Bosses.Botwoon;

/// <summary>Botwoon's fight: its opening delay, its spit volleys and cooldown, the floor he dies on and his head's trail.</summary>
internal static class BotwoonFightDefinitions
{
    /// <summary>The segments of his body, from the tail to the segment behind his head.</summary>
    internal const int BodySegments = 13;

    /// <summary>The updates Botwoon waits in his hole before his first traversal.</summary>
    internal const int OpeningDelay = 256;

    /// <summary>
    /// The volley he spits from a hole once his spitting animation reaches its spit: five spits, fanned from two spreads one
    /// way of his aim to two the other, after which his head cools down for 48 updates and he leaves the hole on the next.
    /// </summary>
    internal static BotwoonSpitVolley AimedVolley { get; } = new(Count: 5, FirstOffsetSteps: -32, CooldownUpdates: 48);

    /// <summary>
    /// The volley he spits at once when he aims outside his hole spitting phase: three spits, fanned from one spread one way
    /// of his aim to one the other, after which his head follows his movement again on the next update.
    /// </summary>
    internal static BotwoonSpitVolley ImmediateVolley { get; } = new(Count: 3, FirstOffsetSteps: -16, CooldownUpdates: 1);

    /// <summary>The turn, in steps, between neighbouring spits of a volley.</summary>
    internal const int VolleySpreadSteps = 16;

    /// <summary>The floor row his falling head and body segments land on as he dies.</summary>
    internal const int FloorY = 200;

    /// <summary>Botwoon speeds up once his health falls below his starting health divided by this (#2182).</summary>
    internal const int FasterBelowShareOfHealth = 2;

    /// <summary>Botwoon speeds up again once his health falls below his starting health divided by this (#2182).</summary>
    internal const int FastestBelowShareOfHealth = 4;

    /// <summary>
    /// How many of his head's last positions his head keeps, newest first: the oldest, this many updates back, chooses which
    /// way his head faces.
    /// </summary>
    internal const int HeadHistoryLength = 4;

    /// <summary>His head's collision half-extents facing up or down: eight pixels across, sixteen tall.</summary>
    internal static Extent16<RoomPixels> VerticalHeadRadius { get; } = new(8, 16);

    /// <summary>His head's collision half-extents facing left or right: sixteen pixels across, eight tall.</summary>
    internal static Extent16<RoomPixels> HorizontalHeadRadius { get; } = new(16, 8);

    /// <summary>His head's collision half-extents facing a diagonal: twelve pixels each way.</summary>
    internal static Extent16<RoomPixels> DiagonalHeadRadius { get; } = new(12, 12);
}
