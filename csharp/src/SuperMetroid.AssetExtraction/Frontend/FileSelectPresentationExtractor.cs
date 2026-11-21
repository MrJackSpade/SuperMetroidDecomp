using SuperMetroid.AssetExtraction.Frontend.Map;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.Frontend.FileSelect;
using SuperMetroid.AssetExtraction.Frontend.FileSelect.Artwork;
using SuperMetroid.Core;
using SuperMetroid.Core.Saves;
using SuperMetroid.Core.Spatial;
using SuperMetroid.Core.Frontend.FileSelect;
using SuperMetroid.Core.Frontend.Menus;
using SuperMetroid.Core.Frontend.FileSelect.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.CartridgeTables;
using SuperMetroid.AssetExtraction.Frontend.Menus;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Saves;

namespace SuperMetroid.AssetExtraction.Frontend;

/// <summary>
/// Imports file-select page templates, dynamic slot-field artwork and menu actors.
/// Save validation and copy/clear behavior are deliberately not asset data.
/// </summary>
public static class FileSelectPresentationExtractor
{
    /// <summary>Builds file-select page templates and imports dynamic slot artwork, borders, cursor frames, and helmet frames.</summary>
    /// <param name="bus">Non-null cartridge import address space supplying native menu tilemaps, text patches, and spritemaps.</param>
    /// <returns>The installed layout document, with its slot fields, pixel-space sprite anchors and frame durations, and the import-data artwork document, with its pages, field patches, digit and slot-letter cells and sprite compositions (#1638).</returns>
    /// <remarks>Includes main, copy, and clear presentation variants without importing save contents, validation, or copy/clear control flow.</remarks>
    /// <exception cref="ArgumentNullException"><paramref name="bus"/> is null.</exception>
    /// <exception cref="InvalidDataException">Imported artwork escapes its page or fails presentation or spritemap validation.</exception>
    public static FileSelectExtraction Extract(ISnesAddressSpace bus)
    {
        Ensure.NotNull(bus);
        var rawPatches = new Dictionary<int, RawPatch>();
        // Cache a decoded native text patch by its bank-local pointer across page variants.
        RawPatch Patch(int pointer)
        {
            if (!rawPatches.TryGetValue(pointer, out RawPatch? patch))
                rawPatches.Add(pointer, patch = ReadPatch(bus, pointer));
            return patch;
        }

        ushort[] mainWithData = BlankPage();
        AddMainStatic(mainWithData, includeDataCommands: true);
        ushort[] mainEmpty = BlankPage();
        AddMainStatic(mainEmpty, includeDataCommands: false);

        // Build a copy-mode page with its prompt, optional YES/NO question, and optional completion text.
        ushort[] CopyPage(int prompt, int promptDestination,
            bool confirmation = false, int? completion = null)
        {
            ushort[] page = DataBase(FileSelectTilemapSourceDefinitions.DataCopyMode,
                FileSelectNativeLayout.DataModeCopyDestination, prompt, promptDestination);
            if (confirmation)
                AddConfirmation(page);
            if (completion is int completed)
                Apply(page, Patch(completed), FileSelectNativeLayout.CopyCompletedDestination);
            return page;
        }

        // Build a clear-mode page with its prompt, optional confirmation, and optional cleared-result text.
        ushort[] ClearPage(int prompt, int promptDestination,
            bool confirmation = false, bool completed = false)
        {
            ushort[] page = DataBase(FileSelectTilemapSourceDefinitions.DataClearMode,
                FileSelectNativeLayout.DataModeClearDestination, prompt, promptDestination);
            if (confirmation)
                AddConfirmation(page);
            if (completed)
                Apply(page, Patch(FileSelectTilemapSourceDefinitions.DataCleared),
                    FileSelectNativeLayout.DataClearedDestination);
            return page;
        }

        var pages = new Dictionary<string, MapPresentationCell[]>(StringComparer.Ordinal)
        {
            [FileSelectPresentationFormat.BackgroundPageName] = Cells(
                RomDataReader.ReadFixedBank(CartridgeImportSource.Require(bus), MapSourceDefinitions.Menu.Background,
                    MapSourceDefinitions.Menu.TilemapBytes)),
            [FileSelectParts.PageName(FileSelectPage.MainWithData)] = Cells(mainWithData),
            [FileSelectParts.PageName(FileSelectPage.MainEmpty)] = Cells(mainEmpty),
            [FileSelectParts.PageName(FileSelectPage.CopySource)] = Cells(CopyPage(
                FileSelectTilemapSourceDefinitions.CopyWhichData, FileSelectNativeLayout.CopySourcePromptDestination)),
            [FileSelectParts.PageName(FileSelectPage.CopyDestination)] = Cells(CopyPage(
                FileSelectTilemapSourceDefinitions.CopySamusToWhere, FileSelectNativeLayout.CopyDestinationPromptDestination)),
            [FileSelectParts.PageName(FileSelectPage.CopyConfirm)] = Cells(CopyPage(
                FileSelectTilemapSourceDefinitions.CopySamusToSamus,
                FileSelectNativeLayout.CopyConfirmationPromptDestination, confirmation: true)),
            [FileSelectParts.PageName(FileSelectPage.CopyCompleted)] = Cells(CopyPage(
                FileSelectTilemapSourceDefinitions.CopySamusToSamus,
                FileSelectNativeLayout.CopyConfirmationPromptDestination, confirmation: true,
                completion: FileSelectTilemapSourceDefinitions.CopyCompleted)),
            [FileSelectParts.PageName(FileSelectPage.ClearSelection)] = Cells(ClearPage(
                FileSelectTilemapSourceDefinitions.ClearWhichData, FileSelectNativeLayout.ClearPromptDestination)),
            [FileSelectParts.PageName(FileSelectPage.ClearConfirm)] = Cells(ClearPage(
                FileSelectTilemapSourceDefinitions.ClearSamus, FileSelectNativeLayout.ClearPromptDestination,
                confirmation: true)),
            [FileSelectParts.PageName(FileSelectPage.ClearCompleted)] = Cells(ClearPage(
                FileSelectTilemapSourceDefinitions.ClearSamus, FileSelectNativeLayout.ClearPromptDestination,
                confirmation: true, completed: true)),
        };

        var patches = new Dictionary<string, FileSelectPatchDocument>(StringComparer.Ordinal)
        {
            [FileSelectParts.PatchName(FileSelectPatch.Energy)] = Document(Patch(FileSelectTilemapSourceDefinitions.Energy)),
            [FileSelectParts.PatchName(FileSelectPatch.NoData)] = Document(Patch(FileSelectTilemapSourceDefinitions.NoData)),
            [FileSelectParts.PatchName(FileSelectPatch.TimeColon)] = Document(Patch(FileSelectTilemapSourceDefinitions.TimeColon)),
        };
        var digits = Enumerable.Range(0, FileSelectPresentationDefinitions.DigitCount)
            .Select(value => Cell(BackgroundTileWords.ToWord(FileSelectFieldTiles.Digit(value))))
            .ToArray();
        Dictionary<string, MapPresentationCell> letters = SaveSlots.All.ToDictionary(slot => SaveSlotKeys.Installed.Of(slot),
            slot => Cell(BackgroundTileWords.ToWord(FileSelectFieldTiles.SlotLetter(slot))), StringComparer.Ordinal);

        var sprites = new Dictionary<string, SpriteVisualPart[]>(StringComparer.Ordinal)
        {
            [FileSelectParts.BorderSpriteName(FileSelectBorder.Main)] =
                MenuSpriteExtractor.Read(bus, FileSelectNativeLayout.NormalBorderSpritemap),
            [FileSelectParts.BorderSpriteName(FileSelectBorder.Copy)] =
                MenuSpriteExtractor.Read(bus, FileSelectNativeLayout.CopyBorderSpritemap),
            [FileSelectParts.BorderSpriteName(FileSelectBorder.Clear)] =
                MenuSpriteExtractor.Read(bus, FileSelectNativeLayout.ClearBorderSpritemap),
        };
        foreach (MenuMissileFrame frame in MenuMissileAnimationDefinitions.Roll)
            sprites.Add(MenuMissileFrames.Name(frame),
                MenuSpriteExtractor.Read(bus, MenuMissileSpritemaps.SpritemapId(frame)));
        foreach (FileSelectHelmetFrame frame in FileSelectHelmetAnimation.Turn)
            sprites.Add(FileSelectParts.HelmetSpriteName(frame),
                MenuSpriteExtractor.Read(bus, FileSelectHelmetSpritemaps.SpritemapId(frame)));

        using var artwork = new MemoryStream();
        FileSelectSourcePresentation.WriteArtwork(artwork, new()
        {
            Version = FileSelectPresentationFormat.ArtworkVersion,
            Pages = pages,
            Patches = patches,
            Digits = digits,
            SlotLetters = letters,
            Sprites = sprites,
            ObjectPalette = MenuRenderDefinitions.ObjectPalette,
        });
        using var output = new MemoryStream();
        FileSelectSourcePresentation.Write(output, new()
        {
            Version = FileSelectPresentationFormat.Version,
            // The native layout places the slots' fields one under another, in the slots' order.
            MainSlots = SaveSlots.All.Select((slot, place) => KeyValuePair.Create(SaveSlotKeys.Installed.Of(slot),
                Slot(FileSelectNativeLayout.MainSlotDestination(place, FileSelectSlotField.Energy),
                    FileSelectNativeLayout.MainSlotDestination(place, FileSelectSlotField.TimeValue)))).ToDictionary(StringComparer.Ordinal),
            DataSlots = SaveSlots.All.Select((slot, place) => KeyValuePair.Create(SaveSlotKeys.Installed.Of(slot),
                Slot(FileSelectNativeLayout.DataSlotDestination(place, FileSelectSlotField.Energy),
                    FileSelectNativeLayout.DataSlotDestination(place, FileSelectSlotField.TimeValue)))).ToDictionary(StringComparer.Ordinal),
            BorderAnchors = new(StringComparer.Ordinal)
            {
                [FileSelectPresentationFormat.BorderName(FileSelectBorder.Main)] = FileSelectNativeLayout.NormalBorderAnchor,
                [FileSelectPresentationFormat.BorderName(FileSelectBorder.Copy)] = FileSelectNativeLayout.CopyBorderAnchor,
                [FileSelectPresentationFormat.BorderName(FileSelectBorder.Clear)] = FileSelectNativeLayout.ClearBorderAnchor,
            },
            MainCursorAnchors = FileSelectItemRows.MainWithData.ToDictionary(row => FileSelectItemKeys.Installed.Of(row),
                row => new Offset16<ScreenPixels>(FileSelectCursorPositions.MainCursorX, FileSelectCursorPositions.MainSelectionY(row)), StringComparer.Ordinal),
            DataCursorAnchors = FileSelectItemRows.Data.ToDictionary(row => FileSelectItemKeys.Installed.Of(row),
                row => new Offset16<ScreenPixels>(FileSelectCursorPositions.DataCursorX, FileSelectCursorPositions.DataSelectionY(row)), StringComparer.Ordinal),
            ConfirmationCursorAnchors = new(StringComparer.Ordinal)
            {
                [YesNoChoiceKeys.Installed.Of(YesNoChoice.Yes)] = FileSelectCursorPositions.ConfirmationYesAnchor,
                [YesNoChoiceKeys.Installed.Of(YesNoChoice.No)] = FileSelectCursorPositions.ConfirmationNoAnchor,
            },
            HelmetAnchors = FileSelectCursorPositions.Slots.ToDictionary(slot => SaveSlotKeys.Installed.Of(slot),
                slot => new Offset16<ScreenPixels>(FileSelectCursorPositions.HelmetX, FileSelectCursorPositions.HelmetY(slot)), StringComparer.Ordinal),
            DynamicAnchors = new(StringComparer.Ordinal)
            {
                [FileSelectPresentationFormat.AnchorName(FileSelectAnchor.CopyDestinationSource)] =
                    Shown(FileSelectNativeLayout.CopyDestinationSourceLetterDestination),
                [FileSelectPresentationFormat.AnchorName(FileSelectAnchor.CopyConfirmSource)] =
                    Shown(FileSelectNativeLayout.CopyConfirmationSourceLetterDestination),
                [FileSelectPresentationFormat.AnchorName(FileSelectAnchor.CopyConfirmDestination)] =
                    Shown(FileSelectNativeLayout.CopyConfirmationDestinationLetterDestination),
                [FileSelectPresentationFormat.AnchorName(FileSelectAnchor.ClearConfirmSource)] =
                    Shown(FileSelectNativeLayout.ClearConfirmationSourceLetterDestination),
            },
            CursorFrameDuration = MenuAnimationTimings.MissileFrameDuration(bus),
            HelmetFrameDuration = MenuAnimationTimings.HelmetFrameDuration(bus),
        });
        return new FileSelectExtraction(output.ToArray(), artwork.ToArray());

        // Overlay the title, three slot labels/time fields, Exit, and copy/clear commands only for occupied menus.
        void AddMainStatic(ushort[] page, bool includeDataCommands)
        {
            Apply(page, Patch(FileSelectTilemapSourceDefinitions.SamusData), FileSelectNativeLayout.SamusDataDestination);
            AddSlotLabels(page, FileSelectNativeLayout.MainSlotDestination);
            if (includeDataCommands)
            {
                Apply(page, Patch(FileSelectTilemapSourceDefinitions.DataCopy), FileSelectNativeLayout.DataCopyDestination);
                Apply(page, Patch(FileSelectTilemapSourceDefinitions.DataClear), FileSelectNativeLayout.DataClearDestination);
            }
            Apply(page, Patch(FileSelectTilemapSourceDefinitions.Exit), FileSelectNativeLayout.ExitDestination);
        }

        // Start a copy/clear page with its mode title, prompt, Exit, and three data-slot label/time fields.
        ushort[] DataBase(int mode, int modeDestination, int prompt,
            int promptDestination)
        {
            ushort[] page = BlankPage();
            Apply(page, Patch(mode), modeDestination);
            Apply(page, Patch(prompt), promptDestination);
            Apply(page, Patch(FileSelectTilemapSourceDefinitions.Exit), FileSelectNativeLayout.ExitDestination);
            AddSlotLabels(page, FileSelectNativeLayout.DataSlotDestination);
            return page;
        }

        // Overlay each save slot's label and time label where <paramref name="destination"/> places the slot's fields.
        void AddSlotLabels(ushort[] page, Func<int, FileSelectSlotField, int> destination)
        {
            for (int slot = 0; slot < SaveSlots.All.Count; slot++)
            {
                Apply(page, Patch(FileSelectTilemapFormat.SlotLabel(slot)), destination(slot, FileSelectSlotField.Label));
                Apply(page, Patch(FileSelectTilemapSourceDefinitions.Time), destination(slot, FileSelectSlotField.TimeLabel));
            }
        }

        // Overlay the shared question and YES/NO choices on a copy or clear page.
        void AddConfirmation(ushort[] page)
        {
            Apply(page, Patch(FileSelectTilemapSourceDefinitions.IsThisOkay),
                FileSelectNativeLayout.ConfirmationQuestionDestination);
            Apply(page, Patch(FileSelectTilemapSourceDefinitions.Yes), FileSelectNativeLayout.ConfirmationYesDestination);
            Apply(page, Patch(FileSelectTilemapSourceDefinitions.No), FileSelectNativeLayout.ConfirmationNoDestination);
        }
    }

