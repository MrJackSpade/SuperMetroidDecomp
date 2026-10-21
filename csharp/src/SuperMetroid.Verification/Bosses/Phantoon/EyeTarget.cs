namespace SuperMetroid.Verification.Bosses.Phantoon;

/// <summary>The x, y and direction of one eye target.</summary>
/// <param name="X">The x.</param>
/// <param name="Y">The y.</param>
/// <param name="Direction">The direction.</param>
internal readonly record struct EyeTarget(short X, short Y, ushort Direction);
