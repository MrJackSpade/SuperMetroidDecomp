namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>
/// One leg of the Baby Metroid's ceiling-to-Samus route (<c>$A9:CA24-$CA63</c>).
/// </summary>
/// <param name="Target">The waypoint the Baby steers toward.</param>
/// <param name="AccelerationDivisorIndex">Index 0 through 15 into the gradual-acceleration divisor table.</param>
/// <param name="MovementFunction">The steering routine, which selects the wrong-way correction.</param>
/// <param name="EndsWithLatchOntoSamus">
/// Whether reaching this waypoint ends the route and installs the latch-onto-Samus function,
/// rather than moving on to the next leg.
/// </param>
internal readonly record struct BabyMetroidRouteRecord(
    Position16<RoomPixels> Target,
    int AccelerationDivisorIndex,
    BabyMetroidRouteMovementFunction MovementFunction,
    bool EndsWithLatchOntoSamus);
