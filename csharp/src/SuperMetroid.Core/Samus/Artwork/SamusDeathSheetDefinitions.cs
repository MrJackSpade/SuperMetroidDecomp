using F = SuperMetroid.Core.Samus.Artwork.SamusDeathExplosionFrame;

namespace SuperMetroid.Core.Samus.Artwork;

/// <summary>
/// The pre-coloured sheet of Samus's suit explosion as she dies (#1392): each explosion frame facing right and left,
/// which Samus's death drawing shows over her death pose.
/// </summary>
public static partial class SamusDeathSheetDefinitions
{
    /// <summary>
    /// The explosion's pictures in the order its animation shows them (#2285, #2388): the suit glowing, cracking and
    /// shattering, its pieces flying out and scattering, then Samus without it, her hair streaming, gleaming and dimming.
    /// </summary>
    public static IReadOnlyList<F> Explosion { get; } =
    [
        F.SuitGlowing, F.SuitCracking, F.SuitShattering, F.PiecesFlying, F.PiecesSpreading, F.PiecesScattered,
        F.SuitlessHairStreaming, F.SuitlessHairGleaming, F.SuitlessHairDimming,
    ];

    /// <summary>The explosion's picture <paramref name="frame"/>, facing <paramref name="facing"/>.</summary>
    public static string ExplosionFrame(HorizontalDirection facing, F frame) =>
        $"Explosion.{HorizontalDirectionKeys.Installed.Of(facing)}.{SamusDeathExplosionFrameKeys.Installed.Of(frame)}";
}
