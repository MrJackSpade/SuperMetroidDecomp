namespace SuperMetroid.Rendering.Direct3D11;

/// <summary>Mutually exclusive subscreen descriptors in the source-aware gameplay shader header.</summary>
internal enum D3D11GameplaySubscreenKind : uint
{
    None,
    CapturedBg3,
    GameplayBg2,
}
