using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend.Intro;
using SuperMetroid.Core.Frontend.Intro.Artwork;

namespace SuperMetroid.Core.Frontend.CeresCinematics.Artwork;

/// <summary>
/// Draws the Ceres destruction scene's actors from the scene's current sheet (#1392): a destruction pose from the
/// destruction family's frames, any other from the shared Ceres flight family's.
/// </summary>
/// <param name="sheet">The scene's current sheet.</param>
/// <param name="shading">
/// The shading the scene shows a draw named in a colouring's drawn shade in: that shade with the record of any colour effect
/// animating the colouring (#1733).
/// </param>
internal sealed class CeresSceneSpriteSheet(SpriteSheet sheet, Func<PictureShade, PictureShading> shading) : IIntroCinematicSpritePresentation
{
    /// <summary>The poses the destruction family shows; the scene's other poses are the Ceres flight's.</summary>
    private static readonly HashSet<CinematicSpritePose> DestructionPoses =
        [.. CeresDestructionSpriteDefinitions.Poses];

    /// <summary>Dispatches a scene frame to the destruction family's frames or the shared Ceres flight family's, in its drawn shade's shading (#1512).</summary>
    public void Draw(CinematicSpritePose pose, ScreenSprites objects, Position16<SpritePixels> origin,
        PictureShade drawn) =>
        new CinematicSpriteSheet(sheet, DestructionPoses.Contains(pose) ? CinematicSpriteFamily.CeresDestruction : CinematicSpriteFamily.CeresFlight,
                shading)
            .Draw(pose, objects, origin, drawn);
}
