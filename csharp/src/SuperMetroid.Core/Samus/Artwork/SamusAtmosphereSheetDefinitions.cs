using SuperMetroid.Core.Samus.Drawing;
using SuperMetroid.Core.Samus.Movement;

namespace SuperMetroid.Core.Samus.Artwork;

/// <summary>
/// The pre-coloured sheet of Samus's atmospheric effects (#1392): the footstep splash and the shared lava-spray and dust
/// sprite in each animation frame, and each frame of the diving splash and bubbles, which Samus's spritemap table draws.
/// </summary>
public static class SamusAtmosphereSheetDefinitions
{
    /// <summary>The Samus spritemap the diving splash's first frame shows; later frames follow it.</summary>
    public const int DivingSplashFirstSpritemap = 399;

    /// <summary>The Samus spritemap the bubbles' first frame shows; later frames follow it.</summary>
    public const int BubblesFirstSpritemap = 390;

    /// <summary>The footstep splash in animation frame <paramref name="frame"/>.</summary>
    public static string FootstepFrame(int frame) => $"Footstep.{frame}";

    /// <summary>The lava spray and dust, which share one sprite list, in animation frame <paramref name="frame"/>.</summary>
    public static string LavaAndDustFrame(int frame) => $"LavaAndDust.{frame}";

    /// <summary>The diving splash in animation frame <paramref name="frame"/>.</summary>
    public static string DivingSplashFrame(int frame) => $"DivingSplash.{frame}";

    /// <summary>The bubbles in animation frame <paramref name="frame"/>.</summary>
    public static string BubblesFrame(int frame) => $"Bubbles.{frame}";

    /// <summary>The animation frames the diving splash shows.</summary>
    internal static int DivingSplashFrameCount => SamusAtmosphericAnimations.FrameCount(SamusAtmosphericEffectType.DivingSplash);

    /// <summary>The animation frames the bubbles show.</summary>
    internal static int BubblesFrameCount => SamusAtmosphericAnimations.FrameCount(SamusAtmosphericEffectType.Bubbles);

    /// <summary>Every frame the sheet holds.</summary>
    public static IEnumerable<string> Frames =>
        Enumerable.Range(0, SamusMovementDefinitions.Environment.DirectAtmosphericFrameCount).Select(FootstepFrame)
            .Concat(Enumerable.Range(0, SamusMovementDefinitions.Environment.DirectAtmosphericFrameCount).Select(LavaAndDustFrame))
            .Concat(Enumerable.Range(0, DivingSplashFrameCount).Select(DivingSplashFrame))
            .Concat(Enumerable.Range(0, BubblesFrameCount).Select(BubblesFrame));
}
