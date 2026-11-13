using System.Collections.Immutable;
using SuperMetroid.Core.Samus.Drawing;
using SuperMetroid.Core.Samus.Projectiles;

namespace SuperMetroid.Core.Samus.Artwork;

/// <summary>
/// Where one pose draws the arm cannon's cover (#1463): whether and when the pose draws it, the aim direction whose
/// cover picture it shows, and the cover's displacement from Samus at each animation frame it is drawn at.
/// </summary>
/// <remarks>
/// The import pipeline decodes each pose's placement once (#1638). The installation holds the displacement of the
/// animation frames the pose shows, which are the frames the cover is drawn at, wherever the import's data holds one; an
/// animation frame without one has no installed placement.
/// </remarks>
public sealed class SamusArmCannonPlacement
{
    /// <summary>The displacement of each animation frame with one, by animation frame.</summary>
    private readonly ImmutableSortedDictionary<int, Offset16<RoomPixels>> offsets;

    /// <summary>Creates a pose's placement.</summary>
    /// <param name="drawOrder">Whether the pose draws the cover, and before or after Samus's body.</param>
    /// <param name="firstDirection">The aim direction shown at animation frame zero, or at every frame when <paramref name="laterDirection"/> is null.</param>
    /// <param name="laterDirection">The aim direction shown at every animation frame after zero, or null when the pose shows one direction throughout.</param>
    /// <param name="offsets">The cover's displacement from Samus at each animation frame with one, by animation frame.</param>
    public SamusArmCannonPlacement(SamusArmCannonDrawOrder drawOrder, SamusProjectileDirection firstDirection,
        SamusProjectileDirection? laterDirection, IReadOnlyDictionary<int, Offset16<RoomPixels>> offsets)
    {
        DrawOrder = drawOrder;
        FirstDirection = firstDirection;
        LaterDirection = laterDirection;
        this.offsets = Ensure.NotNull(offsets).ToImmutableSortedDictionary();
    }

    /// <summary>Whether the pose draws the cover, and before or after Samus's body.</summary>
    public SamusArmCannonDrawOrder DrawOrder { get; }

    /// <summary>The aim direction shown at animation frame zero, or at every frame when <see cref="LaterDirection"/> is null.</summary>
    public SamusProjectileDirection FirstDirection { get; }

    /// <summary>The aim direction shown at every animation frame after zero, or null when the pose shows one direction throughout.</summary>
    public SamusProjectileDirection? LaterDirection { get; }

    /// <summary>The cover's displacement at each animation frame with one, in animation-frame order.</summary>
    public IReadOnlyDictionary<int, Offset16<RoomPixels>> Offsets => offsets;

    /// <summary>The aim direction whose cover picture the pose shows at <paramref name="animationFrame"/>.</summary>
    public SamusProjectileDirection Direction(int animationFrame) =>
        LaterDirection is { } later && animationFrame != 0 ? later : FirstDirection;

    /// <summary>The cover's displacement from Samus at <paramref name="animationFrame"/>.</summary>
    /// <exception cref="InvalidDataException">The animation frame has no installed displacement.</exception>
    public Offset16<RoomPixels> Offset(int animationFrame) => offsets.TryGetValue(animationFrame, out Offset16<RoomPixels> offset)
        ? offset
        : throw new InvalidDataException($"Arm-cannon animation frame {animationFrame} has no installed placement.");
}
