using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Rendering;

public static partial class SnesGameplayFrameRenderer
{
    /// <summary>Resolves FX type, liquid visibility and wave phase into a backend-neutral BG equation.</summary>
    public static Bg2BppColorMathRenderLayer? CaptureRoomLayer3Fx(RoomLayer3FxRenderSnapshot fx)
    {
        LayerBlendingConfiguration required = fx.Type switch
        {
            RoomFxType.Lava or RoomFxType.Acid => LayerBlendingConfiguration.LavaAcidAdditive,
            RoomFxType.Water => fx.LayerBlendConfiguration,
            RoomFxType.Rain => LayerBlendingConfiguration.Rain,
            RoomFxType.Fog => LayerBlendingConfiguration.FogAdditive,
            _ => throw new NotSupportedException($"Room FX type {fx.Type} has no captured BG3 compositor."),
        };
        if (fx.Type == RoomFxType.Water && fx.LayerBlendConfiguration is not
            (LayerBlendingConfiguration.WaterSubtractive or LayerBlendingConfiguration.WaterfallSubtractive or
            LayerBlendingConfiguration.LiquidOrFogAdditive))
            throw new InvalidDataException("Water has an incompatible layer-blending configuration.");
        if (fx.LayerBlendConfiguration != required) throw new InvalidDataException("Room FX has an incompatible layer-blending configuration.");
        bool liquid = fx.Type is RoomFxType.Water or RoomFxType.Lava or RoomFxType.Acid;
        if (liquid && unchecked((short)fx.CurrentYPosition) < 0) return null;
        bool atmosphere = fx.Type is RoomFxType.Rain or RoomFxType.Fog;
        var scrolls = new BackgroundLineScroll[Height];
        int firstSurfaceLine = fx.WaterSurfaceScreenY - (SnesPpuLayout.BackgroundTileSizePixels - 1);
        for (int y = HudHeight; y < Height; y++)
        {
            ushort vertical = liquid && y < firstSurfaceLine ? (ushort)0 : fx.VerticalScroll;
            int wave = fx.Type == RoomFxType.Water && y > fx.WaterSurfaceScreenY
                ? RoomFxRomData.Water.WaveDisplacements[(y - fx.WaterSurfaceScreenY - 1 - fx.WaterBg3WavePhase +
                    RoomFxRomData.Water.WaveDisplacementCount) % RoomFxRomData.Water.WaveDisplacementCount] : 0;
            scrolls[y] = new(unchecked((ushort)(fx.HorizontalScroll + wave)), vertical);
        }
        bool subtract = fx.Type == RoomFxType.Water && fx.LayerBlendConfiguration is
            LayerBlendingConfiguration.WaterSubtractive or LayerBlendingConfiguration.WaterfallSubtractive;
        return new(atmosphere ? RoomFxRomData.Layer3.FullScreenAtmosphereTilemapBaseWord : RoomFxRomData.Layer3.LiquidTilemapBaseWord,
            SnesPpuLayout.GameplayHudCharacterBaseWord,
            ((atmosphere ? RoomFxRomData.Layer3.FullScreenAtmosphereVerticalCoordinateMask : RoomFxRomData.Layer3.LiquidVerticalCoordinateMask) + 1)
                / SnesPpuLayout.BackgroundTileSizePixels,
            HudHeight, subtract ? ExpandedColorMathOperation.Subtract : ExpandedColorMathOperation.Add, scrolls);
    }
}
