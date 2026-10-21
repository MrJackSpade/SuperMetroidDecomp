namespace SuperMetroid.Verification.Bosses.Draygon;

/// <summary>The power bombed, goop x and samus x of one verify draygon goop touch order case.</summary>
/// <param name="PowerBombed">The power bombed.</param>
/// <param name="GoopX">The goop x.</param>
/// <param name="SamusX">The samus x.</param>
internal readonly record struct VerifyDraygonGoopTouchOrderCase(bool PowerBombed, ushort GoopX, ushort SamusX);
