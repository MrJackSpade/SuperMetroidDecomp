
namespace SuperMetroid.Core.Enemies.Zoa;

/// <summary>Cartridge values of <see cref="ZoaAnimationDefinitions"/> that only verification reads.</summary>
internal static class ZoaAnimationDefinitionsConstants
{
    /// <summary>
    /// Left-shooting, left-rising, right-shooting, and right-rising instruction lists at
    /// <c>$A3:B40D-$A3:B414</c>, indexed by <see cref="ZoaAnimationSelector"/>.
    /// </summary>
    public const int ReferenceAddress = 0xa3b40d;

    /// <summary>Provides the native Zoa animation reference address for movement presentation checks.</summary>
    extension(ZoaAnimationDefinitions)
    {
        /// <inheritdoc cref="ZoaAnimationDefinitionsConstants.ReferenceAddress"/>
        internal static int ReferenceAddress => ZoaAnimationDefinitionsConstants.ReferenceAddress;
    }
}
