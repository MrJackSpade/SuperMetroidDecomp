namespace SuperMetroid.Core.Areas.Ceres;

/// <summary>The door-transition palette fade an update of the Ceres haze owner sees.</summary>
public enum CeresHazeRoomFade
{
    /// <summary>No door fade is running.</summary>
    None,

    /// <summary>The destination room's palette is fading in.</summary>
    FadeIn,

    /// <summary>The source room's palette is fading out.</summary>
    FadeOut,
}
