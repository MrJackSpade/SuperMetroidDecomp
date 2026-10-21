namespace SuperMetroid.Verification.Rooms.Scrolling;

/// <summary>The center y, radius and should wake of one verify room scroll plms case.</summary>
/// <param name="CenterY">The center y.</param>
/// <param name="Radius">The radius.</param>
/// <param name="ShouldWake">The should wake.</param>
internal readonly record struct VerifyRoomScrollPlmsCase(ushort CenterY, ushort Radius, bool ShouldWake);
