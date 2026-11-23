namespace SuperMetroid.Core.Samus.Movement;

/// <summary>A block of a plain terrain (<see cref="PlainTerrain"/>). The values are arbitrary: compare kinds by name.</summary>
internal enum PlainTerrainBlock
{
    /// <summary>Empty air.</summary>
    Air,
    /// <summary>A fully solid block.</summary>
    Solid,
    /// <summary>A slope whose geometry is the empty behaviour's.</summary>
    Slope,
}
