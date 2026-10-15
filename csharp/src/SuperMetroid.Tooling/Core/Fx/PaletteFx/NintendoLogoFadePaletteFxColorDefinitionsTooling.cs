namespace SuperMetroid.Core.Fx.PaletteFx;

/// <summary>Development-tool members of <see cref="NintendoLogoFadePaletteFxColorDefinitions"/>; never linked by player hosts.</summary>
internal static class NintendoLogoFadePaletteFxColorDefinitionsTooling
{
    /// <summary>Shared two-entry list backing the boot-logo and copyright fade catalog.</summary>
    internal static readonly NintendoLogoFadePaletteFxColorDefinitionsTooling.DefinitionList ReadOnlyDefinitions = new();
    /// <summary>The boot-logo and copyright entries in native definition order, calculated from their semantic entry contracts.</summary>
    [AccessedByReflection]
    public static IReadOnlyList<NintendoLogoFadePaletteFxColorDefinition> All => ReadOnlyDefinitions;
    /// <summary>$8D:E198/E19C select two distinct entry operations: boot sets its slot then falls through; copyright sets its slot then explicitly branches to the shared fade body.</summary>
    internal sealed class DefinitionList : IReadOnlyList<NintendoLogoFadePaletteFxColorDefinition>
    {
        /// <summary>The boot-logo and copyright fade entries.</summary>
        public int Count => 2;
        /// <summary>Constructs the requested semantic fade entry without storing ROM data.</summary>
        public NintendoLogoFadePaletteFxColorDefinition this[int index] => index switch
        {
            0 => new(),
            1 => new(),
            _ => throw new ArgumentOutOfRangeException(nameof(index)),
        };
        /// <summary>Visits boot-logo before copyright, matching native definition order.</summary>
        public IEnumerator<NintendoLogoFadePaletteFxColorDefinition> GetEnumerator()
        {
            for (int index = 0; index < Count; index++) yield return this[index];
        }
        /// <summary>Provides the same two-entry sequence to non-generic collection consumers.</summary>
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
