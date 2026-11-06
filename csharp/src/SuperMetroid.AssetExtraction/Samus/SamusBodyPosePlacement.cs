using System.Text.Json.Serialization;

namespace SuperMetroid.AssetExtraction.Samus;

/// <summary>
/// Where one pose places Samus's body, as the installed body file holds it (#1517): the pose's art origin, and for a pose
/// whose animation frames place the body differently from it, each frame's own placement. The import decodes the
/// cartridge's placement tables once into these; the installation never holds those tables.
/// </summary>
/// <param name="GraphicsYOffset">
/// The pose's art origin, -128 through 127: how many pixels above Samus's center the arm cannon, the Grapple beam, the
/// charge flare and the speed-booster echoes are drawn, and the body is when the frames hold no placement of their own.
/// </param>
/// <param name="FrameRises">
/// How many pixels above Samus's drawn point each of the pose's animation frames, from the first, places the body's
/// origin; a frame past the list has no placement and is not installed. Null when every frame is placed by
/// <paramref name="GraphicsYOffset"/>.
/// </param>
public sealed record SamusBodyPosePlacement(
    int GraphicsYOffset,
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] int[]? FrameRises = null)
{
    /// <summary>How many pixels above Samus's drawn point <paramref name="animationFrame"/> places the body, when it has a placement.</summary>
    /// <param name="animationFrame">The pose's animation frame, from zero.</param>
    /// <param name="rise">Receives the frame's origin rise.</param>
    /// <returns><see langword="true"/> when the frame has a placement.</returns>
    public bool TryFrameRise(int animationFrame, out int rise)
    {
        if (FrameRises is null)
        {
            rise = GraphicsYOffset;
            return animationFrame >= 0;
        }
        bool placed = (uint)animationFrame < (uint)FrameRises.Length;
        rise = placed ? FrameRises[animationFrame] : 0;
        return placed;
    }
}
