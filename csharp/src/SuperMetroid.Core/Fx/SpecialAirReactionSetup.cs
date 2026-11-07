namespace SuperMetroid.Core.Fx;

/// <summary>What a special-air block does to Samus when she is inside or touches it.
/// Its values are arbitrary; verification's view names each member's original identity (#1415).
/// </summary>
public enum SpecialAirReactionSetup
{
    /// <summary>No reaction.</summary>
    Nothing = 1,
    /// <summary>Clears the carry: the block reacts as air.</summary>
    ClearCarry,
    /// <summary>The Brinstar floor plant, selected by Brinstar's first special air block.</summary>
    BrinstarFloorPlant,
    /// <summary>The Brinstar ceiling plant, selected by Brinstar's second special air block.</summary>
    BrinstarCeilingPlant,
    /// <summary>Ice physics.</summary>
    IcePhysics,
    /// <summary>Quicksand surface; bottom, center, and top body sampling.</summary>
    QuicksandSurface,
    /// <summary>Submerging quicksand.</summary>
    SubmergingQuicksand,
    /// <summary>Slow sandfall.</summary>
    SandFallsSlow,
    /// <summary>Fast sandfall.</summary>
    SandFallsFast,
    /// <summary>Quicksand surface collision; carry and quicksand contact publication.</summary>
    QuicksandSurfaceCollision,
    /// <summary>Submerging sand collision clears vertical speed and gravity.</summary>
    SubmergingQuicksandCollision,
    /// <summary>Sandfall collision setup that returns clear carry.</summary>
    SandFallsCollision,
    /// <summary>Speed Booster block collision setup.</summary>
    SpeedBlock,
    /// <summary>Lower Norfair Chozo-hand collision check.</summary>
    LowerNorfairChozoHand,
    /// <summary>Wrecked Ship Chozo-hand collision check.</summary>
    WreckedShipChozoHand,
}