    /// <summary>Decodes a bank-local text stream into relative cells, honoring row breaks until its end marker.</summary>
    private static RawPatch ReadPatch(ISnesAddressSpace bus, int pointer)
    {
        var cells = new List<RawPatchCell>();
        int address = FileSelectTilemapFormat.Bank | pointer;
        int x = 0, y = 0;
        while (true)
        {
            int word = RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus), address);
            address = FileSelectTilemapFormat.Bank | (SnesAddressMath.OffsetInBank(address + 2));
            if (FileSelectTextControls.Of(word) is { } control)
            {
                switch (control)
                {
                    case FileSelectTextControl.End:
                        return new(cells.ToArray());
                    case FileSelectTextControl.NextRow:
                        x = 0;
                        y++;
                        continue;
                    default:
                        throw new InvalidOperationException($"Undefined {nameof(FileSelectTextControl)} {word}.");
                }
            }
            cells.Add(new(new(checked((short)(x++)), checked((short)y)), new SnesBgTilemapWord(checked((ushort)word))));
        }
    }

    /// <summary>Places a relative text patch at a tilemap byte anchor and rejects cells escaping the page.</summary>
    private static void Apply(ushort[] page, RawPatch patch, int byteOffset)
    {
        Position16<CharacterCells> anchor = Point(byteOffset);
        foreach (RawPatchCell cell in patch.Cells)
        {
            int index = FileSelectPresentationFormat.PageSize.RowMajorIndex(anchor + cell.Offset);
            if ((uint)index >= page.Length)
                throw new InvalidDataException("Extracted file-select text escaped its page.");
            page[index] = cell.Word.Raw;
        }
    }

    /// <summary>Fills a complete file-select page with the cartridge's blank tile word.</summary>
    private static ushort[] BlankPage()
    {
        var page = new ushort[FileSelectPresentationFormat.CellCount];
        Array.Fill(page, BackgroundTileWords.ToWord(FileSelectFieldTileDefinitions.BlankTile));
        return page;
    }

    /// <summary>Derives one slot's energy, health, no-data, and time anchors from native tilemap offsets.</summary>
    private static FileSelectSlotFieldDocument Slot(int energyOffset, int timeOffset) => new()
    {
        EnergyAnchor = Shown(energyOffset),
        HealthAnchor = Shown(energyOffset + FileSelectNativeLayout.NextTilemapRowByteOffset + sizeof(ushort)),
        NoDataAnchor = Shown(energyOffset + FileSelectNativeLayout.NextTilemapRowByteOffset),
        TimeValueAnchor = Shown(timeOffset),
    };

    /// <summary>Converts a decoded text patch to editable tile cells and relative coordinates.</summary>
    private static FileSelectPatchDocument Document(RawPatch patch) => new()
    {
        Cells = patch.Cells.Select(cell => new FileSelectPatchCellDocument
        {
            X = cell.Offset.X,
            Y = cell.Offset.Y,
            Cell = Cell(cell.Word.Raw),
        }).ToArray(),
    };

    /// <summary>Reads complete little-endian tile words from a native page before converting their visual fields.</summary>
    private static MapPresentationCell[] Cells(ReadOnlySpan<byte> bytes)
    {
        if (bytes.Length % sizeof(ushort) != 0)
            throw new InvalidDataException("File-select page contains a partial word.");
        var words = new ushort[bytes.Length / sizeof(ushort)];
        for (int index = 0; index < words.Length; index++)
            words[index] = unchecked(LittleEndianBytes.Word(bytes[index * 2], bytes[index * 2 + 1]));
        return Cells(words);
    }

    /// <summary>Converts native tilemap words into editable file-select cell descriptors in row-major order.</summary>
    private static MapPresentationCell[] Cells(ReadOnlySpan<ushort> words)
    {
        var cells = new MapPresentationCell[words.Length];
        for (int index = 0; index < cells.Length; index++)
            cells[index] = Cell(words[index]);
        return cells;
    }

    /// <summary>Splits one SNES BG tile word into atlas position, palette, priority, and flip fields.</summary>
    private static MapPresentationCell Cell(int raw) =>
        MapPresentationCell.Of(BackgroundTileWords.ToTile(checked((ushort)raw)), FileSelectPresentationFormat.CharacterColumns);

    /// <summary>Maps a tilemap byte offset to column and row within the file-select page width.</summary>
    private static Position16<CharacterCells> Point(int byteOffset)
    {
        int cell = byteOffset / sizeof(ushort);
        return new(checked((ushort)(cell % FileSelectPresentationFormat.PageColumns)),
            checked((ushort)(cell / FileSelectPresentationFormat.PageColumns)));
    }

    /// <summary>The page pixel the game shows a field at whose tilemap byte offset is <paramref name="byteOffset"/>.</summary>
    private static Position16<PlanePixels> Shown(int byteOffset) => Point(byteOffset).CornerPixel<PlanePixels>();

    /// <summary>Decoded relative tile cells from one native file-select text patch.</summary>
    private sealed record RawPatch(RawPatchCell[] Cells);
    /// <summary>One patch tile word and its row/column offset from the destination anchor.</summary>
    private readonly record struct RawPatchCell(Offset16<CharacterCells> Offset, SnesBgTilemapWord Word);
}
