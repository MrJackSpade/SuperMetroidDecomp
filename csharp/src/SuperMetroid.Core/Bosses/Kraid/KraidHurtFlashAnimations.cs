using SuperMetroid.Core.Animation;
using S = SuperMetroid.Core.Bosses.Kraid.KraidHurtFlashShade;

namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>
/// Kraid's hurt flash as a shade sequence played by the shared player (#2400). A mouth hit starts it: it waits its opening beat,
/// counting the hit's own update, then shows his hurt colours, his band's, his hurt colours, his band's, his hurt colours
/// and his band's, each for <see cref="KraidArenaDefinitions.HurtFlashStepUpdates"/>, and rests on his band's colours. A hit while it plays starts it again; his health
/// running out cuts it short.
/// </summary>
internal static class KraidHurtFlashAnimations
{
    /// <summary>The flash's runs, each beat the colours it shows with the updates it shows them.</summary>
    internal static AnimationSegments<KraidHurtFlashSegment, S, NoAnimationCue> Segments { get; } = new(
        new Dictionary<KraidHurtFlashSegment, AnimationBeat<S, NoAnimationCue>[]>
        {
            [KraidHurtFlashSegment.Flashing] =
            [
                Show(S.HurtColours), Show(S.BandColours), Show(S.HurtColours), Show(S.BandColours), Show(S.HurtColours),
                new AnimationBeat<S, NoAnimationCue>.HeldFrame(S.BandColours),
            ],
            [KraidHurtFlashSegment.Settled] = [new AnimationBeat<S, NoAnimationCue>.HeldFrame(S.BandColours)],
        });

    /// <summary>The flash a mouth hit plays, resting on his band's colours once it has played.</summary>
    internal static Animation<KraidHurtFlashSegment> Flash { get; } = new([Segments.Whole(KraidHurtFlashSegment.Flashing)], AnimationEnd.Holds);

    /// <summary>No flash: his band's colours for good.</summary>
    internal static Animation<KraidHurtFlashSegment> Rest { get; } = new([Segments.Whole(KraidHurtFlashSegment.Settled)], AnimationEnd.Holds);

    /// <summary>The flash as a mouth hit starts it: its first colours show on the second update, counting the hit's own.</summary>
    internal static AnimationPlayer<KraidHurtFlashSegment, S, NoAnimationCue> Starting() =>
        AnimationPlayer<KraidHurtFlashSegment, S, NoAnimationCue>.StartingAfter(Segments, Flash, KraidArenaDefinitions.HurtFlashStepUpdates);

    /// <summary>No flash, resting on his band's colours, which it shows from the start.</summary>
    internal static AnimationPlayer<KraidHurtFlashSegment, S, NoAnimationCue> Resting() =>
        AnimationPlayer<KraidHurtFlashSegment, S, NoAnimationCue>.StartShowing(Segments, Rest);

    /// <summary>Shows <paramref name="shade"/> for one step of the flash.</summary>
    private static AnimationBeat<S, NoAnimationCue> Show(S shade) =>
        new AnimationBeat<S, NoAnimationCue>.Frame(shade, KraidArenaDefinitions.HurtFlashStepUpdates);
}
