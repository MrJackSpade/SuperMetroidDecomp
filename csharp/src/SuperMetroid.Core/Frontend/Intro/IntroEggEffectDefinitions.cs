namespace SuperMetroid.Core.Frontend.Intro;

/// <summary>Fixed cinematic-actor definitions for egg shell fragments and slime drops.</summary>
internal static class IntroEggEffectDefinitions
{
    /// <summary>The shell's pieces in the order the hatching egg spawns them, which is the order they update and draw in.</summary>
    internal static IReadOnlyList<IntroEggShellFragment> ShellFragments { get; } =
    [
        IntroEggShellFragment.UpperLeft, IntroEggShellFragment.UpperRight, IntroEggShellFragment.Left,
        IntroEggShellFragment.Centre, IntroEggShellFragment.Right, IntroEggShellFragment.Lower,
    ];

    /// <summary>The screen height a falling slime drop lands at, where its puddle plays in place.</summary>
    public const int SlimeDropLandingY = 168;

    /// <summary>The slime drops' actor: they fly as slime drops do and show the flying drop's picture.</summary>
    /// <remarks>
    /// A landed drop shows the four pictures of its puddle instead, each in the egg's colours in front of the scene.
    /// </remarks>
    public static CinematicActorDefinition<IntroEggEffectAnimation> SlimeDrop =>
        new(IntroEggEffectAnimation.SlimeDrop);
}
