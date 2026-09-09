using SuperMetroid.Core.Rendering;

namespace SuperMetroid.Rendering.Direct3D11;

public sealed partial class D3D11FrameRenderer
{
    private unsafe void DispatchSubscreen(BgSubscreenAddRenderLayer layer, uint objectCount, byte objectSelection)
    {
        var data = ClearUploadConstants();
        data[0] = (uint)D3D11TileOperation.SubscreenAdd;
        data[1] = layer.TilemapWord; data[2] = layer.CharacterWord;
        data[4] = layer.VerticalScroll;
        data[5] = 32; data[6] = 32; data[8] = 1; data[26] = 224;
        data[27] = layer.FourBpp ? 1u : 0u;
        data[10] = layer.IncludeObjects ? 1u : 0u;
        data[11] = layer.MainObjects ? 1u : 0u;
        data[13] = objectCount; data[14] = objectSelection;
        if (layer.MainCoverage is { } coverage)
        {
            int offset = D3D11ShaderLayout.ScanlineParametersWordOffset;
            data[offset] = coverage.TilemapWord; data[offset + 1] = coverage.CharacterWord;
            data[offset + 2] = coverage.HorizontalScroll; data[offset + 3] = coverage.VerticalScroll;
            data[offset + 4] = (uint)coverage.MapWidthTiles; data[offset + 5] = (uint)coverage.MapHeightTiles;
            data[offset + 6] = Priority(coverage.Priority); data[offset + 7] = 1;
        }
        fixed (uint* source = data) UploadBuffer(constants, (nint)source, data.Length * sizeof(uint));
        owner.Context.Dispatch(32, 28, 1);
    }
}
