namespace SuperMetroid.Core.Samus.Artwork;

/// <summary>The beam character sheets <c>BeamTilesPointers</c> at $90:C3B1 can select, valued by long address.</summary>
public enum BeamTileSource
{
    /// <summary>$9A:F200, Tiles_PowerBeam; native power-beam character source.</summary>
    Power = 0x9af200,
    /// <summary>$9A:F400, Tiles_IceBeam; native ice-beam character source.</summary>
    Ice = 0x9af400,
    /// <summary>$9A:F600, Tiles_WaveBeam; native wave and ice/wave character source.</summary>
    Wave = 0x9af600,
    /// <summary>$9A:F800, Tiles_PlasmaBeam; shared native plasma-combination character source.</summary>
    Plasma = 0x9af800,
    /// <summary>$9A:FA00, Tiles_Spazer; shared native Spazer-combination character source.</summary>
    Spazer = 0x9afa00,
    /// <summary>The bounded Chainsaw sheet read from adjacent native data.</summary>
    Chainsaw = SuperMetroid.Core.Samus.Projectiles.ChainsawBeamGraphicsDefinitionsConstants.TileSource,
    /// <summary>The bounded SpaceTime sheet read from adjacent native data.</summary>
    Spacetime = SuperMetroid.Core.Samus.Projectiles.SpacetimeBeamGraphicsDefinitionsConstants.TileSource,
}
