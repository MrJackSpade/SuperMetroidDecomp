namespace SuperMetroid.Core.Fx.Artwork;

/// <summary>
/// The palette blend a room-FX record selects; <see cref="None"/> clears the blend. Its values are arbitrary; the
/// import decodes the record's selector byte (#1415).
/// </summary>
public enum RoomFxPaletteBlend : byte
{
    /// <summary>Selection zero clears the layer-3 blend color instead of loading a resource.</summary>
    None = 0,
    /// <summary>FX-record selector $02, used primarily for lava/acid.</summary>
    Lava = 1,
    /// <summary>FX-record selector $22, used by Landing Site rain.</summary>
    LandingSiteRain,
    /// <summary>FX-record selector $42, used by several Maridia water rooms.</summary>
    MaridiaWaterA,
    /// <summary>FX-record selector $48, used by Ceres and other water/acid rooms.</summary>
    WaterAndAcid,
    /// <summary>FX-record selector $62, used by fog and nonliquid room states.</summary>
    Fog,
    /// <summary>FX-record selector $E2, used by western Maridia water rooms.</summary>
    MaridiaWaterB,
    /// <summary>FX-record selector $E8, used by central Maridia water rooms.</summary>
    MaridiaWaterC,
    /// <summary>FX-record selector $EE, used by eastern Maridia water rooms.</summary>
    MaridiaWaterD,
}
