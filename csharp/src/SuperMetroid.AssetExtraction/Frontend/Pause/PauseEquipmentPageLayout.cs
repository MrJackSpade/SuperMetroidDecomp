using SuperMetroid.Core;
using SuperMetroid.Core.Frontend.Pause;
using SuperMetroid.Core.Frontend.Pause.Artwork;
using SuperMetroid.Core.Progression;
using SuperMetroid.AssetExtraction.Frontend.Pause.Artwork;
using SuperMetroid.AssetExtraction.Frontend.Map;
using SuperMetroid.AssetExtraction.Artwork;

namespace SuperMetroid.AssetExtraction.Frontend.Pause;

/// <summary>
/// The equipment page as the cartridge's tilemap holds it (#1467, #1515). <see cref="LayOut"/> lays a shown page out the
/// way the cartridge's <c>LoadEquipmentScreen</c> writes it: the template, every label as its item's state describes it,
/// the wireframe, the reserve labels and digits, and the arrow's colouring. The import cuts the page's pictures from these
/// layouts and verification compares them with native VRAM.
/// </summary>
internal static class PauseEquipmentPageLayout
{
    /// <summary>Cells across the equipment page.</summary>
    internal const int Columns = PauseEquipmentBaseDefinitions.Columns;

    /// <summary>The equipment page's tiles, row by row, as the cartridge writes them for <paramref name="page"/>.</summary>
    internal static BackgroundTile[] LayOut(ImportAreaMapPresentation maps, ShownEquipmentPage page)
    {
        Ensure.NotNull(maps);
        Ensure.NotNull(page);
        BackgroundTile[] tiles = maps.Pause.EquipmentBase.CreateTilemap();
        PauseEquipmentLabelPresentation labels = maps.Pause.EquipmentLabels;
        // LoadEquipmentScreen writes each whole label from the inventory its state describes.
        SamusBeamFlags collectedBeams = SamusBeamFlags.None, equippedBeams = SamusBeamFlags.None;
        SamusEquipmentFlags collectedItems = SamusEquipmentFlags.None, equippedItems = SamusEquipmentFlags.None;
        foreach (PauseEquipmentLabelKey key in PauseEquipmentLabels.Labels())
        {
            PauseEquipmentBit bit = PauseEquipmentRules.Mask(key.Category, key.Item);
            PauseEquipmentLabelState state = page.Label(key.Label).State;
            if (state == PauseEquipmentLabelState.Blank)
                continue;
            collectedBeams |= bit.Beam;
            collectedItems |= bit.Item;
            if (state == PauseEquipmentLabelState.Enabled)
            {
                equippedBeams |= bit.Beam;
                equippedItems |= bit.Item;
            }
        }
        labels.ApplyInventory(tiles, collectedBeams, equippedBeams, collectedItems, equippedItems, page.HyperBeam);
        maps.Pause.Wireframes.ApplyTo(tiles, page.Wireframe);
        if (page.Reserve is { } reserve)
        {
            PauseReserveUiPresentation ui = maps.Pause.ReserveUi;
            ui.ApplyLabel(tiles, PauseReserveLabel.Mode);
            ui.ApplyLabel(tiles, PauseReserveLabel.ReserveTank);
            if (reserve.Mode != ReserveTankMode.None)
                ui.ApplyLabel(tiles, reserve.Mode == ReserveTankMode.Auto ? PauseReserveLabel.Auto : PauseReserveLabel.Manual, LabelAttributes.Preserve);
            int[] digits = reserve.Digits();
            for (int place = 0; place < digits.Length; place++)
                ui.ApplyDigit(tiles, place, digits[place]);
        }
        if (page.Arrow != ShownReserveArrow.NoReserves)
            maps.Pause.ReserveUi.ApplyArrowTilePalettes(tiles, page.Arrow == ShownReserveArrow.Lit);
        return tiles;
    }

    /// <summary>
    /// The page the pictures are cut from before one part is varied: every label blank, no Hyper, the Power Suit wireframe,
    /// no reserve display and the arrow as the template colours it.
    /// </summary>
    internal static ShownEquipmentPage Reference { get; } = new(
        [.. PauseEquipmentLabels.Labels().Select(key => new ShownEquipmentLabel(key.Label, PauseEquipmentLabelState.Blank))],
        HyperBeam: false, PauseWireframeKind.PowerSuit, Reserve: null, ShownReserveArrow.NoReserves);

