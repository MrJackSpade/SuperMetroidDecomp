using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Fx.PaletteFx;
using SuperMetroid.Core.Areas.Tourian.Artwork;
namespace SuperMetroid.Core.Areas.Tourian;

/// <summary>
/// The four Tourian entrance statues' background animations: the values their shared typed animation
/// (<see cref="TourianStatueTileAnimations"/>) acts on.
/// </summary>
public static class TourianStatueAnimationDefinitions
{
    /// <summary>Each statue's event, first-frame duration, boss test, shades, eye and palette effect, by statue.</summary>
    internal static readonly IReadOnlyDictionary<TourianGreyStatues, TourianStatueAnimationDefinition> Statues =
        new Dictionary<TourianGreyStatues, TourianStatueAnimationDefinition>
    {
        [TourianGreyStatues.Phantoon] = new()
        {
            Statue = TourianGreyStatues.Phantoon,
            GreyEvent = StoryEvent.PhantoonStatueGrey,
            FirstFrameDuration = 6,
            BossArea = AreaId.WreckedShip,
            DefeatedBosses = DefeatedBosses.AreaBoss,
            ClearedShade = TourianStatueShades.PhantoonCleared,
            Eye = new(new(132, 144), TourianStatueShades.EyeGlow(0)),
            PaletteFx = PaletteFxObject.GreyOutTourianStatuePhantoon,
            GreyShade = TourianStatueShades.PhantoonGrey,
        },
        [TourianGreyStatues.Ridley] = new()
        {
            Statue = TourianGreyStatues.Ridley,
            GreyEvent = StoryEvent.RidleyStatueGrey,
            FirstFrameDuration = 10,
            BossArea = AreaId.Norfair,
            DefeatedBosses = DefeatedBosses.AreaBoss,
            ClearedShade = TourianStatueShades.RidleyCleared,
            Eye = new(new(122, 81), TourianStatueShades.EyeGlow(1)),
            PaletteFx = PaletteFxObject.GreyOutTourianStatueRidley,
            GreyShade = TourianStatueShades.RidleyGrey,
        },
        [TourianGreyStatues.Kraid] = new()
        {
            Statue = TourianGreyStatues.Kraid,
            GreyEvent = StoryEvent.KraidStatueGrey,
            FirstFrameDuration = 4,
            BossArea = AreaId.Brinstar,
            DefeatedBosses = DefeatedBosses.AreaBoss,
            ClearedShade = TourianStatueShades.KraidCleared,
            Eye = new(new(104, 114), TourianStatueShades.EyeGlow(3)),
            PaletteFx = PaletteFxObject.GreyOutTourianStatueKraid,
            GreyShade = TourianStatueShades.KraidGrey,
        },
        [TourianGreyStatues.Draygon] = new()
        {
            Statue = TourianGreyStatues.Draygon,
            GreyEvent = StoryEvent.DraygonStatueGrey,
            FirstFrameDuration = 8,
            BossArea = AreaId.Maridia,
            DefeatedBosses = DefeatedBosses.AreaBoss,
            ClearedShade = TourianStatueShades.DraygonCleared,
            Eye = new(new(158, 128), TourianStatueShades.EyeGlow(2)),
            PaletteFx = PaletteFxObject.GreyOutTourianStatueDraygon,
            GreyShade = TourianStatueShades.DraygonGrey,
        },
    };
}
