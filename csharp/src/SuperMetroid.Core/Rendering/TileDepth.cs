namespace SuperMetroid.Core.Rendering;

/// <summary>Which tiles of a tile layer are drawn: those its cells place at the back, or those they place in front.</summary>
public enum TileDepth
{
    /// <summary>The tiles placed at the back.</summary>
    Back,

    /// <summary>The tiles placed in front.</summary>
    Front,
}
