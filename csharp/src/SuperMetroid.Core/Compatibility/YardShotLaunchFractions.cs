namespace SuperMetroid.Core.Compatibility;

/// <summary>
/// The speed a Yard shot into the air flies at (#2184): the launch's whole pixels, a pixel an update up and a pixel an
/// update the way Samus faces, with the fractions of a pixel its speeds already held. A crawling or resting Yard holds
/// no fractions, since every landing and drop stops it; a shell shot again while still flying from an earlier shot keeps
/// what that flight's slowing and fall left in each speed, so its new flight starts a fraction of a pixel an update off
/// the launch's whole pixels and reaches each next whole pixel on the update those fractions decide.
/// </summary>
/// <remarks>
/// Consulted when a shot that is not a missile launches a Yard that is neither dropped nor kicked, during the enemy pass's
/// shot handling. It owns no state of its own: the fractions are those of the Yard's own airborne speed, so it is a rule
/// over the speed the Yard holds rather than a store. Evidence: the shot launch (<c>$A3:D557</c>) stores the whole word
/// of the vertical speed (<c>$A3:D583</c>) and of the sideways speed (<c>$A3:D595</c> or <c>$A3:D59E</c>) only, never a
/// fraction word, while the airborne movement's slowing and fall (<c>$A3:D1B3</c>) leave fractions in both speeds on
/// every update of a shot flight, so a second shot mid-flight launches with them. The Yard shot launch check shoots a
/// flying Yard through the production launch with each fraction class and both facings and requires these words.
/// </remarks>
[CompatibilityService("a Yard shot into the air takes the launch's whole pixels of speed and keeps the fractions of a pixel its flight before the shot left, which decide the updates on which its new flight reaches each whole pixel")]
internal static partial class YardShotLaunchFractions
{
    /// <summary>
    /// The speed a Yard holding <paramref name="held"/> flies at once a shot launches it at <paramref name="launch"/>'s
    /// whole pixels an update.
    /// </summary>
    /// <param name="held">The Yard's airborne speed before the shot; only its fractions are kept.</param>
    /// <param name="launch">The launch's whole pixels an update on each axis.</param>
    internal static FixedVector<RoomPixels> Launched(FixedVector<RoomPixels> held, Offset16<RoomPixels> launch) =>
        held.WithWhole(launch);
}
