using SuperMetroid.Core.Game;

/// <summary>Cartridge values of <see cref="EnemyAiCodePointers.BankB2"/> that only verification reads.</summary>
internal static class EnemyAiCodePointersBankB2Constants
{
    /// <summary>ExtendedSpritemap_CommonB2_Nothing at $B2:804F; one point hitbox selecting the common touch/shot callbacks.</summary>
    public const ushort EmptyExtendedSpritemap = 0x804f;

    /// <summary>Provides the empty extended spritemap pointer in enemy AI bank B2 for frame checks.</summary>
    extension(EnemyAiCodePointers.BankB2)
    {
        /// <inheritdoc cref="EnemyAiCodePointersBankB2Constants.EmptyExtendedSpritemap"/>
        internal static ushort EmptyExtendedSpritemap => EnemyAiCodePointersBankB2Constants.EmptyExtendedSpritemap;
    }
}
