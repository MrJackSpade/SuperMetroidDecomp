namespace SuperMetroid.Core.Game;

/// <summary>Development-tool members of <see cref="CeresCinematicLightPaletteFxProgramMechanicsDefinitions"/>; never linked by player hosts.</summary>
internal static class CeresCinematicLightPaletteFxProgramMechanicsDefinitionsTooling
{
    /// <summary>All native definitions whose mechanics are owned by this catalog.</summary>
    [AccessedByReflection]
    public static IReadOnlyList<CeresCinematicLightPaletteFxProgramDefinition> All { get; } = new ProgramEntries();
    internal sealed class ProgramEntries : IReadOnlyList<CeresCinematicLightPaletteFxProgramDefinition>
    {
        public int Count => 3;
        public CeresCinematicLightPaletteFxProgramDefinition this[int index] =>
            (CeresCinematicLightPaletteFxProgramOwner)index switch
            {
                CeresCinematicLightPaletteFxProgramOwner.GunshipEngine => new(),
                CeresCinematicLightPaletteFxProgramOwner.SpriteNavigationLights => new(),
                CeresCinematicLightPaletteFxProgramOwner.BackgroundNavigationLights => new(),
                _ => throw new ArgumentOutOfRangeException(nameof(index)),
            };
        public IEnumerator<CeresCinematicLightPaletteFxProgramDefinition> GetEnumerator()
        {
            for (int index = 0; index < Count; index++)
                yield return this[index];
        }
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
