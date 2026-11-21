using SuperMetroid.Core.Samus.Projectiles;

namespace SuperMetroid.Core.Samus.Grapple;

/// <summary>Verification access to <see cref="SamusGrappleState"/> members production does not use.</summary>
internal static class SamusGrappleStateAccess
{
    /// <summary>Exposes <see cref="SamusGrappleState"/> state that only verification reads.</summary>
    extension(SamusGrappleState self)
    {
        /// <summary>The installed flare origins, or none before they are installed; never changes physical hand offsets.</summary>
        public SuperMetroid.Core.Samus.Artwork.ChargeFlarePlacements FlarePlacement => self.Artwork.GrappleFlarePlacement;
        /// <summary>The installed swing pictures, or none before they are installed; never a collision/body offset source.</summary>
        public SuperMetroid.Core.Samus.Artwork.InstalledGrappleSwingFrames SwingFrames => self.Artwork.GrappleSwingFrames;
<<<<<<< ours

        /// <summary>
        /// The grapple flare's picture as the original numbers the main flare's pictures (#2396): sixteen opening pictures,
        /// then the loop's fourteen.
        /// </summary>
        internal int FlareFrame
        {
            get => NativePicture(self.Flare.Player.Playback);
            set => ShowNative(self.Flare.Player, value, self.Flare.Player.UpdatesLeft);
        }

        /// <summary>The grapple flare's wait as the original keeps it: the updates its picture still shows less one.</summary>
        internal WrappingCountdown FlareAnimationTimer
        {
            get => WrappingCountdown.Start(self.Flare.Player.UpdatesLeft - 1);
            set => ShowNative(self.Flare.Player, self.FlareFrame, value.IsNegative
                ? throw new ArgumentOutOfRangeException(nameof(value), value, "The grapple's flare never leaves a wait run past zero.")
                : value.RemainingFrames + 1);
        }
||||||| base

        /// <summary>
        /// The grapple flare's picture as the original numbers the main flare's pictures (#2396): sixteen opening pictures,
        /// then the loop's fourteen.
        /// </summary>
        internal int FlareFrame
        {
            get => self.Flare.NativePicture ?? throw new InvalidOperationException("The grapple's flare shows no picture.");
            set => self.Flare.ShowNative(value, self.Flare.NativeWait);
        }

        /// <summary>The grapple flare's wait as the original keeps it: the updates its picture still shows less one.</summary>
        internal WrappingCountdown FlareAnimationTimer
        {
            get => self.Flare.NativeWait;
            set => self.Flare.ShowNative(self.FlareFrame, value);
        }
=======
>>>>>>> theirs
    }

    /// <summary>The number of the main flare's picture <paramref name="playback"/> shows, its place standing just past the beat shown.</summary>
    private static int NativePicture(SuperMetroid.Core.Animation.AnimationPlayback<ChargeFlareSegment> playback)
    {
        SuperMetroid.Core.Animation.AnimationPlace place = playback.Place;
        IReadOnlyList<SuperMetroid.Core.Animation.AnimationPhase<ChargeFlareSegment>> phases = playback.Animation.Phases;
        SuperMetroid.Core.Animation.AnimationPhase<ChargeFlareSegment> playing = phases[place.Phase];
        int playsBefore = playing.IsRepeated ? playing.Plays - place.PlaysLeft : 0;
        return phases.Take(place.Phase).Sum(phase => phase.BeatCount * phase.Plays) + playsBefore * playing.BeatCount + place.Beat - 1;
    }

    /// <summary>Shows the main flare's picture numbered <paramref name="number"/> for <paramref name="updates"/> updates.</summary>
    private static void ShowNative(SuperMetroid.Core.Animation.ShowingAnimationPlayer<ChargeFlareSegment, ChargeFlarePicture, SuperMetroid.Core.Animation.NoAnimationCue> flare,
        int number, int updates)
    {
        flare.ShowFrom(ChargeFlareAnimationDefinitions.Animations[ChargeFlareComponent.MainFlare],
            ChargeFlareAnimationDefinitions.PlaceOfPicture(ChargeFlareComponent.MainFlare, number));
        flare.HoldShownFrameFor(updates);
    }
}
