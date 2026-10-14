using SuperMetroid.Core.Assets;

/// <summary>Cartridge values of <see cref="SmCompressionFormat"/> that only verification reads.</summary>
internal static class SmCompressionFormatConstants
{
    /// <summary>Maximum expanded byte count encoded by a long compression command.</summary>
    public const int MaximumLongLength = 1024;
    /// <summary>Maximum expanded byte count encoded by a short compression command.</summary>
    public const int MaximumShortLength = 32;

    /// <summary>Provides short and long command limits for SNES compression decoding checks.</summary>
    extension(SmCompressionFormat)
    {
        /// <inheritdoc cref="SmCompressionFormatConstants.MaximumLongLength"/>
        internal static int MaximumLongLength => SmCompressionFormatConstants.MaximumLongLength;
        /// <inheritdoc cref="SmCompressionFormatConstants.MaximumShortLength"/>
        internal static int MaximumShortLength => SmCompressionFormatConstants.MaximumShortLength;
    }
}
