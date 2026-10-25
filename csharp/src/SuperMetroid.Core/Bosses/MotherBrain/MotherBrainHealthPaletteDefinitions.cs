namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>The health bands that select Mother Brain's final-phase damage colours. Each comparison is "at least".</summary>
public static class MotherBrainHealthPaletteDefinitions
{
    /// <summary>The lowest health still shown undamaged.</summary>
    public const int FirstThreshold = 9000;

    /// <summary>The lowest health shown in the first damaged band.</summary>
    public const int SecondThreshold = 5400;

    /// <summary>The lowest health shown in the second damaged band; below it the most damaged colours show.</summary>
    public const int FinalThreshold = 1800;
}
