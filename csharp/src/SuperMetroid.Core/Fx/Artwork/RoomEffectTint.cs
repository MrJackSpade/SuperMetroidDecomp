namespace SuperMetroid.Core.Fx.Artwork;

/// <summary>
/// The tint a room effect lays over its overlay and the HUD; <see cref="None"/> leaves their colours but darkens one.
/// Each is named for what it
/// tints, the four Maridia waters by their colour. Its values are arbitrary (#1415, #1740, #1898).
/// </summary>
public enum RoomEffectTint
{
    /// <summary>
    /// The untinted look: the overlay and the HUD keep their own colours, except one that every tint recolours, which
    /// shows black.
    /// </summary>
    None,
    /// <summary>The tint of lava and acid.</summary>
    Lava,
    /// <summary>The tint of the Landing Site's rain.</summary>
    LandingSiteRain,
    /// <summary>
    /// The deep indigo of the water around Plasma, Draygon, the Maridia elevator and the Forgotten Highway's entrance
    /// shafts, and of their dry rooms.
    /// </summary>
    IndigoMaridiaWater,
    /// <summary>The tint of Ceres and of other water and acid rooms.</summary>
    WaterAndAcid,
    /// <summary>The tint of fog and of room states without a liquid.</summary>
    Fog,
    /// <summary>The teal of western Maridia's open water: Main Street, the glass tunnel, the crab rooms and the fish tank.</summary>
    TealMaridiaWater,
    /// <summary>The dark green of central Maridia's sand rooms: the sand halls, the Oasis, Shaktool's and the Spring Ball's rooms.</summary>
    GreenMaridiaWater,
    /// <summary>The purple of eastern Maridia's water: the aqueduct, Botwoon's rooms, the Colosseum and the cactus alleys.</summary>
    PurpleMaridiaWater,
}
