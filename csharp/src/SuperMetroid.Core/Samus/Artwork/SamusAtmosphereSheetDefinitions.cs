namespace SuperMetroid.Core.Samus.Artwork;

/// <summary>
/// The pre-coloured sheet of Samus's atmospheric effects (#1392): the footstep splash and the shared lava-spray and dust
/// sprite in each animation frame, and each frame of the diving splash and bubbles, which Samus's effect drawing shows.
/// </summary>
public static class SamusAtmosphereSheetDefinitions
{
    /// <summary>The footstep splash in animation frame <paramref name="frame"/>.</summary>
    public static string FootstepFrame(int frame) => $"Footstep.{frame}";

    /// <summary>The lava spray and dust, which share one sprite list, in animation frame <paramref name="frame"/>.</summary>
    public static string LavaAndDustFrame(int frame) => $"LavaAndDust.{frame}";

    /// <summary>The diving splash in animation frame <paramref name="frame"/>.</summary>
    public static string DivingSplashFrame(int frame) => $"DivingSplash.{frame}";

    /// <summary>The bubbles in animation frame <paramref name="frame"/>.</summary>
    public static string BubblesFrame(int frame) => $"Bubbles.{frame}";
}
