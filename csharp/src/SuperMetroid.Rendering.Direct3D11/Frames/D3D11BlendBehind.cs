namespace SuperMetroid.Rendering.Direct3D11.Frames;

/// <summary>What lies behind a gameplay blend's blended pixels for it to combine with (#1514); Xray.hlsli's Behind constants.</summary>
internal enum D3D11BlendBehind : uint
{
    /// <summary>The room's effect overlay, its rows as the room effect shows them.</summary>
    Overlay = 1,
    /// <summary>The room's background.</summary>
    Background = 2,
    /// <summary>The room's background with the effect overlay: the overlay's front tiles over the background, its back tiles under it.</summary>
    BackgroundAndOverlay = 3,
}
