using static SuperMetroid.Core.Frontend.Intro.IntroMotherBrainDefinitions;

namespace SuperMetroid.Core.Frontend.Intro;

/// <summary>
/// Looks up the intro Mother Brain explosions' placements (<see cref="IntroMotherBrainDefinitions"/>) by parameter.
/// </summary>
internal static class IntroMotherBrainExplosionPlacements
{
    /// <summary>Returns one complete <c>$8B:B98D</c> large-explosion placement record.</summary>
    public static IntroMotherBrainExplosionPlacement BigExplosion(int index) => Ensure.Element(BigExplosions, index);

    /// <summary>Returns one complete <c>$8B:B9D4</c> small-explosion placement record.</summary>
    public static IntroMotherBrainExplosionPlacement SmallExplosion(int index) => Ensure.Element(SmallExplosions, index);
}
