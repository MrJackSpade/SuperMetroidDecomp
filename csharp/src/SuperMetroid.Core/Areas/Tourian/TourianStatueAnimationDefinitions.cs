using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Fx.ColourEffects;
using SuperMetroid.Core.Areas.Tourian.Artwork;
namespace SuperMetroid.Core.Areas.Tourian;

/// <summary>
/// The four Tourian entrance statues' background animations: the values their shared typed animation
/// (<see cref="TourianStatueBlockAnimations"/>) acts on.
/// </summary>
public static partial class TourianStatueAnimationDefinitions
{
    /// <summary>Each statue's event, first-frame duration, boss test, shades, eye and colour effect, by statue.</summary>
    internal static readonly IdentityTable<TourianStatue, TourianStatueAnimationDefinition> Statues = new(new Dictionary<TourianStatue, TourianStatueAnimationDefinition>
    {
        [TourianStatue.Phantoon] = new()
        {
            Statue = TourianStatue.Phantoon,
            GreyEvent = StoryEvent.PhantoonStatueGrey,
            FirstFrameDuration = 6,
            BossArea = AreaId.WreckedShip,
            DefeatedBosses = DefeatedBosses.AreaBoss,
            ClearedShade = TourianStatueShades.PhantoonCleared,
            Eye = new(new(132, 144), TourianStatueShades.PhantoonEyeGlow),
            ColourEffect = ColourEffectId.GreyOutTourianStatuePhantoon,
            GreyedSprite = TourianSpriteStatue.Phantoon,
            GreyShade = TourianStatueShades.PhantoonGrey,
        },
        [TourianStatue.Ridley] = new()
        {
            Statue = TourianStatue.Ridley,
            GreyEvent = StoryEvent.RidleyStatueGrey,
            FirstFrameDuration = 10,
            BossArea = AreaId.Norfair,
            DefeatedBosses = DefeatedBosses.AreaBoss,
            ClearedShade = TourianStatueShades.RidleyCleared,
            Eye = new(new(122, 81), TourianStatueShades.RidleyEyeGlow),
            ColourEffect = ColourEffectId.GreyOutTourianStatueRidley,
            GreyedSprite = TourianSpriteStatue.Ridley,
            GreyShade = TourianStatueShades.RidleyGrey,
        },
        [TourianStatue.Kraid] = new()
        {
            Statue = TourianStatue.Kraid,
            GreyEvent = StoryEvent.KraidStatueGrey,
            FirstFrameDuration = 4,
            BossArea = AreaId.Brinstar,
            DefeatedBosses = DefeatedBosses.AreaBoss,
            ClearedShade = TourianStatueShades.KraidCleared,
            Eye = new(new(104, 114), TourianStatueShades.KraidEyeGlow),
            ColourEffect = ColourEffectId.GreyOutTourianStatueKraid,
            GreyedSprite = null,
            GreyShade = TourianStatueShades.KraidGrey,
        },
        [TourianStatue.Draygon] = new()
        {
            Statue = TourianStatue.Draygon,
            GreyEvent = StoryEvent.DraygonStatueGrey,
            FirstFrameDuration = 8,
            BossArea = AreaId.Maridia,
            DefeatedBosses = DefeatedBosses.AreaBoss,
            ClearedShade = TourianStatueShades.DraygonCleared,
            Eye = new(new(158, 128), TourianStatueShades.DraygonEyeGlow),
            ColourEffect = ColourEffectId.GreyOutTourianStatueDraygon,
            GreyedSprite = null,
            GreyShade = TourianStatueShades.DraygonGrey,
        },
    });
}
