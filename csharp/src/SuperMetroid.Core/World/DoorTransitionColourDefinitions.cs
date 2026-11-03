using SuperMetroid.Core.Assets;
namespace SuperMetroid.Core.World;

/// <summary>The colours and timing of a door's fades (#1405, #1512), as <c>GameState_A_LoadingNextRoom</c> runs them.</summary>
public static class DoorTransitionColourDefinitions
{
    /// <summary>The ordinary green visor colour loaded when door scrolling completes.</summary>
    public static readonly RgbColor DoorCompletionVisorGreen = new(0, 31, 14);

    /// <summary>The length of a door's fades: the source room fades out, and the room it enters fades in, over this many steps.</summary>
    public const int FadeDenominator = 12;

    /// <summary>
    /// The HUD pictures' shades a door's fade shows, one per step taken, from none through every step and the completing
    /// call: the fade's length plus two.
    /// </summary>
    public const int HudFadeShades = FadeDenominator + 3;
}
