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
