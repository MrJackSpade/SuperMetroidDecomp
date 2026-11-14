using SuperMetroid.Core.Samus.Movement;

namespace SuperMetroid.TestSupport.SamusNativeCodecs;

/// <summary>
/// Converts the medium whose physics Samus moves by to and from the native word the cartridge keeps it in, and to its
/// place in the cartridge's per-medium speed tables, for native traces, snapshot imports and comparisons (#1857).
/// </summary>
internal static class SamusLiquidMediumCodec
{
    /// <summary>The native word a medium is stored as, which is also its place in each per-medium table.</summary>
    internal static ushort ToNative(SamusLiquidMedium medium) => medium switch
    {
        SamusLiquidMedium.Air => 0,
        SamusLiquidMedium.Water => 1,
        SamusLiquidMedium.LavaOrAcid => 2,
        _ => throw new InvalidOperationException($"Undefined {nameof(SamusLiquidMedium)} {medium}."),
    };

    /// <summary>The medium a native word holds.</summary>
    /// <exception cref="InvalidDataException">The word is none of the three media the cartridge writes.</exception>
    internal static SamusLiquidMedium FromNative(int word) => word switch
    {
        0 => SamusLiquidMedium.Air,
        1 => SamusLiquidMedium.Water,
        2 => SamusLiquidMedium.LavaOrAcid,
        _ => throw new InvalidDataException($"Liquid physics word {word:X4} is none of the three media the cartridge writes."),
    };
}
