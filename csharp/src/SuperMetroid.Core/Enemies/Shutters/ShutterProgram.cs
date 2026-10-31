namespace SuperMetroid.Core.Enemies.Shutters;

/// <summary>
/// The growing, vertical and horizontal shutters' poses and Kamer's platform loop as typed programs (#641), one per animation a spawn, branch or resumed sleep enters, in authored order.
/// Their steps are <see cref="ShutterAnimations"/>.
/// </summary>
internal enum ShutterProgram
{
    /// <summary><c>AnimationScript_ShutterGrowing_10px</c>.</summary>
    GrowingTenPixels,
    /// <summary><c>AnimationScript_ShutterGrowing_20px</c>.</summary>
    GrowingTwentyPixels,
    /// <summary><c>AnimationScript_ShutterGrowing_30px</c>.</summary>
    GrowingThirtyPixels,
    /// <summary><c>AnimationScript_ShutterGrowing_40px</c>.</summary>
    FortyPixels,
    /// <summary>The horizontal animation.</summary>
    Horizontal,
    /// <summary><c>AnimationScript_KamerPlatform</c>.</summary>
    KamerPlatform,
}
