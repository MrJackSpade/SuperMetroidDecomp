namespace SuperMetroid.Core.Frontend.Intro;

/// <summary>Fixed cinematic-object definitions for egg shell fragments and slime drops.</summary>
internal static class IntroEggEffectDefinitions
{
    /// <summary>The shell's pieces in the order the hatching egg spawns them, which is the order they update and draw in.</summary>
    internal static IReadOnlyList<IntroEggShellFragment> ShellFragments { get; } =
    [
        IntroEggShellFragment.UpperLeft, IntroEggShellFragment.UpperRight, IntroEggShellFragment.Left,
        IntroEggShellFragment.Centre, IntroEggShellFragment.Right, IntroEggShellFragment.Lower,
    ];

    /// <summary>Number of slime drops allocated by the confused-baby frame behaviour.</summary>
    public const int SlimeDropCount = 4;

    /// <summary>The animation each piece of the shell shows.</summary>
    internal static IReadOnlyDictionary<IntroEggShellFragment, CinematicSpriteSegment> ShellFragmentAnimations { get; } =
        new Dictionary<IntroEggShellFragment, CinematicSpriteSegment>
        {
            [IntroEggShellFragment.UpperLeft] = CinematicSpriteSegment.MetroidEggShellUpperLeft,
            [IntroEggShellFragment.UpperRight] = CinematicSpriteSegment.MetroidEggShellUpperRight,
            [IntroEggShellFragment.Left] = CinematicSpriteSegment.MetroidEggShellLeft,
            [IntroEggShellFragment.Centre] = CinematicSpriteSegment.MetroidEggShellCentre,
            [IntroEggShellFragment.Right] = CinematicSpriteSegment.MetroidEggShellRight,
            [IntroEggShellFragment.Lower] = CinematicSpriteSegment.MetroidEggShellLower,
        };

    /// <summary>The shared egg-slime initialization, motion, and animation definition.</summary>
    /// <remarks>
    /// Issues #625 and #985: the moving animation and the four impact
    /// animations select five one-entry visual frames,
    /// one for each of f=0..4, matching the authored data.
    /// Each record has one part, at X=508, Y=252, drawn in the egg's colours in front of the
    /// scene, showing that frame's own picture.
    /// These are authored artwork choices; preserve the five bounded picture
    /// identities and do not infer a sixth frame from adjacent data.
    /// </remarks>
    public static CinematicActorDefinition SlimeDrop =>
        new(CinematicSpriteFrameBehavior.MetroidEggSlimeDrops, CinematicSpriteSegment.MetroidEggSlimeDrop);
}
