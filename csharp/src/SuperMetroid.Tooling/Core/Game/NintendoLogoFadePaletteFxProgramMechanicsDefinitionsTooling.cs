namespace SuperMetroid.Core.Game;

/// <summary>Development-tool members of <see cref="NintendoLogoFadePaletteFxProgramMechanicsDefinitions"/>; never linked by player hosts.</summary>
internal static class NintendoLogoFadePaletteFxProgramMechanicsDefinitionsTooling
{
    internal static readonly NintendoLogoFadePaletteFxProgramMechanicsDefinitionsTooling.DefinitionList ReadOnlyDefinitions = new();
    /// <summary>The boot-logo and copyright entries in native definition order, calculated from their semantic entry contracts.</summary>
    [AccessedByReflection]
    public static IReadOnlyList<NintendoLogoFadePaletteFxProgramDefinition> All => ReadOnlyDefinitions;
    /// <summary>$8D:E198/E19C select two distinct entry operations: boot sets its slot then falls through; copyright sets its slot then explicitly branches to the shared fade body.</summary>
    internal sealed class DefinitionList : IReadOnlyList<NintendoLogoFadePaletteFxProgramDefinition>
    {
        public int Count => 2;
        public NintendoLogoFadePaletteFxProgramDefinition this[int index] => index switch
        {
            0 => new(),
            1 => new(),
            _ => throw new ArgumentOutOfRangeException(nameof(index)),
        };
        public IEnumerator<NintendoLogoFadePaletteFxProgramDefinition> GetEnumerator()
        {
            for (int index = 0; index < Count; index++) yield return this[index];
        }
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
