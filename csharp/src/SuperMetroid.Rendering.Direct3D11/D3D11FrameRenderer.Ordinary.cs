using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rendering;

namespace SuperMetroid.Rendering.Direct3D11;

public sealed partial class D3D11FrameRenderer
{
    private void DispatchOrdinaryGameplay(OrdinaryGameplayRenderLayer layer)
    {
        OrdinaryGameplayRegisters r = layer.Registers;
        // Preserve the Mode-1 priority ladder. Resolving OAM before these passes
        // prevents a hidden higher-index sprite from reappearing at another rank.
        Objects(0); Objects(1); Background2(false); Background1(false);
        Objects(2); Background2(true); Background1(true); Objects(3);
        DispatchTile(D3D11TileOperation.Bg2, SnesPpuLayout.GameplayHudTilemapWord,
            r.HudCharacterWord, transparentZero: 0, endScanline: SnesPpuLayout.GameplayHudHeightPixels,
            windows: r.Windows, windowMask: r.MainScreenWindowMask, windowTarget: SnesWindowTarget.Bg3);

        void Objects(uint priority)
        {
            if ((r.MainScreenLayers & SnesMainScreenLayers.Obj) != 0)
                DispatchTile(D3D11TileOperation.InsertObj, priority: priority + 1,
                    firstScanline: SnesPpuLayout.GameplayHudHeightPixels,
                    windows: r.Windows, windowMask: r.MainScreenWindowMask, windowTarget: SnesWindowTarget.Obj);
        }
        void Background1(bool high)
        {
            if ((r.MainScreenLayers & SnesMainScreenLayers.Bg1) != 0)
                DispatchTile(D3D11TileOperation.Bg4, SnesPpuLayout.GameplayBg1TilemapWord,
                    r.Bg1CharacterWord, r.Bg1X, r.Bg1Y, 64, 32, Priority(high),
                    firstScanline: SnesPpuLayout.GameplayHudHeightPixels,
                    windows: r.Windows, windowMask: r.MainScreenWindowMask, windowTarget: SnesWindowTarget.Bg1);
        }
        void Background2(bool high)
        {
            if ((r.MainScreenLayers & SnesMainScreenLayers.Bg2) != 0) DispatchGameplayBg2(layer, high);
        }
    }

    private unsafe void DispatchGameplayBg2(OrdinaryGameplayRenderLayer layer, bool high)
    {
        var r = layer.Registers;
        var data = ClearUploadConstants();
        data[0] = (uint)D3D11TileOperation.Bg4;
        data[1] = r.Bg2TilemapWord; data[2] = r.Bg2CharacterWord;
        data[5] = (uint)r.Bg2WidthTiles; data[6] = (uint)r.Bg2HeightTiles;
        data[7] = Priority(high); data[8] = 1; data[15] = 1;
        data[25] = (uint)r.Bg2FirstScanline; data[26] = (uint)r.Bg2EndScanline;
        SetWindowConstants(data, r.Windows, r.MainScreenWindowMask, SnesWindowTarget.Bg2);
        for (int y = SnesPpuLayout.GameplayHudHeightPixels; y < SnesPpuLayout.ScreenHeightPixels; y++)
        {
            int line = y - SnesPpuLayout.GameplayHudHeightPixels;
            int offset = D3D11ShaderLayout.ScanlineParametersWordOffset + y * 4;
            data[offset] = layer.HorizontalScrolls.IsEmpty ? r.Bg2X : layer.HorizontalScrolls[line];
            data[offset + 1] = layer.VerticalScrolls.IsEmpty ? r.Bg2Y : layer.VerticalScrolls[line];
        }
        fixed (uint* source = data) UploadBuffer(constants, (nint)source, data.Length * sizeof(uint));
        owner.Context.Dispatch(32, 28, 1);
    }
}
