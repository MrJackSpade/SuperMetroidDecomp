namespace SuperMetroid.Rendering.Direct3D11;

/// <summary>Mutually exclusive subscreen descriptors in the source-aware gameplay shader header.</summary>
internal enum D3D11GameplaySubscreenKind : uint
{
    /// <summary>Identifies captured bg3.</summary>
    CapturedBg3 = 1,
    /// <summary>Identifies gameplay bg2.</summary>
    GameplayBg2 = 2,
    /// <summary>Identifies gameplay bg2 and captured bg3.</summary>
    GameplayBg2AndCapturedBg3 = 3,
}
