namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>
/// One leg of the Baby Metroid's ceiling-to-Samus route.
/// </summary>
/// <param name="Target">The waypoint the Baby steers toward.</param>
/// <param name="AccelerationDivisor">What the gradual acceleration divides the remaining distance by, 1 through 16.</param>
/// <param name="Movement">The steering behaviour, which selects the wrong-way correction.</param>
/// <param name="EndsWithLatchOntoSamus">
/// Whether reaching this waypoint ends the route and starts the latch-onto-Samus phase,
/// rather than moving on to the next leg.
/// </param>
internal readonly record struct BabyMetroidRouteRecord(
    Position16<RoomPixels> Target,
    int AccelerationDivisor,
    BabyMetroidRouteMovement Movement,
    bool EndsWithLatchOntoSamus);
