namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>Which settled colouring a Torizo's body and belly show (#1512).</summary>
public enum TorizoLookKind
{
    /// <summary>Bomb Torizo's statue colours, before he wakes.</summary>
    BombStatue,

    /// <summary>Golden Torizo's colours before his first hurt frame.</summary>
    GoldenInitial,

    /// <summary>Bomb Torizo's normal colours, which his fade wakes him into and his hurt frames restore.</summary>
    Normal,

    /// <summary>Golden Torizo's colours, which his fade wakes him into.</summary>
    Golden,

    /// <summary>White, as a hurt frame flashes.</summary>
    White,

    /// <summary>Black, as a dead Torizo fades out.</summary>
    Black,

    /// <summary>One of Golden Torizo's health bands.</summary>
    GoldenHealth,
}
