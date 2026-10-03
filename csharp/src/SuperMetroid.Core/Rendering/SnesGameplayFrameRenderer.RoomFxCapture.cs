using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Rendering;

public static partial class SnesGameplayFrameRenderer
{
    /// <summary>Composes native configuration $16 with both BG2 and BG3 on the subscreen.</summary>
    public static GameplayColorMathRenderLayer CaptureWaterfall(OrdinaryGameplayRenderLayer gameplay,
        RoomLayer3FxRenderSnapshot fx)
    {
        if (fx.LayerBlendConfiguration != LayerBlendingConfiguration.WaterfallSubtractive)
            throw new ArgumentException("Waterfall capture requires configuration $16.", nameof(fx));
        var windows = new XrayWindowLine[Height];
        Array.Fill(windows, new XrayWindowLine(255, 0));
        return new(gameplay, windows, false, WaterfallRoomDisplayRules.ColorMath,
            true, 0, 0, 0, CaptureRoomLayer3Fx(fx), subscreenUsesBg2: true);
    }

    /// <summary>Resolves FX type, liquid visibility and wave phase into a backend-neutral BG equation.</summary>
    public static Bg2BppColorMathRenderLayer? CaptureRoomLayer3Fx(RoomLayer3FxRenderSnapshot fx)
    {
        LayerBlendingConfiguration required = fx.Type switch
        {
            RoomFxType.Lava or RoomFxType.Acid => LayerBlendingConfiguration.LavaAcidAdditive,
            RoomFxType.Water or RoomFxType.TourianEntranceStatue => fx.LayerBlendConfiguration,
            RoomFxType.Rain => LayerBlendingConfiguration.Rain,
            RoomFxType.Fog => LayerBlendingConfiguration.FogAdditive,
            RoomFxType.Spores => LayerBlendingConfiguration.Spores,
            _ => throw new NotSupportedException($"Room FX type {fx.Type} has no captured BG3 compositor."),
        };
        if (RoomFxTypes.UsesWater(fx.Type) && fx.LayerBlendConfiguration is not
            (LayerBlendingConfiguration.WaterSubtractive or LayerBlendingConfiguration.WaterfallSubtractive or
            LayerBlendingConfiguration.LiquidOrFogAdditive))
            throw new InvalidDataException("Water has an incompatible layer-blending configuration.");
        if (fx.LayerBlendConfiguration != required) throw new InvalidDataException("Room FX has an incompatible layer-blending configuration.");
        bool liquid = fx.Type is RoomFxType.Water or RoomFxType.TourianEntranceStatue or RoomFxType.Lava or RoomFxType.Acid;
        if (liquid && unchecked((short)fx.CurrentYPosition) < 0) return null;
        bool atmosphere = fx.Type is RoomFxType.Rain or RoomFxType.Fog or RoomFxType.Spores;
        var scrolls = new BackgroundLineScroll[Height];
        int firstSurfaceLine = fx.WaterSurfaceScreenY - (SnesPpuLayout.BackgroundTileSizePixels - 1);
        for (int y = HudHeight; y < Height; y++)
        {
            ushort vertical = liquid && y < firstSurfaceLine ? (ushort)0 : fx.VerticalScroll;
            int wave = RoomFxTypes.UsesWater(fx.Type) && y > fx.WaterSurfaceScreenY
                ? RoomFxRomData.Water.WaveDisplacements[(y - fx.WaterSurfaceScreenY - 1 - fx.WaterBg3WavePhase +
                    RoomFxRomData.Water.WaveDisplacementCount) % RoomFxRomData.Water.WaveDisplacementCount] : 0;
            scrolls[y] = new(unchecked((ushort)(fx.HorizontalScroll + wave)), vertical);
        }
        bool subtract = RoomFxTypes.UsesWater(fx.Type) && fx.LayerBlendConfiguration is
            LayerBlendingConfiguration.WaterSubtractive or LayerBlendingConfiguration.WaterfallSubtractive;
        return new(atmosphere ? RoomFxRomData.Layer3.FullScreenAtmosphereTilemapBaseWord : RoomFxRomData.Layer3.LiquidTilemapBaseWord,
            SnesPpuLayout.GameplayHudCharacterBaseWord,
            ((atmosphere ? RoomFxRomData.Layer3.FullScreenAtmosphereVerticalCoordinateMask : RoomFxRomData.Layer3.LiquidVerticalCoordinateMask) + 1)
                / SnesPpuLayout.BackgroundTileSizePixels,
            HudHeight, subtract ? ExpandedColorMathOperation.Subtract : ExpandedColorMathOperation.Add, scrolls);
    }

    /// <summary>
    /// FX $08 retains the winning main-screen source until color math. A post-frame
    /// overlay would incorrectly brighten foreground terrain (and OBJ palettes 0-3).
    /// Use the same literal Mode-1 composition as the other source-aware effects.
    /// </summary>
    public static GameplayColorMathRenderLayer CaptureSpores(OrdinaryGameplayRenderLayer gameplay,
        RoomLayer3FxRenderSnapshot fx)
    {
        if (fx.Type != RoomFxType.Spores)
            throw new ArgumentException("Spore blending requires a spores FX snapshot.", nameof(fx));
        var emptyWindows = new XrayWindowLine[Height];
        Array.Fill(emptyWindows, new XrayWindowLine(255, 0));
        return new(gameplay, emptyWindows, false, (SnesColorMathControl)RoomFxRomData.Spores.ColorMathSources,
            true, 0, 0, 0, CaptureRoomLayer3Fx(fx));
    }
}
