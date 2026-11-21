using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Fx.ColourEffects;

namespace SuperMetroid.Core.Areas.Tourian;

/// <summary>The entrance statue's state and the unlock effects the statue sequence drives.</summary>
internal interface ITourianStatueActors
{
    /// <summary>Which statues are animating, whether a release is in progress, and whether all four have released.</summary>
    TourianStatueAnimation Animation { get; }

    /// <summary>Spawns the glow of a statue's <paramref name="eye"/>.</summary>
    void SpawnEyeGlow(TourianStatueEye eye);

    /// <summary>Spawns a statue's released soul at its <paramref name="eye"/>.</summary>
    void SpawnSoul(TourianStatueEye eye);

    /// <summary>Spawns the descent's dust.</summary>
    void SpawnDescentDust();

    /// <summary>
    /// Shows a released statue's look, its blacked-out eyes or its grey, as the shade the import pre-coloured it in (#1512):
    /// over the room's pictures for a statue <paramref name="drawnInRoomBackground"/>, otherwise over the scene's sprites.
    /// </summary>
    void ShowStatueLook(PictureShade look, bool drawnInRoomBackground);

    /// <summary>
    /// The entrance statue's sprite statue <paramref name="sprite"/>, the Ridley or Phantoon statue, which that statue's
    /// grey-out shows on (#2288).
    /// </summary>
    IGlowingEnemy SpriteStatueGreyOut(TourianSpriteStatue sprite);
}
