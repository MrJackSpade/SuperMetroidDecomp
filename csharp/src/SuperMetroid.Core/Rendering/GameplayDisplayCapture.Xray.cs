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
        if (runtime.Samus is not { } samus || !samus.Xray.IsActive || samus.Xray.SetupStage != 0) return null;
        if (basis.Layers[0] is not OrdinaryGameplayRenderLayer ordinary)
            throw new NotSupportedException("X-ray display requires ordinary gameplay layers.");
        var room = runtime.ActiveRoom ?? throw new InvalidOperationException("X-ray has no room.");
        var ppu = runtime.DisplayedGameplayPpu;
        var mode = XrayRoomDisplayRules.Select(room.Pointer, runtime.RoomLayer3Fx.Type, runtime.Enemies.BossId);
        bool reveal = mode == XrayRoomBlendMode.RevealBlocks;
        byte[] bytes = basis.Memory.Vram.ToArray();
        var registers = ordinary.Registers;
        if (reveal)
        {
            ushort[] map = XraySetupMemory.ReadReveal(runtime.AddressSpace);
            for (int i = 0; i < map.Length; i++)
            {
                int offset = (SnesPpuLayout.GameplayBg2TilemapWord + i) * 2;
                bytes[offset] = (byte)map[i]; bytes[offset + 1] = (byte)(map[i] >> 8);
            }
            registers = registers with
            {
                Bg2X = (ushort)(ppu.Bg1HorizontalScroll & 15), Bg2Y = (ushort)(ppu.Bg1VerticalScroll & 15),
                Bg2WidthTiles = 64, Bg2HeightTiles = 32, Bg2TilemapWord = SnesPpuLayout.GameplayBg2TilemapWord,
            };
        }
        registers = registers with
        {
            MainScreenLayers = room.Pointer == XrayRoomDisplayRules.ExcludedRoomWithHiddenBg2 && mode == XrayRoomBlendMode.PreserveBackgrounds
                ? SnesMainScreenLayers.Bg1 | SnesMainScreenLayers.Obj
                : SnesMainScreenLayers.Bg1 | SnesMainScreenLayers.Bg2 | SnesMainScreenLayers.Obj,
        };
        // Reveal BG2 must not inherit the room's parallax or liquid HDMA. Excluded
        // rooms retain their original map and per-line register values.
        var gameplay = reveal ? new OrdinaryGameplayRenderLayer(registers)
            : new OrdinaryGameplayRenderLayer(registers, ordinary.HorizontalScrolls, ordinary.VerticalScrolls);
        var sub = mode != XrayRoomBlendMode.Fireflea && runtime.DisplayedRoomLayer3Fx is { } fx
            ? SnesGameplayFrameRenderer.CaptureRoomLayer3Fx(fx) : null;
        // The arithmetic sign belongs to the room's blend configuration, even when
        // its liquid plane is currently offscreen/disabled and supplies no pixels.
        bool subtract = runtime.RoomLayer3Fx.LayerBlendConfiguration is
            LayerBlendingConfiguration.WaterSubtractive or LayerBlendingConfiguration.WaterfallSubtractive;
        var control = XrayRoomDisplayRules.ColorMath(mode, subtract);
        byte red = Fixed(XrayRoomDisplayRules.FixedRedMirror), green = Fixed(XrayRoomDisplayRules.FixedGreenMirror),
            blue = Fixed(XrayRoomDisplayRules.FixedBlueMirror);
        if (reveal || mode == XrayRoomBlendMode.Fireflea && red < XrayWindowRenderDefinitions.FixedColorComponent)
            red = green = blue = XrayWindowRenderDefinitions.FixedColorComponent;
        var colors = basis.Memory.Cgram.ToArray();
        bool restoring = samus.Xray.BeamPhase is XrayBeamPhase.RestoreSecondHalf or XrayBeamPhase.Finish;
        colors[0] = restoring ? (ushort)0 : XrayRoomDisplayRules.ActiveBackdrop;
        var memory = new PpuMemorySnapshot(bytes, colors, basis.Memory.Oam, basis.Memory.ModeledSpriteCount);
        // Releasing Run merely advances the native dispatcher to phase three. That
        // phase closes the HDMA window on the NEXT call and clears the backdrop;
        // restoration keeps ownership until phase five finally unfreezes gameplay.
        bool closed = restoring || samus.Xray.BeamPhase == XrayBeamPhase.NoBeam;
        var lines = closed ? Enumerable.Repeat(new XrayWindowLine(255, 0), SnesPpuLayout.ScreenHeightPixels).ToArray()
            : SnesGameplayFrameRenderer.CaptureXrayWindowLines(runtime.AddressSpace, samus, ppu.Layer1XPosition, ppu.Layer1YPosition);
        var layer = new XrayGameplayRenderLayer(gameplay,
            lines,
            reveal, control, mode != XrayRoomBlendMode.Fireflea, red, green, blue, sub);
        return new(memory, new RenderLayer[] { layer }, basis.ObjectSelection, basis.Brightness);

        byte Fixed(int address) => (byte)(runtime.AddressSpace.ReadByte(address) & 31);
    }
}
