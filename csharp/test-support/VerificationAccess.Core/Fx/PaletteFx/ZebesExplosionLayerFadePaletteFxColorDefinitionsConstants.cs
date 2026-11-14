using SuperMetroid.AssetExtraction.Fx.PaletteFx;

namespace SuperMetroid.Core.Fx.ColourEffects;

/// <summary>Cartridge values and helpers of <see cref="ZebesExplosionLayerFadePaletteFxColorDefinitions"/> that only verification reads.</summary>
internal static class ZebesExplosionLayerFadePaletteFxColorDefinitionsConstants
{
    /// <summary>Both one-shot fades contain eight timed records.</summary>
    public static int FrameCount => ZebesExplosionLayerFadePaletteFxColorDefinitions.All[0].ColorFrames.FrameCount;
}
