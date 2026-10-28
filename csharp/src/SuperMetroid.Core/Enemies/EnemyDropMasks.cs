namespace SuperMetroid.Core.Enemies;

/// <summary>
/// Drop availability bits, one per drop column in the enemy's chance table (small energy, big energy, missile, nothing,
/// Super Missile, Power Bomb), and the random selections that scatter a multi-drop's positions.
/// </summary>
internal static class EnemyDropMasks
{
    /// <summary>Both energy drops: small (bit 0) and big (bit 1).</summary>
    internal const byte Energy = 0x03;

    /// <summary>The missile drop.</summary>
    internal const byte Missile = 0x04;

    /// <summary>The "nothing" drop.</summary>
    internal const byte Nothing = 0x08;

    /// <summary>The Super Missile drop.</summary>
    internal const byte SuperMissile = 0x10;

    /// <summary>The Power Bomb drop.</summary>
    internal const byte PowerBomb = 0x20;

    /// <summary>The random selection that scatters each drop 0-31 pixels horizontally before the 16-pixel centring.</summary>
    internal static readonly RandomSampleField HorizontalScatter = new(Shift: 0, Width: 5);

    /// <summary>The random selection that scatters each drop 0-31 pixels vertically before the 16-pixel centring.</summary>
    internal static readonly RandomSampleField VerticalScatter = new(Shift: 8, Width: 5);
}
