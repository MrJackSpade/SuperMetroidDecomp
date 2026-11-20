namespace SuperMetroid.Core.Fx;

/// <summary>What a special-air block does when Samus's movement reaches it.</summary>
internal enum SpecialAirCollisionEffect
{
    /// <summary>Samus passes on.</summary>
    None,

    /// <summary>One of the Speed Booster blocks her boosted movement breaks.</summary>
    SpeedBlock,

    /// <summary>Maridia's quicksand surface, on which a falling or standing Samus lands.</summary>
    QuicksandSurface,

    /// <summary>Maridia's submerging quicksand, which stops her vertical speed and gravity.</summary>
    SubmergingQuicksand,

    /// <summary>A sandfall, through which she passes.</summary>
    Sandfall,
}
