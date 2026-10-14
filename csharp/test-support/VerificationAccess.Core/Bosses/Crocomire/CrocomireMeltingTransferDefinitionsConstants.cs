
namespace SuperMetroid.Core.Bosses.Crocomire;

/// <summary>Cartridge values of <see cref="CrocomireMeltingTransferDefinitions"/> that only verification reads.</summary>
internal static class CrocomireMeltingTransferDefinitionsConstants
{
    /// <summary>Exclusive end of the two native records.</summary>
    public const int NativeByteCount = 0x00b4;
    /// <summary>Native bank-$A4 base for independent ROM-oracle comparison.</summary>
    public const int NativeSourceAddress = 0xa49bc5;

    /// <summary>Provides the source address and byte count for checking Crocomire melting transfers.</summary>
    extension(CrocomireMeltingTransferDefinitions)
    {
        /// <inheritdoc cref="CrocomireMeltingTransferDefinitionsConstants.NativeByteCount"/>
        internal static int NativeByteCount => CrocomireMeltingTransferDefinitionsConstants.NativeByteCount;
        /// <inheritdoc cref="CrocomireMeltingTransferDefinitionsConstants.NativeSourceAddress"/>
        internal static int NativeSourceAddress => CrocomireMeltingTransferDefinitionsConstants.NativeSourceAddress;
    }
}
