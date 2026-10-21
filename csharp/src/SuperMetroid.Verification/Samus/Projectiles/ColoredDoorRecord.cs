namespace SuperMetroid.Verification.Samus.Projectiles;

/// <summary>The header, x, y and argument of one colored door record.</summary>
/// <param name="Header">The header.</param>
/// <param name="X">The x.</param>
/// <param name="Y">The y.</param>
/// <param name="Argument">The argument.</param>
internal readonly record struct ColoredDoorRecord(ushort Header, byte X, byte Y, ushort Argument);
