namespace SuperMetroid.Core.Enemies.FuneNamihe;

/// <summary>Fune's and Namihe's parameter-one fields.</summary>
internal static class FuneNamiheParameterMasks
{
    /// <summary>The low nibble: the variant.</summary>
    internal const ushort VariantBits = 0x000f;

    /// <summary>The low byte's upper nibble: any bit set selects right-facing art.</summary>
    internal const ushort RightFacingBits = 0x00f0;
}
