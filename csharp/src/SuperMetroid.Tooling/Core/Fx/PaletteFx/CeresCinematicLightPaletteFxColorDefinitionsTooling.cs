namespace SuperMetroid.Core.Fx.PaletteFx;

/// <summary>Development-tool members of <see cref="CeresCinematicLightPaletteFxColorDefinitions"/>; never linked by player hosts.</summary>
internal static class CeresCinematicLightPaletteFxColorDefinitionsTooling
{
    /// <summary>All native definitions whose mechanics are owned by this catalog.</summary>
    [AccessedByReflection]
    public static IReadOnlyList<CeresCinematicLightPaletteFxColorDefinition> All { get; } = new ProgramEntries();
    /// <summary>Enumerates the three Ceres light owners without retaining duplicate program tables.</summary>
    internal sealed class ProgramEntries : IReadOnlyList<CeresCinematicLightPaletteFxColorDefinition>
    {
        /// <summary>Covers the gunship engine and both sprite and background navigation lights.</summary>
        public int Count => 3;
        /// <summary>Converts an owner slot to its compiled light-program definition, rejecting unknown slots.</summary>
        public CeresCinematicLightPaletteFxColorDefinition this[int index] =>
            (CeresCinematicLightPaletteFxProgramOwner)index switch
            {
                CeresCinematicLightPaletteFxProgramOwner.GunshipEngine => new(),
                CeresCinematicLightPaletteFxProgramOwner.SpriteNavigationLights => new(),
                CeresCinematicLightPaletteFxProgramOwner.BackgroundNavigationLights => new(),
                _ => throw new ArgumentOutOfRangeException(nameof(index)),
            };
        /// <summary>Visits the owner slots in the same order as the catalog indexer.</summary>
        public IEnumerator<CeresCinematicLightPaletteFxColorDefinition> GetEnumerator()
        {
            for (int index = 0; index < Count; index++)
                yield return this[index];
        }
        /// <summary>Shares the typed owner-slot traversal with non-generic consumers.</summary>
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
