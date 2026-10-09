namespace SuperMetroid.Core.Game;

/// <summary>Development-tool members of <see cref="CeresCinematicLightPaletteFxProgramMechanicsDefinitions"/>; never linked by player hosts.</summary>
internal static class CeresCinematicLightPaletteFxProgramMechanicsDefinitionsTooling
{
    /// <summary>All native definitions whose mechanics are owned by this catalog.</summary>
    [AccessedByReflection]
    public static IReadOnlyList<CeresCinematicLightPaletteFxProgramDefinition> All { get; } = new ProgramEntries();

    /// <summary>Provides the three Ceres cinematic-light owners as a fixed, allocation-free indexed list.</summary>
    internal sealed class ProgramEntries : IReadOnlyList<CeresCinematicLightPaletteFxProgramDefinition>
    {
        /// <summary>Number of compiled cinematic-light program owners.</summary>
        public int Count => 3;

        /// <summary>Creates the definition metadata for the owner at the requested ordinal.</summary>
        /// <param name="index">Ordinal matching the owner order: gunship engine, sprite navigation lights, then background navigation lights.</param>
        /// <returns>Definition metadata for the selected owner.</returns>
        /// <exception cref="ArgumentOutOfRangeException">The ordinal is not one of the three owners.</exception>
        public CeresCinematicLightPaletteFxProgramDefinition this[int index] =>
            (CeresCinematicLightPaletteFxProgramOwner)index switch
            {
                CeresCinematicLightPaletteFxProgramOwner.GunshipEngine => new(),
                CeresCinematicLightPaletteFxProgramOwner.SpriteNavigationLights => new(),
                CeresCinematicLightPaletteFxProgramOwner.BackgroundNavigationLights => new(),
                _ => throw new ArgumentOutOfRangeException(nameof(index)),
            };

        /// <summary>Enumerates definitions in the same stable order as the indexed owner list.</summary>
        /// <returns>An enumerator over the three cinematic-light program definitions.</returns>
        public IEnumerator<CeresCinematicLightPaletteFxProgramDefinition> GetEnumerator()
        {
            for (int index = 0; index < Count; index++)
                yield return this[index];
        }
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
