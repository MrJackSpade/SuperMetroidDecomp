namespace SuperMetroid.Core.Samus.Death;

/// <summary>
/// Cartridge places of the suit explosion's timing that only verification reads; the port plays the explosion as an
/// animation of its pictures (#2388, <see cref="SamusDeathAnimations"/>).
/// </summary>
internal static class SamusDeathExplosionTimingDefinitionsConstants
{
    /// <summary>
    /// First timer byte in the nine two-byte timer/palette-index records at
    /// <c>$9B:B823-$9B:B834</c>.
    /// </summary>
    public const int NativeFirstTimerAddress = 0x9bb823;

    /// <summary>Bytes occupied by each interleaved timer/palette-index record.</summary>
    public const ushort RecordByteCount = 2;

    /// <summary>The native timer/palette-index records, one per explosion picture.</summary>
    public const int RecordCount = 9;
}
