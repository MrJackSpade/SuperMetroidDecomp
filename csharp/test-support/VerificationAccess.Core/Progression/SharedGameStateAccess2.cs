namespace SuperMetroid.Core.Progression;

/// <summary>Verification access to <see cref="SharedGameState"/> members production does not use.</summary>
internal static class SharedGameStateAccess2
{
    /// <summary>Exposes <see cref="SharedGameState"/> state that only verification reads.</summary>
    extension(SharedGameState self)
    {
        /// <summary>
        /// Overwrites the shared random state, as a state import does. A species reseeds through
        /// <see cref="SharedRandom.Reseed"/> instead.
        /// </summary>
        public void SetRandomNumber(int value) => self.Random.Seed(value);
    }
}
