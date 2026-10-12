using SuperMetroid.Core.Hardware;
namespace SuperMetroid.Rendering.Direct3D11;

/// <summary>CPU/HLSL upload-layout identities; changes require matching shader and verification changes.</summary>
[PpuLayoutBoundary("codec: GPU upload layout of the VRAM/CGRAM/OAM render snapshot")]
internal static class D3D11ShaderLayout
{
    /// <summary>Four control words precede the variable brightness-pass levels in the solid shader buffer.</summary>
    internal const int SolidHeaderWords = 4;
    /// <summary>Caps the per-frame brightness commands accepted by the solid shader upload.</summary>
    internal const int MaximumBrightnessPasses = 1024;
    /// <summary>Allocates the solid constant buffer for its header and worst-case brightness commands.</summary>
    internal const int SolidConstantWords = SolidHeaderWords + MaximumBrightnessPasses;
    /// <summary>Compute dispatches cover the target in eight-pixel workgroup tiles.</summary>
    internal const int DispatchTileEdge = 8;
    /// <summary>Per-scanline shader parameters begin after the fixed dispatch header.</summary>
    internal const int ScanlineParametersWordOffset = 32;
    /// <summary>Shader header slot selecting the two SNES window masks.</summary>
    internal const int WindowSelectionWord = 28;
    /// <summary>Shader header slot carrying the selected window combination operation.</summary>
    internal const int WindowLogicWord = 29;
    /// <summary>Shader header slot packing the horizontal edges of both windows.</summary>
    internal const int WindowEdgesWord = 30;
    /// <summary>Shader header slot indicating that window clipping applies to this dispatch.</summary>
    internal const int WindowEnabledWord = 31;
    /// <summary>Embedded compute shader for solid color and brightness passes.</summary>
    internal const string SolidResourceName = "SuperMetroid.Shaders.Solid.cso";
    /// <summary>Embedded compute shader for tile, object, and color-composition dispatches.</summary>
    internal const string TileResourceName = "SuperMetroid.Shaders.Tiles.cso";
    /// <summary>Embedded shader for the title-screen gradient pass.</summary>
    internal const string TitleGradientResourceName = "SuperMetroid.Shaders.TitleGradient.cso";
    /// <summary>Embedded vertex shader for presenting the composed frame.</summary>
    internal const string DisplayVertexResourceName = "SuperMetroid.Shaders.DisplayVertex.cso";
    /// <summary>Embedded pixel shader for presenting the composed frame.</summary>
    internal const string DisplayPixelResourceName = "SuperMetroid.Shaders.DisplayPixel.cso";
    /// <summary>Number of 32-bit upload words occupied by the VRAM snapshot.</summary>
    internal const int VramPackedWords = Core.Hardware.SnesPpuLayout.VramByteCount / sizeof(uint);
    /// <summary>Start of the OAM snapshot after packed VRAM and one word per CGRAM color.</summary>
    internal const int OamPackedWordOffset = VramPackedWords + Core.Hardware.SnesPpuLayout.CgramColorCount;
    /// <summary>Total upload words needed for VRAM, CGRAM, and packed OAM.</summary>
    internal const int PpuMemoryWords = OamPackedWordOffset + Core.Hardware.SnesPpuLayout.OamUploadByteCount / sizeof(uint);
}

/// <summary>Exclusive compute operations shared with Tiles.hlsl.</summary>
internal enum D3D11TileOperation : uint
{
    /// <summary>Initializes the output with the current backdrop color.</summary>
    Backdrop = 0,
    /// <summary>Composes a four-bit background layer.</summary>
    Bg4 = 1,
    /// <summary>Composes a two-bit background layer.</summary>
    Bg2 = 2,
    /// <summary>Applies one requested brightness level.</summary>
    Brightness = 3,
    /// <summary>Adds a fixed color to the current composition.</summary>
    FixedAdd = 4,
    /// <summary>Decodes object pixels and priorities from the uploaded OAM snapshot.</summary>
    ResolveObj = 5,
    /// <summary>Inserts selected object pixels into the current layer order.</summary>
    InsertObj = 6,
    /// <summary>Composes the affine Mode 7 background.</summary>
    Mode7 = 7,
    /// <summary>Adds per-scanline color contributions.</summary>
    ScanlineAdd = 8,
    /// <summary>Adds the selected background colors to the current composition.</summary>
    BgAdd = 9,
    /// <summary>Subtracts the selected background colors from the current composition.</summary>
    BgSubtract = 10,
    /// <summary>Adds the subscreen contribution to the main-screen output.</summary>
    SubscreenAdd = 11,
    /// <summary>Composes the message-box tilemap over the scene.</summary>
    Message = 12,
    /// <summary>Composes the X-ray gameplay overlay.</summary>
    XrayGameplay = 14
}
