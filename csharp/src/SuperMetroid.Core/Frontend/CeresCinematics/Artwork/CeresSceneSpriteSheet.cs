using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend.Intro;
using SuperMetroid.Core.Frontend.Intro.Artwork;

namespace SuperMetroid.Core.Frontend.CeresCinematics.Artwork;

/// <summary>
/// Draws the Ceres destruction scene's actors from the scene's current sheet (#1392): a destruction pose from the
/// destruction family's frames, any other from the shared Ceres flight family's.
/// </summary>
/// <param name="sheet">The sheet over the OBJ characters the scene's uploads have left.</param>
internal sealed class CeresSceneSpriteSheet(SpriteSheet sheet) : IIntroCinematicSpritePresentation
{
    /// <summary>The poses the destruction family shows; the scene's other poses are the Ceres flight's.</summary>
    private static readonly HashSet<CinematicSpritePose> DestructionPoses =
        [.. CeresDestructionSpriteDefinitions.Frames.Select(frame => frame.Pose)];

    /// <summary>Dispatches a scene frame to the destruction family's frames or the shared Ceres flight family's.</summary>
    public void Draw(CinematicSpritePose pose, ScreenSprites objects, Position16<OamPixels> origin,
        ushort paletteBits, bool originIsOnScreen) =>
        new CinematicSpriteSheet(sheet, DestructionPoses.Contains(pose) ? CinematicSpriteFamily.CeresDestruction : CinematicSpriteFamily.CeresFlight)
            .Draw(pose, objects, origin, paletteBits, originIsOnScreen);
}
