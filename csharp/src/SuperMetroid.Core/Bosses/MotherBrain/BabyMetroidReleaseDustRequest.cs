using SuperMetroid.Core.Samus.Projectiles;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>
/// One of the three parameter-nine dust clouds emitted
/// when the Baby releases Mother Brain's head.
/// </summary>
public readonly record struct BabyMetroidReleaseDustRequest(
    Position16<RoomPixels> Position,
    MiscDustAnimation Animation);
