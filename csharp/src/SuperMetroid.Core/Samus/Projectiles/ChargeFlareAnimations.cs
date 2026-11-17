using SuperMetroid.Core.Animation;
using static SuperMetroid.Core.Samus.Projectiles.ChargeFlareAnimationDefinitions;

namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>
/// How a charge-flare component moves through its animation (#1624, #2285): which picture it shows next and the wait it
/// holds that picture for. A component holds a picture for the wait and one update more: it counts the wait down each
/// update and shows the next picture on the update its count passes below zero.
/// </summary>
internal static class ChargeFlareAnimations
{
    /// <summary>The picture <paramref name="component"/>'s animation starts at, with its wait.</summary>
    internal static ChargeFlareStep First(ChargeFlareComponent component)
    {
        AnimationPlace place = AnimationPlace.StartOfAnimation;
        return Play(component, ref place);
    }

    /// <summary>The main flare's loop's first picture, which the grapple beam's flare restarts at when fired, with its wait.</summary>
    internal static ChargeFlareStep MainFlareLoop() => After(ChargeFlareComponent.MainFlare, LastOf(ChargeFlareSegment.MainFlareOpening));

    /// <summary>The picture <paramref name="component"/> shows after the picture at <paramref name="shown"/>, with its wait.</summary>
    /// <param name="component">The flare component whose pictures are read.</param>
    /// <param name="shown">The picture shown, one of the component's: only the Hyper Beam's backward count leaves a
    /// component outside its pictures, and the ordinary flare never plays again once Samus has the Hyper Beam.</param>
    /// <exception cref="InvalidOperationException">The place is none of the component's pictures.</exception>
    internal static ChargeFlareStep Next(ChargeFlareComponent component, ChargeFlarePlace shown) => After(component, shown);

    /// <summary>The place of <paramref name="component"/>'s first picture, where a cleared flare stands.</summary>
    internal static ChargeFlarePlace FirstPlace(ChargeFlareComponent component) => new(Runs[component][0], 0);

    /// <summary>The place of the last of <paramref name="component"/>'s pictures, where the Hyper Beam's flare starts it.</summary>
    internal static ChargeFlarePlace LastPlace(ChargeFlareComponent component) => LastOf(Runs[component][^1]);

    /// <summary>
    /// The place of the picture before <paramref name="place"/> among <paramref name="component"/>'s pictures, or null
    /// before its first: the Hyper Beam's backward count.
    /// </summary>
    internal static ChargeFlarePlace? Previous(ChargeFlareComponent component, ChargeFlarePlace place)
    {
        if (place.Beat > 0)
            return place with { Beat = place.Beat - 1 };
        int run = RunOf(component, place);
        return run > 0 ? LastOf(Runs[component][run - 1]) : null;
    }

    /// <summary>The picture <paramref name="component"/> shows at <paramref name="place"/>, or null when the place is none of its pictures.</summary>
    internal static ChargeFlarePicture? PictureAt(ChargeFlareComponent component, ChargeFlarePlace place) =>
        IsPicture(component, place) && Segments.Beats(place.Segment)[place.Beat] is AnimationBeat<ChargeFlarePicture, NoAnimationCue>.Frame shown
            ? shown.Shown
            : null;

    /// <summary>Whether <paramref name="place"/> is one of <paramref name="component"/>'s pictures.</summary>
    internal static bool IsPicture(ChargeFlareComponent component, ChargeFlarePlace place) =>
        Runs[component].Contains(place.Segment) && place.Beat >= 0 && place.Beat < Segments.Beats(place.Segment).Count;

    /// <summary>The place of the last picture of <paramref name="segment"/>.</summary>
    private static ChargeFlarePlace LastOf(ChargeFlareSegment segment) => new(segment, Segments.Beats(segment).Count - 1);

    /// <summary>Which of <paramref name="component"/>'s runs holds <paramref name="place"/>.</summary>
    /// <exception cref="InvalidOperationException">The place is none of the component's pictures.</exception>
    private static int RunOf(ChargeFlareComponent component, ChargeFlarePlace place) =>
        IsPicture(component, place)
            ? Runs[component].TakeWhile(run => run != place.Segment).Count()
            : throw new InvalidOperationException($"Charge-flare {component} has no picture at {place}.");

    /// <summary>The picture <paramref name="component"/>'s animation plays after the picture at <paramref name="shown"/>.</summary>
    /// <exception cref="InvalidOperationException">The place is not one of the component's pictures.</exception>
    private static ChargeFlareStep After(ChargeFlareComponent component, ChargeFlarePlace shown)
    {
        if (!IsPicture(component, shown))
            throw new InvalidOperationException($"Charge-flare {component} picture {shown} is outside its animation.");
        AnimationPlace at = AnimationSegments<ChargeFlareSegment, ChargeFlarePicture, NoAnimationCue>.PlaceOf(Animations[component], shown.Segment, shown.Beat);
        AnimationPlace place = at with { Beat = at.Beat + 1 };
        return Play(component, ref place);
    }

    /// <summary>Plays <paramref name="component"/>'s animation on from <paramref name="place"/> to its next picture.</summary>
    private static ChargeFlareStep Play(ChargeFlareComponent component, ref AnimationPlace place)
    {
        Animation<ChargeFlareSegment> animation = Animations[component];
        AnimationShown<ChargeFlareSegment, ChargeFlarePicture> shown =
            Segments.PlayToNextFrame(animation, ref place, static _ => { });
        // The play stands just past the beat it showed, in the phase that showed it.
        AnimationPhase<ChargeFlareSegment> phase = animation.Phases[place.Phase];
        return shown is AnimationShown<ChargeFlareSegment, ChargeFlarePicture>.Frame frame
            ? new(new ChargeFlarePlace(phase.Segment, phase.FirstBeat + place.Beat - 1), WrappingCountdown.Start(frame.Duration - 1))
            : throw new InvalidOperationException($"Charge-flare {component}'s animation shows no next picture.");
    }
}
