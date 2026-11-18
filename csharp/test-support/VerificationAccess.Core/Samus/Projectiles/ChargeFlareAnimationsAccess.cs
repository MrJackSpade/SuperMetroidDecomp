using SuperMetroid.Core.Animation;

namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>
/// Charge-flare pictures by their number among a component's pictures, its phases in order, as the original programs and
/// the flare's selector table count them (#2285, #2396), and a component's animation read and set as the original's picture
/// number and wait: checks compare the shared player's play with those numbers.
/// </summary>
internal static class ChargeFlareAnimationsAccess
{
    /// <summary>Pictures by number.</summary>
    extension(ChargeFlareAnimationDefinitions)
    {
        /// <summary>How many pictures <paramref name="component"/> has across its phases.</summary>
        internal static int PictureCount(ChargeFlareComponent component) =>
            ChargeFlareAnimationDefinitions.Animations[component].Phases.Sum(phase => phase.BeatCount * phase.Plays);

        /// <summary>Every picture of <paramref name="component"/>, its phases in order.</summary>
        internal static IReadOnlyList<AnimationBeat<ChargeFlarePicture, NoAnimationCue>> Pictures(ChargeFlareComponent component) =>
            ChargeFlareAnimationDefinitions.Segments.PlayedBeats(ChargeFlareAnimationDefinitions.Animations[component]);

        /// <summary>The picture of <paramref name="component"/> numbered <paramref name="number"/>.</summary>
        /// <exception cref="ArgumentOutOfRangeException">The component has no picture of that number.</exception>
        internal static ChargeFlarePicture PictureNumbered(ChargeFlareComponent component, int number) =>
            number >= 0 && number < ChargeFlareAnimationDefinitions.PictureCount(component)
                ? ChargeFlareAnimationDefinitions.Pictures(component)[number].ShownFrame
                    ?? throw new InvalidOperationException($"Charge-flare {component} picture {number} shows no picture.")
                : throw new ArgumentOutOfRangeException(nameof(number), number, $"Charge-flare {component} has no picture {number}.");

        /// <summary>The place of <paramref name="component"/>'s animation whose next beat shows its picture numbered <paramref name="number"/>.</summary>
        /// <exception cref="ArgumentOutOfRangeException">The component has no picture of that number.</exception>
        internal static AnimationPlace PlaceOfPicture(ChargeFlareComponent component, int number)
        {
            IReadOnlyList<AnimationPhase<ChargeFlareSegment>> phases = ChargeFlareAnimationDefinitions.Animations[component].Phases;
            int left = number;
            for (int phase = 0; phase < phases.Count; phase++)
            {
                // A repeated phase's pictures are numbered through each of its plays in turn.
                AnimationPhase<ChargeFlareSegment> numbered = phases[phase];
                if (left >= 0 && left < numbered.BeatCount * numbered.Plays)
                    return new AnimationPlace(phase, left % numbered.BeatCount)
                        .WithPlaysLeft(numbered.IsRepeated ? numbered.Plays - left / numbered.BeatCount : 0);
                left -= numbered.BeatCount * numbered.Plays;
            }
            throw new ArgumentOutOfRangeException(nameof(number), number, $"Charge-flare {component} has no picture {number}.");
        }
    }

    /// <summary>A component's animation as the original counts it.</summary>
    extension(ChargeFlareComponentState self)
    {
        /// <summary>The play of the component's animation, which only the component's own updates step.</summary>
        internal AnimationPlayer<ChargeFlareSegment, ChargeFlarePicture, NoAnimationCue> Player() =>
            PrivateState.Field<ChargeFlareComponentState, AnimationPlayer<ChargeFlareSegment, ChargeFlarePicture, NoAnimationCue>>(self, "player");

        /// <summary>
        /// The number of the picture the component shows among its pictures, or null once the Hyper Beam's count has
        /// stepped back past its first.
        /// </summary>
        internal int? NativePicture
        {
            get
            {
                if (self.Picture is null)
                    return null;
                // The place stands just past the beat shown.
                AnimationPlace place = self.Player().Playback.Place;
                IReadOnlyList<AnimationPhase<ChargeFlareSegment>> phases = self.Player().Playback.Animation.Phases;
                AnimationPhase<ChargeFlareSegment> playing = phases[place.Phase];
                int playsBefore = playing.IsRepeated ? playing.Plays - place.PlaysLeft : 0;
                return phases.Take(place.Phase).Sum(phase => phase.BeatCount * phase.Plays) +
                    playsBefore * playing.BeatCount + place.Beat - 1;
            }
        }

        /// <summary>
        /// The ordinary flare's wait as the original keeps it: the updates its picture still shows less one, so the update
        /// that takes it below zero shows the next picture.
        /// </summary>
        internal WrappingCountdown NativeWait => WrappingCountdown.Start(self.Player().UpdatesLeft - 1);

        /// <summary>
        /// Shows the ordinary flare's picture numbered <paramref name="number"/> with the original's wait
        /// <paramref name="wait"/>, which the ordinary flare never leaves run past zero.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">
        /// The component has no picture of that number, or the wait has run past zero.
        /// </exception>
        internal void ShowNative(int number, WrappingCountdown wait) =>
            self.Player().ShowAt(ChargeFlareAnimationDefinitions.Animations[self.Component],
                ChargeFlareAnimationDefinitions.PlaceOfPicture(self.Component, number),
                wait.IsNegative
                    ? throw new ArgumentOutOfRangeException(nameof(wait), wait, "The ordinary flare never leaves a wait run past zero.")
                    : wait.RemainingFrames + 1);

        /// <summary>
        /// Starts the Hyper Beam's count on the component and counts it back until it has stepped back past its first
        /// picture, where it shows nothing.
        /// </summary>
        internal void RunPastFirstPicture()
        {
            self.StartBackward();
            while (self.Picture is not null)
                _ = self.PlayBackward();
        }
    }
}
