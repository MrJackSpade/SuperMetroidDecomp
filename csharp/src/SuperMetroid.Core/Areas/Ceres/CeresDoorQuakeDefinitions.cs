namespace SuperMetroid.Core.Areas.Ceres;

/// <summary>
/// How far Ceres Ridley's scene shakes the escape door it draws (<see cref="CeresDoorQuake"/>): one horizontal offset per
/// phase of the earthquake's frame count, as authored, and how the door's own earthquake chooses its strength.
/// </summary>
internal static class CeresDoorQuakeDefinitions
{
    /// <summary>
    /// The random selection, read from the current sample without drawing, that chooses the door's earthquake: 4096
    /// rolls, of which the 128 lowest start the stronger quake.
    /// </summary>
    internal static readonly RandomSampleField StrengthRoll = new(0, 12);

    /// <summary>The door's horizontal offset in pixels, rightward positive, for each earthquake phase in order.</summary>
    internal static readonly int[] PhaseOffsets = [0, -4, -1, 3];
}
