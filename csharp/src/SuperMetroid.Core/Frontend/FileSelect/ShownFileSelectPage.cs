using System.Collections.Immutable;
using SuperMetroid.Core.Frontend.FileSelect.Artwork;
using SuperMetroid.Core.Frontend.Menus;
using SuperMetroid.Core.Saves;
using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Frontend.FileSelect;

/// <summary>
/// What the file-select page shows (#1467): the page last selected, each slot's field (none for an empty slot) and the slots
/// the copy and clear pages name, as they were when the page was last selected or its slots last changed.
/// </summary>
/// <param name="Page">The page.</param>
/// <param name="Slots">Each slot's field, or none where the slot holds no save.</param>
/// <param name="SlotChoice">The slots the copy or clear has chosen, which its prompts name.</param>
internal sealed partial record ShownFileSelectPage(FileSelectPage Page, ImmutableDictionary<SaveSlot, ShownFileSelectSave?> Slots, FileSelectSlotChoice SlotChoice)
{
    /// <summary>Whether the page lays its slots out as the main menu does, rather than as copy and clear do.</summary>
    internal bool Main => Page is FileSelectPage.MainWithData or FileSelectPage.MainEmpty;

    /// <summary>
    /// The menus' pictures the page shows, in drawing order, each at the page pixel it is placed at: the page, then each
    /// slot's field, then the slot letters its copy or clear prompt names.
    /// </summary>
    internal IEnumerable<PlacedFrame> Pictures(FileSelectPresentation presentation)
    {
        Ensure.NotNull(presentation);
        yield return new PlacedFrame(FileSelectParts.PageFrame(Page), default);
        foreach (SaveSlot slot in SaveSlots.All)
        {
            foreach (PlacedFrame field in Field(Ensure.Listed(Slots, slot), presentation.Slot(Main ? FileSelectCursorPage.Main : FileSelectCursorPage.DataManagement, slot)))
                yield return field;
        }
        foreach (FileSelectPromptLetter letter in Letters())
            yield return new PlacedFrame(FileSelectParts.LetterFrame(letter.Slot), presentation.DynamicAnchor(letter.Anchor));
    }

    /// <summary>The slot letters the page names and where.</summary>
    private FileSelectPromptLetter[] Letters() => Page switch
    {
        FileSelectPage.CopyDestination => [new(FileSelectAnchor.CopyDestinationSource, SlotChoice.ChosenSource)],
        FileSelectPage.CopyConfirm or FileSelectPage.CopyCompleted =>
            [new(FileSelectAnchor.CopyConfirmSource, SlotChoice.ChosenSource), new(FileSelectAnchor.CopyConfirmDestination, SlotChoice.ChosenTarget)],
        FileSelectPage.ClearConfirm or FileSelectPage.ClearCompleted => [new(FileSelectAnchor.ClearConfirmSource, SlotChoice.ChosenSource)],
        FileSelectPage.MainWithData or FileSelectPage.MainEmpty or FileSelectPage.CopySource or FileSelectPage.ClearSelection => [],
        _ => throw new InvalidOperationException($"Undefined file-select page {Page}."),
    };

    /// <summary>
    /// One slot's field at <paramref name="layout"/>'s anchors: NO DATA for an empty slot, else ENERGY, the health's last two
    /// digits, a filled or empty tank for each tank's worth of maximum health (<see cref="SamusEnergyDefinitions.EnergyPerTank"/>), and the time as HH:MM.
    /// </summary>
    private static IEnumerable<PlacedFrame> Field(ShownFileSelectSave? save, FileSelectSlotFieldDocument layout)
    {
        if (save is null)
        {
            yield return new PlacedFrame(FileSelectParts.PatchFrame(FileSelectPatch.NoData), layout.NoDataAnchor);
            yield break;
        }
        yield return new PlacedFrame(FileSelectParts.PatchFrame(FileSelectPatch.Energy), layout.EnergyAnchor);
        int health = save.Health % SamusEnergyDefinitions.EnergyPerTank;
        int tens = health / FileSelectFieldDefinitions.DigitBase;
        yield return new PlacedFrame(FileSelectParts.DigitFrame(tens), layout.HealthAnchor);
        yield return new PlacedFrame(FileSelectParts.DigitFrame(health % FileSelectFieldDefinitions.DigitBase), Right(layout.HealthAnchor, 1));
        int filled = save.Health / SamusEnergyDefinitions.EnergyPerTank;
        int tanks = save.MaxHealth / SamusEnergyDefinitions.EnergyPerTank;
        // Tanks fill rows left to right, the first row below ENERGY and each later row one tank above the last.
        for (int tank = 0; tank < tanks; tank++)
        {
            int column = FileSelectEnergyTankDefinitions.ColumnOffset + tank % FileSelectEnergyTankDefinitions.TanksPerRow;
            int row = FileSelectEnergyTankDefinitions.RowOffset - tank / FileSelectEnergyTankDefinitions.TanksPerRow;
            int x = layout.EnergyAnchor.X + column * FileSelectEnergyTankDefinitions.TankPixels;
            int y = layout.EnergyAnchor.Y + row * FileSelectEnergyTankDefinitions.TankPixels;
            yield return new PlacedFrame(FileSelectParts.TankFrame(tank < filled), new(x, y));
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
