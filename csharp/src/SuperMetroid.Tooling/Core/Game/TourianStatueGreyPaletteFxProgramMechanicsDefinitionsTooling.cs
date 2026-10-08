namespace SuperMetroid.Core.Game;

/// <summary>Development-tool members of <see cref="TourianStatueGreyPaletteFxProgramMechanicsDefinitions"/>; never linked by player hosts.</summary>
internal static class TourianStatueGreyPaletteFxProgramMechanicsDefinitionsTooling
{
    /// <summary>The Draygon, Kraid, Ridley, and Phantoon entries in cartridge order.</summary>
    [AccessedByReflection]
    public static IReadOnlyList<TourianStatueGreyPaletteFxProgramDefinition> All =>
        TourianStatueGreyPaletteFxProgramMechanicsDefinitions.Definitions;
}
