using SuperMetroid.Core.Animation;

namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>
/// Charge-flare places by their number among a component's pictures, its runs in order, as the original programs and the
/// flare's selector table count them (#2285): checks compare the typed places with those numbers.
/// </summary>
internal static class ChargeFlareAnimationsAccess
{
    /// <summary>Places by number.</summary>
    extension(ChargeFlareAnimations)
    {
        /// <summary>How many pictures <paramref name="component"/> has across its runs.</summary>
        internal static int PictureCount(ChargeFlareComponent component) =>
            ChargeFlareAnimationDefinitions.Runs[component].Sum(run => ChargeFlareAnimationDefinitions.Segments.Beats(run).Count);

        /// <summary>Every picture of <paramref name="component"/>, its runs in order.</summary>
        internal static IReadOnlyList<AnimationBeat<ChargeFlarePicture, NoAnimationCue>> Pictures(ChargeFlareComponent component) =>
            [.. ChargeFlareAnimationDefinitions.Runs[component].SelectMany(run => ChargeFlareAnimationDefinitions.Segments.Beats(run))];

        /// <summary>The place of <paramref name="component"/>'s picture numbered <paramref name="number"/>.</summary>
        /// <exception cref="ArgumentOutOfRangeException">The component has no picture of that number.</exception>
        internal static ChargeFlarePlace PlaceAt(ChargeFlareComponent component, int number)
        {
            int left = number;
            foreach (ChargeFlareSegment run in ChargeFlareAnimationDefinitions.Runs[component])
            {
                int count = ChargeFlareAnimationDefinitions.Segments.Beats(run).Count;
                if (left >= 0 && left < count)
                    return new(run, left);
                left -= count;
            }
            throw new ArgumentOutOfRangeException(nameof(number), number, $"Charge-flare {component} has no picture {number}.");
        }

        /// <summary>The number of the picture at <paramref name="place"/> among <paramref name="component"/>'s pictures.</summary>
        /// <exception cref="ArgumentOutOfRangeException">The place is none of the component's pictures.</exception>
        internal static int NumberOf(ChargeFlareComponent component, ChargeFlarePlace place)
        {
            int before = 0;
            foreach (ChargeFlareSegment run in ChargeFlareAnimationDefinitions.Runs[component])
            {
                int count = ChargeFlareAnimationDefinitions.Segments.Beats(run).Count;
                if (run == place.Segment && place.Beat >= 0 && place.Beat < count)
                    return before + place.Beat;
                before += count;
            }
            throw new ArgumentOutOfRangeException(nameof(place), place, $"Charge-flare {component} has no picture at {place}.");
        }

        /// <summary>The place just past <paramref name="component"/>'s last picture, which is none of its pictures.</summary>
        internal static ChargeFlarePlace PastLastPlace(ChargeFlareComponent component)
        {
            ChargeFlareSegment last = ChargeFlareAnimationDefinitions.Runs[component][^1];
            return new(last, ChargeFlareAnimationDefinitions.Segments.Beats(last).Count);
        }
    }
}
