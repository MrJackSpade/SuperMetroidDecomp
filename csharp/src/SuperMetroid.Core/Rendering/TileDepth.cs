namespace SuperMetroid.Core.Rendering;

/// <summary>Which tiles of a background a render pass draws, by their tilemap priority bit.</summary>
public enum TileDepth
{
    /// <summary>The low-priority tiles.</summary>
    Back,

    /// <summary>The high-priority tiles.</summary>
    Front,
}
