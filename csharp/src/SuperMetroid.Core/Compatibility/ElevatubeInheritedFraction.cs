namespace SuperMetroid.Core.Compatibility;

/// <summary>
/// The Maridia elevatube's tracked height starts from the fraction of a pixel the previous room's main left (#2016,
/// #2572): the tube's doors set its starting height, speed and acceleration but never its fraction, so the ride moves from
/// whatever the room mains last left, read as a fraction, and leaves its own fraction on every update. A first ride after
/// the Ceres hall therefore starts from the hall's last countdown, and an escape room carrying on its diagonal quake reads
/// the tube's last fraction as the quake's countdown.
/// </summary>
/// <remarks>
/// Read when the elevatube's main is composed for the room, left on every update the tube moves. The value is the one the
/// room mains share (<see cref="RoomMainCarryover"/>). Evidence: the elevatube's door behaviours (<c>$8F:E26C/$E291</c>)
/// set the tube's whole height and leave the fraction in the room mains' shared word as they find it, and the tube's room
/// main moves its height in that word.
/// </remarks>
/// <param name="carryover">The value the room mains share.</param>
[CompatibilityService("the Maridia elevatube's tracked height starts from the fraction the previous room's main left and leaves its own for the next")]
public sealed partial class ElevatubeInheritedFraction(RoomMainCarryover carryover)
{
    /// <summary>The value the room mains share.</summary>
    private readonly RoomMainCarryover _carryover = Ensure.NotNull(carryover);

    /// <summary>The fraction the tube's height starts from, 0 through <see cref="FixedCoordinate.LastFraction"/>.</summary>
    public int Fraction => _carryover.Value;

    /// <summary>Leaves the tube's fraction for the next room's main.</summary>
    /// <param name="fraction">The fraction, 0 through <see cref="FixedCoordinate.LastFraction"/>.</param>
    public void Leave(int fraction) => _carryover.Leave(Ensure.BetweenInclusive(0, FixedCoordinate.LastFraction, fraction));
}
