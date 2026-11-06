namespace SuperMetroid.Core.Areas.Ceres;

/// <summary>
/// How far Ceres Ridley's scene shakes the escape door it draws (<see cref="CeresDoorQuake"/>): one horizontal offset per
/// phase of the earthquake's frame count, as authored.
/// </summary>
internal static class CeresDoorQuakeDefinitions
{
    /// <summary>The door's horizontal offset in pixels, rightward positive, for each earthquake phase in order.</summary>
    internal static readonly int[] PhaseOffsets = [0, -4, -1, 3];
}
