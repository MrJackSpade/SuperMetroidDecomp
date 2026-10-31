namespace SuperMetroid.Core.Frontend.Menus;

/// <summary>Shared file-select, options and game-over selection-missile animation.</summary>
public static class MenuMissileAnimationDefinitions
{
    /// <summary>The animation masks the incremented animation frame to its four-frame cycle.</summary>
    public const int FrameCount = 4;

    /// <summary>
    /// The animation has four identical eight-call timer reloads. Independently
    /// verified for #1165; this constant already replaces that separate logical table.
    /// The consumer decrements before advancing and may use editable presentation timing.
    /// </summary>
    public const int FrameDuration = 8;
}
