namespace SuperMetroid.Core.Assets;

/// <summary>Verification access to <see cref="FadeLevelColour"/> members production does not use.</summary>
internal static class FadeLevelColourAccess
{
    /// <summary>Exposes <see cref="FadeLevelColour"/> state that only verification reads.</summary>
    extension(FadeLevelColour self)
    {
        /// <summary>Combines matching channels' levels of these and <paramref name="other"/>; the results must be levels.</summary>
        public FadeLevelColour Zip(FadeLevelColour other, Func<ColorChannel, int, int, int> combine)
        {
            Ensure.NotNull(combine);
            return new(combine(ColorChannel.Red, self.Red, other.Red), combine(ColorChannel.Green, self.Green, other.Green),
                combine(ColorChannel.Blue, self.Blue, other.Blue));
        }
    }
}
