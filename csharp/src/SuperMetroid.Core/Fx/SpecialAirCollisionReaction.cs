namespace SuperMetroid.Core.Fx;

/// <summary>
/// What a special-air block does when Samus's movement reaches it, as her room's area names the block's reaction
/// (<see cref="SpecialAirReactions.CollisionOf"/>). Its values are arbitrary.
/// </summary>
public enum SpecialAirCollisionReaction
{
    /// <summary>Nothing happens: Samus passes on.</summary>
    Nothing = 1,

    /// <summary>Samus passes through the block as through air.</summary>
    PassesThrough,

    /// <summary>Brinstar's slow respawning Speed Booster block.</summary>
    BrinstarSlowRespawningSpeedBlock,

    /// <summary>Brinstar's slow permanent Speed Booster block.</summary>
    BrinstarSlowPermanentSpeedBlock,

    /// <summary>The Dachora room's respawning Speed Booster block.</summary>
    DachoraRespawningSpeedBlock,

    /// <summary>Brinstar's permanent Speed Booster block.</summary>
    PermanentSpeedBlock,

    /// <summary>The Lower Norfair Chozo hand's trigger.</summary>
    LowerNorfairChozoHand,

    /// <summary>The Wrecked Ship Chozo hand's trigger.</summary>
    WreckedShipChozoHand,

    /// <summary>Maridia's quicksand surface, on which a falling or standing Samus lands.</summary>
    QuicksandSurface,

    /// <summary>Maridia's submerging quicksand, which stops Samus's vertical speed and gravity.</summary>
    SubmergingQuicksand,

    /// <summary>Maridia's slow sandfall, through which Samus passes.</summary>
    SlowSandfall,

    /// <summary>Maridia's fast sandfall, through which Samus passes.</summary>
    FastSandfall,
}
