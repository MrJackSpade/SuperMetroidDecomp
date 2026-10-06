namespace SuperMetroid.Core.Assets;

/// <summary>Calculated native visor color phases with narrowly retained painted-light composition choices.</summary>
internal static class SamusVisorColorDefinitions
{
    /// <summary>$9B:A3C0, SamusPalettes_Visor: first widening color (0,31,14), the exact normal-suit visor ink at $9B:9408/9528/9808.</summary>
    /// <remarks>Covered only by the independently reviewed normalAndLoadingBaseInkDisposition
    /// of SamusSuitColorCatalog: fixed OBJ palette4/color4 is a categorical painted ink,
    /// not a measured quantity determining its RGB value. The same3BE0 begins this visor
    /// transition. Catalogs remain independently editable; this is shared source identity,
    /// not mutable cross-catalog coupling. FullBeamStart and DarkeningStep have a separate narrow visor-light disposition.</remarks>
    internal const ushort WideningStart = 0x3BE0;

    /// <summary>$9B:A3C6, SamusPalettes_Visor: selected yellow-white full-beam/room-cycle ink (31,31,16); retained painted visor-light choice.</summary>
    internal const ushort FullBeamStart = 0x43FF;

    /// <summary>$9B:A3C6-$A3CB, SamusPalettes_Visor: selected five-level decrement per RGB5 channel; retained visor-light brightness contrast.</summary>
    /// <remarks>Only the six original visor RGB outputs are covered. Native $91:D86F/DCD3/DD09
    /// publish these to sprite palette4/color4. Timing, including the separate FrameDelay5,
    /// and beam geometry do not determine these selected color choices and are not exempt.</remarks>
    internal const int DarkeningStep = 5;

    /// <summary>$9B:A3C0-$A3C5: widening start, midpoint and full-white endpoint.</summary>
    internal const int WideningPhaseCount = 3;

    internal static ushort Color(int index)
    {
        if ((uint)index >= SamusVisorColorFormat.ColorCount)
            throw new ArgumentOutOfRangeException(nameof(index));
        int result = 0;
        for (int shift = 0; shift <= 10; shift += 5)
        {
            int channel;
            if (index < WideningPhaseCount)
            {
                int start = (WideningStart >> shift) & 31;
                int intervals = WideningPhaseCount - 1;
                channel = (start * (intervals - index) + 31 * index + intervals / 2) / intervals;
            }
            else
                channel = ((FullBeamStart >> shift) & 31) - (index - WideningPhaseCount) * DarkeningStep;
            result |= channel << shift;
        }
        return (ushort)result;
    }
}
