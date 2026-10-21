namespace SuperMetroid.Verification.Bosses.Draygon;

/// <summary>The x, xsub, y, ysub and angle of one spawn goop result.</summary>
/// <param name="X">The x.</param>
/// <param name="XSub">The xsub.</param>
/// <param name="Y">The y.</param>
/// <param name="YSub">The ysub.</param>
/// <param name="Angle">The angle.</param>
internal readonly record struct SpawnGoopResult(ushort X, ushort XSub, ushort Y, ushort YSub, ushort Angle);
