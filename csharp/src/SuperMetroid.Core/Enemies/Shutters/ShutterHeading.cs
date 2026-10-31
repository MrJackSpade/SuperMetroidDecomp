namespace SuperMetroid.Core.Enemies.Shutters;

/// <summary>Which end of its track a moving shutter heads for (#1472).</summary>
public enum ShutterHeading
{
    /// <summary>The left end of a horizontal shutter's track, or the top of a vertical one's.</summary>
    LeftOrUp,

    /// <summary>The right end of a horizontal shutter's track, or the bottom of a vertical one's.</summary>
    RightOrDown,
}
