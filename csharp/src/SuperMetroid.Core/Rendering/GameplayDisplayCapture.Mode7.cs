using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Runtime;

namespace SuperMetroid.Core.Rendering;

public static partial class GameplayDisplayCapture
{
    private static LayeredRenderSnapshot CaptureMode7Base(SuperMetroidRuntime runtime)
    {
        GameplayPpuRenderSnapshot ppu = runtime.DisplayedGameplayPpu;
        RoomShakeFrameResult shake = ppu.RoomShake;
        Mode7RenderRegisters registers;
        Mode1FloorBand? floor = null;
        if (runtime.Enemies.CeresRidley is { Mode7Active: true } getaway)
        {
            registers = new(unchecked((short)getaway.Mode7MatrixA), unchecked((short)getaway.Mode7MatrixB),
                unchecked((short)getaway.Mode7MatrixC), unchecked((short)getaway.Mode7MatrixD),
                unchecked((short)getaway.Mode7CenterX), unchecked((short)getaway.Mode7CenterY),
                unchecked((short)(getaway.Mode7HorizontalOffset + shake.Bg1X)),
                unchecked((short)(getaway.Mode7VerticalOffset + shake.Bg1Y)));
            floor = new(GameplayRenderDefinitions.CeresFloorFirstScanline, SnesPpuLayout.GameplayBg2TilemapWord,
                GameplayRenderDefinitions.CeresCharacterWord, Add(ppu.Bg2HorizontalScroll, shake.Bg2X),
                Add(ppu.Bg2VerticalScroll, shake.Bg2Y), 64, 32);
        }
        else
        {
            SamusMode7Transform m = runtime.DisplayedSamusMode7Transform
                ?? throw new InvalidOperationException("The Ceres elevator shaft has no NMI-published Mode 7 matrix.");
            registers = new(unchecked((short)m.MatrixA), unchecked((short)m.MatrixB),
                unchecked((short)m.MatrixC), unchecked((short)m.MatrixA),
                unchecked((short)m.CenterX), unchecked((short)m.CenterY),
                unchecked((short)Add(ppu.Bg1HorizontalScroll, shake.Bg1X)),
                unchecked((short)Add(ppu.Bg1VerticalScroll, shake.Bg1Y)));
        }
        var layer = new Mode7GameplayRenderLayer(registers, SnesPpuLayout.GameplayHudTilemapWord,
            SnesPpuLayout.GameplayHudCharacterBaseWord, SnesPpuLayout.GameplayHudHeightPixels, floor);
        return new(PpuMemorySnapshot.Capture(runtime.Vram, runtime.Cgram, runtime.DisplayedOam),
            new RenderLayer[] { layer }, GameplayRenderDefinitions.ObjectSelection, SnesPpuLayout.MaximumMasterBrightness);
    }
}
