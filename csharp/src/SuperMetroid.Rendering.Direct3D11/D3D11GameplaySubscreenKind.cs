namespace SuperMetroid.Rendering.Direct3D11;

/// <summary>Mutually exclusive subscreen descriptors in the source-aware gameplay shader header.</summary>
internal enum D3D11GameplaySubscreenKind : uint
{
    /// <summary>The subscreen uses the captured BG3 layer and its scroll state.</summary>
    CapturedBg3 = 1,
    /// <summary>The subscreen uses gameplay BG2 registers without a captured BG3 layer.</summary>
    GameplayBg2 = 2,
    /// <summary>The subscreen combines gameplay BG2 registers with the captured BG3 layer.</summary>
    GameplayBg2AndCapturedBg3 = 3,
}
