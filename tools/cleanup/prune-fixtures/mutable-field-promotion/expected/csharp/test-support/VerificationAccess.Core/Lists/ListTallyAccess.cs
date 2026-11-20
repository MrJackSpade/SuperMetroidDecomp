namespace SuperMetroid.Core.Lists;

/// <summary>Verification access to <see cref="ListTally"/> members production does not use.</summary>
internal static class ListTallyAccess
{
    /// <summary>Exposes <see cref="ListTally"/> state that only verification reads.</summary>
    extension(ListTally self)
    {
        /// <summary>Twice the entries counted, which only checks read.</summary>
        internal int Doubled => self.counted * 2;
    }
}
