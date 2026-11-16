namespace SuperMetroid.AssetExtraction.Cartridge;

/// <summary>Verification access to <see cref="EndingText"/> members production does not use.</summary>
internal static class EndingTextAccess2
{
    /// <summary>Exposes <see cref="EndingText"/> state that only verification reads.</summary>
    extension(EndingText self)
    {
        /// <summary>The text as the typewriter types it, which only checks read.</summary>
        internal string Typewriter() => self.Text.ToLowerInvariant();
    }
}
