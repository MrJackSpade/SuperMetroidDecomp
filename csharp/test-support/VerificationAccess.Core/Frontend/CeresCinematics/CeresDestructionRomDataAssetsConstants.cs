namespace SuperMetroid.Core.Frontend.CeresCinematics;

/// <summary>Cartridge values of <see cref="CeresDestructionRomData.Assets"/> that only verification reads.</summary>
internal static class CeresDestructionRomDataAssetsConstants
{
    /// <summary>Bank-$9A source of the shared cinematic object character tiles at $9A:D200.</summary>
    public const int SharedObjectCharacters = 0x9ad200;

    /// <summary>Provides the shared Ceres object character source used to check the destruction asset import.</summary>
    extension(CeresDestructionRomData.Assets)
    {
        /// <inheritdoc cref="CeresDestructionRomDataAssetsConstants.SharedObjectCharacters"/>
        internal static int SharedObjectCharacters => CeresDestructionRomDataAssetsConstants.SharedObjectCharacters;
    }
}
