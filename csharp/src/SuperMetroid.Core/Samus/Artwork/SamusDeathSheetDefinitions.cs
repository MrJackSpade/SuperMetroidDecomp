namespace SuperMetroid.Core.Samus.Artwork;

/// <summary>
/// The pre-coloured sheet of Samus's suit explosion as she dies (#1392): each explosion frame facing right and left,
/// which Samus's death drawing shows over her death pose.
/// </summary>
public static class SamusDeathSheetDefinitions
{
    /// <summary>
    /// The explosion's animation, its frames named for what they show (#2285): the suit glowing, cracking and shattering,
    /// its pieces flying out and scattering, then Samus without it, her hair streaming, gleaming and dimming.
    /// </summary>
    public static IReadOnlyList<string> Explosion { get; } =
    [
        "SuitGlowing", "SuitCracking", "SuitShattering", "PiecesFlying", "PiecesSpreading", "PiecesScattered",
        "SuitlessHairStreaming", "SuitlessHairGleaming", "SuitlessHairDimming",
    ];

    /// <summary>The explosion's frame at step <paramref name="step"/> of its animation, facing <paramref name="facing"/>.</summary>
    public static string ExplosionFrame(HorizontalDirection facing, int step) => $"Explosion.{facing}.{Explosion[step]}";
}
