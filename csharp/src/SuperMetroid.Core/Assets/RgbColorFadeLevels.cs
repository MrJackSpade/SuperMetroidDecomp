namespace SuperMetroid.Core.Assets;

/// <summary>Reading a plain colour in the levels the game authored it in (#1544).</summary>
public static class RgbColorFadeLevels
{
    /// <summary>A plain colour's levels.</summary>
    extension(RgbColor colour)
    {
        /// <summary>
        /// The colour's levels: exact for every colour the game shows, which each lie on the fade scale; any other colour
        /// is rejected (<see cref="FadeLevelColour.Of"/>). Each read looks the levels up again, so code that reads several
        /// of a colour's levels takes them once.
        /// </summary>
        public FadeLevelColour FadeLevels => FadeLevelColour.Of(colour);
    }
}
