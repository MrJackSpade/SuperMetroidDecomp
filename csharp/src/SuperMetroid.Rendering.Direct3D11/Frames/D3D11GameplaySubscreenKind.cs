namespace SuperMetroid.Rendering.Direct3D11.Frames;

/// <summary>Mutually exclusive subscreen descriptors in the source-aware gameplay shader header.</summary>
internal enum D3D11GameplaySubscreenKind : uint
{
    /// <summary>Uses a captured BG3 tilemap as the shader's subscreen source.</summary>
    CapturedBg3 = 1,
    /// <summary>Uses live gameplay BG2 registers as the shader's subscreen source.</summary>
    GameplayBg2 = 2,
    /// <summary>Combines live gameplay BG2 with captured BG3 in the shader's subscreen source.</summary>
    GameplayBg2AndCapturedBg3 = 3,
}
