using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Areas.Ceres.Artwork;

/// <summary>
/// The shades Ceres's doors and the elevator room are pre-coloured in (#1512): the doors' normal and escaping colours,
/// which the falling tiles of the escape also show, and each step of the elevator room's six-colour glow. The import
/// colours them from the installed Ceres door colours.
/// </summary>
internal static class CeresDoorShades
{
    /// <summary>The doors' normal colours.</summary>
    internal static PictureShade Normal { get; } = new("ceres-door-normal", 0);

    /// <summary>The doors' colours once Ridley has escaped.</summary>
    internal static PictureShade Escaping { get; } = new("ceres-door-escaping", 0);

    /// <summary>A draw in the doors' normal colours.</summary>
    internal static PictureShading InNormalColours { get; } = PictureShading.Shaded(Normal);

    /// <summary>A draw in the doors' escaping colours.</summary>
    internal static PictureShading InEscapingColours { get; } = PictureShading.Shaded(Escaping);

    /// <summary>The effect name of the elevator room's glow, which the rotating-room door cycles.</summary>
    internal const string ElevatorRoomGlowEffect = "ceres-elevator-room-glow";

    /// <summary>The glow's steps.</summary>
    internal const int ElevatorRoomGlowSteps = 8;

    /// <summary>The shade of glow step <paramref name="step"/>.</summary>
    internal static PictureShade ElevatorRoomGlow(int step) =>
        new(ElevatorRoomGlowEffect, Ensure.BetweenInclusive(0, ElevatorRoomGlowSteps - 1, step));
}
