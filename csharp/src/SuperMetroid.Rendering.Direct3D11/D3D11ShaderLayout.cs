using SuperMetroid.Core.Hardware;
namespace SuperMetroid.Rendering.Direct3D11;

/// <summary>CPU/HLSL upload-layout identities; changes require matching shader and verification changes.</summary>
[PpuLayoutBoundary("codec: GPU upload layout of the VRAM/CGRAM/OAM render snapshot")]
internal static class D3D11ShaderLayout
{
    /// <summary>Stores solid header words.</summary>
    internal const int SolidHeaderWords = 4;
    /// <summary>Stores maximum brightness passes.</summary>
    internal const int MaximumBrightnessPasses = 1024;
    /// <summary>Stores solid constant words.</summary>
    internal const int SolidConstantWords = SolidHeaderWords + MaximumBrightnessPasses;
    /// <summary>Stores dispatch tile edge.</summary>
    internal const int DispatchTileEdge = 8;
    /// <summary>Stores scanline parameters word offset.</summary>
    internal const int ScanlineParametersWordOffset = 32;
    /// <summary>Stores window selection word.</summary>
    internal const int WindowSelectionWord = 28;
    /// <summary>Stores window logic word.</summary>
    internal const int WindowLogicWord = 29;
    /// <summary>Stores window edges word.</summary>
    internal const int WindowEdgesWord = 30;
    /// <summary>Stores window enabled word.</summary>
    internal const int WindowEnabledWord = 31;
    /// <summary>Stores solid resource name.</summary>
    internal const string SolidResourceName = "SuperMetroid.Shaders.Solid.cso";
    /// <summary>Stores tile resource name.</summary>
    internal const string TileResourceName = "SuperMetroid.Shaders.Tiles.cso";
    /// <summary>Stores title gradient resource name.</summary>
    internal const string TitleGradientResourceName = "SuperMetroid.Shaders.TitleGradient.cso";
    /// <summary>Stores display vertex resource name.</summary>
    internal const string DisplayVertexResourceName = "SuperMetroid.Shaders.DisplayVertex.cso";
    /// <summary>Stores display pixel resource name.</summary>
    internal const string DisplayPixelResourceName = "SuperMetroid.Shaders.DisplayPixel.cso";
    /// <summary>Stores vram packed words.</summary>
    internal const int VramPackedWords = Core.Hardware.SnesPpuLayout.VramByteCount / sizeof(uint);
    /// <summary>Stores oam packed word offset.</summary>
    internal const int OamPackedWordOffset = VramPackedWords + Core.Hardware.SnesPpuLayout.CgramColorCount;
    /// <summary>Stores ppu memory words.</summary>
    internal const int PpuMemoryWords = OamPackedWordOffset + Core.Hardware.SnesPpuLayout.OamUploadByteCount / sizeof(uint);
}

/// <summary>Exclusive compute operations shared with Tiles.hlsl.</summary>
internal enum D3D11TileOperation : uint { /** <summary>Identifies backdrop.</summary> */ Backdrop = 0, /** <summary>Identifies bg4.</summary> */ Bg4 = 1, /** <summary>Identifies bg2.</summary> */ Bg2 = 2, /** <summary>Identifies brightness.</summary> */ Brightness = 3, /** <summary>Identifies fixed add.</summary> */ FixedAdd = 4, /** <summary>Identifies resolve obj.</summary> */ ResolveObj = 5, /** <summary>Identifies insert obj.</summary> */ InsertObj = 6, /** <summary>Identifies mode7.</summary> */ Mode7 = 7, /** <summary>Identifies scanline add.</summary> */ ScanlineAdd = 8, /** <summary>Identifies bg add.</summary> */ BgAdd = 9, /** <summary>Identifies bg subtract.</summary> */ BgSubtract = 10, /** <summary>Identifies subscreen add.</summary> */ SubscreenAdd = 11, /** <summary>Identifies message.</summary> */ Message = 12, /** <summary>Identifies xray gameplay.</summary> */ XrayGameplay = 14 }
