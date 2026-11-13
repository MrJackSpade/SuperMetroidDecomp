namespace SuperMetroid.Desktop.Lists;

/// <summary>Verification access to <see cref="ListMeter"/> members production does not use.</summary>
internal static class ListMeterAccess
{
    /// <summary>Exposes <see cref="ListMeter"/> state that only verification reads.</summary>
    extension(ListMeter self)
    {
        /// <summary>Twice the level, which only checks read.</summary>
        internal int Doubled() => self.Level * 2;
        /// <summary>Three times the level, which only checks read.</summary>
        internal int Tripled { get => self.Level * 3; }
    }
}
