namespace SuperMetroid.Core.Game;

/// <summary>
/// One of the three parameter-nine `$86:E509` dust clouds emitted by
/// <c>$A9:C98C-C9C2</c> when the Baby releases Mother Brain's head.
/// </summary>
public readonly record struct BabyMetroidReleaseDustRequest(
    Position16<RoomPixels> Position,
    MiscDustAnimation Animation);
