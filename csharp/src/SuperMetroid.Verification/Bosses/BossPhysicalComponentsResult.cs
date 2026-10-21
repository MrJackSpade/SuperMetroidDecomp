namespace SuperMetroid.Verification.Bosses;

/// <summary>The x, y and boxes of one boss physical components result.</summary>
/// <param name="X">The x.</param>
/// <param name="Y">The y.</param>
/// <param name="Boxes">The boxes.</param>
internal readonly record struct BossPhysicalComponentsResult(short X, short Y, ushort[][] Boxes);
