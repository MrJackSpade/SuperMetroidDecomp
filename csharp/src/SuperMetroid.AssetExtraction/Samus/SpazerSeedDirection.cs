namespace SuperMetroid.AssetExtraction.Samus;

/// <summary>$93:D10E/D25A/D3A6/D4F2/D63E/D6EA/D796/D842: native seed travel directions; chosen footprint lengths/glyphs/priority remain REQUIRED.</summary>
internal enum SpazerSeedDirection
{
    /// <summary>Up-right four-cell diagonal seed.</summary>
    UpRight,
    /// <summary>Down-right four-cell diagonal seed.</summary>
    DownRight,
    /// <summary>Down-left four-cell diagonal seed.</summary>
    DownLeft,
    /// <summary>Up-left four-cell diagonal seed.</summary>
    UpLeft,
    /// <summary>Downward two-cell axial seed.</summary>
    Down,
    /// <summary>Leftward two-cell axial seed.</summary>
    Left,
    /// <summary>Upward two-cell axial seed.</summary>
    Up,
    /// <summary>Rightward two-cell axial seed.</summary>
    Right,
}
