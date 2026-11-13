namespace SuperMetroid.Core.Fx.Artwork;

/// <summary>
/// The palette blend a room effect tints its overlay and the HUD with; <see cref="None"/> clears the blend. Each is named for
/// what it tints, the four Maridia waters by their retail colour. Its values are arbitrary (#1415, #1740).
/// </summary>
public enum RoomFxPaletteBlend
{
    /// <summary>Selection zero clears the effect overlay blend color instead of loading a resource.</summary>
    None = 0,
    /// <summary>The blend used primarily for lava/acid.</summary>
    Lava = 1,
    /// <summary>The blend used by Landing Site rain.</summary>
    LandingSiteRain,
    /// <summary>
    /// The deep indigo of the water around Plasma, Draygon, the Maridia elevator and the Forgotten Highway's entrance
    /// shafts, and of their dry rooms.
    /// </summary>
    IndigoMaridiaWater,
    /// <summary>The blend used by Ceres and other water/acid rooms.</summary>
    WaterAndAcid,
    /// <summary>The blend used by fog and nonliquid room states.</summary>
    Fog,
    /// <summary>The teal of western Maridia's open water: Main Street, the glass tunnel, the crab rooms and the fish tank.</summary>
    TealMaridiaWater,
    /// <summary>The dark green of central Maridia's sand rooms: the sand halls, the Oasis, Shaktool's and the Spring Ball's rooms.</summary>
    GreenMaridiaWater,
    /// <summary>The purple of eastern Maridia's water: the aqueduct, Botwoon's rooms, the Colosseum and the cactus alleys.</summary>
    PurpleMaridiaWater,
}
