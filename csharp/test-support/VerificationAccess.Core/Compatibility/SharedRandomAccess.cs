namespace SuperMetroid.Core.Compatibility;

/// <summary>Verification access to <see cref="SharedRandom"/> members production does not use.</summary>
internal static class SharedRandomAccess
{
    /// <summary>Exposes <see cref="SharedRandom"/> state that only verification reads.</summary>
    extension(SharedRandom self)
    {
        /// <summary>Overwrites the state with <paramref name="value"/>, as a state import does.</summary>
        /// <param name="value">The new state, 0 through 65535.</param>
        public void Seed(int value) => self.State = new SharedRandomState(value);
    }
}
