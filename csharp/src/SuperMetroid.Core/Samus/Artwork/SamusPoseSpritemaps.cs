namespace SuperMetroid.Core.Samus.Artwork;

/// <summary>
/// The compositions one pose's top and bottom halves show, at each animation frame the import covers for the pose. A
/// frame with no composition draws nothing for that half.
/// </summary>
public sealed class SamusPoseSpritemaps
{
    /// <summary>Holds the compositions each half shows, by animation frame.</summary>
    /// <param name="top">The top half's composition at each animation frame, or none.</param>
    /// <param name="bottom">The bottom half's composition at each animation frame, or none.</param>
    public SamusPoseSpritemaps(IReadOnlyList<SamusSpritemapId?> top, IReadOnlyList<SamusSpritemapId?> bottom)
    {
        Top = Ensure.NotNull(top);
        Bottom = Ensure.NotNull(bottom);
    }

    /// <summary>The top half's composition at each animation frame, or none.</summary>
    public IReadOnlyList<SamusSpritemapId?> Top { get; }

    /// <summary>The bottom half's composition at each animation frame, or none.</summary>
    public IReadOnlyList<SamusSpritemapId?> Bottom { get; }

    /// <summary>The compositions <paramref name="half"/> shows, by animation frame.</summary>
    public IReadOnlyList<SamusSpritemapId?> Frames(SamusBodyHalf half) => half switch
    {
        SamusBodyHalf.Top => Top,
        SamusBodyHalf.Bottom => Bottom,
        _ => throw new ArgumentOutOfRangeException(nameof(half), half, "Samus's body has only a top and a bottom half."),
    };
}
