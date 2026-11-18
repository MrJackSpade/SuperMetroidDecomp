using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Samus.Abilities;
using W = SuperMetroid.Core.Samus.Death.SamusWhiteoutSegment;

namespace SuperMetroid.Core.Samus.Death;

/// <summary>
/// The shade sequences Samus's death plays (#2387): the flash alternates her second death shade for one update with her
/// first for three, after waiting three updates, until the suit explodes; the explosion's whiteout brightens through its
/// shades one update each and rests on its brightest.
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
            [W.Brightening] =
            [
                Whiteout(0), Whiteout(1), Whiteout(2), Whiteout(3), Whiteout(4), Whiteout(5), Whiteout(6), Whiteout(7), Whiteout(8),
                Whiteout(9), Whiteout(10), Whiteout(11), Whiteout(12), Whiteout(13), Whiteout(14), Whiteout(15), Whiteout(16),
                Whiteout(17), Whiteout(18), Whiteout(19),
            ],
            [W.Brightest] = [new AnimationBeat<SamusWhiteoutShade, NoAnimationCue>.Frame(SamusWhiteoutShade.Brightest, 1)],
        });

    /// <summary>The whiteout, brightening and then showing its brightest shade on every update.</summary>
    internal static Animation<W> WhiteoutAnimation { get; } =
        Animation<W>.LeadingInto([WhiteoutSegments.Whole(W.Brightening)], [WhiteoutSegments.Whole(W.Brightest)]);

    /// <summary>The flash as the death pose starts it: its first shade shows on its third update.</summary>
    internal static AnimationPlayer<SamusDeathFlashSegment, SamusDeathShade, NoAnimationCue> FlashStarting() =>
        AnimationPlayer<SamusDeathFlashSegment, SamusDeathShade, NoAnimationCue>.StartingAfter(
            FlashSegments, Flash, SamusDeathSequenceDefinitions.FirstFlashShadeUpdates);

    /// <summary>The whiteout before its first shade, which its first update shows.</summary>
    internal static AnimationPlayer<W, SamusWhiteoutShade, NoAnimationCue> WhiteoutStarting() =>
        AnimationPlayer<W, SamusWhiteoutShade, NoAnimationCue>.Starting(WhiteoutSegments, WhiteoutAnimation);

    /// <summary>Shows whiteout shade <paramref name="step"/> for one update.</summary>
    private static AnimationBeat<SamusWhiteoutShade, NoAnimationCue> Whiteout(int step) =>
        new AnimationBeat<SamusWhiteoutShade, NoAnimationCue>.Frame(new SamusWhiteoutShade(step), 1);
}
