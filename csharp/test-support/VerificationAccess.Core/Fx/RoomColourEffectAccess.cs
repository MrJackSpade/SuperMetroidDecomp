using SuperMetroid.Core.Animation;

namespace SuperMetroid.Core.Fx;

/// <summary>Verification access to <see cref="RoomColourEffect"/> members production does not use.</summary>
internal static class RoomColourEffectAccess
{
    /// <summary>Exposes <see cref="RoomColourEffect"/> state that only verification reads.</summary>
    extension(RoomColourEffect self)
    {
        /// <summary>Where the effect stands in its animation: the phase, and the phase's next beat.</summary>
        internal AnimationPlace Place => self.play.Playback.Place;
    }
}
