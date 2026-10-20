namespace SuperMetroid.Core.Rendering;

/// <summary>Which tiles of a background a render pass draws, by their tilemap priority bit.</summary>
public enum BgTilePriority
{
    /// <summary>The low-priority tiles.</summary>
    Low,

    /// <summary>The high-priority tiles.</summary>
    High,
}
