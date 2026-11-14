using SuperMetroid.Core.Animation;
using static SuperMetroid.Core.Samus.Projectiles.ChargeFlareAnimationDefinitions;

namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>
/// How a charge-flare component moves through its animation (#1624): which picture it shows next and the wait it holds
/// that picture for. A component holds a picture for the wait and one update more: it counts the wait down each update
/// and shows the next picture on the update its count passes below zero.
/// </summary>
internal static class ChargeFlareAnimations
{
    /// <summary>The picture <paramref name="component"/>'s animation starts at, with its wait.</summary>
    internal static ChargeFlareStep First(ChargeFlareComponent component)
    {
        AnimationPlace place = AnimationPlace.StartOfAnimation;
        return Play(component, ref place);
    }

    /// <summary>The main flare's loop start, which the grapple beam's flare restarts at when fired, with its wait.</summary>
    internal static ChargeFlareStep MainFlareLoop() => After(ChargeFlareComponent.MainFlare, MainFlareLoopStart - 1);

    /// <summary>The picture <paramref name="component"/> shows after picture <paramref name="phase"/>, with its wait.</summary>
    /// <param name="component">The component.</param>
    /// <param name="phase">The picture shown, one of the animation's: only the Hyper Beam's backward count leaves a component
    /// outside its animation, and the ordinary flare never plays again once Samus has the Hyper Beam.</param>
    /// <exception cref="InvalidOperationException">The phase is outside the animation.</exception>
    internal static ChargeFlareStep Next(ChargeFlareComponent component, int phase) => After(component, phase);

    /// <summary>The main flare's picture after its place <paramref name="place"/>, one of its animation's, as the grapple beam's flare plays it.</summary>
    /// <exception cref="InvalidOperationException">The place is outside the main flare's animation.</exception>
    internal static ChargeFlareStep NextMainFlare(int place) => After(ChargeFlareComponent.MainFlare, place);

    /// <summary>Every component's first place, where a cleared flare stands.</summary>
    internal const int FirstPlace = 0;

    /// <summary>The picture <paramref name="component"/> shows at place <paramref name="place"/>, or null when the place is none of its animation's.</summary>
    internal static ChargeFlarePicture? PictureAt(ChargeFlareComponent component, int place) =>
        IsPicture(component, place) && Segments.Beats(component)[place] is AnimationBeat<ChargeFlarePicture, NoAnimationCue>.Frame shown
            ? shown.Shown
            : null;

    /// <summary>The last of <paramref name="component"/>'s pictures, where the Hyper Beam's flare starts it.</summary>
    internal static int LastPicture(ChargeFlareComponent component) => Segments.Beats(component).Count - 1;

    /// <summary>Whether <paramref name="phase"/> is one of <paramref name="component"/>'s pictures.</summary>
    internal static bool IsPicture(ChargeFlareComponent component, int phase) =>
        phase >= 0 && phase < Segments.Beats(component).Count;

    /// <summary>The picture <paramref name="component"/>'s animation plays after picture <paramref name="phase"/>.</summary>
    /// <exception cref="InvalidOperationException">The phase is not one of the animation's pictures.</exception>
    private static ChargeFlareStep After(ChargeFlareComponent component, int phase)
    {
        if (!IsPicture(component, phase))
            throw new InvalidOperationException($"Charge-flare {component} picture {phase} is outside its program.");
        AnimationPlace shown = AnimationSegments<ChargeFlareComponent, ChargeFlarePicture, NoAnimationCue>.PlaceOf(ChargeFlareAnimationDefinitions.Animations[component], component, phase);
        AnimationPlace place = shown with { Beat = shown.Beat + 1 };
        return Play(component, ref place);
    }

    /// <summary>Plays <paramref name="component"/>'s animation on from <paramref name="place"/> to its next picture.</summary>
    private static ChargeFlareStep Play(ChargeFlareComponent component, ref AnimationPlace place)
    {
        WrappingCountdown repeats = WrappingCountdown.Start(0);
        Animation<ChargeFlareComponent> animation = ChargeFlareAnimationDefinitions.Animations[component];
        AnimationShown<ChargeFlareComponent, ChargeFlarePicture> shown =
            Segments.PlayToNextFrame(animation, ref place, ref repeats, static _ => { });
        // The play stands just past the beat it showed, whose place in the component's beats is the place shown.
        return shown is AnimationShown<ChargeFlareComponent, ChargeFlarePicture>.Frame frame
            ? new(animation.Phases[place.Phase].FirstBeat + place.Beat - 1, WrappingCountdown.Start(frame.Duration - 1))
            : throw new InvalidOperationException($"Charge-flare {component}'s program shows no next picture.");
    }
}
