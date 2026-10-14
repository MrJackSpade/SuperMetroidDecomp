namespace SuperMetroid.Core.Assets;

/// <summary>The native ending and credits transfers whose exact byte lengths the catalog enforces.</summary>
public enum EndingObjectTransfer
{
    /// <summary>Atmospheric-cloud OBJ character sheet.</summary>
    Clouds,
    /// <summary>Zebes-explosion OBJ character sheet.</summary>
    Explosion,
    /// <summary>Waiting-Samus character sheet, also used for suited rewards.</summary>
    WaitingSamus,
    /// <summary>Post-credits Samus-shooting OBJ sheet.</summary>
    ShootingScreen,
    /// <summary>Under-three-hour suitless reward OBJ sheet.</summary>
    SuitlessSamus,
    /// <summary>BG2 waiting-scene tilemap.</summary>
    WaitingTilemap,
    /// <summary>BG3 transformation-character transfer.</summary>
    PostCreditsFragmentA,
    /// <summary>BG3 transformation tilemap.</summary>
    PostCreditsFragmentB,
    /// <summary>Final-logo character stream.</summary>
    PostShotLogoTiles,
    /// <summary>Final-logo tilemap.</summary>
    PostShotLogoMap,
    /// <summary>Each of the four raw explosion tilemap fragments.</summary>
    ExplosionFragment,
}
