using SuperMetroid.Core.Escape;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.World;

/// <summary>An escape room's diagonal shaking (#2287): how its shaking turns diagonal, and the countdown of the diagonal shake.</summary>
/// <param name="Shaking">The room's horizontal and diagonal shakes and the chance an update turns it diagonal.</param>
/// <param name="Countdown">The updates left of the diagonal shake, carried from the previous room's main.</param>
[RoomLifetime]
internal sealed partial record EscapeDiagonalQuake(EscapeDiagonalShaking Shaking, EscapeQuakeCarriedAcrossRooms Countdown);
