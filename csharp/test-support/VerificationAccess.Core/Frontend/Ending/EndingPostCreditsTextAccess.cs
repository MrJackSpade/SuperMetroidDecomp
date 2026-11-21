namespace SuperMetroid.Core.Frontend.Ending;

/// <summary>Verification access to <see cref="EndingPostCreditsText"/> members production does not use.</summary>
internal static class EndingPostCreditsTextAccess
{
    /// <summary>Exposes <see cref="EndingPostCreditsText"/> state that only verification reads.</summary>
    extension(EndingPostCreditsText self)
    {
        /// <summary>The typed texts the screen shows, in the order they started: the item percentage, then the final message.</summary>
        internal IReadOnlyList<EndingTypewriter> Typed =>
        [
            .. new[] { self.ItemPercentage, self.FinalMessage }.OfType<EndingTypewriter>(),
        ];
    }
}
