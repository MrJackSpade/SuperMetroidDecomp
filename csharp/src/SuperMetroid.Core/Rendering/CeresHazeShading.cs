using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Fx.Artwork;

namespace SuperMetroid.Core.Rendering;

/// <summary>The colour each scanline of the Ceres haze gradient adds at the current fade.</summary>
internal static class CeresHazeShading
{
    /// <summary>
    /// Resolves only the cosmetic RGB amplitude. The native scanline bands and fade
    /// counter remain compiled and are shared by software and captured-PPU rendering.
    /// </summary>
    internal static RgbColor ResolveComponents(
        int screenY, int intensity, bool ridleyIsDead, RoomFxBlendInks? colors)
    {
        int component = screenY < CeresHazeRenderDefinitions.RampFirstLine
            ? CeresHazeRenderDefinitions.InitialComponent
            : Math.Min(CeresHazeRenderDefinitions.MaximumComponent,
                CeresHazeRenderDefinitions.RampFirstComponent + (screenY - CeresHazeRenderDefinitions.RampFirstLine) / CeresHazeRenderDefinitions.BandHeight);
        component = Math.Max(0, component + intensity - CeresHazeRenderDefinitions.MaximumComponent);
        RgbColor tint = colors is null
            ? RoomFxPaletteBlendRules.StockCeresHazeTint(ridleyIsDead)
            : ridleyIsDead ? colors.CeresHazeRed : colors.CeresHazeBlue;
        static int Scale(int component, int channel) =>
            Math.Min(RgbColor.MaxChannel, (component * channel + CeresHazeRenderDefinitions.MaximumComponent / 2) / CeresHazeRenderDefinitions.MaximumComponent);
        return new RgbColor(Scale(component, tint.Red), Scale(component, tint.Green), Scale(component, tint.Blue));
    }
}
