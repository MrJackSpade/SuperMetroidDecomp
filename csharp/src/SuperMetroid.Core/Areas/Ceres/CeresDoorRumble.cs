using static SuperMetroid.Core.Areas.Ceres.CeresDoorRumbleGeometryDefinitions;

namespace SuperMetroid.Core.Areas.Ceres;

/// <summary>
/// Places the four impact effects of a Ceres door's rumble (<see cref="CeresDoorRumbleGeometryDefinitions"/>) around the door.
/// </summary>
internal static class CeresDoorRumble
{
    /// <summary>Converts emission order into the paired horizontal and native vertical destruction offsets.</summary>
    internal static Offset16<RoomPixels> Offset(int index)
    {
        if ((uint)index >= Count)
            throw new ArgumentOutOfRangeException(nameof(index));
        int pair = index / 2;
        int side = index % 2;
        return new Offset16<RoomPixels>((short)(ColumnSpacing * (2 * side + pair - 2)), VerticalPlacements[index]);
    }
}
