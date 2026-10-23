namespace SuperMetroid.Core.Bosses.Phantoon;

/// <summary>Phantoon's death mosaic, as <c>Phantoon_DyingFadeOut</c> ($A7:DA86) grows it on the eye's mosaic word.</summary>
internal static class PhantoonDeathDefinitions
{
    /// <summary>The eye's mosaic word once the mosaic has finished growing (native -1).</summary>
    internal const ushort MosaicFinished = 0xffff;

    /// <summary>The last MOSAIC register value: the largest size, $F, applied to BG2 (bit $02).</summary>
    internal const byte FinalMosaicRegister = 0xf2;

    /// <summary>One mosaic size step in the MOSAIC register's high nibble.</summary>
    internal const byte MosaicSizeStep = 16;

    /// <summary>The eye's first death-mosaic value: the smallest size, applied to BG2 (bit $02).</summary>
    internal const byte InitialMosaic = 0x02;

    /// <summary>Bits below the mosaic size nibble in the eye's mosaic counter.</summary>
    internal const int MosaicSizeShift = 4;
}
