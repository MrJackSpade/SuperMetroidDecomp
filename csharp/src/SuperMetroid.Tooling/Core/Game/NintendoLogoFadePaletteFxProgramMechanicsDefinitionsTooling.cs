namespace SuperMetroid.Core.Game;

/// <summary>Development-tool members of <see cref="NintendoLogoFadePaletteFxProgramMechanicsDefinitions"/>; never linked by player hosts.</summary>
internal static class NintendoLogoFadePaletteFxProgramMechanicsDefinitionsTooling
{
    /// <summary>Read-only view of the two compiled logo-fade entries in native definition order.</summary>
    internal static readonly NintendoLogoFadePaletteFxProgramMechanicsDefinitionsTooling.DefinitionList ReadOnlyDefinitions = new();
    /// <summary>The boot-logo and copyright entries in native definition order, calculated from their semantic entry contracts.</summary>
    [AccessedByReflection]
    public static IReadOnlyList<NintendoLogoFadePaletteFxProgramDefinition> All => ReadOnlyDefinitions;
    /// <summary>$8D:E198/E19C select two distinct entry operations: boot sets its slot then falls through; copyright sets its slot then explicitly branches to the shared fade body.</summary>
    internal sealed class DefinitionList : IReadOnlyList<NintendoLogoFadePaletteFxProgramDefinition>
    {
        /// <summary>Number of logo-fade entries: boot logo followed by copyright.</summary>
        public int Count => 2;

        /// <summary>Gets the boot-logo or copyright entry at its native ordinal.</summary>
        /// <param name="index">Zero for the boot logo or one for the copyright entry.</param>
        /// <returns>A freshly constructed definition for the requested entry.</returns>
        /// <exception cref="ArgumentOutOfRangeException">The index is not zero or one.</exception>
        public NintendoLogoFadePaletteFxProgramDefinition this[int index] => index switch
        {
            0 => new(),
            1 => new(),
            _ => throw new ArgumentOutOfRangeException(nameof(index)),
        };

        /// <summary>Enumerates both definitions in boot-logo then copyright order.</summary>
        /// <returns>An enumerator that constructs each definition as it is requested.</returns>
        public IEnumerator<NintendoLogoFadePaletteFxProgramDefinition> GetEnumerator()
        {
            for (int index = 0; index < Count; index++) yield return this[index];
        }
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
