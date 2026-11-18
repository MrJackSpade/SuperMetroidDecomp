using SuperMetroid.Core.Fx.ColourEffects;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Areas.Tourian;

/// <summary>
/// The entrance statue's sprite statue a grey-out shows on (#2288): the statue sequence starts the Ridley or Phantoon
/// statue's grey-out with it, and the grey-out hands each of its frames to it, as the gunship's glow does the gunship
/// (#1783).
/// </summary>
/// <param name="statue">The entrance statue, whose sprites draw the Ridley and Phantoon statues.</param>
/// <param name="sprite">The Ridley or Phantoon statue, whose colours the grey-out recolours.</param>
[RoomLifetime]
internal sealed class TourianSpriteStatueGreyOut(TourianEntranceStatueBehavior statue, TourianSpriteStatue sprite) : IGlowingEnemy
{
    /// <inheritdoc/>
    public void ShowGlow(ColourEffectFrame frame) => statue.ShowStatueGreyOut(sprite, frame);
}
