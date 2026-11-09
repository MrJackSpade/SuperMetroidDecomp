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
    /// Each statue's transfer geometry, event, boss test, shades, eye and palette effect, by its animated-tile
    /// object; no other animated-tile object is a statue (<see cref="TourianStatueAnimatedTiles"/>).
    /// </summary>
    internal static readonly IReadOnlyDictionary<AnimatedTileObject, TourianStatueAnimatedTileProgramDefinition> Statues =
        new Dictionary<AnimatedTileObject, TourianStatueAnimatedTileProgramDefinition>
    {
        [AnimatedTileObject.TourianStatuePhantoon] = new()
        {
            AnimatedObject = AnimatedTileObject.TourianStatuePhantoon,
            FrameByteCount = TourianStatueTileStrip.PictureArtworkSize(AnimatedTileObject.TourianStatuePhantoon),
            Statue = TourianGreyStatues.Phantoon,
            GreyEvent = StoryEvent.PhantoonStatueGrey,
            FirstFrameDuration = 6,
            BossArea = AreaId.WreckedShip,
            DefeatedBossBits = BossBits.AreaBoss,
            ClearedShade = TourianStatueShades.PhantoonCleared,
            Eye = new(new(132, 144), TourianStatueShades.EyeGlow(0)),
            PaletteFx = PaletteFxObject.GreyOutTourianStatuePhantoon,
            GreyShade = TourianStatueShades.PhantoonGrey,
        },
        [AnimatedTileObject.TourianStatueRidley] = new()
        {
            AnimatedObject = AnimatedTileObject.TourianStatueRidley,
            FrameByteCount = TourianStatueTileStrip.PictureArtworkSize(AnimatedTileObject.TourianStatueRidley),
            Statue = TourianGreyStatues.Ridley,
            GreyEvent = StoryEvent.RidleyStatueGrey,
            FirstFrameDuration = 10,
            BossArea = AreaId.Norfair,
            DefeatedBossBits = BossBits.AreaBoss,
            ClearedShade = TourianStatueShades.RidleyCleared,
            Eye = new(new(122, 81), TourianStatueShades.EyeGlow(1)),
            PaletteFx = PaletteFxObject.GreyOutTourianStatueRidley,
            GreyShade = TourianStatueShades.RidleyGrey,
        },
        [AnimatedTileObject.TourianStatueKraid] = new()
        {
            AnimatedObject = AnimatedTileObject.TourianStatueKraid,
            FrameByteCount = TourianStatueTileStrip.PictureArtworkSize(AnimatedTileObject.TourianStatueKraid),
            Statue = TourianGreyStatues.Kraid,
            GreyEvent = StoryEvent.KraidStatueGrey,
            FirstFrameDuration = 4,
            BossArea = AreaId.Brinstar,
            DefeatedBossBits = BossBits.AreaBoss,
            ClearedShade = TourianStatueShades.KraidCleared,
            Eye = new(new(104, 114), TourianStatueShades.EyeGlow(3)),
            PaletteFx = PaletteFxObject.GreyOutTourianStatueKraid,
            GreyShade = TourianStatueShades.KraidGrey,
        },
        [AnimatedTileObject.TourianStatueDraygon] = new()
        {
            AnimatedObject = AnimatedTileObject.TourianStatueDraygon,
            FrameByteCount = TourianStatueTileStrip.PictureArtworkSize(AnimatedTileObject.TourianStatueDraygon),
            Statue = TourianGreyStatues.Draygon,
            GreyEvent = StoryEvent.DraygonStatueGrey,
            FirstFrameDuration = 8,
            BossArea = AreaId.Maridia,
            DefeatedBossBits = BossBits.AreaBoss,
            ClearedShade = TourianStatueShades.DraygonCleared,
            Eye = new(new(158, 128), TourianStatueShades.EyeGlow(2)),
            PaletteFx = PaletteFxObject.GreyOutTourianStatueDraygon,
            GreyShade = TourianStatueShades.DraygonGrey,
        },
    };
}
