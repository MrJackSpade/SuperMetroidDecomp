using SuperMetroid.Core.Samus.Artwork;
using static SuperMetroid.Core.Samus.Colours.SamusShadeDefinitions.FullBodyCycles;

namespace SuperMetroid.Core.Samus.Colours;

/// <summary>
/// The full-body cycle shade the Metroid-attachment flash alternates with Samus's normal suit colours. Every cycle shows
/// only its own suit's colours (#1736); the cycles themselves are named shade sequences (#2387,
/// <see cref="SamusGlowAnimationDefinitions"/>).
/// </summary>
public static partial class SamusFullBodyCycleShades
{
    /// <summary>The cycle shade the special Metroid-attachment flash alternates with normal suit colors, the Speed Booster family's last shade.</summary>
    /// <param name="suit">The suit Samus shows.</param>
    internal static SamusFullBodyCycleShade SpeedBoostShade(SamusSuit suit) =>
        new(SamusFullBodyCycleFamily.SpeedBooster, suit, SpeedBoostFlashShade);
}
