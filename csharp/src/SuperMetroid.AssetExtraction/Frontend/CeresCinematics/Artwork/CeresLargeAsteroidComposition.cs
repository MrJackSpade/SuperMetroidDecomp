namespace SuperMetroid.AssetExtraction.Frontend.CeresCinematics.Artwork;

/// <summary>The two bank-$8C OAM records that draw the three large asteroids.</summary>
internal enum CeresLargeAsteroidComposition
{
    /// <summary>8C:909D,station-under-attack large asteroids at OBJ priority 3,also exposed by the legacy discovery catalog.</summary>
    UnderAttack,
    /// <summary>8C:94F7,same three large asteroids at OBJ priority0.</summary>
    Approach,
}
