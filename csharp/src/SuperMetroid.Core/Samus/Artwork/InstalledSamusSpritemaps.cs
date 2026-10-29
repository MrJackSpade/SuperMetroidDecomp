using SuperMetroid.Core.Samus.Poses;

namespace SuperMetroid.Core.Samus.Artwork;

/// <summary>Which pre-coloured body picture each pose's top and bottom halves show at each animation frame.</summary>
/// <remarks>
/// The import chooses, once, the composition each pose half shows at every animation frame the body sheets cover; a
/// frame with none (an animation command's slot, or a half the pose omits) draws nothing. Each composition is one
/// picture on the body sheets, which shows the top or bottom picture selected for the frame when the picture reads that
/// half (#1463). The compositions' parts are the import pipeline's, which composes the sheets from them; timing and
/// collisions remain in the simulation.
/// </remarks>
public abstract class InstalledSamusSpritemaps
{
    /// <summary>The compositions each pose's halves show.</summary>
    private readonly Dictionary<SamusPoseId, SamusPoseSpritemaps> poses;
    /// <summary>The body halves each composition's picture shows, at its identity's ordinal.</summary>
    private readonly SamusBodyHalvesShown[] halves;

    /// <summary>Validates that every pose is covered and every composition it shows is installed.</summary>
    /// <param name="poses">The compositions each of the 253 poses' halves show, by animation frame.</param>
    /// <param name="halves">The body halves each composition's picture shows, each at its composition's ordinal.</param>
    /// <exception cref="InvalidDataException">A pose is missing or unknown, or a frame names a composition that is not installed.</exception>
    protected InstalledSamusSpritemaps(IReadOnlyDictionary<SamusPoseId, SamusPoseSpritemaps> poses, SamusBodyHalvesShown[] halves)
    {
        Ensure.NotNull(poses);
        Ensure.NotNull(halves);
        if (poses.Count != InstalledSamusBodyArtwork.PoseCount || poses.Keys.Any(pose => !SamusPoseNumbering.HasRecord(pose)) ||
            poses.Values.Any(pose => pose is null))
            throw new InvalidDataException("Samus spritemaps do not cover exactly the poses with records.");
        this.halves = (SamusBodyHalvesShown[])halves.Clone();
        foreach (SamusPoseSpritemaps pose in poses.Values)
            foreach (SamusSpritemapId? frame in pose.Top.Concat(pose.Bottom))
                if (frame is { } composition && (uint)composition.Ordinal >= (uint)this.halves.Length)
                    throw new InvalidDataException($"Samus spritemap {composition.Ordinal} is absent from installed art.");
        this.poses = poses.ToDictionary();
    }

    /// <summary>SHA-256 of the compositions each pose half shows and the compositions' parts.</summary>
    public abstract string ContentIdentity { get; }

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

    /// <summary>Whether <paramref name="frame"/> is an animation frame installed for its pose half.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The pose has no record.</exception>
    internal bool Covers(SamusBodyHalfFrame frame) => (uint)frame.AnimationFrame < (uint)Frames(frame).Count;

    /// <summary>
    /// The pre-coloured body picture <paramref name="frame"/> shows with <paramref name="records"/> selected: its
    /// composition with the selected record of each half the picture shows. False only for a frame with no composition,
    /// which draws nothing.
    /// </summary>
    public bool TryGetFrameKey(SamusBodyHalfFrame frame, SamusBodyRecords records, out SamusBodyFrameKey key)
    {
        if (!TryGetComposition(frame, out SamusSpritemapId composition))
        {
            key = default;
            return false;
        }
        SamusBodyHalvesShown shown = halves[composition.Ordinal];
        key = new SamusBodyFrameKey(composition,
            shown is SamusBodyHalvesShown.Top or SamusBodyHalvesShown.Both ? records.Top : null,
            shown is SamusBodyHalvesShown.Bottom or SamusBodyHalvesShown.Both ? records.Bottom : null);
        return true;
    }

    /// <summary>The compositions <paramref name="frame"/>'s pose half shows, by animation frame.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The pose has no record.</exception>
    private IReadOnlyList<SamusSpritemapId?> Frames(SamusBodyHalfFrame frame) =>
        poses.TryGetValue(frame.Pose, out SamusPoseSpritemaps? pose)
            ? pose.Frames(frame.Half)
            : throw new ArgumentOutOfRangeException(nameof(frame), frame.Pose, "The pose has no Samus spritemaps.");
}
