using SuperMetroid.Core.Samus.Death;
using SuperMetroid.Core.Samus.Poses;

namespace SuperMetroid.Core.Samus.Artwork;

/// <summary>Which body half pictures each Samus pose frame shows, and where the body is placed.</summary>
/// <remarks>
/// These records affect presentation only. Pose transitions, animation delays, collision,
/// and projectile timing remain in the compiled simulation. The import resolves, once, the half pictures each pose shows
/// at every animation frame the body sheets cover (#1465) and where each of those frames places the body (#1517); their
/// characters are composed into the pre-coloured sheets at installation, and this catalog names the pictures a frame
/// shows, carries their characters' identity, and places the body by pose and frame.
/// </remarks>
public abstract class InstalledSamusBodyArtwork
{
    /// <summary>Each pose's art origin, independent of physical collision offsets.</summary>
    internal readonly Dictionary<SamusPoseId, int> graphicsYOffsets;
    /// <summary>Each pose's installed animation frames, each holding its pictures and placement, or none where not installed.</summary>
    private readonly Dictionary<SamusPoseId, IReadOnlyList<SamusBodyFramePictures?>> poseFrames;

    /// <summary>Which body picture each pose half shows at each animation frame, for these body frames.</summary>
    public InstalledSamusSpritemaps Spritemaps { get; }

    /// <summary>The selected appearance of Samus's atmospheric effects.</summary>
    public InstalledSamusAtmosphericArtwork Atmosphere { get; }

    /// <summary>The order her death sequence steps through and the room's whiteout colours (#1512).</summary>
    public SamusDeathSteps DeathSteps { get; }

    /// <summary>Where each pose places the independent arm-cannon cover.</summary>
    public InstalledSamusArmCannonArtwork ArmCannon { get; }

    /// <summary>Validates a complete Samus body-art catalog.</summary>
    /// <param name="content">The frames each pose shows with their placements, each pose's art origin and the catalogs it binds.</param>
    /// <remarks>The import supplies every pose the game's tables describe; which poses those are is its knowledge.</remarks>
    /// <exception cref="InvalidDataException">
    /// A pose has no frame list, the art origins do not cover exactly the poses with frames, or an art origin lies outside
    /// -128 through 127 pixels.
    /// </exception>
    protected InstalledSamusBodyArtwork(SamusBodyArtworkContent content)
    {
        Ensure.NotNull(content);
        Ensure.NotNull(content.Frames);
        Ensure.NotNull(content.GraphicsYOffsets);
        Ensure.NotNull(content.Spritemaps);
        Ensure.NotNull(content.Atmosphere);
        Ensure.NotNull(content.DeathSteps);
        Ensure.NotNull(content.ArmCannon);
        if (content.Frames.Values.Any(frames => frames is null))
            throw new InvalidDataException("Samus body pictures name a pose without a frame list.");
        if (content.GraphicsYOffsets.Count != content.Frames.Count ||
            content.Frames.Keys.Any(pose => !content.GraphicsYOffsets.ContainsKey(pose)))
            throw new InvalidDataException("Samus art origins do not cover exactly the poses with body pictures.");
        if (content.GraphicsYOffsets.Values.Any(value => value is < sbyte.MinValue or > sbyte.MaxValue))
            throw new InvalidDataException("Samus art origins must lie from -128 through 127 pixels.");
        this.graphicsYOffsets = content.GraphicsYOffsets.ToDictionary();
        this.poseFrames = content.Frames.ToDictionary();
        Spritemaps = content.Spritemaps;
        Atmosphere = content.Atmosphere;
        DeathSteps = content.DeathSteps;
        ArmCannon = content.ArmCannon;
    }

    /// <summary>Signed pose art origin; changing it never changes a physical projectile origin.</summary>
    /// <exception cref="InvalidDataException">The pose has no art origin.</exception>
    public int GraphicsYOffset(SamusPoseId pose) =>
        graphicsYOffsets.TryGetValue(pose, out int offset)
            ? offset
            : throw new InvalidDataException($"Pose {pose} has no authored graphics Y offset.");

    /// <summary>The half pictures <paramref name="pose"/> shows at <paramref name="animationFrame"/>, and where that frame places the body.</summary>
    /// <exception cref="InvalidDataException">The pose has no record, or the frame is not installed.</exception>
    public SamusBodyFramePictures Frame(SamusPoseId pose, int animationFrame) =>
        TryFrame(pose, animationFrame, out SamusBodyFramePictures pictures)
            ? pictures
            : throw new InvalidDataException($"Samus pose {pose} animation frame {animationFrame} shows no installed body pictures.");

    /// <summary>
    /// The half pictures <paramref name="pose"/> shows at <paramref name="animationFrame"/> with the frame's placement, when
    /// the pose has a record and the frame is installed; false for any other pair, which <see cref="Frame"/> rejects.
    /// </summary>
    public bool TryFrame(SamusPoseId pose, int animationFrame, out SamusBodyFramePictures pictures)
    {
        pictures = default;
        if (!poseFrames.TryGetValue(pose, out IReadOnlyList<SamusBodyFramePictures?>? frames) ||
            (uint)animationFrame >= (uint)frames.Count || frames[animationFrame] is not { } shown)
            return false;
        pictures = shown;
        return true;
    }
}
