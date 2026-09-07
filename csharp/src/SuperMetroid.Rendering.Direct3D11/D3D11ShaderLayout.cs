namespace SuperMetroid.Rendering.Direct3D11;

/// <summary>CPU/HLSL upload-layout identities; changes require matching shader and verification changes.</summary>
internal static class D3D11ShaderLayout
{
    internal const int SolidHeaderWords = 4;
    internal const int MaximumBrightnessPasses = 1024;
    internal const int SolidConstantWords = SolidHeaderWords + MaximumBrightnessPasses;
    internal const int DispatchTileEdge = 8;
    internal const string SolidResourceName = "SuperMetroid.Shaders.Solid.cso";
    internal const string TileResourceName = "SuperMetroid.Shaders.Tiles.cso";
    internal const int VramPackedWords = Core.Hardware.SnesPpuLayout.VramByteCount / sizeof(uint);
    internal const int PpuMemoryWords = VramPackedWords + Core.Hardware.SnesPpuLayout.CgramColorCount;
}

/// <summary>Exclusive compute operations shared with Tiles.hlsl.</summary>
internal enum D3D11TileOperation : uint { Backdrop = 0, Bg4 = 1, Bg2 = 2, Brightness = 3, FixedAdd = 4 }
