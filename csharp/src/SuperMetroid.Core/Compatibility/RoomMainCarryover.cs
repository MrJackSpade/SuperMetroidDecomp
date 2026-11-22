namespace SuperMetroid.Core.Compatibility;

/// <summary>
/// The one working value every room's main shares with the next room's (#1470, #2572), which room loading never clears.
/// It has no meaning of its own: each main that reads or leaves it does so through the hand-over named for what it carries,
/// so a value one main left is read in the next main's own terms (<see cref="RoomMainHandOvers"/>). The Ceres elevator
/// shaft leaves its turn step and the pre-elevator hall reads it as its first wait for falling debris
/// (<see cref="ShaftTurnSetsDebrisWait"/>); an escape room's diagonal quake carries on what the previous room's main left
/// (<see cref="EscapeQuakeCarriedAcrossRooms"/>); the Maridia elevatube adopts the fraction it finds as its tracked
/// height's (<see cref="ElevatubeInheritedFraction"/>). Because the three share this one value, whichever main ran last
/// decides what the next reads: the elevatube's first ride starts from the fraction the Ceres hall's countdown left, and
/// an escape room whose setup does not clear its quake carries on the elevatube's last fraction as its countdown.
/// </summary>
/// <remarks>
/// Read and left only by the hand-overs above (SME6466), when a room main is set up for a room and whenever a main
/// changes what it carries. Lifetime: the session's, held by the active room for every room it loads. Evidence: the
/// original game's room mains share one working word that room loading never clears; the Ceres pre-elevator hall's debris
/// timer (<c>$8F:E525</c>) starts from the shaft's turn index (<c>$89:AD41</c>), the escape rooms' diagonal quake timer
/// is the same word, and the elevatube's door behaviours (<c>$8F:E26C/$E291</c>) leave its fraction as they find it.
/// </remarks>
[CompatibilityService("the room mains share one working value no room load clears, so each main that does not set it up reads, in its own terms, what the previous room's main left")]
[EncodingOwner("room main working value carried between rooms")]
public sealed partial class RoomMainCarryover
{
    /// <summary>The value the last main left, 0 through 65535.</summary>
    public int Value { get; private set; }

    /// <summary>Leaves <paramref name="value"/> for the next main that reads the shared value.</summary>
    /// <param name="value">The value, 0 through 65535.</param>
    public void Leave(int value) => Value = Ensure.BetweenInclusive(0, ushort.MaxValue, value);
}
