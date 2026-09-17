using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Frontend;

public sealed partial class FileSelectMenuState
{
    /// <summary>Rebinds host-owned file-select visuals after debugger-state restoration.</summary>
    internal void BindMapPresentation(AreaMapPresentationCatalog? catalog)
    {
        mapPresentation = catalog;
        ppu.BindWorldArtwork(bus, catalog?.WorldArtwork);
        ppu.BindMapTiles(bus, catalog?.Tiles);
        ppu.BindMapSprites(bus, catalog?.Sprites);
        ppu.BindMapPalettes(bus, catalog?.Palettes);
        if (catalog is null)
        {
            ppu.LoadInitialBackground(bus);
            RebuildLegacyPresentationPage();
        }
        else
        {
            catalog.FileSelect.LoadBackground(ppu.Vram);
            RebuildInstalledPresentationPage();
        }
        UploadBg1Tilemap();
    }

    private void RebuildInstalledPresentationPage()
    {
        FileSelectPresentation presentation = mapPresentation?.FileSelect ??
            throw new InvalidOperationException("File-select presentation is not bound.");
        presentation.CopyPage(currentPresentationPage, bg1Tilemap);
        bool main = currentPresentationPage is
            FileSelectPresentationDefinitions.MainWithDataPage or
            FileSelectPresentationDefinitions.MainEmptyPage;
        for (int slot = 0; slot < saveSlots.Length; slot++)
            DrawInstalledSlot(saveSlots[slot], presentation.Slot(!main, slot));

        switch (currentPresentationPage)
        {
            case FileSelectPresentationDefinitions.CopyDestinationPage:
                presentation.WriteSlotLetter(bg1Tilemap,
                    presentation.DynamicAnchor(
                        FileSelectPresentationDefinitions.CopyDestinationSourceAnchor),
                    operationSourceSlot);
                break;
            case FileSelectPresentationDefinitions.CopyConfirmPage:
            case FileSelectPresentationDefinitions.CopyCompletedPage:
                presentation.WriteSlotLetter(bg1Tilemap,
                    presentation.DynamicAnchor(
                        FileSelectPresentationDefinitions.CopyConfirmSourceAnchor),
                    operationSourceSlot);
                presentation.WriteSlotLetter(bg1Tilemap,
                    presentation.DynamicAnchor(
                        FileSelectPresentationDefinitions.CopyConfirmDestinationAnchor),
                    operationDestinationSlot);
                break;
            case FileSelectPresentationDefinitions.ClearConfirmPage:
            case FileSelectPresentationDefinitions.ClearCompletedPage:
                presentation.WriteSlotLetter(bg1Tilemap,
                    presentation.DynamicAnchor(
                        FileSelectPresentationDefinitions.ClearConfirmSourceAnchor),
                    operationSourceSlot);
                break;
        }
    }

    private void DrawInstalledSlot(
        SuperMetroid.Core.Game.SuperMetroidSaveSlot? slot,
        FileSelectSlotFieldDocument layout)
    {
        FileSelectPresentation presentation = mapPresentation?.FileSelect ??
            throw new InvalidOperationException("File-select presentation is not bound.");
        if (slot is null)
        {
            presentation.ApplyPatch(bg1Tilemap,
                FileSelectPresentationDefinitions.NoDataPatch, layout.NoDataAnchor);
            return;
        }

        presentation.ApplyPatch(bg1Tilemap,
            FileSelectPresentationDefinitions.EnergyPatch, layout.EnergyAnchor);
        int health = slot.Health % 100;
        presentation.WriteDigit(bg1Tilemap, layout.HealthAnchor, 0, health / 10);
        presentation.WriteDigit(bg1Tilemap, layout.HealthAnchor, 1, health % 10);
        int hours = Math.Min(slot.GameTimeHours, (ushort)99);
        int minutes = Math.Min(slot.GameTimeMinutes, (ushort)99);
        presentation.WriteDigit(bg1Tilemap, layout.TimeValueAnchor, 0, hours / 10);
        presentation.WriteDigit(bg1Tilemap, layout.TimeValueAnchor, 1, hours % 10);
        presentation.ApplyPatch(bg1Tilemap,
            FileSelectPresentationDefinitions.TimeColonPatch,
            new(layout.TimeValueAnchor.X + 2, layout.TimeValueAnchor.Y));
        presentation.WriteDigit(bg1Tilemap, layout.TimeValueAnchor, 3, minutes / 10);
        presentation.WriteDigit(bg1Tilemap, layout.TimeValueAnchor, 4, minutes % 10);
    }

    private void RebuildLegacyPresentationPage()
    {
        switch (currentPresentationPage)
        {
            case FileSelectPresentationDefinitions.MainWithDataPage:
            case FileSelectPresentationDefinitions.MainEmptyPage:
                BuildSaveTilemap();
                break;
            case FileSelectPresentationDefinitions.CopySourcePage:
                BuildCopySourceTilemap();
                break;
            case FileSelectPresentationDefinitions.CopyDestinationPage:
                BuildCopyDestinationTilemap();
                break;
            case FileSelectPresentationDefinitions.CopyConfirmPage:
                BuildCopyConfirmationTilemap();
                break;
            case FileSelectPresentationDefinitions.CopyCompletedPage:
                BuildCopyCompletedTilemap();
                break;
            case FileSelectPresentationDefinitions.ClearSelectionPage:
                BuildClearSelectionTilemap();
                break;
            case FileSelectPresentationDefinitions.ClearConfirmPage:
                BuildClearConfirmationTilemap();
                break;
            case FileSelectPresentationDefinitions.ClearCompletedPage:
                BuildClearCompletedTilemap();
                break;
            default:
                throw new InvalidDataException(
                    $"Unknown file-select presentation page {currentPresentationPage}.");
        }
    }
}
