using SuperMetroid.Core;
using SuperMetroid.Core.Frontend.Map.Artwork;
using SuperMetroid.Core.Frontend.Pause;
using SuperMetroid.Core.Frontend.Pause.Artwork;
using SuperMetroid.Core.Progression;
using SuperMetroid.AssetExtraction.Frontend.Pause.Artwork;
using SuperMetroid.AssetExtraction.Frontend.Map;
using SuperMetroid.AssetExtraction.Artwork;

namespace SuperMetroid.AssetExtraction.Frontend.Pause;

/// <summary>
/// The equipment page as the cartridge's tilemap holds it (#1467). <see cref="LayOut"/> replays the cartridge's writes, in
/// their order, from a shown page: the template, every label as <c>LoadEquipmentScreen</c> writes them, the Plasma overrun
/// and the beam column's five-cell head writes, the wireframe, the reserve labels and digits, and the arrow's colouring.
/// The import cuts the page's pictures from these layouts and verification compares them with native VRAM.
/// </summary>
internal static class PauseEquipmentPageLayout
{
    /// <summary>Cells across the equipment page.</summary>
    internal const int Columns = PauseEquipmentBaseDefinitions.Columns;

    /// <summary>The equipment page's tiles, row by row, as the cartridge's writes leave them for <paramref name="page"/>.</summary>
    /// <exception cref="InvalidDataException">The page shows a state no write sequence produces.</exception>
    internal static BackgroundTile[] LayOut(AreaMapPresentation maps, ShownEquipmentPage page)
    {
        Ensure.NotNull(maps);
        Ensure.NotNull(page);
        BackgroundTile[] tiles = InstalledAreaMapPresentation.PauseOf(maps).EquipmentBase.CreateTilemap();
        PauseEquipmentLabelPresentation labels = InstalledAreaMapPresentation.PauseOf(maps).EquipmentLabels;
        // LoadEquipmentScreen writes each whole label from inventory; the rest of each label shows the state its last
        // whole write left.
        SamusBeamFlags collectedBeams = SamusBeamFlags.None, equippedBeams = SamusBeamFlags.None;
        SamusEquipmentFlags collectedItems = SamusEquipmentFlags.None, equippedItems = SamusEquipmentFlags.None;
        foreach (PauseEquipmentLabelKey key in PauseEquipmentLabelTiles.Labels())
        {
            PauseEquipmentBit bit = PauseEquipmentRules.Mask(key.Category, key.Item);
            PauseEquipmentLabelState rest = page.Label(key.Label).Rest;
            if (rest == PauseEquipmentLabelState.Blank)
                continue;
            collectedBeams |= bit.Beam;
            collectedItems |= bit.Item;
            if (rest == PauseEquipmentLabelState.Enabled)
            {
                equippedBeams |= bit.Beam;
                equippedItems |= bit.Item;
            }
        }
        labels.ApplyInventory(tiles, collectedBeams, equippedBeams, collectedItems, equippedItems, page.HyperBeam);
        // A Boots- or suit-dispatched press on Plasma writes nine cells; a later beam-dispatched press rewrites its head.
        if (page.PlasmaTail is { } tail)
            labels.ApplyLabel(tiles, PauseEquipmentCategory.Beams, PauseEquipmentCategories.PlasmaItem, PauseEquipmentLabelDefinitions.EquipmentWords,
                Disabled(tail));
        foreach (PauseEquipmentLabelKey key in PauseEquipmentLabelTiles.Labels())
        {
            ShownEquipmentLabel shown = page.Label(key.Label);
            bool rewritesHead = PauseEquipmentPictures.HasRest(key.Label) ? shown.Head != shown.Rest
                : key.Label == PauseEquipmentLabelId.Plasma && page.PlasmaTail is not null && !page.HyperBeam;
            if (rewritesHead)
                labels.ApplyLabel(tiles, key.Category, key.Item, PauseEquipmentLabelDefinitions.BeamWords, Disabled(shown.Head));
        }
        InstalledAreaMapPresentation.PauseOf(maps).Wireframes.ApplyTo(tiles, page.Wireframe);
        if (page.Reserve is { } reserve)
        {
            PauseReserveUiPresentation ui = InstalledAreaMapPresentation.PauseOf(maps).ReserveUi;
            ui.ApplyLabel(tiles, PauseReserveLabel.Mode);
            ui.ApplyLabel(tiles, PauseReserveLabel.ReserveTank);
            if (reserve.Mode != ReserveTankMode.None)
                ui.ApplyLabel(tiles, reserve.Mode == ReserveTankMode.Auto ? PauseReserveLabel.Auto : PauseReserveLabel.Manual, LabelAttributes.Preserve);
            int[] digits = reserve.Digits();
            for (int place = 0; place < digits.Length; place++)
                ui.ApplyDigit(tiles, place, digits[place]);
        }
        if (page.Arrow != ShownReserveArrow.Unpainted)
            InstalledAreaMapPresentation.PauseOf(maps).ReserveUi.ApplyArrowTilePalettes(tiles, page.Arrow == ShownReserveArrow.Lit);
        return tiles;
    }

