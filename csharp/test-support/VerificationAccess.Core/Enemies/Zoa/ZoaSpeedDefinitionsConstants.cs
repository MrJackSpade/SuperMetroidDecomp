
namespace SuperMetroid.Core.Enemies.Zoa;

/// <summary>Cartridge values of <see cref="ZoaSpeedDefinitions"/> that only verification reads.</summary>
internal static class ZoaSpeedDefinitionsConstants
{
    /// <summary>$A3:B415, ZoaXSpeedTable: five whole/fraction records, including the trailing zero record.</summary>
    public const int ReferenceAddress = 0xa3b415;

    /// <summary>Provides the native Zoa speed reference address for movement comparisons.</summary>
    extension(ZoaSpeedDefinitions)
    {
        /// <inheritdoc cref="ZoaSpeedDefinitionsConstants.ReferenceAddress"/>
        internal static int ReferenceAddress => ZoaSpeedDefinitionsConstants.ReferenceAddress;
    }
}
