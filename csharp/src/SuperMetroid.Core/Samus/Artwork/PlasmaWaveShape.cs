namespace SuperMetroid.Core.Samus.Artwork;

/// <summary>Native Charged_PW_PIW composition groups, identified by the actual directional instruction consumers at $93:8D4F/8D9B/8DE7/8E33 and $93:9C1B..9E3D.</summary>
internal enum PlasmaWaveShape
{
    /// <summary>Four-tile horizontal core and short lobe phases.</summary>
    HorizontalShort,
    /// <summary>Seven-tile horizontal charged core and lobes.</summary>
    HorizontalLong,
    /// <summary>Six-part down-right diagonal core.</summary>
    DownRightShort,
    /// <summary>Ten-part down-right charged core.</summary>
    DownRightLong,
    /// <summary>Four-tile vertical core.</summary>
    VerticalShort,
    /// <summary>Seven-tile vertical charged core.</summary>
    VerticalLong,
    /// <summary>Alternate charged horizontal seven-part group.</summary>
    HorizontalAlternate,
    /// <summary>Alternate charged down-right twelve-part group.</summary>
    DownRightAlternate,
    /// <summary>Alternate charged vertical seven-part group.</summary>
    VerticalAlternate,
    /// <summary>Alternate charged down-left twelve-part group.</summary>
    DownLeftAlternate,
    /// <summary>Six-part down-left diagonal core.</summary>
    DownLeftShort,
    /// <summary>Ten-part down-left charged core.</summary>
    DownLeftLong,
}
