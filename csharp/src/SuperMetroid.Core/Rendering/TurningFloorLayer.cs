using SuperMetroid.Core.Rooms.Backgrounds;
namespace SuperMetroid.Core.Rendering;

/// <summary>Draws a turning, scaling floor at its place among the sprite depths (#1403).</summary>
/// <param name="Transform">How the floor is turned, scaled and scrolled onto the screen.</param>
/// <param name="Floor">The floor's map and tiles.</param>
/// <param name="SubtractsSprites">Subtracts the frontmost sprite from the floor.</param>
/// <param name="AddsForeground">Adds the floor's colour, unhalved, over the floor and the blending sprites; the backdrop and the sprites that do not blend are unchanged.</param>
[IndependentFlag("independent blends: subtracting the sprites, and adding the floor")]
public sealed record TurningFloorLayer(AffineTransform Transform, AffineFloor Floor, bool SubtractsSprites = false,
    bool AddsForeground = false) : DisplayLayer
{
    /// <summary>The floor's map and tiles.</summary>
    public AffineFloor Floor
    {
        get;
        init => field = Ensure.NotNull(value);
    } = Ensure.NotNull(Floor);
}
