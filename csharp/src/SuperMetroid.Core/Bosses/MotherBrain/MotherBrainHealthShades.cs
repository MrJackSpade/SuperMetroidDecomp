
namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>Mother Brain's strict health comparisons, which select the health band her colours show.</summary>
public static class MotherBrainHealthShades
{
    /// <summary>
    /// The damage band her health-based colours select for unsigned head health
    /// <paramref name="health"/>: zero at 9000 or more, one from 5400, two from 1800, and three below.
    /// </summary>
    /// <remarks>The caller owns the recovered-corpse-state gate; this selects the band on every call without checking encounter phase.</remarks>
    public static int Band(int health) =>
        health >= MotherBrainHealthShadeDefinitions.FirstThreshold ? 0 :
        health >= MotherBrainHealthShadeDefinitions.SecondThreshold ? 1 :
        health >= MotherBrainHealthShadeDefinitions.FinalThreshold ? 2 : 3;
}
