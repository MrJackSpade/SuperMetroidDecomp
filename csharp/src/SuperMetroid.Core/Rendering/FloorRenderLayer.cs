using SuperMetroid.Core.Rooms.Backgrounds;
namespace SuperMetroid.Core.Rendering;

/// <summary>Inserts a turning, scaling floor at an explicit position among the sprite priorities (#1403).</summary>
/// <param name="Transform">How the floor is turned, scaled and scrolled onto the screen.</param>
/// <param name="Floor">The floor's map and tiles.</param>
/// <param name="SubtractSpriteSubscreen">Subtracts the frontmost sprite from the floor.</param>
/// <param name="AddBg1Subscreen">Owns floor/OBJ main selection and adds the floor to eligible main pixels, without halving. Backdrop and OBJ palettes zero through three are excluded.</param>
[IndependentFlag("independent colour-math settings: object subtraction and floor addition")]
public sealed record FloorRenderLayer(AffineTransform Transform, AffineFloor Floor, bool SubtractSpriteSubscreen = false,
    bool AddBg1Subscreen = false) : RenderLayer
{
    /// <summary>The floor's map and tiles.</summary>
    public AffineFloor Floor
    {
        get;
        init => field = Ensure.NotNull(value);
    } = Ensure.NotNull(Floor);
}
