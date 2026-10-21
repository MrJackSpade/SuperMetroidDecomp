using System.Diagnostics.CodeAnalysis;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Fx.PaletteFx;
namespace SuperMetroid.Core.Areas.Tourian;

/// <summary>
/// The four Tourian entrance statues' animated-tile definitions: artwork transfer geometry and the operands their
/// shared typed program (<see cref="TourianStatueTileAnimations"/>) acts on.
/// </summary>
/// <remarks>
/// The pictures each program shows are presentation references in
/// <see cref="SuperMetroid.Core.Areas.Tourian.Artwork.TourianStatueAnimatedTileArtworkDefinitions"/>.
/// </remarks>
public static class TourianStatueAnimatedTileMechanicsDefinitions
{
    /// <summary>
    /// The statue identity selects its transfer geometry, event, boss test, palette destinations and effect
    /// parameters. Animated-tile objects that are not statues return null.
    /// </summary>
    private static TourianStatueAnimatedTileProgramDefinition? SelectObject(AnimatedTileObject objectPointer) =>
        objectPointer switch
    {
        AnimatedTileObject.TourianStatuePhantoon => new()
        {
            ObjectPointer = AnimatedTileObject.TourianStatuePhantoon,
            TransferByteCount = 128,
            EncodedVramDestination = 0x7800,
            AnimatingFlag = TourianStatueAnimationState.PhantoonAnimating,
            GreyEvent = EventNumber.PhantoonStatueGrey,
            FirstFrameDuration = 6,
            BossArea = AreaId.WreckedShip,
            DefeatedBossBits = BossBits.AreaBoss,
            ClearedColorIndex = 172,
            UnlockEffectParameter = 0,
            PaletteFx = PaletteFxObject.GreyOutTourianStatuePhantoon,
            GreyColorIndex = 160,
        },
        AnimatedTileObject.TourianStatueRidley => new()
        {
            ObjectPointer = AnimatedTileObject.TourianStatueRidley,
            TransferByteCount = 64,
            EncodedVramDestination = 0x7220,
            AnimatingFlag = TourianStatueAnimationState.RidleyAnimating,
            GreyEvent = EventNumber.RidleyStatueGrey,
            FirstFrameDuration = 10,
            BossArea = AreaId.Norfair,
            DefeatedBossBits = BossBits.AreaBoss,
            ClearedColorIndex = 153,
            UnlockEffectParameter = 2,
            PaletteFx = PaletteFxObject.GreyOutTourianStatueRidley,
            GreyColorIndex = 144,
        },
        AnimatedTileObject.TourianStatueKraid => new()
        {
            ObjectPointer = AnimatedTileObject.TourianStatueKraid,
            TransferByteCount = 64,
            EncodedVramDestination = 0x0b40,
            AnimatingFlag = TourianStatueAnimationState.KraidAnimating,
            GreyEvent = EventNumber.KraidStatueGrey,
            FirstFrameDuration = 4,
            BossArea = AreaId.Brinstar,
            DefeatedBossBits = BossBits.AreaBoss,
            ClearedColorIndex = 124,
            UnlockEffectParameter = 6,
            PaletteFx = PaletteFxObject.GreyOutTourianStatueKraid,
            GreyColorIndex = 112,
        },
        AnimatedTileObject.TourianStatueDraygon => new()
        {
            ObjectPointer = AnimatedTileObject.TourianStatueDraygon,
            TransferByteCount = 128,
            EncodedVramDestination = 0x0ca0,
            AnimatingFlag = TourianStatueAnimationState.DraygonAnimating,
            GreyEvent = EventNumber.DraygonStatueGrey,
            FirstFrameDuration = 8,
            BossArea = AreaId.Maridia,
            DefeatedBossBits = BossBits.AreaBoss,
            ClearedColorIndex = 105,
            UnlockEffectParameter = 4,
            PaletteFx = PaletteFxObject.GreyOutTourianStatueDraygon,
            GreyColorIndex = 96,
        },
        AnimatedTileObject.None or AnimatedTileObject.Empty or AnimatedTileObject.HorizontalSpikes or
            AnimatedTileObject.VerticalSpikes or AnimatedTileObject.CrateriaLake or
            AnimatedTileObject.UnusedCrateriaLava or AnimatedTileObject.BrinstarPlant or
            AnimatedTileObject.WreckedShipScreen or AnimatedTileObject.MaridiaSandCeiling or
            AnimatedTileObject.MaridiaSandFalling or AnimatedTileObject.WreckedShipTreadmillRightwards or
            AnimatedTileObject.WreckedShipTreadmillLeftwards or AnimatedTileObject.Lava or
            AnimatedTileObject.Acid or AnimatedTileObject.Rain or AnimatedTileObject.Spores => null,
        _ => throw new InvalidOperationException($"Undefined AnimatedTileObject {objectPointer}."),
    };

    /// <summary>The definition of an object this catalog supports.</summary>
    private static TourianStatueAnimatedTileProgramDefinition Require(AnimatedTileObject objectPointer) => SelectObject(objectPointer) ??
        throw new InvalidOperationException($"{objectPointer} has no definition in this catalog.");

    /// <summary>The four boss-statue programs in cartridge header order, without a stored roster.</summary>
    public static IEnumerable<TourianStatueAnimatedTileProgramDefinition> All
    {
        get
        {
            yield return Require(AnimatedTileObject.TourianStatuePhantoon);
            yield return Require(AnimatedTileObject.TourianStatueRidley);
            yield return Require(AnimatedTileObject.TourianStatueKraid);
            yield return Require(AnimatedTileObject.TourianStatueDraygon);
        }
    }

    /// <summary>Resolves one statue's animated-tile definition.</summary>
    public static bool TryResolveObjectHeader(
        AnimatedTileObject objectPointer,
        [NotNullWhen(true)] out TourianStatueAnimatedTileProgramDefinition? definition)
    {
        definition = SelectObject(objectPointer);
        return definition is not null;
    }
}
