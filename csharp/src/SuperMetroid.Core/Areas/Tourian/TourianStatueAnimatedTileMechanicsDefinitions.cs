using SuperMetroid.Core.Fx.Artwork;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Fx.PaletteFx;
using SuperMetroid.Core.Areas.Tourian.Artwork;
namespace SuperMetroid.Core.Areas.Tourian;

/// <summary>
/// The four Tourian entrance statues' animated-tile definitions: artwork transfer geometry and the operands their
/// shared typed program (<see cref="TourianStatueTileAnimations"/>) acts on.
/// </summary>
/// <remarks>
/// <see cref="TourianStatueAnimatedTiles"/> resolves them. The pictures each program shows are presentation references in
/// <see cref="SuperMetroid.Core.Fx.Artwork.TourianStatueTileStripDefinitions"/>.
/// </remarks>
public static class TourianStatueAnimatedTileMechanicsDefinitions
{
    /// <summary>
    /// Each statue's transfer geometry, event, boss test, shades and effect parameters, by its animated-tile
    /// object; no other animated-tile object is a statue (<see cref="TourianStatueAnimatedTiles"/>).
    /// </summary>
    internal static readonly IReadOnlyDictionary<AnimatedTileObject, TourianStatueAnimatedTileProgramDefinition> Statues =
        new Dictionary<AnimatedTileObject, TourianStatueAnimatedTileProgramDefinition>
    {
        [AnimatedTileObject.TourianStatuePhantoon] = new()
        {
            AnimatedObject = AnimatedTileObject.TourianStatuePhantoon,
            FrameByteCount = TourianStatueTileStrip.WindowByteCount(AnimatedTileObject.TourianStatuePhantoon),
            AnimatingFlag = TourianStatueAnimationState.PhantoonAnimating,
            GreyEvent = EventNumber.PhantoonStatueGrey,
            FirstFrameDuration = 6,
            BossArea = AreaId.WreckedShip,
            DefeatedBossBits = BossBits.AreaBoss,
            ClearedShade = TourianStatueShades.PhantoonCleared,
            UnlockEffectParameter = 0,
            PaletteFx = PaletteFxObject.GreyOutTourianStatuePhantoon,
            GreyShade = TourianStatueShades.PhantoonGrey,
        },
        [AnimatedTileObject.TourianStatueRidley] = new()
        {
            AnimatedObject = AnimatedTileObject.TourianStatueRidley,
            FrameByteCount = TourianStatueTileStrip.WindowByteCount(AnimatedTileObject.TourianStatueRidley),
            AnimatingFlag = TourianStatueAnimationState.RidleyAnimating,
            GreyEvent = EventNumber.RidleyStatueGrey,
            FirstFrameDuration = 10,
            BossArea = AreaId.Norfair,
            DefeatedBossBits = BossBits.AreaBoss,
            ClearedShade = TourianStatueShades.RidleyCleared,
            UnlockEffectParameter = 2,
            PaletteFx = PaletteFxObject.GreyOutTourianStatueRidley,
            GreyShade = TourianStatueShades.RidleyGrey,
        },
        [AnimatedTileObject.TourianStatueKraid] = new()
        {
            AnimatedObject = AnimatedTileObject.TourianStatueKraid,
            FrameByteCount = TourianStatueTileStrip.WindowByteCount(AnimatedTileObject.TourianStatueKraid),
            AnimatingFlag = TourianStatueAnimationState.KraidAnimating,
            GreyEvent = EventNumber.KraidStatueGrey,
            FirstFrameDuration = 4,
            BossArea = AreaId.Brinstar,
            DefeatedBossBits = BossBits.AreaBoss,
            ClearedShade = TourianStatueShades.KraidCleared,
            UnlockEffectParameter = 6,
            PaletteFx = PaletteFxObject.GreyOutTourianStatueKraid,
            GreyShade = TourianStatueShades.KraidGrey,
        },
        [AnimatedTileObject.TourianStatueDraygon] = new()
        {
            AnimatedObject = AnimatedTileObject.TourianStatueDraygon,
            FrameByteCount = TourianStatueTileStrip.WindowByteCount(AnimatedTileObject.TourianStatueDraygon),
            AnimatingFlag = TourianStatueAnimationState.DraygonAnimating,
            GreyEvent = EventNumber.DraygonStatueGrey,
            FirstFrameDuration = 8,
            BossArea = AreaId.Maridia,
            DefeatedBossBits = BossBits.AreaBoss,
            ClearedShade = TourianStatueShades.DraygonCleared,
            UnlockEffectParameter = 4,
            PaletteFx = PaletteFxObject.GreyOutTourianStatueDraygon,
            GreyShade = TourianStatueShades.DraygonGrey,
        },
    };
}
