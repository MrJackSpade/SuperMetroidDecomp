using System.Collections.Immutable;

namespace SuperMetroid.Core.Samus.Artwork;

/// <summary>
/// Where one pose draws the arm cannon's cover (#1463): the drawing mode the pose sets, the aim direction whose cover
/// picture it shows, and the cover's displacement from Samus at each animation frame.
/// </summary>
/// <remarks>
/// The import pipeline decodes each pose's cartridge descriptor into this placement. A pose's displacements continue
/// through the descriptors stored after its own, as the cartridge's per-frame read does, up to the end of the installed
/// drawing window; an animation frame beyond them has no installed placement.
/// </remarks>
public sealed class SamusArmCannonPlacement
{
    /// <summary>The displacement of each animation frame, from frame zero.</summary>
    private readonly ImmutableArray<Offset16<RoomPixels>> offsets;

    /// <summary>Creates a pose's placement.</summary>
    /// <param name="drawingMode">The drawing-mode value the pose sets, 0 through 255; its low four bits choose whether and when the cover draws.</param>
    /// <param name="firstDirection">The aim direction shown at animation frame zero, or at every frame when <paramref name="laterDirection"/> is null, 0 through 127.</param>
    /// <param name="laterDirection">The aim direction shown at every animation frame after zero, 0 through 127, or null when the pose shows one direction throughout.</param>
    /// <param name="offsets">The cover's displacement from Samus at each animation frame, from frame zero.</param>
    public SamusArmCannonPlacement(int drawingMode, int firstDirection, int? laterDirection, IEnumerable<Offset16<RoomPixels>> offsets)
    {
        DrawingMode = Ensure.BetweenInclusive(0, byte.MaxValue, drawingMode);
        FirstDirection = Ensure.BetweenInclusive(0, sbyte.MaxValue, firstDirection);
        LaterDirection = laterDirection is { } later ? Ensure.BetweenInclusive(0, sbyte.MaxValue, later) : null;
        this.offsets = [.. Ensure.NotNull(offsets)];
    }

    /// <summary>The drawing-mode value the pose sets, 0 through 255; its low four bits choose whether and when the cover draws.</summary>
    public int DrawingMode { get; }

    /// <summary>The aim direction shown at animation frame zero, or at every frame when <see cref="LaterDirection"/> is null.</summary>
    public int FirstDirection { get; }

    /// <summary>The aim direction shown at every animation frame after zero, or null when the pose shows one direction throughout.</summary>
    public int? LaterDirection { get; }

    /// <summary>The animation frames with an installed displacement, from frame zero.</summary>
    public int FrameCount => offsets.Length;

    /// <summary>The aim direction whose cover picture the pose shows at <paramref name="animationFrame"/>.</summary>
    public int Direction(int animationFrame) =>
        LaterDirection is { } later && animationFrame != 0 ? later : FirstDirection;

    /// <summary>The cover's displacement from Samus at <paramref name="animationFrame"/>.</summary>
    /// <exception cref="InvalidDataException">The animation frame has no installed displacement.</exception>
    public Offset16<RoomPixels> Offset(int animationFrame) => (uint)animationFrame < (uint)offsets.Length
        ? offsets[animationFrame]
        : throw new InvalidDataException($"Arm-cannon animation frame {animationFrame} has no installed placement.");
}
