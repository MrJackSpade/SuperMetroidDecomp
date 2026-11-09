using SuperMetroid.Core.Samus.Death;
using SuperMetroid.Core.Samus.Poses;

namespace SuperMetroid.Core.Samus.Artwork;

/// <summary>Where Samus's body is placed at each installed pose frame, and the catalogs her presentation binds.</summary>
/// <remarks>
/// These records affect presentation only. Pose transitions, animation delays, collision,
/// and projectile timing remain in the compiled simulation. The import composes, once, the whole picture each pose shows
/// at every picture frame the body sheet covers (#1585) and bakes where each installed frame places the body (#1517);
/// the body draws its picture by pose and picture frame from the pre-coloured sheet.
/// </remarks>
public abstract class InstalledSamusBodyArtwork
{
    /// <summary>Each pose's art origin, independent of physical collision offsets.</summary>
    internal readonly Dictionary<SamusPoseId, int> graphicsYOffsets;
    /// <summary>Where each pose's installed animation frames place the body, or none where not installed.</summary>
    private readonly Dictionary<SamusPoseId, IReadOnlyList<int?>> originRises;

    /// <summary>The selected appearance of Samus's atmospheric effects.</summary>
    public InstalledSamusAtmosphericArtwork Atmosphere { get; }

    /// <summary>The order her death sequence steps through and the room's whiteout colours (#1512).</summary>
    public SamusDeathSteps DeathSteps { get; }

    /// <summary>Where each pose places the independent arm-cannon cover.</summary>
    public InstalledSamusArmCannonArtwork ArmCannon { get; }

    /// <summary>Validates a complete Samus body-art catalog.</summary>
    /// <param name="content">Where each pose's frames place the body, each pose's art origin and the catalogs it binds.</param>
    /// <remarks>The import supplies every pose the game's tables describe; which poses those are is its knowledge.</remarks>
    /// <exception cref="InvalidDataException">
    /// A pose has no frame list, the art origins do not cover exactly the poses with frames, or an art origin lies outside
    /// -128 through 127 pixels.
    /// </exception>
    protected InstalledSamusBodyArtwork(SamusBodyArtworkContent content)
    {
        Ensure.NotNull(content);
        Ensure.NotNull(content.OriginRises);
        Ensure.NotNull(content.GraphicsYOffsets);
        Ensure.NotNull(content.Atmosphere);
        Ensure.NotNull(content.DeathSteps);
        Ensure.NotNull(content.ArmCannon);
        if (content.OriginRises.Values.Any(frames => frames is null))
            throw new InvalidDataException("Samus body placements name a pose without a frame list.");
        if (content.GraphicsYOffsets.Count != content.OriginRises.Count ||
            content.OriginRises.Keys.Any(pose => !content.GraphicsYOffsets.ContainsKey(pose)))
            throw new InvalidDataException("Samus art origins do not cover exactly the poses with body frames.");
        if (content.GraphicsYOffsets.Values.Any(value => value is < sbyte.MinValue or > sbyte.MaxValue))
            throw new InvalidDataException("Samus art origins must lie from -128 through 127 pixels.");
        this.graphicsYOffsets = content.GraphicsYOffsets.ToDictionary();
        this.originRises = content.OriginRises.ToDictionary();
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

    /// <summary>
    /// How many pixels above Samus's drawn point the body's origin sits when <paramref name="pose"/> shows
    /// <paramref name="animationFrame"/>, negative below it: the placement the import baked into that installed frame (#1517).
    /// </summary>
    /// <exception cref="InvalidDataException">The pose has no record, or the frame is not installed.</exception>
    public int OriginRise(SamusPoseId pose, int animationFrame) =>
        TryOriginRise(pose, animationFrame, out int rise)
            ? rise
            : throw new InvalidDataException($"Samus pose {pose} animation frame {animationFrame} is not an installed body frame.");

    /// <summary>
    /// Where <paramref name="pose"/>'s <paramref name="animationFrame"/> places the body, when the pose has a record and the
    /// frame is installed; false for any other pair, which <see cref="OriginRise"/> rejects.
    /// </summary>
    public bool TryOriginRise(SamusPoseId pose, int animationFrame, out int rise)
    {
        rise = 0;
        if (!originRises.TryGetValue(pose, out IReadOnlyList<int?>? frames) ||
            (uint)animationFrame >= (uint)frames.Count || frames[animationFrame] is not { } installed)
            return false;
        rise = installed;
        return true;
    }
}
