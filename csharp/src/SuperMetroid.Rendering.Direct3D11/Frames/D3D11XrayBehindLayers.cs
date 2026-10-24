namespace SuperMetroid.Rendering.Direct3D11.Frames;

/// <summary>The layers the X-ray room pass blends the screen with, behind what it shows (Xray.hlsli's Behind constants).</summary>
internal enum D3D11XrayBehindLayers : uint
{
    /// <summary>Layer 3, its rows as the room effect shows them.</summary>
    Layer3 = 1,
    /// <summary>The room's layer 2.</summary>
    Layer2 = 2,
    /// <summary>The room's layer 2, then layer 3 over it.</summary>
    Layer2AndLayer3 = 3,
}
