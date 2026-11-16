namespace SuperMetroid.Diagnostics.SaveStates;

/// <summary>Verification access to <see cref="DefinitionObjects"/> members production does not use.</summary>
internal static class DefinitionObjectsAccess
{
    /// <summary>Exposes <see cref="DefinitionObjects"/> state that only verification reads.</summary>
    extension(DefinitionObjects self)
    {
        /// <summary>Every catalog of the game's static members, in their prefixes' ordinal order, none indexed until needed.</summary>
        internal IReadOnlyList<DefinitionCatalog> Catalogs
        {
            get
            {
                lock (self.gate)
                    return self.Declared.Catalogs;
            }
        }
    }
}
