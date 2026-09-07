namespace SuperMetroid.Rendering.Direct3D11;

public sealed partial class D3D11FrameRenderer
{
    private unsafe void DispatchTitleGradient(ReadOnlySpan<Core.Frontend.TitleGradientLine> lines, int objects, byte selection)
    {
        var data = ClearUploadConstants();
        data[13] = (uint)objects; data[14] = selection; data[26] = 224;
        for (int y = 0; y < lines.Length; y++)
        {
            int offset = D3D11ShaderLayout.ScanlineParametersWordOffset + y * 4;
            data[offset] = lines[y].Red; data[offset + 1] = lines[y].Green;
            data[offset + 2] = lines[y].Blue; data[offset + 3] = lines[y].Control;
        }
        fixed (uint* source = data) UploadBuffer(constants, (nint)source, data.Length * sizeof(uint));
        owner.Context.CSSetShader(titleGradientShader);
        owner.Context.Dispatch(32, 28, 1);
        owner.Context.CSSetShader(tileShader);
    }
}
