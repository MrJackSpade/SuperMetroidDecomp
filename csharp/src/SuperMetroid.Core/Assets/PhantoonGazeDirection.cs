namespace SuperMetroid.Core.Assets;

/// <summary>Eight mutually exclusive compass gaze directions selected at $A7:CCA7..CCD6.</summary>
internal enum PhantoonGazeDirection
{
    /// <summary>Pupil above the center of the eye.</summary>
    Up,
    /// <summary>Pupil toward the upper right.</summary>
    UpRight,
    /// <summary>Pupil right of center.</summary>
    Right,
    /// <summary>Pupil toward the lower right.</summary>
    DownRight,
    /// <summary>Pupil below the center of the eye.</summary>
    Down,
    /// <summary>Pupil toward the lower left.</summary>
    DownLeft,
    /// <summary>Pupil left of center.</summary>
    Left,
    /// <summary>Pupil toward the upper left.</summary>
    UpLeft,
}
