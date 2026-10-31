using System.Collections.Immutable;
using SuperMetroid.Core.Frontend.FileSelect.Artwork;
using SuperMetroid.Core.Frontend.Menus;

namespace SuperMetroid.Core.Frontend.FileSelect;

/// <summary>
/// What the file-select page shows (#1467): the page last selected, each slot's field (none for an empty slot) and the slots
/// the copy and clear pages name, as they were when the page was last selected or its slots last changed.
/// </summary>
/// <param name="Page">The page.</param>
/// <param name="Slots">Each slot's field, or none where the slot holds no save.</param>
/// <param name="OperationSource">The slot a copy or clear names first.</param>
/// <param name="CopyTarget">The slot a copy names as its destination.</param>
internal sealed record ShownFileSelectPage(FileSelectPage Page, ImmutableArray<ShownFileSelectSave?> Slots, int OperationSource, int CopyTarget)
{
    /// <summary>Whether the page lays its slots out as the main menu does, rather than as copy and clear do.</summary>
    internal bool Main => Page is FileSelectPage.MainWithData or FileSelectPage.MainEmpty;

    /// <summary>
    /// The menus' pictures the page shows, in drawing order, each at the page cell it is placed at: the page, then each slot's
    /// field, then the slot letters its copy or clear prompt names.
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
            yield return new PlacedFrame(FileSelectParts.LetterFrame(letter.Slot), presentation.DynamicAnchor(letter.Anchor));
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
        // Tanks fill the row below ENERGY left to right, then each later row starts one row higher, back where the row
        // before it started, as Draw_FileSelection_Energy walks the page.
        int column = layout.EnergyAnchor.X + FileSelectEnergyTankDefinitions.ColumnOffset;
        int row = layout.EnergyAnchor.Y + FileSelectEnergyTankDefinitions.RowOffset;
        int rowRemaining = FileSelectEnergyTankDefinitions.FirstRowCount;
        for (int tank = 0; tank < tanks; tank++)
        {
            if ((uint)column >= FileSelectPresentationDefinitions.Width || (uint)row >= FileSelectPresentationDefinitions.Height)
                throw new InvalidDataException("File-select energy tanks escaped the menu page.");
            yield return new PlacedFrame(FileSelectParts.TankFrame(tank < filled, tens), new(column, row));
            column++;
            if (--rowRemaining == 0)
            {
                column -= FileSelectEnergyTankDefinitions.RowRewindColumns;
                row--;
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

    /// <summary>The cell <paramref name="columns"/> to the right of <paramref name="anchor"/>.</summary>
    private static Position16<TilemapCells> Right(Position16<TilemapCells> anchor, int columns) =>
        new(anchor.X + columns, anchor.Y);
}
