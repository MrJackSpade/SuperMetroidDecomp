using SuperMetroid.Core.Rendering;

namespace SuperMetroid.Rendering.Direct3D11;

public sealed partial class D3D11FrameRenderer
{
    /// <summary>Uploads the BG2 tilemap, character base, operation, and physical-line scroll data before dispatching its color-math plane.</summary>
    /// <param name="layer">Validated BG2 2-bpp layer defining VRAM sources, visible scanlines, operation, and one scroll pair per output line.</param>
    private unsafe void DispatchBackgroundMath(Bg2BppColorMathRenderLayer layer)
    {
        var data = ClearUploadConstants();
        data[0] = (uint)(layer.Operation == ExpandedColorMathOperation.Add
            ? D3D11TileOperation.BgAdd : D3D11TileOperation.BgSubtract);
        data[1] = layer.TilemapWord; data[2] = layer.CharacterWord;
        data[5] = 32; data[6] = (uint)layer.MapHeightTiles; data[8] = 1;
        data[25] = (uint)layer.FirstScanline; data[26] = 224;
        for (int y = 0; y < layer.Scrolls.Length; y++)
        {
            int offset = D3D11ShaderLayout.ScanlineParametersWordOffset + y * 4;
            data[offset] = layer.Scrolls[y].X;
            data[offset + 1] = layer.Scrolls[y].Y;
        }
        fixed (uint* source = data) UploadBuffer(constants, (nint)source, data.Length * sizeof(uint));
        owner.Context.Dispatch(32, 28, 1);
    }

    /// <summary>Uploads inclusive per-scanline window bounds and expanded RGB additions, then dispatches the color-window pass.</summary>
    /// <param name="layer">Validated 224-line color-add program consumed by the compute shader in physical display order.</param>
    private unsafe void DispatchColorWindows(ScanlineColorAddRenderLayer layer)
    {
        var data = ClearUploadConstants();
        data[0] = (uint)D3D11TileOperation.ScanlineAdd;
        data[26] = 224;
        for (int y = 0; y < layer.Windows.Length; y++)
        {
            ColorAddWindow line = layer.Windows[y];
            int offset = D3D11ShaderLayout.ScanlineParametersWordOffset + y * 4;
            data[offset] = (uint)(line.Left | line.Right << 8);
            data[offset + 1] = line.Red;
            data[offset + 2] = line.Green;
            data[offset + 3] = line.Blue;
        }
        fixed (uint* source = data) UploadBuffer(constants, (nint)source, data.Length * sizeof(uint));
        owner.Context.Dispatch(32, 28, 1);
    }
}
