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
    /// <summary>Composites a four-bit-per-pixel background with its scroll, priority, transparency, and window parameters.</summary>
    Bg4 = 1,
    /// <summary>Composites a two-bit-per-pixel background with its scroll, priority, transparency, and window parameters.</summary>
    Bg2 = 2,
    /// <summary>Scales the composed output by the submitted display-brightness level.</summary>
    Brightness = 3,
    /// <summary>Adds the submitted fixed RGB color to the composed output.</summary>
    FixedAdd = 4,
    /// <summary>Resolves modeled OAM entries into the intermediate object buffer.</summary>
    ResolveObj = 5,
    /// <summary>Composites resolved objects for the selected priority and scanline range.</summary>
    InsertObj = 6,
    /// <summary>Composites an affine Mode 7 background from the submitted matrix and scroll registers.</summary>
    Mode7 = 7,
    /// <summary>Adds per-scanline fixed colors inside the submitted horizontal spans.</summary>
    ScanlineAdd = 8,
    /// <summary>Adds a tile-background color-math source to the existing output.</summary>
    BgAdd = 9,
    /// <summary>Subtracts a tile-background color-math source from the existing output.</summary>
    BgSubtract = 10,
    /// <summary>Builds the selected background-and-object subscreen and adds it to the main output.</summary>
    SubscreenAdd = 11,
    /// <summary>Draws the centered gameplay message window from its character tilemap.</summary>
    Message = 12,
    /// <summary>Composites captured gameplay layers with X-ray window and color-math behavior.</summary>
    XrayGameplay = 14
}
