namespace SuperMetroid.Core.Assets;

/// <summary>Native bank-$89 room-FX blend selectors: byte offsets into $89:AA02, zero clearing the blend.</summary>
public enum RoomFxPaletteBlend : byte
{
    /// <summary>Selection zero clears the layer-3 blend color instead of loading a resource.</summary>
    None = 0,
    /// <summary>FX-record selector $02, used primarily for lava/acid.</summary>
    Lava = 0x02,
    /// <summary>FX-record selector $22, used by Landing Site rain.</summary>
    LandingSiteRain = 0x22,
    /// <summary>FX-record selector $42, used by several Maridia water rooms.</summary>
    MaridiaWaterA = 0x42,
    /// <summary>FX-record selector $48, used by Ceres and other water/acid rooms.</summary>
    WaterAndAcid = 0x48,
    /// <summary>FX-record selector $62, used by fog and nonliquid room states.</summary>
    Fog = 0x62,
    /// <summary>FX-record selector $E2, used by western Maridia water rooms.</summary>
    MaridiaWaterB = 0xe2,
    /// <summary>FX-record selector $E8, used by central Maridia water rooms.</summary>
    MaridiaWaterC = 0xe8,
    /// <summary>FX-record selector $EE, used by eastern Maridia water rooms.</summary>
    MaridiaWaterD = 0xee,
}
