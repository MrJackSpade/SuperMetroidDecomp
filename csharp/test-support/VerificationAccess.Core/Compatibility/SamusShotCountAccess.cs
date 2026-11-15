namespace SuperMetroid.Core.Compatibility;

/// <summary>Verification access to <see cref="SamusShotCount"/>: placing a count a check or an imported snapshot starts from.</summary>
internal static class SamusShotCountAccess
{
    /// <summary>Restores a count a check or an imported snapshot starts from.</summary>
    extension(SamusShotCount self)
    {
        /// <summary>Clears the count, then counts <paramref name="count"/> shots, as firing would.</summary>
        internal void Restore(int count)
        {
            self.Clear();
            for (int shot = 0; shot < Ensure.AtLeastZero(count); shot++)
                self.CountShot();
        }
    }
}
