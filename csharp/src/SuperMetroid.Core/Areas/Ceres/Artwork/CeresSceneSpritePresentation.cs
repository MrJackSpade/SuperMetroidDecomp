using SuperMetroid.Core.Frontend.Intro.Artwork;

namespace SuperMetroid.Core.Areas.Ceres.Artwork;

/// <summary>Routes the scene's shared approach frames and unique destruction frames.</summary>
internal sealed class CeresSceneSpritePresentation(
    CeresFlightSpritePresentation flight,
    CeresDestructionSpritePresentation destruction) : IIntroCinematicSpritePresentation
{
    /// <summary>Dispatches a scene frame to the unique destruction catalog or the shared Ceres flight catalog.</summary>
    public void Draw(ushort pointer, OamBuffer oam, Position16<OamPixels> origin,
        ushort paletteBits, bool originIsOnScreen)
    {
        IIntroCinematicSpritePresentation owner = destruction.Contains(pointer)
            ? destruction : flight;
        owner.Draw(pointer, oam, origin, paletteBits, originIsOnScreen);
    }
}
