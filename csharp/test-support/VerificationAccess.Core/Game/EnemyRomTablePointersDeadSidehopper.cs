namespace SuperMetroid.Core.Game;

/// <summary>Dead Sidehopper launch tables.</summary>
internal static class EnemyRomTablePointersDeadSidehopper
{
    /// <summary>Four signed vertical velocity words at $A9:D951 (8 bytes).</summary>
    public const int VerticalVelocityWords = 0xa9d951;
    /// <summary>Four signed horizontal velocity words at $A9:D959 (8 bytes).</summary>
    public const int HorizontalVelocityWords = 0xa9d959;
}
