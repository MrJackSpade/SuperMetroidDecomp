namespace SuperMetroid.Rendering.Direct3D11;

/// <summary>CPU/HLSL upload-layout identities; changes require matching shader and verification changes.</summary>
internal static class D3D11ShaderLayout
{
    internal const int SolidHeaderWords = 4;
    internal const int MaximumBrightnessPasses = 1024;
    internal const int SolidConstantWords = SolidHeaderWords + MaximumBrightnessPasses;
    internal const int DispatchTileEdge = 8;
    internal const int ScanlineParametersWordOffset = 32;
    internal const string SolidResourceName = "SuperMetroid.Shaders.Solid.cso";
    internal const string TileResourceName = "SuperMetroid.Shaders.Tiles.cso";
    internal const string DisplayVertexResourceName = "SuperMetroid.Shaders.DisplayVertex.cso";
    internal const string DisplayPixelResourceName = "SuperMetroid.Shaders.DisplayPixel.cso";
    internal const int VramPackedWords = Core.Hardware.SnesPpuLayout.VramByteCount / sizeof(uint);
    internal const int OamPackedWordOffset = VramPackedWords + Core.Hardware.SnesPpuLayout.CgramColorCount;
    internal const int PpuMemoryWords = OamPackedWordOffset + Core.Hardware.SnesPpuLayout.OamUploadByteCount / sizeof(uint);
}

/// <summary>Exclusive compute operations shared with Tiles.hlsl.</summary>
internal enum D3D11TileOperation : uint { Backdrop = 0, Bg4 = 1, Bg2 = 2, Brightness = 3, FixedAdd = 4, ResolveObj = 5, InsertObj = 6, Mode7 = 7, ScanlineAdd = 8, BgAdd = 9, BgSubtract = 10, SubscreenAdd = 11, Message = 12 }
