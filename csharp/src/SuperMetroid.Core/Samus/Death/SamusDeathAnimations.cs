using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Samus.Abilities;
using E = SuperMetroid.Core.Samus.Artwork.SamusDeathExplosionFrame;
using W = SuperMetroid.Core.Samus.Death.SamusWhiteoutSegment;

namespace SuperMetroid.Core.Samus.Death;

/// <summary>
/// The shade sequences Samus's death plays (#2387): the flash alternates her second death shade for one update with her
/// first for three, after waiting three updates, until the suit explodes; the suit's explosion shows its nine pictures for
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

    /// <summary>The whiteout's shades, one update each.</summary>
    internal static AnimationSegments<W, SamusWhiteoutShade, NoAnimationCue> WhiteoutSegments { get; } = new(
        new Dictionary<W, AnimationBeat<SamusWhiteoutShade, NoAnimationCue>[]>
        {
            [W.Brightening] = [.. SamusWhiteoutShade.Brightening.Select(Whiteout)],
            [W.Brightest] = [Whiteout(SamusWhiteoutShade.Brightest)],
        });

    /// <summary>The whiteout, brightening and then showing its brightest shade on every update.</summary>
    internal static Animation<W> WhiteoutAnimation { get; } =
        Animation<W>.LeadingInto([WhiteoutSegments.Whole(W.Brightening)], [WhiteoutSegments.Whole(W.Brightest)]);

    /// <summary>The suit explosion's pictures, each with the updates it shows for.</summary>
    internal static AnimationSegments<SamusDeathExplosionSegment, E, NoAnimationCue> ExplosionSegments { get; } = new(
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

    /// <summary>The whiteout before its first shade, which its first update shows.</summary>
    internal static AnimationPlayer<W, SamusWhiteoutShade, NoAnimationCue> WhiteoutStarting() =>
        AnimationPlayer<W, SamusWhiteoutShade, NoAnimationCue>.Starting(WhiteoutSegments, WhiteoutAnimation);

    /// <summary>Shows explosion picture <paramref name="frame"/> for <paramref name="updates"/> updates.</summary>
    private static AnimationBeat<E, NoAnimationCue> Picture(E frame, int updates) => new AnimationBeat<E, NoAnimationCue>.Frame(frame, updates);

    /// <summary>Shows whiteout shade <paramref name="shade"/> for one update.</summary>
    private static AnimationBeat<SamusWhiteoutShade, NoAnimationCue> Whiteout(SamusWhiteoutShade shade) =>
        new AnimationBeat<SamusWhiteoutShade, NoAnimationCue>.Frame(shade, 1);
}
