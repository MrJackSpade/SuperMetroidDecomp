namespace SuperMetroid.Core.Rooms.LevelData;

/// <summary>
/// What a special-air block does with its behaviour (#1722): a conveyor or the scroll trigger every area shares, or one of the
/// reactions the room's area gives special air, which mean what that area's table makes of them (the special-air reaction
/// tables in <c>SpecialAirReactions</c>). Its values are arbitrary; a behaviour written for another collision kind is read
/// as one of these through <see cref="RoomBlockBehavior"/>.
/// </summary>
public enum SpecialAirReaction
{
    /// <summary>A common reaction that lets Samus pass with no reaction.</summary>
    PassesThrough,

    /// <summary>A conveyor carrying Samus rightward while she stands on it, unless the area's power is off.</summary>
    RightwardConveyorOnFoot,

    /// <summary>A conveyor carrying Samus leftward while she stands on it, unless the area's power is off.</summary>
    LeftwardConveyorOnFoot,

    /// <summary>A conveyor always carrying Samus rightward.</summary>
    RightwardConveyor,

    /// <summary>A conveyor always carrying Samus leftward.</summary>
    LeftwardConveyor,

    /// <summary>The trigger a scroll room object gives its block; the X-ray scope reveals it.</summary>
    ScrollTrigger,

    /// <summary>
    /// The area reaction that is Crateria's ice, Brinstar's floor plant, Wrecked Ship's Chozo hand and Maridia's quicksand
    /// surface.
    /// </summary>
    IceFloorPlantChozoHandOrQuicksandSurface,

    /// <summary>The area reaction that is Brinstar's ceiling plant and Maridia's quicksand surface.</summary>
    CeilingPlantOrQuicksandSurface,

    /// <summary>The area reaction that is Brinstar's slowly crumbling Speed Booster block that grows back and Maridia's quicksand surface.</summary>
    SlowRespawningSpeedBlockOrQuicksandSurface,

    /// <summary>
    /// The area reaction that is Brinstar's slowly crumbling Speed Booster block that breaks for good, Lower Norfair's Chozo
    /// hand and Maridia's submerging quicksand.
    /// </summary>
    SlowPermanentSpeedBlockChozoHandOrSubmergingQuicksand,

    /// <summary>The area reaction that is Brinstar's Speed Booster block that grows back and Maridia's slow sandfall.</summary>
    RespawningSpeedBlockOrSlowSandfall,

    /// <summary>The area reaction that is Brinstar's Speed Booster block that breaks for good and Maridia's fast sandfall.</summary>
    PermanentSpeedBlockOrFastSandfall,

    /// <summary>An area reaction no area gives anything to do.</summary>
    AreaNothing,

    /// <summary>An area reaction past the areas' tables: the special-air dispatchers reject it.</summary>
    PastAreaTable,
}
