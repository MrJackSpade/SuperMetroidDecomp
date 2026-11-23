using SuperMetroid.Core.Rooms.Backgrounds;
namespace SuperMetroid.Core.Rendering;

/// <summary>Draws a turning, scaling floor at its place among the sprite depths (#1403).</summary>
/// <param name="Transform">How the floor is turned, scaled and scrolled onto the screen.</param>
/// <param name="Floor">The floor's map and pictures.</param>
/// <param name="Blend">
/// How the floor blends (#2505): not at all, subtracting the frontmost sprite from the floor, or adding the floor's colour,
/// unhalved, over the floor and the blending sprites while the backdrop and the sprites that do not blend are unchanged.
/// </param>
public sealed partial record TurningFloorLayer(FloorView Transform, AffineFloor Floor, FloorColorBlend Blend = FloorColorBlend.None) : DisplayLayer
{
    /// <summary>The floor's map and pictures.</summary>
    public AffineFloor Floor { get; } = Ensure.NotNull(Floor);
}
