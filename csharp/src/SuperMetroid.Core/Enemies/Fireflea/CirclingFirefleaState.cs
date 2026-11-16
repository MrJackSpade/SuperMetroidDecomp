namespace SuperMetroid.Core.Enemies.Fireflea;

/// <summary>The flight of a Fireflea that circles where it started.</summary>
public sealed class CirclingFirefleaState : FirefleaFlightState
{
    /// <summary>How far, and which way, the Fireflea turns around its circle each update, from its spawn settings.</summary>
    public AngularSpeed AngularSpeed { get; internal set; }

    /// <summary>The circle's radius in pixels, 8 through 64, from its spawn settings.</summary>
    public int Radius { get; internal set; }

    /// <summary>
    /// The circle angle, wrapping around the turn. Main AI uses its whole step as the circle's trigonometry angle.
    /// </summary>
    public RotationAngle Angle { get; internal set; }

    /// <summary>Initial whole-pixel room position used as the fixed circle center.</summary>
    public Position16<RoomPixels> Center { get; internal set; }
}
