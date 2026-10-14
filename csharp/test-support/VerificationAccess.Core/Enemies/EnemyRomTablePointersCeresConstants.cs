
namespace SuperMetroid.Core.Enemies;

/// <summary>Cartridge values of <see cref="EnemyRomTablePointers.Ceres"/> that only verification reads.</summary>
internal static class EnemyRomTablePointersCeresConstants
{
    /// <summary>Two Ceres door VRAM-transfer list pointers at $A6:F900 (4 bytes).</summary>
    public const int DoorTransferPointers = 0xa6f900;
    /// <summary>Ridley rotation divisors at $A6:D712.</summary>
    public const int RidleyRotationDivisorBytes = 0xa6d712;
    /// <summary>Ridley tail-tip spritemap pointers at $A6:DCBA.</summary>
    public const int TailTipSpritemapPointers = 0xa6dcba;
    /// <summary>Ridley wing spritemap pointers at $A6:DB02.</summary>
    public const int WingSpritemapPointers = 0xa6db02;

    /// <summary>Provides Ceres enemy door, Ridley rotation, tail tip, and wing table pointers for native comparison.</summary>
    extension(EnemyRomTablePointers.Ceres)
    {
        /// <inheritdoc cref="EnemyRomTablePointersCeresConstants.DoorTransferPointers"/>
        internal static int DoorTransferPointers => EnemyRomTablePointersCeresConstants.DoorTransferPointers;
        /// <inheritdoc cref="EnemyRomTablePointersCeresConstants.RidleyRotationDivisorBytes"/>
        internal static int RidleyRotationDivisorBytes => EnemyRomTablePointersCeresConstants.RidleyRotationDivisorBytes;
        /// <inheritdoc cref="EnemyRomTablePointersCeresConstants.TailTipSpritemapPointers"/>
        internal static int TailTipSpritemapPointers => EnemyRomTablePointersCeresConstants.TailTipSpritemapPointers;
        /// <inheritdoc cref="EnemyRomTablePointersCeresConstants.WingSpritemapPointers"/>
        internal static int WingSpritemapPointers => EnemyRomTablePointersCeresConstants.WingSpritemapPointers;
    }
}
