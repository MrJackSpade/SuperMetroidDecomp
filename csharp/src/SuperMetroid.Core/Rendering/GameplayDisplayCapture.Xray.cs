using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;

namespace SuperMetroid.Core.Rendering;

public static partial class GameplayDisplayCapture
{
    /// <summary>Captures X-ray's layer ownership before priority resolution and color math.</summary>
    private static LayeredRenderSnapshot? CaptureXray(SuperMetroidRuntime runtime, LayeredRenderSnapshot basis)
    {
        bool finishedThisFrame = runtime.LastXrayBeamStep is { Completed: true, PhaseAtStart: XrayBeamPhase.Finish };
        if (runtime.Samus is not { } samus || !samus.Xray.IsActive && !finishedThisFrame) return null;
        if (!samus.Xray.SetupStage.ShowsBlendedRoom()) return null;
        bool settingUp = samus.Xray.SetupStage != XraySetupStage.Complete;
        if (basis.Layers[0] is not OrdinaryGameplayRenderLayer ordinary)
            throw new NotSupportedException("X-ray display requires ordinary gameplay layers.");
        var room = runtime.ActiveRoom ?? throw new InvalidOperationException("X-ray has no room.");
        ISnesMemoryBus memoryBus = runtime.MemoryBus;
        var ppu = runtime.DisplayedGameplayPpu;
        var mode = XrayRoomDisplayRules.Select(room.Pointer, runtime.RoomLayer3Fx.Type, runtime.Enemies.BossId);
        bool reveal = mode == XrayRoomBlendMode.RevealBlocks;
        byte[] bytes = basis.Memory.Vram.ToArray();
        var registers = ordinary.Registers;
        if (reveal)
        {
            ushort[] map = new XrayMemory(memoryBus).RevealTilemap.ToArray();
            for (int i = 0; i < map.Length; i++)
            {
                int offset = (SnesPpuLayout.GameplayBg2TilemapWord + i) * 2;
                bytes[offset] = (byte)map[i]; bytes[offset + 1] = (byte)(map[i] >> 8);
            }
            registers = registers with
            {
                // The reveal map is drawn on BG2 at BG1's fine scroll within one 16-pixel block.
                Bg2Scroll = new Position16<Bg2Pixels>((ushort)(ppu.Bg1Scroll.X & 15), (ushort)(ppu.Bg1Scroll.Y & 15)),
                Bg2MapSize = new Extent16<TilemapCells>(64, 32), Bg2TilemapWord = SnesPpuLayout.GameplayBg2TilemapWord,
            };
        }
        registers = registers with
        {
            MainScreenLayers = XrayRoomDisplayRules.RemovesBg2(room.Pointer) && mode == XrayRoomBlendMode.PreserveBackgrounds
                ? SnesMainScreenLayers.Bg1 | SnesMainScreenLayers.Obj
                : SnesMainScreenLayers.Bg1 | SnesMainScreenLayers.Bg2 | SnesMainScreenLayers.Obj,
        };
        // Reveal BG2 must not inherit the room's parallax or liquid HDMA. Excluded
        // rooms retain their original map and per-line register values.
        var gameplay = reveal ? new OrdinaryGameplayRenderLayer(registers)
            : new OrdinaryGameplayRenderLayer(registers, ordinary.Bg2LineScrolls);
        var sub = mode != XrayRoomBlendMode.Fireflea && runtime.DisplayedRoomLayer3Fx is { } fx
            ? SnesGameplayFrameRenderer.CaptureRoomLayer3Fx(fx) : null;
        if (mode != XrayRoomBlendMode.Fireflea && sub is null)
        {
            // Clear_FX_Tilemap leaves an opaque BG3 plane even without a visible FX
            // actor. Room initialization supplies zero scroll and the 32x64 BG3 map;
            // capture its real pixels rather than substituting transparent backdrop.
            sub = new Bg2BppColorMathRenderLayer(
                RoomFxRomData.Layer3.LiquidTilemapBaseWord,
                runtime.GameplayHudCharacterBaseWord,
                (RoomFxRomData.Layer3.LiquidVerticalCoordinateMask + 1) / SnesPpuLayout.BackgroundTileSizePixels,
                SnesPpuLayout.GameplayHudHeightPixels,
                ExpandedColorMathOperation.Add,
                new Position16<BgPlanePixels>[SnesPpuLayout.ScreenHeightPixels]);
        }
        // The arithmetic sign belongs to the room's blend configuration, even when
        // its liquid plane is currently offscreen/disabled and supplies no pixels.
        bool subtract = runtime.RoomLayer3Fx.LayerBlendConfiguration is
            LayerBlendingConfiguration.WaterSubtractive or LayerBlendingConfiguration.WaterfallSubtractive;
        var control = XrayRoomDisplayRules.ColorMath(mode, subtract);
        var fixedColor = new FixedColorMemory(memoryBus);
        byte red = fixedColor.RedIntensity, green = fixedColor.GreenIntensity, blue = fixedColor.BlueIntensity;
        if (reveal || mode == XrayRoomBlendMode.Fireflea && red < XrayWindowRenderDefinitions.FixedColorComponent)
            red = green = blue = XrayWindowRenderDefinitions.FixedColorComponent;
        // Phase five deletes the HDMA object but its blend bits have already been
        // published for this pass. Only its fixed color is cleared immediately.
        if (finishedThisFrame && mode != XrayRoomBlendMode.Fireflea) red = green = blue = 0;
        var colors = basis.Memory.Cgram.ToArray();
        bool restoring = finishedThisFrame || samus.Xray.BeamPhase is XrayBeamPhase.RestoreSecondHalf or XrayBeamPhase.Finish;
        if (!settingUp) colors[0] = restoring ? Bgr555.Black : XrayRoomDisplayRules.ActiveBackdrop;
        var memory = new PpuMemorySnapshot(bytes, colors, basis.Memory.Oam, basis.Memory.ModeledSpriteCount);
        // Releasing Run merely advances the native dispatcher to phase three. That
        // phase closes the HDMA window on the NEXT call and clears the backdrop;
        // restoration keeps ownership until phase five finally unfreezes gameplay.
        bool closed = settingUp || restoring || samus.Xray.BeamPhase == XrayBeamPhase.NoBeam;
        var lines = closed ? Enumerable.Repeat(new XrayWindowLine(255, 0), SnesPpuLayout.ScreenHeightPixels).ToArray()
            : SnesGameplayFrameRenderer.CaptureXrayWindowLines(runtime.AddressSpace, samus, ppu.Layer1Position);
        var layer = new GameplayColorMathRenderLayer(gameplay,
            lines,
            reveal, control, mode != XrayRoomBlendMode.Fireflea, red, green, blue, sub);
        return new(memory, new RenderLayer[] { layer }, basis.ObjectSelection, basis.Brightness);
    }
}