    /// <summary>Whether a written label in <paramref name="state"/> is recoloured grey.</summary>
    /// <exception cref="InvalidDataException">A button write never blanks a label.</exception>
    private static bool Disabled(PauseEquipmentLabelState state) => state switch
    {
        PauseEquipmentLabelState.Enabled => false,
        PauseEquipmentLabelState.Disabled => true,
        PauseEquipmentLabelState.Blank => throw new InvalidDataException("An equipment button never writes a blank label."),
        _ => throw new ArgumentOutOfRangeException(nameof(state), state, "Undefined equipment label state."),
    };

    /// <summary>
    /// The page the pictures are cut from before one part is varied: every label blank, no Hyper or overrun tail, the
    /// Power Suit wireframe, no reserve display and the arrow as the template colours it.
    /// </summary>
    internal static ShownEquipmentPage Reference { get; } = new(
        [.. PauseEquipmentLabelTiles.Labels().Select(key => ShownEquipmentLabel.Whole(key.Label, PauseEquipmentLabelState.Blank))],
        HyperBeam: false, PlasmaTail: null, PauseWireframeKind.PowerSuit, Reserve: null, ShownReserveArrow.Unpainted);

    /// <summary>
    /// Every picture of the equipment page (#1467). No part's picture shows a cell another part writes: the base leaves out
    /// every cell a part can write, each part's picture covers its bounding rectangle, the template showing in its cells the
    /// part does not write, the overrun tail leaves out the wireframe's cell, which the cartridge rewrites after every label
    /// write, and Hyper's column covers every beam label it blanks.
    /// </summary>
    /// <exception cref="InvalidDataException">
    /// The installed artwork places two parts over each other, or a part within another's bounds, in a way the pictures
    /// cannot keep apart.
    /// </exception>
    internal static IEnumerable<ScreenPictureSubject> Subjects(AreaMapPresentation maps)
    {
        Ensure.NotNull(maps);
        PageFootprints parts = PageFootprints.Of(maps);
        var dynamic = new HashSet<int>(parts.Dynamic);
        // The cells each part's pictures may cover with the template: those only parts placed after it write, whose own
        // pictures, always placed, cover those cells again.
        HashSet<int> afterReserve = [.. parts.Arrow];
        HashSet<int> afterWireframe = [.. afterReserve, .. parts.Reserve];
        HashSet<int> afterTail = [.. afterWireframe, .. parts.Wireframe];
        HashSet<int> afterLabels = [.. afterTail, .. parts.ShownTail];
        HashSet<int> afterHyper = [.. afterLabels, .. PauseEquipmentLabelTiles.Labels().SelectMany(key => InstalledAreaMapPresentation.PauseOf(maps).EquipmentLabels.PageCells(key.Label))];
        yield return ScreenPictureSubject.Page(PauseEquipmentPictures.BaseFrame, InstalledAreaMapPresentation.PauseOf(maps).EquipmentBase.CreateTilemap(), Columns, parts.Dynamic);
        PauseEquipmentLabelState[] states = Enum.GetValues<PauseEquipmentLabelState>();
        foreach (PauseEquipmentLabelKey key in PauseEquipmentLabelTiles.Labels())
        {
            int[] cells = [.. InstalledAreaMapPresentation.PauseOf(maps).EquipmentLabels.PageCells(key.Label)];
            foreach (PauseEquipmentLabelState state in states)
            {
                BackgroundTile[] page = LayOut(maps, Reference with { Labels = Reference.Labels.Replace(Reference.Label(key.Label), ShownEquipmentLabel.Whole(key.Label, state)) });
                yield return Part(PauseEquipmentPictures.LabelFrame(key.Label, PauseEquipmentLabelSegment.Head, state), page,
                    cells[..Math.Min(cells.Length, PauseEquipmentLabelDefinitions.BeamWords)], afterLabels);
                if (PauseEquipmentPictures.HasRest(key.Label))
                    yield return Part(PauseEquipmentPictures.LabelFrame(key.Label, PauseEquipmentLabelSegment.Rest, state), page,
                        cells[PauseEquipmentLabelDefinitions.BeamWords..], afterLabels);
            }
        }
        yield return Part(PauseEquipmentPictures.HyperBeamsFrame, LayOut(maps, Reference with { HyperBeam = true }), parts.HyperColumn, afterHyper);
        foreach (PauseEquipmentLabelState? tail in new PauseEquipmentLabelState?[] { null, PauseEquipmentLabelState.Enabled, PauseEquipmentLabelState.Disabled })
        {
            ShownEquipmentPage page = tail is { } state
                ? Reference.WithLabelWritten(PauseEquipmentLabelId.Plasma, PauseEquipmentLabelDefinitions.EquipmentWords, state) : Reference;
            yield return Part(PauseEquipmentPictures.PlasmaTailFrame(tail), LayOut(maps, page), parts.ShownTail, afterTail);
        }
        foreach (PauseWireframeKind kind in Enum.GetValues<PauseWireframeKind>())
            yield return Part(PauseEquipmentPictures.WireframeFrame(kind), LayOut(maps, Reference.WithWireframe(kind)), parts.Wireframe, afterWireframe);
        yield return Part(PauseEquipmentPictures.ReserveAbsentFrame, LayOut(maps, Reference), parts.Reserve, afterReserve);
        foreach (ReserveTankMode mode in Enum.GetValues<ReserveTankMode>())
        {
            BackgroundTile[] page = LayOut(maps, Reference with { Reserve = new ShownReserveDisplay(mode, 0) });
            yield return Part(PauseEquipmentPictures.ReserveModeFrame(mode), page, parts.Mode, afterReserve);
            if (mode == ReserveTankMode.None)
                yield return Part(PauseEquipmentPictures.ReserveTankFrame, page, parts.Tank, afterReserve);
        }
        int placeValue = PauseReserveEnergyDefinitions.Hundred;
        for (int place = 0; place < PauseReserveUiDefinitions.SupplyDigitPlaces; place++, placeValue /= PauseReserveEnergyDefinitions.DecimalBase)
        {
            for (int digit = 0; digit < PauseReserveUiDefinitions.DigitCount; digit++)
            {
                BackgroundTile[] page = LayOut(maps, Reference with { Reserve = new ShownReserveDisplay(ReserveTankMode.None, digit * placeValue) });
                yield return Part(PauseEquipmentPictures.ReserveDigitFrame(place, digit), page, [InstalledAreaMapPresentation.PauseOf(maps).ReserveUi.DigitCell(place)], afterReserve);
            }
        }
        foreach (ShownReserveArrow arrow in Enum.GetValues<ShownReserveArrow>())
            yield return Part(PauseEquipmentPictures.ReserveArrowFrame(arrow), LayOut(maps, Reference.WithArrow(arrow)), parts.Arrow, new HashSet<int>());

        // The renderer shows, at each pixel, the last placed picture whose bounds cover it, so a part's picture paints its
        // whole bounding rectangle: its own cells as the part shows them and the template's cells around them. Only a part
        // placed after it, in `later`, may write a cell there: that part's picture covers the cell again, as the cartridge's
        // later write does.
        ScreenPictureSubject Part(string name, BackgroundTile[] page, IReadOnlyCollection<int> cells, IReadOnlySet<int> later)
        {
            int left = cells.Min(cell => cell % Columns), right = cells.Max(cell => cell % Columns);
            int top = cells.Min(cell => cell / Columns), bottom = cells.Max(cell => cell / Columns);
            int[] bounds = [.. Enumerable.Range(top, bottom - top + 1).SelectMany(row => Enumerable.Range(row * Columns + left, right - left + 1))];
            foreach (int cell in bounds.Except(cells))
            {
                if (dynamic.Contains(cell) && !later.Contains(cell))
                    throw new InvalidDataException($"Picture {name}'s bounds cover cell {cell}, which a part of the equipment page placed before it writes.");
            }
            return ScreenPictureSubject.Cells(name, page, Columns, bounds);
        }
    }

