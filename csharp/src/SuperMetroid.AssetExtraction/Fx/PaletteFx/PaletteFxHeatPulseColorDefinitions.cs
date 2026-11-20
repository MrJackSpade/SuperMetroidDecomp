using SuperMetroid.Core.Fx.ColourEffects;

namespace SuperMetroid.AssetExtraction.Fx.PaletteFx;

/// <summary>
/// The three suits' heat-pulse records and the colours each writes, for the import.
/// </summary>
public static class PaletteFxHeatPulseColorDefinitions
{
    /// <summary>The Power Suit's heat-pulse records and colours.</summary>
    private static readonly PaletteFxHeatProgramDefinition Power = new()
    {
        Suit = ColourEffectHeatSuit.Power,
        ColorFrames = PaletteFxColorFrames.Run(ColourEffectAnimations.HeatPulses[ColourEffectHeatSuit.Power], 0, HeatPhase.Count, PaletteFxHeatProgramDefinition.ColorsPerFrame),
    };
    /// <summary>The Varia Suit's heat-pulse records and colours.</summary>
    private static readonly PaletteFxHeatProgramDefinition Varia = new()
    {
        Suit = ColourEffectHeatSuit.Varia,
        ColorFrames = PaletteFxColorFrames.Run(ColourEffectAnimations.HeatPulses[ColourEffectHeatSuit.Varia], 0, HeatPhase.Count, PaletteFxHeatProgramDefinition.ColorsPerFrame),
    };
    /// <summary>The Gravity Suit's heat-pulse records and colours.</summary>
    private static readonly PaletteFxHeatProgramDefinition Gravity = new()
    {
        Suit = ColourEffectHeatSuit.Gravity,
        ColorFrames = PaletteFxColorFrames.Run(ColourEffectAnimations.HeatPulses[ColourEffectHeatSuit.Gravity], 0, HeatPhase.Count, PaletteFxHeatProgramDefinition.ColorsPerFrame),
    };

    /// <summary>The Power, Varia, and Gravity programs in selection order.</summary>
    public static IReadOnlyList<PaletteFxHeatProgramDefinition> All { get; } = [Power, Varia, Gravity];

}
