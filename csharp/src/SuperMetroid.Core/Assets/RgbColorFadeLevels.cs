namespace SuperMetroid.Core.Assets;

/// <summary>Reading a plain colour in the levels the game authored it in (#1544).</summary>
public static class RgbColorFadeLevels
{
    /// <summary>A plain colour's levels.</summary>
    extension(RgbColor colour)
    {
        /// <summary>
        /// The colour's levels: exact for every colour the game authored, which each lie on the scale; any other colour
        /// is rejected (<see cref="FadeLevelColour.Of"/>).
        /// </summary>
        public FadeLevelColour FadeLevels => FadeLevelColour.Of(colour);
    }
}
