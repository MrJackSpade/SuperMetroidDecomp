namespace SuperMetroid.Core.Spatial;

/// <summary>What shows beyond a turning floor's edges, those of its image (#1403, #2104).</summary>
public enum FloorOverflowMode
{
    /// <summary>Nothing shows beyond the floor.</summary>
    Transparent = 0,
    /// <summary>The floor repeats beyond its edges, with the period of its image.</summary>
    Wrap = 2,
}
