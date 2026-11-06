using static SuperMetroid.Core.Areas.Ceres.CeresDoorQuakeDefinitions;

namespace SuperMetroid.Core.Areas.Ceres;

/// <summary>
/// Shakes a Ceres door during the escape (<see cref="CeresDoorQuakeDefinitions"/>): the horizontal offset each earthquake
/// phase draws the door at. The offset moves only the drawn door, never its position.
/// </summary>
internal static class CeresDoorQuake
{
    /// <summary>
    /// Returns the horizontal offset of the earthquake phase <paramref name="earthquakeFrames"/> falls in; the phases
    /// repeat every four frames.
    /// </summary>
    /// <param name="earthquakeFrames">Frames the earthquake still runs, zero or more.</param>
    internal static int XOffset(int earthquakeFrames) => PhaseOffsets[earthquakeFrames % PhaseOffsets.Length];
}
