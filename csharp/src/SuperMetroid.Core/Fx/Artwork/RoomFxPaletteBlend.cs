namespace SuperMetroid.Core.Fx.Artwork;

/// <summary>
/// The palette blend a room-FX record selects; <see cref="None"/> clears the blend. Its values are arbitrary; the
/// import decodes the record's blend selector (#1415).
/// </summary>
public enum RoomFxPaletteBlend
{
    /// <summary>Selection zero clears the effect overlay blend color instead of loading a resource.</summary>
    None = 0,
    /// <summary>The blend used primarily for lava/acid.</summary>
    Lava = 1,
    /// <summary>The blend used by Landing Site rain.</summary>
    LandingSiteRain,
    /// <summary>The blend used by several Maridia water rooms.</summary>
    MaridiaWaterA,
    /// <summary>The blend used by Ceres and other water/acid rooms.</summary>
    WaterAndAcid,
    /// <summary>The blend used by fog and nonliquid room states.</summary>
    Fog,
    /// <summary>The blend used by western Maridia water rooms.</summary>
    MaridiaWaterB,
    /// <summary>The blend used by central Maridia water rooms.</summary>
    MaridiaWaterC,
    /// <summary>The blend used by eastern Maridia water rooms.</summary>
    MaridiaWaterD,
}
