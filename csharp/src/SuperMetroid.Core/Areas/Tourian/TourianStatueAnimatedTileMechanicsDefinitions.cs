using SuperMetroid.Core.Assets;
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
    /// Each statue's transfer geometry, event, boss test, palette destinations and effect parameters, by its animated-tile
    /// object; no other animated-tile object is a statue (<see cref="TourianStatueAnimatedTiles"/>).
    /// </summary>
    internal static readonly IReadOnlyDictionary<AnimatedTileObject, TourianStatueAnimatedTileProgramDefinition> Statues =
        new Dictionary<AnimatedTileObject, TourianStatueAnimatedTileProgramDefinition>
    {
        [AnimatedTileObject.TourianStatuePhantoon] = new()
        {
            AnimatedObject = AnimatedTileObject.TourianStatuePhantoon,
            FrameByteCount = TourianStatueTileStripDefinitions.WindowByteCount(AnimatedTileObject.TourianStatuePhantoon),
            AnimatingFlag = TourianStatueAnimationState.PhantoonAnimating,
            GreyEvent = EventNumber.PhantoonStatueGrey,
            FirstFrameDuration = 6,
            BossArea = AreaId.WreckedShip,
            DefeatedBossBits = BossBits.AreaBoss,
            ClearedColours = new SpriteColourRun(2, 12, 3),
            UnlockEffectParameter = 0,
            PaletteFx = PaletteFxObject.GreyOutTourianStatuePhantoon,
            GreyColours = new SpriteColourRun(2, 0, TourianStatueColorDefinitions.GreyColorCount),
        },
        [AnimatedTileObject.TourianStatueRidley] = new()
        {
            AnimatedObject = AnimatedTileObject.TourianStatueRidley,
            FrameByteCount = TourianStatueTileStripDefinitions.WindowByteCount(AnimatedTileObject.TourianStatueRidley),
            AnimatingFlag = TourianStatueAnimationState.RidleyAnimating,
            GreyEvent = EventNumber.RidleyStatueGrey,
            FirstFrameDuration = 10,
            BossArea = AreaId.Norfair,
            DefeatedBossBits = BossBits.AreaBoss,
            ClearedColours = new SpriteColourRun(1, 9, 3),
            UnlockEffectParameter = 2,
            PaletteFx = PaletteFxObject.GreyOutTourianStatueRidley,
            GreyColours = new SpriteColourRun(1, 0, TourianStatueColorDefinitions.GreyColorCount),
        },
        [AnimatedTileObject.TourianStatueKraid] = new()
        {
            AnimatedObject = AnimatedTileObject.TourianStatueKraid,
            FrameByteCount = TourianStatueTileStripDefinitions.WindowByteCount(AnimatedTileObject.TourianStatueKraid),
            AnimatingFlag = TourianStatueAnimationState.KraidAnimating,
            GreyEvent = EventNumber.KraidStatueGrey,
            FirstFrameDuration = 4,
            BossArea = AreaId.Brinstar,
            DefeatedBossBits = BossBits.AreaBoss,
            ClearedColours = new BackgroundColourRun(7, 12, 3),
            UnlockEffectParameter = 6,
            PaletteFx = PaletteFxObject.GreyOutTourianStatueKraid,
            GreyColours = new BackgroundColourRun(7, 0, TourianStatueColorDefinitions.GreyColorCount),
        },
        [AnimatedTileObject.TourianStatueDraygon] = new()
        {
            AnimatedObject = AnimatedTileObject.TourianStatueDraygon,
            FrameByteCount = TourianStatueTileStripDefinitions.WindowByteCount(AnimatedTileObject.TourianStatueDraygon),
            AnimatingFlag = TourianStatueAnimationState.DraygonAnimating,
            GreyEvent = EventNumber.DraygonStatueGrey,
            FirstFrameDuration = 8,
            BossArea = AreaId.Maridia,
            DefeatedBossBits = BossBits.AreaBoss,
            ClearedColours = new BackgroundColourRun(6, 9, 3),
            UnlockEffectParameter = 4,
            PaletteFx = PaletteFxObject.GreyOutTourianStatueDraygon,
            GreyColours = new BackgroundColourRun(6, 0, TourianStatueColorDefinitions.GreyColorCount),
        },
    };
}
