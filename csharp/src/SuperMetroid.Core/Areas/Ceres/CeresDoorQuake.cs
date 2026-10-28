using static SuperMetroid.Core.Areas.Ceres.CeresDoorQuakeDefinitions;

namespace SuperMetroid.Core.Areas.Ceres;

/// <summary>
/// Shakes a Ceres door during the escape (<see cref="CeresDoorQuakeDefinitions"/>): the signed horizontal offset each earthquake
/// phase selects.
/// </summary>
internal static class CeresDoorQuake
{
    /// <summary>Returns the signed offset selected by the earthquake timer's low two bits.</summary>
    internal static sbyte XOffset(int earthquakeTimer)
    {
        int phase = earthquakeTimer & 3;
        int word = phase < sizeof(ushort) ? 0 : Impulse;
        return unchecked((sbyte)(word >> (8 * (phase & 1))));
    }
}
