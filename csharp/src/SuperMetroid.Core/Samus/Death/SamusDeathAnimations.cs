using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Bosses;
using SuperMetroid.Core.Samus.Abilities;
using E = SuperMetroid.Core.Samus.Artwork.SamusDeathExplosionFrame;

namespace SuperMetroid.Core.Samus.Death;

/// <summary>
/// The shade sequences Samus's death plays (#2387): the flash alternates her second death shade (<see cref="SamusDeathSequenceDefinitions.SecondFlashShadeUpdates"/>) with her
/// first (<see cref="SamusDeathSequenceDefinitions.FirstFlashShadeUpdates"/>), after an opening wait, until the suit explodes; the suit's explosion shows its pictures for
/// their authored dwell times (#2388), the first holding the room as it was before the whiteout and the last holding the
/// white before the room fades; the explosion's whiteout brightens through its shades one update each and rests on its
/// brightest.
/// </summary>
internal static class SamusDeathAnimations
{
    /// <summary>The flash's shades, each with the updates it shows for.</summary>
    internal static AnimationSegments<SamusDeathFlashSegment, SamusDeathShade, NoAnimationCue> FlashSegments { get; } = new(
        new Dictionary<SamusDeathFlashSegment, AnimationBeat<SamusDeathShade, NoAnimationCue>[]>
        {
            [SamusDeathFlashSegment.Flash] =
            [
                new AnimationBeat<SamusDeathShade, NoAnimationCue>.Frame(SamusDeathShade.Second, SamusDeathSequenceDefinitions.SecondFlashShadeUpdates),
                new AnimationBeat<SamusDeathShade, NoAnimationCue>.Frame(SamusDeathShade.First, SamusDeathSequenceDefinitions.FirstFlashShadeUpdates),
            ],
        });

    /// <summary>The flash, alternating its two shades for as long as it lasts.</summary>
    internal static Animation<SamusDeathFlashSegment> Flash { get; } =
        Animation<SamusDeathFlashSegment>.Looping([FlashSegments.Whole(SamusDeathFlashSegment.Flash)]);

    /// <summary>The suit explosion's pictures, each with the updates it shows for.</summary>
    private static AnimationSegments<SamusDeathExplosionSegment, E, NoAnimationCue> ExplosionSegments { get; } = new(
        new Dictionary<SamusDeathExplosionSegment, AnimationBeat<E, NoAnimationCue>[]>
        {
            [SamusDeathExplosionSegment.Explosion] =
            [
                Picture(E.SuitGlowing, 21), Picture(E.SuitCracking, 6), Picture(E.SuitShattering, 3), Picture(E.PiecesFlying, 4),
                Picture(E.PiecesSpreading, 5), Picture(E.PiecesScattered, 5), Picture(E.SuitlessHairStreaming, 6),
                Picture(E.SuitlessHairGleaming, 6), Picture(E.SuitlessHairDimming, 80),
            ],
        });

    /// <summary>The suit explosion, played once: once its last picture has shown, the death sequence is complete.</summary>
    internal static Animation<SamusDeathExplosionSegment> Explosion { get; } =
        new([ExplosionSegments.Whole(SamusDeathExplosionSegment.Explosion)], AnimationEnd.Stops);

    /// <summary>The suit explosion as the flash's last update starts it: its first picture shown at once, for its own updates.</summary>
    internal static AnimationPlayer<SamusDeathExplosionSegment, E, NoAnimationCue> ExplosionStarting() =>
        AnimationPlayer<SamusDeathExplosionSegment, E, NoAnimationCue>.StartShowing(ExplosionSegments, Explosion);

    /// <summary>The flash as the death pose starts it: its first shade shows on its third update.</summary>
    internal static AnimationPlayer<SamusDeathFlashSegment, SamusDeathShade, NoAnimationCue> FlashStarting() =>
        AnimationPlayer<SamusDeathFlashSegment, SamusDeathShade, NoAnimationCue>.StartingAfter(
            FlashSegments, Flash, SamusDeathSequenceDefinitions.FirstFlashShadeUpdates);

    /// <summary>The whiteout's first shade: its shades brighten one update each to its brightest, where it rests.</summary>
    internal static ListStep<SamusWhiteoutShade> WhiteoutStarting() =>
        ListStep<SamusWhiteoutShade>.FirstOf([.. SamusWhiteoutShade.Brightening, SamusWhiteoutShade.Brightest]);

    /// <summary>Shows explosion picture <paramref name="frame"/> for <paramref name="updates"/> updates.</summary>
    private static AnimationBeat<E, NoAnimationCue> Picture(E frame, int updates) => new AnimationBeat<E, NoAnimationCue>.Frame(frame, updates);
}
