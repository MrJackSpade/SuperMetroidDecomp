using SuperMetroid.Core.Fx.ColourEffects;

namespace SuperMetroid.Core.Areas.Tourian;

/// <summary>The colours each of the entrance statue's sprite statues shows, which its grey-out recolours (#1512, #2481).</summary>
internal static partial class TourianSpriteStatueDefinitions
{
    /// <summary>Each sprite statue's colours.</summary>
    internal static IReadOnlyDictionary<TourianSpriteStatue, ColourEffectColours> Colours { get; } = new Dictionary<TourianSpriteStatue, ColourEffectColours>
    {
        [TourianSpriteStatue.Ridley] = ColourEffectColours.RidleyStatue,
        [TourianSpriteStatue.Phantoon] = ColourEffectColours.PhantoonStatue,
    };
}
