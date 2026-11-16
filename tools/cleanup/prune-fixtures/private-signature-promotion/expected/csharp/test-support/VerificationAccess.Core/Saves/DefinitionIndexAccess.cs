namespace SuperMetroid.AssetExtraction.Saves;

/// <summary>Verification access to <see cref="DefinitionIndex"/> members production does not use.</summary>
internal static class DefinitionIndexAccess
{
    /// <summary>Exposes <see cref="DefinitionIndex"/> state that only verification reads.</summary>
    extension(DefinitionIndex self)
    {
        /// <summary>The definitions' names, which only checks read.</summary>
        internal IReadOnlyList<string> Names => self.Declared.Names;
    }
}
