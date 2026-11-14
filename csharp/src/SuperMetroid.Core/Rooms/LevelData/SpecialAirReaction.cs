namespace SuperMetroid.Core.Rooms.LevelData;

/// <summary>
/// What a special-air block does with its behaviour (#1722, #1742): a conveyor or the scroll trigger, the same in every
/// area, or one of the reactions whose meaning depends on the room's area. Its values are
/// arbitrary; a behaviour written for another collision kind is read as one of these through
/// <see cref="RoomBlockBehavior"/>, or as none when special air has no reaction for it.
/// </summary>
public enum SpecialAirReaction
{
    /// <summary>Samus passes with no reaction, in every area.</summary>
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
    /// The reaction that is Crateria's ice, Brinstar's floor plant, Wrecked Ship's Chozo hand and Maridia's quicksand
    /// surface.
    /// </summary>
    IceFloorPlantChozoHandOrQuicksandSurface,

    /// <summary>The reaction that is Brinstar's ceiling plant and Maridia's quicksand surface.</summary>
    CeilingPlantOrQuicksandSurface,

    /// <summary>The reaction that is Brinstar's slowly crumbling Speed Booster block that grows back and Maridia's quicksand surface.</summary>
    SlowRespawningSpeedBlockOrQuicksandSurface,

    /// <summary>
    /// The reaction that is Brinstar's slowly crumbling Speed Booster block that breaks for good, Lower Norfair's Chozo
    /// hand and Maridia's submerging quicksand.
    /// </summary>
    SlowPermanentSpeedBlockChozoHandOrSubmergingQuicksand,

    /// <summary>The reaction that is Brinstar's Speed Booster block that grows back and Maridia's slow sandfall.</summary>
    RespawningSpeedBlockOrSlowSandfall,

    /// <summary>The reaction that is Brinstar's Speed Booster block that breaks for good and Maridia's fast sandfall.</summary>
    PermanentSpeedBlockOrFastSandfall,

    /// <summary>
    /// Samus passes with no reaction in every area, as <see cref="PassesThrough"/>; the block's other collision kinds read
    /// its behaviour otherwise.
    /// </summary>
    PassesThroughInEveryArea,
}
