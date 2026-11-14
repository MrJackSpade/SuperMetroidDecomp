namespace SuperMetroid.Core.Rooms.LevelData;

/// <summary>
/// What a special block does with its behaviour (#1722, #1805): the crumble block, Speed Booster block, station access,
/// collectible or Chozo hand Samus's contact and a bomb select. The import resolves what the room's area makes of the
/// area's own reactions into these, so each names one thing in the areas that have it. Its values are arbitrary; a
/// behaviour written for another collision kind is read as one of these through <see cref="RoomBlockBehavior"/>, or as
/// none when special blocks have no reaction for it.
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

    /// <summary>
    /// A solid block a normal bomb shows as a Speed Booster block for an update, in every area
    /// (<see cref="Compatibility.SpecialBlockBombRunOn{TReaction, TChange}"/>).
    /// </summary>
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

    /// <summary>The Wrecked Ship Chozo statue's hand, which a morphed Samus's landing hands to the statue.</summary>
    WreckedShipChozoHand,

    /// <summary>The Lower Norfair Chozo statue's hand, which a morphed Samus's landing hands to the statue.</summary>
    LowerNorfairChozoHand,

    /// <summary>Brinstar's slowly crumbling Speed Booster block that grows back.</summary>
    SlowRespawningSpeedBlock,

    /// <summary>Brinstar's slowly crumbling Speed Booster block that breaks for good.</summary>
    SlowPermanentSpeedBlock,

    /// <summary>Brinstar's own Speed Booster block that grows back (the Dachora room's), which the X-ray scope shows as Brinstar's.</summary>
    BrinstarRespawningSpeedBlock,

    /// <summary>Brinstar's own Speed Booster block that breaks for good, which the X-ray scope shows as Brinstar's.</summary>
    BrinstarPermanentSpeedBlock,
}