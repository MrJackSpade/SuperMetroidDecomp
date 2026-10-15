namespace SuperMetroid.Core.Fx.PaletteFx;

/// <summary>Cartridge values and helpers of <see cref="PaletteFxHeatProgramDefinition"/> that only verification reads.</summary>
internal static class PaletteFxHeatProgramDefinitionConstants
{
    /// <summary>Calculated duration for one of the sixteen native phases.</summary>
    /// <remarks>Native $8D:E466/E692/E8BE records hold at phase zero for16 frames,
    /// then traverse a mirrored duration ramp with a floor4 and plateau8. Interior
    /// p=1..14 uses min(8,max(4,min(p,15-p)+2)). The terminal phase holds16 for
    /// protected suits; Power uses3 before its loop. These explicit endpoint roles
    /// and the symmetric interior reproduce all48 original durations. No schedule
    /// array remains. Source: supported NTSC J/U v1.0 and pinned bank_8D.asm.</remarks>
    internal static ushort Duration(PaletteFxHeatSuit suit, int phase)
    {
        if ((uint)phase >= 16) throw new ArgumentOutOfRangeException(nameof(phase));
        if (phase == 0) return 16;
        if (phase == 15) return (ushort)(suit == PaletteFxHeatSuit.Power ? 3 : 16);
        return (ushort)Math.Min(8, Math.Max(4, Math.Min(phase, 15 - phase) + 2));
    }
}
