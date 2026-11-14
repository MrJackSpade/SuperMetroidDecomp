namespace SuperMetroid.Core.Rooms.LevelData;

/// <summary>
/// What a special-air block does with its behaviour (#1722, #1742, #1805): a conveyor or the scroll trigger, the same in
/// every area, or one of the reactions an area's own table gives (ice, the plants, quicksand and sandfalls, Speed Booster
/// blocks and the Chozo hands). The import resolves what the room's area makes of the area's own reactions into these, so
/// each names one thing (<c>SpecialAirReactions</c>). Its values are arbitrary; a behaviour written for another collision
/// kind is read as one of these through <see cref="RoomBlockBehavior"/>, or as none when special air has no reaction for it.
/// </summary>
public enum SpecialAirReaction
{
    /// <summary>Samus passes with no reaction.</summary>
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

    /// <summary>Crateria's ice floor.</summary>
    Ice,

    /// <summary>The Brinstar plant that bites Samus from the floor.</summary>
    FloorPlant,

    /// <summary>The Brinstar plant that bites Samus from the ceiling.</summary>
    CeilingPlant,

    /// <summary>Maridia's quicksand surface.</summary>
    QuicksandSurface,

    /// <summary>Maridia's submerging quicksand.</summary>
    SubmergingQuicksand,

    /// <summary>Maridia's slow sandfall.</summary>
    SlowSandfall,

    /// <summary>Maridia's fast sandfall.</summary>
    FastSandfall,

    /// <summary>Brinstar's slowly crumbling Speed Booster block that grows back.</summary>
    SlowRespawningSpeedBlock,

    /// <summary>Brinstar's slowly crumbling Speed Booster block that breaks for good.</summary>
    SlowPermanentSpeedBlock,

    /// <summary>The Dachora room's Speed Booster block that grows back.</summary>
    DachoraRespawningSpeedBlock,

    /// <summary>Brinstar's Speed Booster block that breaks for good.</summary>
    PermanentSpeedBlock,

    /// <summary>The Wrecked Ship Chozo statue hand's trigger.</summary>
    WreckedShipChozoHand,

    /// <summary>The Lower Norfair Chozo statue hand's trigger.</summary>
    LowerNorfairChozoHand,
}
