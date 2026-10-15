namespace SuperMetroid.Core.Frontend.CeresCinematics.Artwork;

/// <summary>The seven unique Ceres destruction and Zebes reveal backdrop spritemaps, valued by their bank-$8C pointer.</summary>
public enum CeresDestructionBackdrop : ushort
{
    /// <summary>8C:909D, SpaceSpritemaps_CeresUnderAttackLargeAsteroids.</summary>
    LargeAsteroids = 0x909d,
    /// <summary>8C:9558, SpaceSpritemaps_Zebes.</summary>
    Planet = 0x9558,
    /// <summary>8C:9654, SpaceSpritemaps_PlanetZebes title.</summary>
    Title = 0x9654,
    /// <summary>8C:975E, SpaceSpritemaps_ZebesStars2, upper-left sheet.</summary>
    UpperLeftStars = 0x975e,
    /// <summary>8C:979C, SpaceSpritemaps_ZebesStars3, upper-right sheet.</summary>
    UpperRightStars = 0x979c,
    /// <summary>8C:97BC, SpaceSpritemaps_ZebesStars4, lower-left sheet.</summary>
    LowerLeftStars = 0x97bc,
    /// <summary>8C:97D2, SpaceSpritemaps_ZebesStars5, lower-right sheet.</summary>
    LowerRightStars = 0x97d2,
}
