using SuperMetroid.Core.Game;
using SuperMetroid.Core.Rendering;

namespace SuperMetroid.Rendering.Direct3D11;

public sealed partial class D3D11FrameRenderer
{
    private unsafe void DispatchMessage(MessageBoxRenderLayer layer)
    {
        var data = ClearUploadConstants();
        data[0] = (uint)D3D11TileOperation.Message;
        data[1] = (uint)(GameplayMessageRomData.Layout.WindowCenterY - layer.RowCount * 4);
        data[2] = GameplayMessageRomData.Layout.CharacterBaseWord;
        data[3] = (uint)layer.RowCount;
        data[10] = GameplayMessageRomData.Palette.TemporaryLightColor;
        data[11] = GameplayMessageRomData.Palette.TemporaryDarkColor;
        data[12] = GameplayMessageRomData.Palette.TemporaryLightIndex;
        data[13] = GameplayMessageRomData.Palette.TemporaryDarkIndex;
        data[25] = (uint)(GameplayMessageRomData.Layout.WindowCenterY - layer.RadiusPixels);
        data[26] = (uint)(GameplayMessageRomData.Layout.WindowCenterY + layer.RadiusPixels);
        for (int i = 0; i < layer.Tilemap.Length; i++)
            data[D3D11ShaderLayout.ScanlineParametersWordOffset + i * 4] = layer.Tilemap[i];
        fixed (uint* source = data) owner.Context.UpdateSubresource(constants, 0, null, (nint)source, 0, 0);
        owner.Context.Dispatch(32, 28, 1);
    }
}