    /// <summary>
    /// The cells each part of the page writes (#1467), checked to keep the parts apart: no two parts share a cell but the
    /// overrun tail and the wireframe, which the wireframe owns, and Hyper's label lies within the beam column it blanks.
    /// </summary>
    /// <param name="Dynamic">Every cell any part writes, which the base leaves out.</param>
    /// <param name="HyperColumn">The beam labels' cells and Hyper's, which Hyper's column picture covers.</param>
    /// <param name="ShownTail">The overrun tail's cells the wireframe does not own.</param>
    /// <param name="Wireframe">The wireframe's cells.</param>
    /// <param name="Reserve">The reserve labels' and digits' cells, which the template shows without reserves.</param>
    /// <param name="Mode">The MODE label's cells and its AUTO and MANUAL replacements'.</param>
    /// <param name="Tank">The RESERVE TANK label's cells.</param>
    /// <param name="Arrow">The reserve arrow's cells.</param>
    private sealed record PageFootprints(int[] Dynamic, int[] HyperColumn, int[] ShownTail, int[] Wireframe, int[] Reserve, int[] Mode, int[] Tank, int[] Arrow)
    {
        /// <summary>The footprints of <paramref name="maps"/>' equipment artwork.</summary>
        /// <exception cref="InvalidDataException">Two parts overlap in a way the pictures cannot keep apart.</exception>
        internal static PageFootprints Of(AreaMapPresentation maps)
        {
            PauseEquipmentLabelPresentation labels = InstalledAreaMapPresentation.PauseOf(maps).EquipmentLabels;
            PauseReserveUiPresentation ui = InstalledAreaMapPresentation.PauseOf(maps).ReserveUi;
            int[] wireframe = [.. Enumerable.Range(0, PauseWireframeDefinitions.Rows).SelectMany(row => Enumerable.Range(
                PauseWireframeDefinitions.FirstPageTile + row * PauseWireframeDefinitions.PageColumns, PauseWireframeDefinitions.Columns))];
            int[] beams = [.. PauseEquipmentLabelTiles.Labels().Where(key => !PauseEquipmentPictures.HasRest(key.Label)).SelectMany(key => labels.PageCells(key.Label))];
            int[] hyper = [.. labels.PageCells(PauseEquipmentLabelId.Hyper)];
            if (hyper.Except(beams).Any())
                throw new InvalidDataException("Hyper's label must lie within the beam labels it replaces.");
            int[] shownTail = [.. labels.PlasmaOverrunCells().Except(wireframe)];
            int[] mode = [.. new[] { PauseReserveLabel.Mode, PauseReserveLabel.Manual, PauseReserveLabel.Auto }.SelectMany(ui.LabelCells).Distinct()];
            int[] tank = [.. ui.LabelCells(PauseReserveLabel.ReserveTank)];
            int[] digits = [.. Enumerable.Range(0, PauseReserveUiDefinitions.SupplyDigitPlaces).Select(ui.DigitCell)];
            int[] arrow = [.. ui.ArrowPageCells()];
            int[][] parts = [.. PauseEquipmentLabelTiles.Labels().Select(key => labels.PageCells(key.Label).ToArray()), shownTail, wireframe, mode, tank, digits, arrow];
            var owned = new HashSet<int>();
            foreach (int cell in parts.SelectMany(part => part))
            {
                if (!owned.Add(cell))
                    throw new InvalidDataException($"Two parts of the equipment page write cell {cell}; its pictures cannot keep them apart.");
            }
            int[] reserve = [.. mode, .. tank, .. digits];
            return new([.. owned.Order()], [.. beams.Union(hyper)], shownTail, wireframe, reserve, mode, tank, arrow);
        }
    }
}
