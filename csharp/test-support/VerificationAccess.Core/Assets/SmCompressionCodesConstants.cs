namespace SuperMetroid.Core.Assets;
using SuperMetroid.AssetExtraction.Compression;

/// <summary>Cartridge values of <see cref="SmCompressionCodes"/> that only verification reads.</summary>
internal static class SmCompressionCodesConstants
{
    /// <summary>Maximum expanded byte count encoded by a long compression command.</summary>
    public const int MaximumLongLength = 1024;
    /// <summary>Maximum expanded byte count encoded by a short compression command.</summary>
    public const int MaximumShortLength = 32;

    /// <summary>Provides short and long command limits for SNES compression decoding checks.</summary>
    extension(SmCompressionCodes)
    {
        /// <inheritdoc cref="SmCompressionCodesConstants.MaximumLongLength"/>
        internal static int MaximumLongLength => SmCompressionCodesConstants.MaximumLongLength;
        /// <inheritdoc cref="SmCompressionCodesConstants.MaximumShortLength"/>
        internal static int MaximumShortLength => SmCompressionCodesConstants.MaximumShortLength;
    }
}
