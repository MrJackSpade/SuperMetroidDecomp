
namespace SuperMetroid.Core.Fx.ColourEffects;

/// <summary>Verification access to <see cref="PaletteFxHeatAnimationScriptDefinitions"/> members production does not use.</summary>
internal static class PaletteFxHeatAnimationScriptDefinitionsAccess
{
    /// <summary>Resolves the native selector-word source for each heat-suit and phase pair.</summary>
    extension(PaletteFxHeatAnimationScriptDefinitions)
    {
        /// <summary>Returns the native bank-$8D selector-word address for parity verification.</summary>
        internal static ushort NativeSourceAddress(ColourEffectHeatSuit suit, int phase)
        {
            if ((uint)phase >= PaletteFxHeatAnimationScriptDefinitions.PhaseCount)
                throw new ArgumentOutOfRangeException(nameof(phase));

            ushort table = suit switch
            {
                ColourEffectHeatSuit.Power => PaletteFxHeatAnimationScriptDefinitions.PowerSourceTable,
                ColourEffectHeatSuit.Varia => PaletteFxHeatAnimationScriptDefinitions.VariaSourceTable,
                ColourEffectHeatSuit.Gravity => PaletteFxHeatAnimationScriptDefinitions.GravitySourceTable,
                _ => throw new ArgumentOutOfRangeException(nameof(suit), suit, "Unknown heat suit."),
            };
            return unchecked((ushort)(table + phase * sizeof(ushort)));
        }
    }
}
