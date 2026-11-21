using SuperMetroid.Core;
using SuperMetroid.Core.Frontend.Pause;
using SuperMetroid.Core.Frontend.Pause.Artwork;
using SuperMetroid.Core.Progression;

namespace SuperMetroid.AssetExtraction.Frontend.Pause;

/// <summary>
/// The order the pause sheet holds the pictures of each kind (#2567), written out so reordering a kind's members never
/// reorders the installed sheet: the pictures and the names the game checks the sheet by both follow these lists.
/// </summary>
internal static class PauseSheetFrameOrder
{
    /// <summary>Each equipment label's states: blank, enabled, disabled.</summary>
    internal static readonly IReadOnlyList<PauseEquipmentLabelState> LabelStates = Ensure.EveryMemberOnce<PauseEquipmentLabelState>(
    [
        PauseEquipmentLabelState.Blank,
        PauseEquipmentLabelState.Enabled,
        PauseEquipmentLabelState.Disabled,
    ]);

    /// <summary>Samus's wireframes: the Power Suit's, with Hi-Jump, then the Varia Suit's, with Hi-Jump.</summary>
    internal static readonly IReadOnlyList<PauseWireframe> Wireframes =
    [
        new(PauseWireframeSuit.PowerSuit, PauseWireframeBoots.RegularBoots),
        new(PauseWireframeSuit.PowerSuit, PauseWireframeBoots.HiJumpBoots),
        new(PauseWireframeSuit.VariaSuit, PauseWireframeBoots.RegularBoots),
        new(PauseWireframeSuit.VariaSuit, PauseWireframeBoots.HiJumpBoots),
    ];

    /// <summary>The reserve tanks' modes: none, auto, manual.</summary>
    internal static readonly IReadOnlyList<ReserveTankMode> ReserveModes = Ensure.EveryMemberOnce<ReserveTankMode>(
    [
        ReserveTankMode.None,
        ReserveTankMode.Auto,
        ReserveTankMode.Manual,
    ]);

    /// <summary>The reserve arrow's looks: no reserves, lit, dim.</summary>
    internal static readonly IReadOnlyList<ShownReserveArrow> ReserveArrows = Ensure.EveryMemberOnce<ShownReserveArrow>(
    [
        ShownReserveArrow.NoReserves,
        ShownReserveArrow.Lit,
        ShownReserveArrow.Dim,
    ]);

    /// <summary>The menu buttons' labels: map, exit, Samus.</summary>
    internal static readonly IReadOnlyList<PauseButtonLabel> ButtonLabels = Ensure.EveryMemberOnce<PauseButtonLabel>(
    [
        PauseButtonLabel.Map,
        PauseButtonLabel.Exit,
        PauseButtonLabel.Samus,
    ]);

    /// <summary>Each button label's shades: lit, dim.</summary>
    internal static readonly IReadOnlyList<PauseButtonLabelShade> ButtonLabelShades = Ensure.EveryMemberOnce<PauseButtonLabelShade>(
    [
        PauseButtonLabelShade.Lit,
        PauseButtonLabelShade.Dim,
    ]);
}
