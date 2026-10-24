namespace SuperMetroid.Rendering.Direct3D11.Frames;

/// <summary>CPU/HLSL upload-layout identities; changes require matching shader and verification changes.</summary>
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
    /// <summary>The word after the line parameters: the tile table of the layer covering a layer added behind it (#1392).</summary>
    internal const int CoverageTableWord = ScanlineParametersWordOffset + 224 * 4;
    /// <summary>Embedded compute shader for solid color and brightness passes.</summary>
    internal const string SolidResourceName = "SuperMetroid.Shaders.Solid.cso";
    /// <summary>Embedded compute shader for tile, object, and color-composition dispatches.</summary>
    internal const string TileResourceName = "SuperMetroid.Shaders.Tiles.cso";
    /// <summary>Embedded shader for the title-screen gradient pass.</summary>
    internal const string TitleGradientResourceName = "SuperMetroid.Shaders.TitleGradient.cso";
    /// <summary>Embedded vertex shader placing each sprite draw's quad (#1408).</summary>
    internal const string SpriteVertexResourceName = "SuperMetroid.Shaders.SpriteVertex.cso";
    /// <summary>Embedded pixel shader drawing each sprite draw's image into the sprite layer (#1408).</summary>
    internal const string SpritePixelResourceName = "SuperMetroid.Shaders.SpritePixel.cso";
    /// <summary>Embedded vertex shader for presenting the composed frame.</summary>
    internal const string DisplayVertexResourceName = "SuperMetroid.Shaders.DisplayVertex.cso";
    /// <summary>Embedded pixel shader for presenting the composed frame.</summary>
    internal const string DisplayPixelResourceName = "SuperMetroid.Shaders.DisplayPixel.cso";
}
