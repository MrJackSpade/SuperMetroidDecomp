namespace SuperMetroid.Core.Game;

/// <summary>Mutually exclusive crawler animation families with distinct initial tables.</summary>
internal enum CrawlerAnimationFamily
{
    /// <summary>Common surface lists used by the first three native species offsets.</summary>
    Shared,
    /// <summary>Viola's distinct initial surface table and later species selection.</summary>
    Viola,
    /// <summary>Sciser's distinct initial surface table and later species selection.</summary>
    Sciser,
    /// <summary>Zero's distinct initial surface table and later species selection.</summary>
    Zero,
    /// <summary>H-Zoomer's initial table; later shared-surface selection does not use this family.</summary>
    HZoomer,
}
