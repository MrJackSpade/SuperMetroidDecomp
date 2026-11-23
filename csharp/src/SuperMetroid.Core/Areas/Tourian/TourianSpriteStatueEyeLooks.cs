using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Fx.Artwork;

namespace SuperMetroid.Core.Areas.Tourian;

/// <summary>The look a sprite statue's eyes show for each of its eye pictures (#2625).</summary>
internal static partial class TourianSpriteStatueEyeLooks
{
    /// <summary>The look <paramref name="statue"/>'s eyes show while its animation shows <paramref name="picture"/>.</summary>
    internal static EncounterLook Of(TourianSpriteStatue statue, TourianStatueBlockPicture picture) =>
        new(TourianSpriteStatueEyeLookDefinitions.Encounters[statue], TourianSpriteStatueEyeLookDefinitions.LookNames[picture]);
}
