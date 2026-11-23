using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Compatibility;

/// <summary>Reading a plain colour in the game's colour steps (#1544, #2563), as the colours that glow and fade a step an update do.</summary>
/// <remarks>
/// Evidence: the original's glow and fade routines read each colour channel as one of its thirty-two steps and step it from
/// there (<see cref="FadeLevelColour"/>); the Kraid replays confirm the roar those steps time.
/// </remarks>
public static partial class RgbColorFadeLevels
{
    /// <summary>A plain colour's steps.</summary>
    extension(RgbColor colour)
    {
        /// <summary>
        /// The colour's steps: exact for every colour the game shows, which each lie on them; any other colour is rejected
        /// (<see cref="FadeLevelColour.Of"/>). Each read looks the steps up again, so code that reads several of a colour's
        /// steps takes them once.
        /// </summary>
        public FadeLevelColour FadeLevels => FadeLevelColour.Of(colour);
    }
}
