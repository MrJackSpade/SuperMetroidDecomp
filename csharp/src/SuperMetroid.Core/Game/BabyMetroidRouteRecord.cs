namespace SuperMetroid.Core.Game;

/// <summary>
/// One compiled <c>$A9:CA24-$CA63</c> Baby Metroid ceiling-to-Samus route record.
/// </summary>
internal readonly record struct BabyMetroidRouteRecord(
    Position16<RoomPixels> Target,
    ushort AccelerationDivisorIndex,
    BabyMetroidRouteMovementFunction MovementFunction,
    ushort FollowingWord);
