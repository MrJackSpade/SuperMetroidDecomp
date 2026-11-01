using SuperMetroid.Core.Assets;
namespace SuperMetroid.Core.Samus.Artwork;

/// <summary>Calculated visor color phases with narrowly retained painted-light composition choices.</summary>
internal static class SamusVisorColorDefinitions
{
    /// <summary>SamusPalettes_Visor: first widening color (0,31,14), the exact normal-suit visor ink.</summary>
    /// <remarks>Covered only by the independently reviewed normalAndLoadingBaseInkDisposition
    /// of InstalledSamusSuitColors: fixed OBJ palette4/color4 is a categorical painted ink,
    /// not a measured quantity determining its RGB value. The same3BE0 begins this visor
    /// transition. Catalogs remain independently editable; this is shared source identity,
    /// not mutable cross-catalog coupling. FullBeamStart and DarkeningStep have a separate narrow visor-light disposition.</remarks>
    internal static readonly RgbColor WideningStart = new(0, 31, 14);

    /// <summary>SamusPalettes_Visor: selected yellow-white full-beam/room-cycle ink (31,31,16); retained painted visor-light choice.</summary>
    internal static readonly RgbColor FullBeamStart = new(31, 31, 16);

    /// <summary>SamusPalettes_Visor: selected five-level decrement per RGB5 channel; retained visor-light brightness contrast.</summary>
    /// <remarks>Only the six original visor RGB outputs are covered. The visor handlers
    /// publish these to sprite palette4/color4. Timing, including the separate FrameDelay5,
    /// and beam geometry do not determine these selected color choices and are not exempt.</remarks>
    internal const int DarkeningStep = 5;

    /// <summary>Widening start, midpoint and full-white endpoint.</summary>
    internal const int WideningPhaseCount = 3;
}
