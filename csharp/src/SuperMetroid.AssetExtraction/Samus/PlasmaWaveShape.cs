namespace SuperMetroid.AssetExtraction.Samus;

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
    /// <summary>Thick charged horizontal seven-part group.</summary>
    HorizontalThick,
    /// <summary>Thick charged down-right twelve-part group.</summary>
    DownRightThick,
    /// <summary>Thick charged vertical seven-part group.</summary>
    VerticalThick,
    /// <summary>Thick charged down-left twelve-part group.</summary>
    DownLeftThick,
    /// <summary>Six-part down-left diagonal core.</summary>
    DownLeftShort,
    /// <summary>Ten-part down-left charged core.</summary>
    DownLeftLong,
}
