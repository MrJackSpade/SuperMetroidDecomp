namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>
/// One frame a collectible draws over its block (#1557, #1762), named for what it shows. The members are declared in the
/// order the frames are published; their values are arbitrary identities.
/// </summary>
internal enum CollectibleDraw
{
    /// <summary>The air block left after an item is removed.</summary>
    Empty,

    /// <summary>A Chozo orb at its dimmest.</summary>
    ChozoOrbDim,

    /// <summary>A Chozo orb glowing, between its dimmest and brightest.</summary>
    ChozoOrbGlowing,

    /// <summary>A Chozo orb at its brightest.</summary>
    ChozoOrbBright,

    /// <summary>The solid white burst a Chozo orb shows as it opens.</summary>
    ChozoOrbBurst,

    /// <summary>An Energy Tank lit brightly, the first of its two frames.</summary>
    EnergyTankBright,

    /// <summary>An Energy Tank dimmed, the second of its two frames.</summary>
    EnergyTankDim,

    /// <summary>A Missile Tank dimmed, the first of its two frames.</summary>
    MissileTankDim,

    /// <summary>A Missile Tank lit brightly, the second of its two frames.</summary>
    MissileTankBright,

    /// <summary>A Super Missile Tank dimmed, the first of its two frames.</summary>
    SuperMissileTankDim,

    /// <summary>A Super Missile Tank lit brightly, the second of its two frames.</summary>
    SuperMissileTankBright,

    /// <summary>A Power Bomb Tank lit brightly, the first of its two frames.</summary>
    PowerBombTankBright,

    /// <summary>A Power Bomb Tank dimmed, the second of its two frames.</summary>
    PowerBombTankDim,

    /// <summary>The first block pair's block under an item's own picture in the picture's first frame.</summary>
    FirstBlockPairFirstFrame,

    /// <summary>The first block pair's block under an item's own picture in the picture's second frame.</summary>
    FirstBlockPairSecondFrame,

    /// <summary>The second block pair's block under an item's own picture in the picture's first frame.</summary>
    SecondBlockPairFirstFrame,

    /// <summary>The second block pair's block under an item's own picture in the picture's second frame.</summary>
    SecondBlockPairSecondFrame,

    /// <summary>The third block pair's block under an item's own picture in the picture's first frame.</summary>
    ThirdBlockPairFirstFrame,

    /// <summary>The third block pair's block under an item's own picture in the picture's second frame.</summary>
    ThirdBlockPairSecondFrame,

    /// <summary>The fourth block pair's block under an item's own picture in the picture's first frame.</summary>
    FourthBlockPairFirstFrame,

    /// <summary>The fourth block pair's block under an item's own picture in the picture's second frame.</summary>
    FourthBlockPairSecondFrame,

    /// <summary>A shot block whole, as a concealed item's reveal begins and its hiding ends.</summary>
    ShotBlockIntact,

    /// <summary>A shot block cracking apart.</summary>
    ShotBlockCracking,

    /// <summary>A shot block crumbled to dust, as a concealed item's reveal ends.</summary>
    ShotBlockDust,
}
