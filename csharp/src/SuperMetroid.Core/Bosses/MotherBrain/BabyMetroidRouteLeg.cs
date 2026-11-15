namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>
/// One leg of the Baby Metroid's ceiling-to-Samus route.
/// </summary>
/// <param name="Target">The waypoint the Baby steers toward.</param>
/// <param name="WrongWayOffScreenXSpeed">The extra speed a reversal adds while the Baby is off screen on this leg.</param>
internal readonly record struct BabyMetroidRouteLeg(
    Position16<RoomPixels> Target,
    SignedEightEight WrongWayOffScreenXSpeed);
