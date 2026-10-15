
namespace SuperMetroid.Core.Samus.Death;

/// <summary>Cartridge values of <see cref="SamusDeathExplosionTimingDefinitions"/> that only verification reads.</summary>
internal static class SamusDeathExplosionTimingDefinitionsConstants
{
    /// <summary>
    /// First timer byte in the nine two-byte timer/palette-index records at
    /// <c>$9B:B823-$9B:B834</c>.
    /// </summary>
    public const int NativeFirstTimerAddress = 0x9bb823;
    /// <summary>Bytes occupied by each interleaved timer/palette-index record.</summary>
    public const ushort RecordByteCount = 2;

    /// <summary>Provides the death-explosion first timer address and record width for timing extraction.</summary>
    extension(SamusDeathExplosionTimingDefinitions)
    {
        /// <inheritdoc cref="SamusDeathExplosionTimingDefinitionsConstants.NativeFirstTimerAddress"/>
        internal static int NativeFirstTimerAddress => SamusDeathExplosionTimingDefinitionsConstants.NativeFirstTimerAddress;
        /// <inheritdoc cref="SamusDeathExplosionTimingDefinitionsConstants.RecordByteCount"/>
        internal static ushort RecordByteCount => SamusDeathExplosionTimingDefinitionsConstants.RecordByteCount;
    }
}
