using SuperMetroid.Core.Frontend.Intro;
using SuperMetroid.Core.Frontend.Intro.Artwork;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Frontend.CeresCinematics.Artwork;

/// <summary>Routes the scene's shared approach frames and unique destruction frames.</summary>
internal sealed class CeresSceneSpritePresentation(
    CeresFlightSpritePresentation flight,
    CeresDestructionSpritePresentation destruction) : IIntroCinematicSpritePresentation
{
    /// <summary>Dispatches a scene frame to the unique destruction catalog or the shared Ceres flight catalog.</summary>
    public void Draw(CinematicSpritePose pose, ScreenSprites objects, Position16<OamPixels> origin,
        ushort paletteBits, bool originIsOnScreen)
    {
        IIntroCinematicSpritePresentation owner = destruction.Contains(pose)
            ? destruction : flight;
        owner.Draw(pose, objects, origin, paletteBits, originIsOnScreen);
    }
}