    /// <summary>
    /// Every picture of the equipment page (#1467, #1515). No part's picture shows a cell another part writes: the base
    /// leaves out every cell a part can write, each part's picture covers its bounding rectangle, the template showing in
    /// its cells the part does not write, each label's picture is the whole label in one state, and Hyper's column covers
    /// every beam label it blanks.
    /// </summary>
    /// <exception cref="InvalidDataException">
    /// The installed artwork places two parts over each other, or a part within another's bounds, in a way the pictures
    /// cannot keep apart.
    /// </exception>
    internal static IEnumerable<ScreenPictureSubject> Subjects(ImportAreaMapPresentation maps)
    {
        Ensure.NotNull(maps);
        PageFootprints parts = PageFootprints.Of(maps);
        var dynamic = new HashSet<int>(parts.Dynamic);
        // The cells each part's pictures may cover with the template: those only parts placed after it write, whose own
        // pictures, always placed, cover those cells again.
        HashSet<int> afterReserve = [.. parts.Arrow];
        HashSet<int> afterWireframe = [.. afterReserve, .. parts.Reserve];
        HashSet<int> afterLabels = [.. afterWireframe, .. parts.Wireframe];
        HashSet<int> afterHyper = [.. afterLabels, .. PauseEquipmentLabels.Labels().SelectMany(key => maps.Pause.EquipmentLabels.PageCells(key.Label))];
        yield return ScreenPictureSubject.Page(PauseEquipmentPictures.BaseFrame, maps.Pause.EquipmentBase.CreateTilemap(), Columns, parts.Dynamic);
        PauseEquipmentLabelState[] states = Enum.GetValues<PauseEquipmentLabelState>();
        foreach (PauseEquipmentLabelKey key in PauseEquipmentLabels.Labels())
        {
            int[] cells = [.. maps.Pause.EquipmentLabels.PageCells(key.Label)];
            foreach (PauseEquipmentLabelState state in states)
            {
                BackgroundTile[] page = LayOut(maps, Reference with { Labels = Reference.Labels.Replace(Reference.Label(key.Label), new ShownEquipmentLabel(key.Label, state)) });
                yield return Part(PauseEquipmentPictures.LabelFrame(key.Label, state), page, cells, afterLabels);
            }
        }
        yield return Part(PauseEquipmentPictures.HyperBeamsFrame, LayOut(maps, Reference with { HyperBeam = true }), parts.HyperColumn, afterHyper);
        foreach (PauseWireframeKind kind in Enum.GetValues<PauseWireframeKind>())
            yield return Part(PauseEquipmentPictures.WireframeFrame(kind), LayOut(maps, Reference with { Wireframe = kind }), parts.Wireframe, afterWireframe);
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
                yield return Part(PauseEquipmentPictures.ReserveDigitFrame(place, digit), page, [maps.Pause.ReserveUi.DigitCell(place)], afterReserve);
            }
        }
        foreach (ShownReserveArrow arrow in Enum.GetValues<ShownReserveArrow>())
            yield return Part(PauseEquipmentPictures.ReserveArrowFrame(arrow), LayOut(maps, Reference with { Arrow = arrow }), parts.Arrow, new HashSet<int>());

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
    /// The name of every picture of the equipment page, in <see cref="Subjects"/> order: the names alone, which the game
    /// needs to check the installed pause sheet and which no installed document holds (#1638).
    /// </summary>
    internal static IEnumerable<string> SubjectNames()
    {
        yield return PauseEquipmentPictures.BaseFrame;
        foreach (PauseEquipmentLabelKey key in PauseEquipmentLabels.Labels())
        foreach (PauseEquipmentLabelState state in Enum.GetValues<PauseEquipmentLabelState>())
            yield return PauseEquipmentPictures.LabelFrame(key.Label, state);
        yield return PauseEquipmentPictures.HyperBeamsFrame;
        foreach (PauseWireframeKind kind in Enum.GetValues<PauseWireframeKind>())
            yield return PauseEquipmentPictures.WireframeFrame(kind);
        yield return PauseEquipmentPictures.ReserveAbsentFrame;
        foreach (ReserveTankMode mode in Enum.GetValues<ReserveTankMode>())
        {
            yield return PauseEquipmentPictures.ReserveModeFrame(mode);
            if (mode == ReserveTankMode.None)
                yield return PauseEquipmentPictures.ReserveTankFrame;
        }
        for (int place = 0; place < PauseReserveUiDefinitions.SupplyDigitPlaces; place++)
        for (int digit = 0; digit < PauseReserveUiDefinitions.DigitCount; digit++)
            yield return PauseEquipmentPictures.ReserveDigitFrame(place, digit);
        foreach (ShownReserveArrow arrow in Enum.GetValues<ShownReserveArrow>())
            yield return PauseEquipmentPictures.ReserveArrowFrame(arrow);
    }

    /// <summary>
    /// The cells each part of the page writes (#1467), checked to keep the parts apart: no two parts share a cell, and
    /// Hyper's label lies within the beam column it blanks.
    /// </summary>
    /// <param name="Dynamic">Every cell any part writes, which the base leaves out.</param>
    /// <param name="HyperColumn">The beam labels' cells and Hyper's, which Hyper's column picture covers.</param>
    /// <param name="Wireframe">The wireframe's cells.</param>
    /// <param name="Reserve">The reserve labels' and digits' cells, which the template shows without reserves.</param>
    /// <param name="Mode">The MODE label's cells and its AUTO and MANUAL replacements'.</param>
    /// <param name="Tank">The RESERVE TANK label's cells.</param>
    /// <param name="Arrow">The reserve arrow's cells.</param>
    private sealed record PageFootprints(int[] Dynamic, int[] HyperColumn, int[] Wireframe, int[] Reserve, int[] Mode, int[] Tank, int[] Arrow)
    {
        /// <summary>The footprints of <paramref name="maps"/>' equipment artwork.</summary>
        /// <exception cref="InvalidDataException">Two parts overlap in a way the pictures cannot keep apart.</exception>
        internal static PageFootprints Of(ImportAreaMapPresentation maps)
        {
            PauseEquipmentLabelPresentation labels = maps.Pause.EquipmentLabels;
            PauseReserveUiPresentation ui = maps.Pause.ReserveUi;
            int[] wireframe = [.. Enumerable.Range(0, PauseWireframeDefinitions.Rows).SelectMany(row => Enumerable.Range(
                PauseWireframeDefinitions.FirstPageTile + row * PauseWireframeDefinitions.PageColumns, PauseWireframeDefinitions.Columns))];
            int[] beams = [.. PauseEquipmentLabels.Labels().Where(key => PauseEquipmentLabels.IsOrdinaryBeam(key.Label)).SelectMany(key => labels.PageCells(key.Label))];
            int[] hyper = [.. labels.PageCells(PauseEquipmentLabelId.Hyper)];
            if (hyper.Except(beams).Any())
                throw new InvalidDataException("Hyper's label must lie within the beam labels it replaces.");
            int[] mode = [.. new[] { PauseReserveLabel.Mode, PauseReserveLabel.Manual, PauseReserveLabel.Auto }.SelectMany(ui.LabelCells).Distinct()];
            int[] tank = [.. ui.LabelCells(PauseReserveLabel.ReserveTank)];
            int[] digits = [.. Enumerable.Range(0, PauseReserveUiDefinitions.SupplyDigitPlaces).Select(ui.DigitCell)];
            int[] arrow = [.. ui.ArrowPageCells()];
            int[][] parts = [.. PauseEquipmentLabels.Labels().Select(key => labels.PageCells(key.Label).ToArray()), wireframe, mode, tank, digits, arrow];
            var owned = new HashSet<int>();
            foreach (int cell in parts.SelectMany(part => part))
            {
                if (!owned.Add(cell))
                    throw new InvalidDataException($"Two parts of the equipment page write cell {cell}; its pictures cannot keep them apart.");
            }
            int[] reserve = [.. mode, .. tank, .. digits];
            return new([.. owned.Order()], [.. beams.Union(hyper)], wireframe, reserve, mode, tank, arrow);
        }
    }
}
