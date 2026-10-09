namespace SuperMetroid.Rendering.Direct3D11;

/// <summary>CPU/HLSL upload-layout identities; changes require matching shader and verification changes.</summary>
internal static class D3D11ShaderLayout
{
    /// <summary>Number of constant-buffer words reserved for the solid shader's color, pass count, width, and height.</summary>
    internal const int SolidHeaderWords = 4;
    /// <summary>Maximum brightness-pass entries accepted by the solid renderer and sized into its constant buffer.</summary>
    internal const int MaximumBrightnessPasses = 1024;
    /// <summary>Total solid-shader constant-buffer capacity: the four-word header followed by brightness-pass values.</summary>
    internal const int SolidConstantWords = SolidHeaderWords + MaximumBrightnessPasses;
    /// <summary>Width and height of each compute-shader thread group used to cover a render target.</summary>
    internal const int DispatchTileEdge = 8;
    /// <summary>First constant-buffer word of the 224 per-scanline parameter vectors shared with tile shaders.</summary>
    internal const int ScanlineParametersWordOffset = 32;
    /// <summary>Constant-buffer word carrying the selected hardware-window enables and inversion bits.</summary>
    internal const int WindowSelectionWord = 28;
    /// <summary>Constant-buffer word choosing the hardware-window combination logic for the current target.</summary>
    internal const int WindowLogicWord = 29;
    /// <summary>Constant-buffer word packing the left and right edges of both hardware windows into four bytes.</summary>
    internal const int WindowEdgesWord = 30;
    /// <summary>Constant-buffer word indicating whether hardware-window masking applies to the draw.</summary>
    internal const int WindowEnabledWord = 31;
    /// <summary>Manifest resource path of the compute shader for solid-color composition.</summary>
    internal const string SolidResourceName = "SuperMetroid.Shaders.Solid.cso";
    /// <summary>Manifest resource path of the compute shader that renders tile-based layers.</summary>
    internal const string TileResourceName = "SuperMetroid.Shaders.Tiles.cso";
    /// <summary>Manifest resource path of the compute shader that composes the title gradient.</summary>
    internal const string TitleGradientResourceName = "SuperMetroid.Shaders.TitleGradient.cso";
    /// <summary>Manifest resource path of the vertex shader used to display the composed frame.</summary>
    internal const string DisplayVertexResourceName = "SuperMetroid.Shaders.DisplayVertex.cso";
    /// <summary>Manifest resource path of the pixel shader used to display the composed frame.</summary>
    internal const string DisplayPixelResourceName = "SuperMetroid.Shaders.DisplayPixel.cso";
    /// <summary>Number of 32-bit upload words occupied by SNES VRAM before CGRAM data begins.</summary>
    internal const int VramPackedWords = Core.Hardware.SnesPpuLayout.VramByteCount / sizeof(uint);
    /// <summary>Word offset immediately after packed VRAM and CGRAM, where packed OAM bytes are copied.</summary>
    internal const int OamPackedWordOffset = VramPackedWords + Core.Hardware.SnesPpuLayout.CgramColorCount;
    /// <summary>Total word count of the packed VRAM, CGRAM, and OAM buffer exposed to shaders.</summary>
    internal const int PpuMemoryWords = OamPackedWordOffset + Core.Hardware.SnesPpuLayout.OamUploadByteCount / sizeof(uint);
}

/// <summary>Exclusive compute operations shared with Tiles.hlsl.</summary>
internal enum D3D11TileOperation : uint
{
    /// <summary>Writes palette entry zero as the backdrop without reading a tile layer.</summary>
    Backdrop = 0,
    Bg4 = 1,
    Bg2 = 2,
    Brightness = 3,
    FixedAdd = 4,
    ResolveObj = 5,
    InsertObj = 6,
    Mode7 = 7,
    ScanlineAdd = 8,
    BgAdd = 9,
    BgSubtract = 10,
    SubscreenAdd = 11,
    Message = 12,
    XrayGameplay = 14
}
