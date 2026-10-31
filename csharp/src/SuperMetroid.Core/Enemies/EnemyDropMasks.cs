namespace SuperMetroid.Core.Enemies;

/// <summary>
/// Drop availability bits, one per drop column in the enemy's chance table (small energy, big energy, missile, nothing,
/// Super Missile, Power Bomb), and the random selections that scatter a multi-drop's positions.
/// </summary>
internal static class EnemyDropMasks
{
    /// <summary>Both energy drops: small (bit 0) and big (bit 1).</summary>
    internal const int Energy = 0b11;

    /// <summary>The missile drop.</summary>
    internal const int Missile = 1 << 2;

    /// <summary>The "nothing" drop.</summary>
    internal const int Nothing = 1 << 3;

    /// <summary>The Super Missile drop.</summary>
    internal const int SuperMissile = 1 << 4;

    /// <summary>The Power Bomb drop.</summary>
    internal const int PowerBomb = 1 << 5;

    /// <summary>The random selection that scatters each drop 0-31 pixels horizontally before the 16-pixel centring.</summary>
    internal static readonly RandomSampleField HorizontalScatter = new(Shift: 0, Width: 5);

    /// <summary>The random selection that scatters each drop 0-31 pixels vertically before the 16-pixel centring.</summary>
    internal static readonly RandomSampleField VerticalScatter = new(Shift: 8, Width: 5);
}
