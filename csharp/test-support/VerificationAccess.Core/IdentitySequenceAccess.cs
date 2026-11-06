using System.Reflection;

namespace SuperMetroid.Core;

/// <summary>Verification access to an identity sequence's length, which gameplay never needs.</summary>
internal static class IdentitySequenceAccess
{
    /// <summary>Verification access to an identity sequence's length.</summary>
    extension<T>(IdentitySequence<T> self) where T : struct, Enum
    {
        /// <summary>The number of identities in the sequence, read from its private member array.</summary>
        /// <exception cref="MissingFieldException">The sequence no longer holds its members as an array of identities.</exception>
        internal int Count =>
            (typeof(IdentitySequence<T>).GetField("_members", BindingFlags.NonPublic | BindingFlags.Instance)?.GetValue(self) as T[]
                ?? throw new MissingFieldException(typeof(IdentitySequence<T>).FullName, "_members")).Length;
    }
}
