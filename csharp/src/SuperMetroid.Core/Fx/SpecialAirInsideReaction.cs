namespace SuperMetroid.Core.Fx;

/// <summary>
/// What a special-air block does to Samus while she is inside it, as her room's area names the block's reaction
/// (<see cref="SpecialAirReactions.InsideOf"/>). Its values are arbitrary.
/// </summary>
public enum SpecialAirInsideReaction
{
    /// <summary>Nothing happens.</summary>
    Nothing = 1,

    /// <summary>Crateria's ice floor, which changes nothing while Samus is inside it.</summary>
    IcePhysics,

    /// <summary>The Brinstar plant that bites Samus from the floor.</summary>
    BrinstarFloorPlant,

    /// <summary>The Brinstar plant that bites Samus from the ceiling.</summary>
    BrinstarCeilingPlant,

    /// <summary>Maridia's quicksand surface: it cancels running momentum and sinks or holds Samus by her vertical direction.</summary>
    QuicksandSurface,

    /// <summary>Maridia's submerging quicksand, which pulls Samus down.</summary>
    SubmergingQuicksand,

    /// <summary>Maridia's slow sandfall, which carries Samus down.</summary>
    SlowSandfall,

    /// <summary>Maridia's fast sandfall, which carries Samus down faster.</summary>
    FastSandfall,
}
