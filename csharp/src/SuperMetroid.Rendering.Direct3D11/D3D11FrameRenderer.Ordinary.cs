using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Spatial;

namespace SuperMetroid.Rendering.Direct3D11;

public sealed partial class D3D11FrameRenderer
{
    [PpuLayoutBoundary("backend: Direct3D 11 PPU dispatch of the ordinary gameplay layers")]
    private void DispatchOrdinaryGameplay(OrdinaryGameplayRenderLayer layer)
    {
        OrdinaryGameplayRegisters r = layer.Registers;
        // Preserve the Mode-1 priority ladder. Resolving OAM before these passes
        // prevents a hidden higher-index sprite from reappearing at another rank.
        int first = SnesPpuLayout.GameplayHudHeightPixels, end;
        SnesMainScreenLayers activeLayers;
        while (first < SnesPpuLayout.ScreenHeightPixels)
        {
            activeLayers = layer.MainScreenLayersByLine.IsEmpty ? r.MainScreenLayers
                : (SnesMainScreenLayers)layer.MainScreenLayersByLine[first - SnesPpuLayout.GameplayHudHeightPixels];
            end = first + 1;
            while (end < SnesPpuLayout.ScreenHeightPixels && (layer.MainScreenLayersByLine.IsEmpty ||
                layer.MainScreenLayersByLine[end - SnesPpuLayout.GameplayHudHeightPixels] == (ushort)activeLayers)) end++;
            Objects(0); Objects(1); Background2(false); Background1(false);
            Objects(2); Background2(true); Background1(true); Objects(3);
            first = end;
        }
        DispatchTile(D3D11TileOperation.Bg2, new D3D11TileDispatch { Map = GameplayVramLayout.HudTilemapWord, Characters = r.HudCharacterWord, TransparentZero = 0, EndScanline = SnesPpuLayout.GameplayHudHeightPixels, Windows = r.Windows, WindowMask = r.MainScreenWindowMask, WindowTarget = SnesWindowTarget.Bg3 });

        void Objects(uint priority)
        {
            if ((activeLayers & SnesMainScreenLayers.Obj) != 0)
                DispatchTile(D3D11TileOperation.InsertObj, new D3D11TileDispatch { Priority = priority + 1, FirstScanline = (uint)first, EndScanline = (uint)end, Windows = r.Windows, WindowMask = r.MainScreenWindowMask, WindowTarget = SnesWindowTarget.Obj });
        }
        void Background1(bool high)
        {
            if ((activeLayers & SnesMainScreenLayers.Bg1) != 0)
                DispatchTile(D3D11TileOperation.Bg4, new D3D11TileDispatch { Map = GameplayVramLayout.Bg1TilemapWord, Characters = r.Bg1CharacterWord, Scroll = r.Bg1Scroll.AsBgPlaneScroll() + new Offset16<BgPlanePixels>(0, SnesPpuLayout.FirstVisibleBackgroundScanline), MapSize = new Extent16<TilemapCells>(64, 32), Priority = Priority(high), FirstScanline = (uint)first, EndScanline = (uint)end, Windows = r.Windows, WindowMask = r.MainScreenWindowMask, WindowTarget = SnesWindowTarget.Bg1 });
        }
        void Background2(bool high)
        {
            if ((activeLayers & SnesMainScreenLayers.Bg2) != 0) DispatchGameplayBg2(layer, high, first, end);
        }
    }

    private unsafe void DispatchGameplayBg2(OrdinaryGameplayRenderLayer layer, bool high, int first, int end)
    {
        var r = layer.Registers;
        var data = ClearUploadConstants();
        data[0] = (uint)D3D11TileOperation.Bg4;
        data[1] = r.Bg2TilemapWord; data[2] = r.Bg2CharacterWord;
        data[5] = (uint)r.Bg2MapSize.X; data[6] = (uint)r.Bg2MapSize.Y;
        data[7] = Priority(high); data[8] = 1; data[15] = 1;
        data[25] = (uint)Math.Max(r.Bg2FirstScanline, first); data[26] = (uint)Math.Min(r.Bg2EndScanline, end);
        // Header word 27 is the BG4 gameplay mosaic width (zero elsewhere).
        data[27] = (uint)r.Bg2Mosaic.Size;
        SetWindowConstants(data, r.Windows, r.MainScreenWindowMask, SnesWindowTarget.Bg2);
        for (int y = SnesPpuLayout.GameplayHudHeightPixels; y < SnesPpuLayout.ScreenHeightPixels; y++)
        {
            int line = y - SnesPpuLayout.GameplayHudHeightPixels;
            int offset = D3D11ShaderLayout.ScanlineParametersWordOffset + y * 4;
            var scroll = layer.Bg2ScrollAt(line);
            data[offset] = scroll.X;
            data[offset + 1] = unchecked((ushort)(scroll.Y + SnesPpuLayout.FirstVisibleBackgroundScanline));
        }
        fixed (uint* source = data) UploadBuffer(constants, (nint)source, data.Length * sizeof(uint));
        owner.Context.Dispatch(32, 28, 1);
    }
}
