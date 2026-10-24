namespace SuperMetroid.Core.Enemies.Corpses;

/// <summary>The dead Torizo's asymmetric touch rectangles, which a contact against its corpse checks in native order.</summary>
internal static class DeadTorizoTouchDefinitions
{
    /// <summary>Dead Torizo's seven asymmetric touch rectangles at $A9:D77C..D7B5.</summary>
    private static readonly EnemyHitboxRectangle[] TouchHitboxRecords =
    [
        new(Left: -31, Top: 37, Right: -11, Bottom: 43),
        new(Left: 16, Top: 37, Right: 38, Bottom: 43),
        new(Left: -24, Top: 18, Right: -13, Bottom: 36),
        new(Left: 11, Top: 26, Right: 25, Bottom: 36),
        new(Left: -10, Top: -30, Right: 16, Bottom: 24),
        new(Left: -7, Top: -44, Right: 34, Bottom: -31),
        new(Left: 17, Top: -31, Right: 40, Bottom: -7),
    ];

    /// <summary>Dead Torizo's seven asymmetric corpse touch rectangles in native order.</summary>
    internal static ReadOnlySpan<EnemyHitboxRectangle> TouchHitboxes => TouchHitboxRecords;
}
