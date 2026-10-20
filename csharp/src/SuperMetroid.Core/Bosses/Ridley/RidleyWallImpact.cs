namespace SuperMetroid.Core.Bosses.Ridley;

/// <summary>Whether Ridley's movement checks the Ceres wall impact.</summary>
internal enum RidleyWallImpact
{
    /// <summary>No wall impact is checked.</summary>
    None,

    /// <summary>Crossing the Ceres wall runs the impact routine.</summary>
    CeresWall,
}
