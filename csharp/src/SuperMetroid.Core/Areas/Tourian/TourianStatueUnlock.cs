using static SuperMetroid.Core.Areas.Tourian.TourianStatueUnlockDefinitions;

namespace SuperMetroid.Core.Areas.Tourian;

/// <summary>
/// Places a Tourian statue's unlock effects (<see cref="TourianStatueUnlockDefinitions"/>): the eye glow and released soul
/// share the position the native doubled boss parameter selects.
/// </summary>
internal static class TourianStatueUnlock
{
    /// <summary>Returns the room position shared by the eye glow and released soul selected by the native doubled boss parameter.</summary>
    internal static Position16<RoomPixels> EyePosition(int parameter)
    {
        if (parameter > 6 || (parameter & 1) != 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(parameter), parameter, "Tourian statue parameter must be zero, two, four, or six.");
        }

        return EyePositions[parameter >> 1];
    }
}
