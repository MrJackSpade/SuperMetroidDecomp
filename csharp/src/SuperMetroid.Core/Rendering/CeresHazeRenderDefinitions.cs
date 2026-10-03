using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;

namespace SuperMetroid.Core.Rendering;

/// <summary>Expanded scanline layout of the bank-$88 FX type $2C fixed-color gradient.</summary>
internal static class CeresHazeRenderDefinitions
{
    /// <summary>Physical line 64 starts the increasing color bands after the initial flat region.</summary>
    internal const int RampFirstLine = 64;
    /// <summary>$88:DF03 begins with 64 scanlines reading table byte zero: component zero when fully faded in.</summary>
    internal const int InitialComponent = 0;
    /// <summary>The next eight scanlines read table byte one: component one when fully faded in.</summary>
    internal const int RampFirstComponent = 1;
    /// <summary>Each subsequent HDMA color band spans eight physical scanlines.</summary>
    internal const int BandHeight = 8;
    /// <summary>$88:DE2D's final table write uses counter fifteen; the following call only changes pre-instruction.</summary>
    internal const int MaximumComponent = CeresHazeDefinitions.FadeSteps - 1;

    /// <summary>
    /// Resolves only the cosmetic RGB amplitude. The native scanline bands and fade
    /// counter remain compiled and are shared by software and captured-PPU rendering.
    /// </summary>
    internal static (byte Red, byte Green, byte Blue) ResolveComponents(
        int screenY, int intensity, bool ridleyIsDead, RoomFxPaletteBlendCatalog? colors)
    {
        int component = screenY < RampFirstLine
            ? InitialComponent
            : Math.Min(MaximumComponent,
                RampFirstComponent + (screenY - RampFirstLine) / BandHeight);
        component = Math.Max(0, component + intensity - MaximumComponent);
        PaletteRgb5 tint = ridleyIsDead
            ? colors?.CeresHazeRed ?? RoomFxPaletteBlendDefinitions.StockCeresHazeRed
            : colors?.CeresHazeBlue ?? RoomFxPaletteBlendDefinitions.StockCeresHazeBlue;
        static byte Scale(int component, int channel) =>
            (byte)Math.Min(31, (component * channel + MaximumComponent / 2) / MaximumComponent);
        return (Scale(component, tint.Red), Scale(component, tint.Green), Scale(component, tint.Blue));
    }
}
