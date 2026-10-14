using SuperMetroid.Core.Game;

/// <summary>Verification access to <see cref="BrinstarBlueSporePaletteFxProgramDefinition"/> members production does not use.</summary>
internal static class BrinstarBlueSporePaletteFxProgramDefinitionAccess
{
    /// <summary>Resolves the standard or Spore Spawn blue-spore palette-FX definition pointer.</summary>
    extension(BrinstarBlueSporePaletteFxProgramDefinition self)
    {
        /// <summary>Native palette-FX definitions $8D:F775 (standard) and $8D:F779 (Spore Spawn).</summary>
        internal ushort DefinitionPointer => (ushort)(0xf775 + 4 * (int)self.Owner);
    }
}
