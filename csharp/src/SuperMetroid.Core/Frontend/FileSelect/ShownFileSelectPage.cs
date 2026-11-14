using System.Collections.Immutable;
using SuperMetroid.Core.Frontend.FileSelect.Artwork;
using SuperMetroid.Core.Frontend.Menus;
using SuperMetroid.Core.Saves;

namespace SuperMetroid.Core.Frontend.FileSelect;

/// <summary>
/// What the file-select page shows (#1467): the page last selected, each slot's field (none for an empty slot) and the slots
/// the copy and clear pages name, as they were when the page was last selected or its slots last changed.
/// </summary>
/// <param name="Page">The page.</param>
/// <param name="Slots">Each slot's field, or none where the slot holds no save.</param>
/// <param name="OperationSource">The slot a copy or clear names first.</param>
/// <param name="CopyTarget">The slot a copy names as its destination.</param>
internal sealed record ShownFileSelectPage(FileSelectPage Page, ImmutableArray<ShownFileSelectSave?> Slots, SaveSlot OperationSource, SaveSlot CopyTarget)
{
    /// <summary>Whether the page lays its slots out as the main menu does, rather than as copy and clear do.</summary>
    internal bool Main => Page is FileSelectPage.MainWithData or FileSelectPage.MainEmpty;

    /// <summary>
    /// The menus' pictures the page shows, in drawing order, each at the page pixel it is placed at: the page, then each
    /// slot's field, then the slot letters its copy or clear prompt names.
    /// </summary>
    /// <exception cref="InvalidDataException">A slot's energy tanks run past the page.</exception>
    internal IEnumerable<PlacedFrame> Pictures(FileSelectPresentation presentation)
    {
        Ensure.NotNull(presentation);
        yield return new PlacedFrame(FileSelectParts.PageFrame(Page), default);
        for (int slot = 0; slot < Slots.Length; slot++)
        {
            foreach (PlacedFrame field in Field(Slots[slot], presentation.Slot(!Main, slot)))
                yield return field;
        }
        foreach (FileSelectPromptLetter letter in Letters())
            yield return new PlacedFrame(FileSelectParts.LetterFrame(SaveSlots.StoreNumber(letter.Slot)), presentation.DynamicAnchor(letter.Anchor));
    }

    /// <summary>The slot letters the page names and where.</summary>
    private FileSelectPromptLetter[] Letters() => Page switch
    {
        FileSelectPage.CopyDestination => [new(FileSelectAnchor.CopyDestinationSource, OperationSource)],
        FileSelectPage.CopyConfirm or FileSelectPage.CopyCompleted =>
            [new(FileSelectAnchor.CopyConfirmSource, OperationSource), new(FileSelectAnchor.CopyConfirmDestination, CopyTarget)],
        FileSelectPage.ClearConfirm or FileSelectPage.ClearCompleted => [new(FileSelectAnchor.ClearConfirmSource, OperationSource)],
        FileSelectPage.MainWithData or FileSelectPage.MainEmpty or FileSelectPage.CopySource or FileSelectPage.ClearSelection => [],
        _ => throw new InvalidOperationException($"Undefined file-select page {Page}."),
    };

    /// <summary>
    /// One slot's field at <paramref name="layout"/>'s anchors: NO DATA for an empty slot, else ENERGY, the health's last two
    /// digits, a filled or empty tank for each hundred of maximum health, and the time as HH:MM.
    /// </summary>
    private static IEnumerable<PlacedFrame> Field(ShownFileSelectSave? save, FileSelectSlotFieldDocument layout)
    {
        if (save is null)
        {
            yield return new PlacedFrame(FileSelectParts.PatchFrame(FileSelectPatch.NoData), layout.NoDataAnchor);
            yield break;
        }
        yield return new PlacedFrame(FileSelectParts.PatchFrame(FileSelectPatch.Energy), layout.EnergyAnchor);
        int health = save.Health % FileSelectEnergyTankDefinitions.EnergyPerTank;
        int tens = health / FileSelectFieldDefinitions.DigitBase;
        yield return new PlacedFrame(FileSelectParts.DigitFrame(tens), layout.HealthAnchor);
        yield return new PlacedFrame(FileSelectParts.DigitFrame(health % FileSelectFieldDefinitions.DigitBase), Right(layout.HealthAnchor, 1));
        int filled = save.Health / FileSelectEnergyTankDefinitions.EnergyPerTank;
        int tanks = save.MaxHealth / FileSelectEnergyTankDefinitions.EnergyPerTank;
        // Tanks fill the row below ENERGY left to right, then each later row starts one tank higher, back where the row
        // before it started, as the file-select energy draw walks the page.
        const int step = FileSelectEnergyTankDefinitions.TankPixels;
        int x = layout.EnergyAnchor.X + FileSelectEnergyTankDefinitions.ColumnOffset * step;
        int y = layout.EnergyAnchor.Y + FileSelectEnergyTankDefinitions.RowOffset * step;
        int rowRemaining = FileSelectEnergyTankDefinitions.FirstRowCount;
        for (int tank = 0; tank < tanks; tank++)
        {
            if ((uint)x >= FileSelectPresentationDefinitions.PageWidthPixels || (uint)y >= FileSelectPresentationDefinitions.PageHeightPixels)
                throw new InvalidDataException("File-select energy tanks escaped the menu page.");
            yield return new PlacedFrame(FileSelectParts.TankFrame(tank < filled, tens), new(x, y));
            x += step;
            if (--rowRemaining == 0)
            {
                x -= FileSelectEnergyTankDefinitions.RowRewindColumns * step;
                y -= step;
                rowRemaining = FileSelectEnergyTankDefinitions.WrappedRowCount;
            }
        }
        yield return new PlacedFrame(FileSelectParts.DigitFrame(save.Hours / FileSelectFieldDefinitions.DigitBase), layout.TimeValueAnchor);
        yield return new PlacedFrame(FileSelectParts.DigitFrame(save.Hours % FileSelectFieldDefinitions.DigitBase), Right(layout.TimeValueAnchor, 1));
        yield return new PlacedFrame(FileSelectParts.PatchFrame(FileSelectPatch.TimeColon), Right(layout.TimeValueAnchor, FileSelectFieldDefinitions.ColonColumn));
        yield return new PlacedFrame(FileSelectParts.DigitFrame(save.Minutes / FileSelectFieldDefinitions.DigitBase),
            Right(layout.TimeValueAnchor, FileSelectFieldDefinitions.ColonColumn + 1));
        yield return new PlacedFrame(FileSelectParts.DigitFrame(save.Minutes % FileSelectFieldDefinitions.DigitBase),
            Right(layout.TimeValueAnchor, FileSelectFieldDefinitions.ColonColumn + 2));
    }

    /// <summary>The page pixel <paramref name="glyphs"/> glyphs' widths right of <paramref name="anchor"/>.</summary>
    private static Position16<PlanePixels> Right(Position16<PlanePixels> anchor, int glyphs) =>
        anchor + new Offset16<PlanePixels>(glyphs * FileSelectFieldDefinitions.GlyphWidthPixels, 0);
}
