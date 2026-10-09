using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;
using System.Buffers.Binary;

namespace SuperMetroid.Core.Frontend;

internal sealed partial class PauseMenuState
{
    /// <summary>Installs replacement map and pause presentation assets, rebinds their VRAM and CGRAM content, and refreshes dependent pause-screen artwork without resetting live animation state.</summary>
    /// <param name="catalog">Validated presentation assets to bind; required because pause rendering depends on installed map visuals.</param>
    internal void BindMapPresentation(AreaMapPresentationCatalog? catalog)
    {
        AreaMapPresentationCatalog? previousCatalog = mapPresentation;
        mapPresentation = catalog ?? throw new InvalidOperationException(
            "Pause menu requires installed map presentation assets.");
        mapArrows ??= new FileSelectMapAnimations(bus, catalog.Arrows);
        mapArrows.BindPresentation(catalog.Arrows);
        paletteAnimation.Bind(catalog.HighlightCycle);
        catalog.PauseTiles.LoadTo(vram, PauseTileAtlasFormat.DestinationByte);
        catalog.Sprites.LoadArtworkTo(vram, MapSpriteFormat.PauseDestination);
        for (int color = 0; color < SuperMetroid.Core.Hardware.SnesCgram.ColorCount; color++)
            // Live highlights and reserve-arrow colors belong to their animation
            // owners. Replacing the static base must not reset their phase.
            if ((color < MapAnimationRomData.PaletteDestination || color >= MapAnimationRomData.PaletteDestination + MapPaletteCycleFormat.ColorCount) &&
                color != PauseReserveArrowRomData.Color6Index && color != PauseReserveArrowRomData.Color11Index)
                cgram.SetColor(color, catalog.Palettes.Pause[color]);
        catalog.Tiles.LoadTo(vram, 0);
        catalog.HudTiles.LoadTo(vram, HudTileAtlasFormat.DestinationWord * 2);
        LoadPauseBackdrop();
        RefreshPauseButtonArtwork();
        // Refresh the authored base and rebuild the now-semantic inventory layer. The
        // explicit overrun bit retains the retail Boots-to-Plasma VAR artifact without
        // preserving stale pixels at a label's old editable destination.
        bool labelsChanged = previousCatalog is null ||
            previousCatalog.PauseEquipmentLabels.ContentIdentity != catalog.PauseEquipmentLabels.ContentIdentity;
        if (labelsChanged)
        {
            catalog.PauseEquipmentBase.RebindBeforeInventoryRefreshInto(equipmentTilemap);
            catalog.PauseEquipmentLabels.ApplyInventory(equipmentTilemap,
                samus.CollectedBeams, samus.EquippedBeams, samus.CollectedItems,
                samus.EquippedItems, samus.HyperBeam != 0);
            if (plasmaLabelOverrunActive)
                catalog.PauseEquipmentLabels.ApplyLabel(equipmentTilemap,
                    PauseEquipmentCategories.Beams, PauseEquipmentCategories.PlasmaItem,
                    PauseEquipmentCategories.Get(PauseEquipmentCategories.Boots).LabelWordCount,
                    disabled: false);
        }
        else catalog.PauseEquipmentBase.RebindBaseInto(
            equipmentTilemap, previousCatalog!.PauseEquipmentLabels);
        // Reapply only the wireframe patch, in its native footprint. The surrounding
        // mutable labels include intentional cartridge overruns and must not be rebuilt.
        WriteSamusWireframe();
        // Reserve presentation has its own bounded footprints and can safely adopt the
        // rebound content without reconstructing unrelated inventory labels.
        if (samus.MaxReserveEnergy != 0)
        {
            WriteReserveLabels();
            WriteReserveSupplyDigits();
            // A rebind is not a tank dispatch: re-apply the owner's latched arrow state with the
            // new palettes. Only a bind before anything has latched asks the owner.
            if (reserveArrowLatched)
                ReapplyLatchedReserveArrow();
            else if (selectedCategory == PauseEquipmentCategories.Reserves)
                UpdateReserveArrow(pauseNmiFrameCounter8);
        }
        if (ScreenMode != 0) UploadEquipmentTilemap();
        // The serialized live button palette rows still own their overlay on
        // the refreshed backdrop, including a Start/fade highlight.
        // Do not rerun controls or rebuild equipment: that would erase native
        // same-frame label overruns and change the state being restored.
        vram.LoadBytes(PauseMenuLayout.ButtonRowsDestinationWord * 2,
            pauseButtonTilemap.AsSpan(PauseMenuLayout.ButtonRowsSourceOffset, PauseMenuLayout.ButtonRowsByteCount));
        // Preserve the serialized scroll position and transition timing. Only refresh
        // BG1 when it currently contains the map; the equipment page shares that VRAM.
        if (ScreenMode == 0) LoadPauseMapTilemap();
    }

    /// <summary>Loads the active area's pause backdrop into BG2 using the currently bound presentation catalog.</summary>
    private void LoadPauseBackdrop()
    {
        (mapPresentation ?? throw new InvalidOperationException(
            "Pause backdrop requires installed presentation assets."))
            .PauseBackdrops.LoadTo(vram, PauseMenuLayout.Bg2TilemapWord * 2, area);
    }

    /// <summary>Rebuilds the pause button tilemap from the bound backdrop while retaining palette bits owned by live menu highlights.</summary>
    private void RefreshPauseButtonArtwork()
    {
        byte[] replacement = (mapPresentation ?? throw new InvalidOperationException(
            "Pause buttons require installed presentation assets."))
            .PauseBackdrops.CreateButtonTilemap();
        // Only these palette bits are owned by the live menu. Carry them forward
        // word-by-word (including restored historical states) rather than inferring
        // a mode from ScreenMode, which lags the button highlight during fades.
        foreach (var span in PauseMenuLayout.ButtonLabelSpans)
        for (int index = 0; index < span.Count; index++)
        {
            int offset = (span.Word - PauseMenuLayout.ButtonSourceWordOrigin + index) * 2;
            var previous = new SnesBgTilemapWord(BinaryPrimitives.ReadUInt16LittleEndian(pauseButtonTilemap.AsSpan(offset)));
            var current = new SnesBgTilemapWord(BinaryPrimitives.ReadUInt16LittleEndian(replacement.AsSpan(offset)));
            BinaryPrimitives.WriteUInt16LittleEndian(replacement.AsSpan(offset), current.WithPaletteIndex(previous.PaletteIndex).Raw);
        }
        replacement.CopyTo(pauseButtonTilemap, 0);
    }
}
