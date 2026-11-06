using SuperMetroid.Core.Samus.Poses;

namespace SuperMetroid.Core.Samus.Artwork;

/// <summary>Which pre-coloured body picture each pose's top and bottom halves show at each animation frame.</summary>
/// <remarks>
/// The import chooses, once, the composition each pose half shows at every animation frame the body sheets cover; a
/// frame with none (an animation command's slot, or a half the pose omits) draws nothing. The halves an update shows
/// are one full-body picture on the body sheet, which shows the top or bottom picture selected for the frame when a
/// composition reads that half (#1463, #1513). The compositions' parts are the import pipeline's, which composes the
/// sheet from them; timing and collisions remain in the simulation.
/// </remarks>
public abstract class InstalledSamusSpritemaps
{
    /// <summary>The compositions each pose's halves show.</summary>
    private readonly Dictionary<SamusPoseId, SamusPoseSpritemaps> poses;
    /// <summary>The body halves each composition's picture shows, at its identity's ordinal.</summary>
    private readonly SamusBodyHalvesShown[] halves;

    /// <summary>Validates that every composition a pose shows is installed; the import supplies every pose its tables describe.</summary>
    /// <param name="poses">The compositions each of the 253 poses' halves show, by animation frame.</param>
    /// <param name="halves">The body halves each composition's picture shows, each at its composition's ordinal.</param>
    /// <exception cref="InvalidDataException">A pose has no compositions, or a frame names a composition that is not installed.</exception>
    protected InstalledSamusSpritemaps(IReadOnlyDictionary<SamusPoseId, SamusPoseSpritemaps> poses, SamusBodyHalvesShown[] halves)
    {
        Ensure.NotNull(poses);
        Ensure.NotNull(halves);
        if (poses.Values.Any(pose => pose is null))
            throw new InvalidDataException("Samus spritemaps name a pose without compositions.");
        this.halves = (SamusBodyHalvesShown[])halves.Clone();
        foreach (SamusPoseSpritemaps pose in poses.Values)
            foreach (SamusSpritemapId? frame in pose.Top.Concat(pose.Bottom))
                if (frame is { } composition && (uint)composition.Ordinal >= (uint)this.halves.Length)
                    throw new InvalidDataException($"Samus spritemap {composition.Ordinal} is absent from installed art.");
        this.poses = poses.ToDictionary();
    }

    /// <summary>The composition <paramref name="frame"/> shows; false only for a frame with none, which draws nothing.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The pose has no record.</exception>
    /// <exception cref="InvalidDataException">The animation frame is past every frame installed for the pose half.</exception>
    public bool TryGetComposition(SamusBodyHalfFrame frame, out SamusSpritemapId composition)
    {
        IReadOnlyList<SamusSpritemapId?> frames = Frames(frame);
        SamusSpritemapId? shown = (uint)frame.AnimationFrame < (uint)frames.Count
            ? frames[frame.AnimationFrame]
            : throw new InvalidDataException($"Samus {frame} is outside installed art.");
        composition = shown ?? default;
        return shown is not null;
    }

    /// <summary>
    /// The pre-coloured full-body picture an update draws with <paramref name="pictures"/> selected (#1513): the
    /// compositions <paramref name="top"/> and <paramref name="bottom"/> show, with the selected half picture of each
    /// body half either composition shows, and the chest cover between them. False only when neither half has a
    /// composition and there is no chest cover, which draws nothing.
    /// </summary>
    /// <param name="top">The top half the body shows.</param>
    /// <param name="bottom">The bottom half the body shows, or null when the frame shows none.</param>
    /// <param name="chestCover">Where the chest cover sits relative to the body's origin, or null for a body without one.</param>
    /// <param name="pictures">The half pictures the body shows.</param>
    /// <param name="key">The full-body frame.</param>
    /// <exception cref="ArgumentOutOfRangeException">The pose has no record.</exception>
    /// <exception cref="InvalidDataException">An animation frame is past every frame installed for its pose half.</exception>
    public bool TryGetFrameKey(SamusBodyHalfFrame top, SamusBodyHalfFrame? bottom, Offset16<ScreenPixels>? chestCover,
        SamusBodyPictures pictures, out SamusBodyFrameKey key)
    {
        SamusSpritemapId? topComposition = TryGetComposition(top, out SamusSpritemapId shownTop) ? shownTop : null;
        SamusSpritemapId? bottomComposition = bottom is { } lower && TryGetComposition(lower, out SamusSpritemapId shownBottom)
            ? shownBottom : null;
        if (topComposition is null && bottomComposition is null && chestCover is null)
        {
            key = default;
            return false;
        }
        bool showsTop = Shows(topComposition, SamusBodyHalvesShown.Top) || Shows(bottomComposition, SamusBodyHalvesShown.Top);
        bool showsBottom = Shows(topComposition, SamusBodyHalvesShown.Bottom) || Shows(bottomComposition, SamusBodyHalvesShown.Bottom);
        key = new SamusBodyFrameKey(topComposition, bottomComposition, showsTop ? pictures.Top : null,
            showsBottom ? pictures.Bottom : null, chestCover);
        return true;
    }

    /// <summary>Whether <paramref name="composition"/>'s picture shows <paramref name="half"/>'s record.</summary>
    private bool Shows(SamusSpritemapId? composition, SamusBodyHalvesShown half) =>
        composition is { } shown && (halves[shown.Ordinal] == half || halves[shown.Ordinal] == SamusBodyHalvesShown.Both);

    /// <summary>The compositions <paramref name="frame"/>'s pose half shows, by animation frame.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The pose has no record.</exception>
    internal IReadOnlyList<SamusSpritemapId?> Frames(SamusBodyHalfFrame frame) =>
        poses.TryGetValue(frame.Pose, out SamusPoseSpritemaps? pose)
            ? pose.Frames(frame.Half)
            : throw new ArgumentOutOfRangeException(nameof(frame), frame.Pose, "The pose has no Samus spritemaps.");
}
