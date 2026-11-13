using SuperMetroid.Core.Frontend.Pause.Artwork;
using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Frontend.Pause;

/// <summary>
/// Names the equipment page's pictures (#1467) and says which a shown page places. Every picture is laid out from the
/// page's corner, and no picture shows a part another part shows: the base leaves out every part that changes, and each
/// part's picture covers its bounding rectangle with the template around the part itself.
/// </summary>
internal static class PauseEquipmentPictures
{
    /// <summary>The template without any label, the wireframe or the reserve display.</summary>
    internal const string BaseFrame = "Pause.Equipment.Base";

    /// <summary>The beam column under Hyper: every beam label blanked and Hyper's label shown over them.</summary>
    internal const string HyperBeamsFrame = "Pause.Equipment.Beams.Hyper";

    /// <summary>The template under the reserve labels and digits, shown while Samus has no reserve capacity.</summary>
    internal const string ReserveAbsentFrame = "Pause.Equipment.Reserve.Absent";

    /// <summary>The RESERVE TANK label.</summary>
    internal const string ReserveTankFrame = "Pause.Equipment.Reserve.Tank";

    /// <summary>The frame name of the whole of <paramref name="label"/> in <paramref name="state"/> (#1515).</summary>
    internal static string LabelFrame(PauseEquipmentLabelId label, PauseEquipmentLabelState state) =>
        $"Pause.Equipment.Label.{label}.{state}";

    /// <summary>The frame name of wireframe <paramref name="kind"/>.</summary>
    internal static string WireframeFrame(PauseWireframeKind kind) => $"Pause.Equipment.Wireframe.{kind}";

    /// <summary>The frame name of the MODE label as written for <paramref name="mode"/>.</summary>
    internal static string ReserveModeFrame(ReserveTankMode mode) => $"Pause.Equipment.Reserve.Mode.{mode}";

    /// <summary>The frame name of decimal <paramref name="digit"/> in supply digit place <paramref name="place"/>, hundreds first.</summary>
    internal static string ReserveDigitFrame(int place, int digit) => $"Pause.Equipment.Reserve.Digit{place}.{digit}";

    /// <summary>The frame name of the reserve arrow's pictures coloured <paramref name="arrow"/>.</summary>
    internal static string ReserveArrowFrame(ShownReserveArrow arrow) => $"Pause.Equipment.Reserve.Arrow.{arrow}";

    /// <summary>The pictures <paramref name="page"/> shows, each placed at the page's corner.</summary>
    internal static IEnumerable<string> Frames(ShownEquipmentPage page)
    {
        Ensure.NotNull(page);
        yield return BaseFrame;
        if (page.HyperBeam)
            yield return HyperBeamsFrame;
        foreach (ShownEquipmentLabel label in page.Labels)
        {
            // Hyper's column covers the ordinary beam labels it replaces.
            if (page.HyperBeam && PauseEquipmentLabelTiles.IsOrdinaryBeam(label.Label))
                continue;
            yield return LabelFrame(label.Label, label.State);
        }
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
