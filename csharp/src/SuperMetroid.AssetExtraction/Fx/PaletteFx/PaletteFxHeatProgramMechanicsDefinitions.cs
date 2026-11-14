using SuperMetroid.Core.Fx.ColourEffects;

namespace SuperMetroid.AssetExtraction.Fx.PaletteFx;

/// <summary>
/// Immutable control words for the three Samus-in-heat palette programs.
/// </summary>
/// <remarks>
/// Each timed record contains fifteen colours between its duration and terminal
/// wait command. Those 720 color words remain live presentation data; this catalog owns
/// only program setup, timing, waits, and loop control.
/// The Power, Varia, and Gravity programs each watch the heat shimmer's condition
/// and the same colour index.
/// Their sixteen records are 34 bytes apart,
/// p=0..15, and each ends in a wait. Each final goto
/// returns to phase zero; p=16 reaches control. All 114
/// mechanics words match the original programs.
/// </remarks>
public static class PaletteFxHeatProgramMechanicsDefinitions
{
    /// <summary>Compiled heat palette control for the Power Suit.</summary>
    private static readonly PaletteFxHeatProgramDefinition Power = new()
    {
        Suit = ColourEffectHeatSuit.Power,
        ColorFrames = PaletteFxColorFrames.Of(ColourEffectAnimation.SamusInHeatPowerSuit, PaletteFxHeatProgramDefinition.ColorsPerFrame),
    };
    /// <summary>Compiled heat palette control for the Varia Suit.</summary>
    private static readonly PaletteFxHeatProgramDefinition Varia = new()
    {
        Suit = ColourEffectHeatSuit.Varia,
        ColorFrames = PaletteFxColorFrames.Of(ColourEffectAnimation.SamusInHeatVariaSuit, PaletteFxHeatProgramDefinition.ColorsPerFrame),
    };
    /// <summary>Compiled heat palette control for the Gravity Suit.</summary>
    private static readonly PaletteFxHeatProgramDefinition Gravity = new()
    {
        Suit = ColourEffectHeatSuit.Gravity,
        ColorFrames = PaletteFxColorFrames.Of(ColourEffectAnimation.SamusInHeatGravitySuit, PaletteFxHeatProgramDefinition.ColorsPerFrame),
    };

    /// <summary>The Power, Varia, and Gravity programs in selection order.</summary>
    public static IReadOnlyList<PaletteFxHeatProgramDefinition> All { get; } = [Power, Varia, Gravity];

}
