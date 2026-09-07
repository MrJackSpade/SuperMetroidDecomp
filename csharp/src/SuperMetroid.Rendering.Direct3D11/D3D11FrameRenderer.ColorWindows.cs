using SuperMetroid.Core.Rendering;

namespace SuperMetroid.Rendering.Direct3D11;

public sealed partial class D3D11FrameRenderer
{
    private unsafe void DispatchColorWindows(ScanlineColorAddRenderLayer layer)
    {
        var data = new uint[D3D11ShaderLayout.SolidConstantWords];
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
        fixed (uint* source = data) owner.Context.UpdateSubresource(constants, 0, null, (nint)source, 0, 0);
        owner.Context.Dispatch(32, 28, 1);
    }
}
