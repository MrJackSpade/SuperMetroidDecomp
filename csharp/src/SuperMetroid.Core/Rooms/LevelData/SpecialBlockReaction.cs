namespace SuperMetroid.Core.Rooms.LevelData;

/// <summary>
/// What a special block does with its behaviour (#1722): the crumble block, Speed Booster block, station access, collectible
/// or Chozo hand Samus's contact and a bomb select. Its values are arbitrary; a behaviour written for another collision kind
/// is read as one of these through <see cref="RoomBlockBehavior"/>.
/// </summary>
public enum SpecialBlockReaction
{
    /// <summary>One block that crumbles under Samus and grows back.</summary>
    RespawningCrumble1x1,

    /// <summary>Two blocks side by side that crumble under Samus and grow back.</summary>
    RespawningCrumble2x1,

    /// <summary>Two stacked blocks that crumble under Samus and grow back.</summary>
    RespawningCrumble1x2,

    /// <summary>A two-by-two square of blocks that crumbles under Samus and grows back.</summary>
    RespawningCrumble2x2,

    /// <summary>One block that crumbles under Samus for good.</summary>
    PermanentCrumble1x1,

    /// <summary>Two blocks side by side that crumble under Samus for good.</summary>
    PermanentCrumble2x1,

    /// <summary>Two stacked blocks that crumble under Samus for good.</summary>
    PermanentCrumble1x2,

    /// <summary>A two-by-two square of blocks that crumbles under Samus for good.</summary>
    PermanentCrumble2x2,

    /// <summary>A solid block the X-ray scope reveals as one block.</summary>
    XrayRevealedSolid,

    /// <summary>The Speed Booster block every area has that grows back.</summary>
    RespawningSpeedBlock,

    /// <summary>The Speed Booster block every area has that breaks for good.</summary>
    PermanentSpeedBlock,

    /// <summary>A solid block a normal bomb shows as a Speed Booster block for an update.</summary>
    BombRevealsSpeedBlock,

    /// <summary>The visible frame of a collectible: Samus's touch is passed to the collectible.</summary>
    CollectibleTrigger,

    /// <summary>The map station's access block on the station's right side.</summary>
    MapStationRight,

    /// <summary>The map station's access block on the station's left side.</summary>
    MapStationLeft,

    /// <summary>The energy station's access block on the station's right side.</summary>
    EnergyStationRight,

    /// <summary>The energy station's access block on the station's left side.</summary>
    EnergyStationLeft,

    /// <summary>The missile station's access block on the station's right side.</summary>
    MissileStationRight,

    /// <summary>The missile station's access block on the station's left side.</summary>
    MissileStationLeft,

    /// <summary>The save station's floor access block.</summary>
    SaveStationFloor,

    /// <summary>A plain solid block: a bomb sets off a one-update object.</summary>
    Solid,

    /// <summary>A reaction past the end of the bombed-special table: a bomb rejects it.</summary>
    PastTable,

    /// <summary>The area reaction Wrecked Ship's Chozo statue gives its hand; elsewhere a plain solid block.</summary>
    WreckedShipChozoHand,

    /// <summary>An area reaction that is a plain solid block in every area.</summary>
    AreaSolid,

    /// <summary>Brinstar's slowly crumbling Speed Booster block that grows back; elsewhere a plain solid block.</summary>
    BrinstarSlowRespawningSpeedBlock,

    /// <summary>
    /// Brinstar's slowly crumbling Speed Booster block that breaks for good, and the area reaction Lower Norfair's Chozo
    /// statue gives its hand; elsewhere a plain solid block.
    /// </summary>
    BrinstarSlowPermanentSpeedBlockOrNorfairChozoHand,

    /// <summary>Brinstar's Speed Booster block that grows back (the Dachora room's); elsewhere a plain solid block.</summary>
    BrinstarRespawningSpeedBlock,

    /// <summary>Brinstar's Speed Booster block that breaks for good; elsewhere a plain solid block.</summary>
    BrinstarPermanentSpeedBlock,

    /// <summary>An area reaction past the area's table: a bomb rejects it.</summary>
    PastAreaTable,
}
