namespace SuperMetroid.Rendering.Direct3D11;

/// <summary>Mutually exclusive subscreen descriptors in the source-aware gameplay shader header.</summary>
internal enum D3D11GameplaySubscreenKind : uint
{
    CapturedBg3 = 1,
    GameplayBg2 = 2,
    GameplayBg2AndCapturedBg3 = 3,
}
