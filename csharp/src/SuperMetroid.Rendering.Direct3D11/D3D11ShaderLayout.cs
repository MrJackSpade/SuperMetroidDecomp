namespace SuperMetroid.Rendering.Direct3D11;

/// <summary>CPU/HLSL upload-layout identities; changes require matching shader and verification changes.</summary>
internal static class D3D11ShaderLayout
{
    internal const int SolidHeaderWords = 4;
    internal const int MaximumBrightnessPasses = 1024;
    internal const int SolidConstantWords = SolidHeaderWords + MaximumBrightnessPasses;
    internal const int DispatchTileEdge = 8;
    internal const string SolidResourceName = "SuperMetroid.Shaders.Solid.cso";
}
