namespace SuperMetroid.Core.Areas.Tourian;

/// <summary>What one of the Tourian entrance statue scene's three placements does as it initializes (#2014).</summary>
internal enum TourianEntranceStatueRole
{
    /// <summary>The scene's first placement: it spawns the statue's base decoration, Ridley and Phantoon.</summary>
    SpawnsDecorations,

    /// <summary>One of the other two placements, which spawns nothing.</summary>
    Inert,
}
