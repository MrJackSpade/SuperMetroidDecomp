namespace SuperMetroid.Core.Game;

/// <summary>
/// Cartridge extra-property bits whose storage is proven but whose independent engine
/// behavior is not yet translated. These names describe ownership, not guessed semantics.
/// </summary>
public static class EnemyExtraPropertyRawBits
{
    /// <summary>
    /// Bit `$0400` installed by Crocomire's initializer alongside the independently proven
    /// extended-spritemap flag. No translated common handler consumes this bit yet.
    /// </summary>
    public const ushort CrocomireInitializerBit0400 = 0x0400;
}
