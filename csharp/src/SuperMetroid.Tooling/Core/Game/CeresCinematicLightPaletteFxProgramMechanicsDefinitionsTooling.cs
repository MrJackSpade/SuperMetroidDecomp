namespace SuperMetroid.Core.Game;

/// <summary>Development-tool members of <see cref="CeresCinematicLightPaletteFxProgramMechanicsDefinitions"/>; never linked by player hosts.</summary>
internal static class CeresCinematicLightPaletteFxProgramMechanicsDefinitionsTooling
{
    /// <summary>All native definitions whose mechanics are owned by this catalog.</summary>
    [AccessedByReflection]
    public static IReadOnlyList<CeresCinematicLightPaletteFxProgramDefinition> All { get; } = new ProgramEntries();
    /// <summary>Represents program entries.</summary>
    internal sealed class ProgramEntries : IReadOnlyList<CeresCinematicLightPaletteFxProgramDefinition>
    {
        /// <summary>Gets count.</summary>
        public int Count => 3;
        /// <summary>Gets an indexed value.</summary>
        public CeresCinematicLightPaletteFxProgramDefinition this[int index] =>
            (CeresCinematicLightPaletteFxProgramOwner)index switch
            {
                CeresCinematicLightPaletteFxProgramOwner.GunshipEngine => new(),
                CeresCinematicLightPaletteFxProgramOwner.SpriteNavigationLights => new(),
                CeresCinematicLightPaletteFxProgramOwner.BackgroundNavigationLights => new(),
                _ => throw new ArgumentOutOfRangeException(nameof(index)),
            };
        /// <summary>Returns an enumerator over this sequence.</summary>
        public IEnumerator<CeresCinematicLightPaletteFxProgramDefinition> GetEnumerator()
        {
            for (int index = 0; index < Count; index++)
                yield return this[index];
        }
        /// <summary>Returns an enumerator over this sequence.</summary>
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
