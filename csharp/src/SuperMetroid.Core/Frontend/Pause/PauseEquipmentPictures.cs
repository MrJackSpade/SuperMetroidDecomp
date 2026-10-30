using SuperMetroid.Core.Frontend.Pause.Artwork;
using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Frontend.Pause;

/// <summary>
/// Names the equipment page's pictures (#1467) and says which a shown page places. Every picture is laid out from the
/// page's corner and covers only the cells it finally shows, so no two a page places overlap: the base leaves out every
/// cell a dynamic part can write, and where the Plasma overrun's tail meets the wireframe the wireframe, which the
/// cartridge rewrites after every label write, owns the cell.
/// </summary>
internal static class PauseEquipmentPictures
{
    /// <summary>The template without any cell a label, the wireframe or the reserve display writes.</summary>
    internal const string BaseFrame = "Pause.Equipment.Base";

    /// <summary>The beam column under Hyper: every beam label blanked and Hyper's label written over them.</summary>
    internal const string HyperBeamsFrame = "Pause.Equipment.Beams.Hyper";

    /// <summary>The template's cells under the reserve labels and digits, shown while Samus has no reserve capacity.</summary>
    internal const string ReserveAbsentFrame = "Pause.Equipment.Reserve.Absent";

    /// <summary>The RESERVE TANK label.</summary>
    internal const string ReserveTankFrame = "Pause.Equipment.Reserve.Tank";

    /// <summary>The frame name of <paramref name="segment"/> of <paramref name="label"/> in <paramref name="state"/>.</summary>
    internal static string LabelFrame(PauseEquipmentLabelId label, PauseEquipmentLabelSegment segment, PauseEquipmentLabelState state) =>
        $"Pause.Equipment.Label.{label}.{segment}.{state}";

    /// <summary>The frame name of the Plasma overrun's tail in <paramref name="tail"/>, or the template's cells there with none.</summary>
    internal static string PlasmaTailFrame(PauseEquipmentLabelState? tail) =>
        $"Pause.Equipment.PlasmaTail.{(tail is { } state ? state.ToString() : "Template")}";

    /// <summary>The frame name of wireframe <paramref name="kind"/>.</summary>
    internal static string WireframeFrame(PauseWireframeKind kind) => $"Pause.Equipment.Wireframe.{kind}";

    /// <summary>The frame name of the MODE label as written for <paramref name="mode"/>.</summary>
    internal static string ReserveModeFrame(ReserveTankMode mode) => $"Pause.Equipment.Reserve.Mode.{mode}";

    /// <summary>The frame name of decimal <paramref name="digit"/> in supply digit place <paramref name="place"/>, hundreds first.</summary>
    internal static string ReserveDigitFrame(int place, int digit) => $"Pause.Equipment.Reserve.Digit{place}.{digit}";

    /// <summary>The frame name of the reserve arrow's tiles coloured <paramref name="arrow"/>.</summary>
    internal static string ReserveArrowFrame(ShownReserveArrow arrow) => $"Pause.Equipment.Reserve.Arrow.{arrow}";

    /// <summary>Whether <paramref name="label"/> has cells after its head: every ordinary label but the five-cell beams.</summary>
    internal static bool HasRest(PauseEquipmentLabelId label) => !PauseEquipmentLabelTiles.IsOrdinaryBeam(label);

    /// <summary>The pictures <paramref name="page"/> shows, each placed at the page's corner.</summary>
    internal static IEnumerable<string> Frames(ShownEquipmentPage page)
    {
        Ensure.NotNull(page);
        yield return BaseFrame;
        if (page.HyperBeam)
            yield return HyperBeamsFrame;
        foreach (ShownEquipmentLabel label in page.Labels)
        {
            bool rest = HasRest(label.Label);
            if (!rest && page.HyperBeam)
                continue;
            yield return LabelFrame(label.Label, PauseEquipmentLabelSegment.Head, label.Head);
            if (rest)
                yield return LabelFrame(label.Label, PauseEquipmentLabelSegment.Rest, label.Rest);
        }
        yield return PlasmaTailFrame(page.PlasmaTail);
        yield return WireframeFrame(page.Wireframe);
        if (page.Reserve is { } reserve)
        {
            yield return ReserveModeFrame(reserve.Mode);
            yield return ReserveTankFrame;
            int[] digits = reserve.Digits();
            for (int place = 0; place < digits.Length; place++)
                yield return ReserveDigitFrame(place, digits[place]);
        }
        else
        {
            yield return ReserveAbsentFrame;
        }
        yield return ReserveArrowFrame(page.Arrow);
    }
}
