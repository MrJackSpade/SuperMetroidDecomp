namespace SuperMetroid.Core;

/// <summary>Verification access to an identity sequence's length, which gameplay never needs.</summary>
internal static class IdentitySequenceAccess
{
    /// <summary>Verification access to an identity sequence's length.</summary>
    extension<T>(IdentitySequence<T> self) where T : struct, Enum
    {
        /// <summary>The number of identities in the sequence.</summary>
        internal int Count
        {
            get
            {
                int count = 0;
                while (self.TryAt(count, out _))
                    count++;
                return count;
            }
        }
    }
}
