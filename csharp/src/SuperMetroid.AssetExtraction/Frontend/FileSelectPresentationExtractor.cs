using SuperMetroid.Core;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Spatial;
using SuperMetroid.Core.Frontend.FileSelect;
using SuperMetroid.Core.Frontend.Menus;
using SuperMetroid.Core.Frontend.FileSelect.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Map.Artwork;
using SuperMetroid.AssetExtraction.CartridgeTables;

namespace SuperMetroid.AssetExtraction.Frontend;

/// <summary>
/// Imports file-select page templates, dynamic slot-field artwork and menu actors.
/// Save validation and copy/clear behavior are deliberately not asset data.
/// </summary>
public static class FileSelectPresentationExtractor
{
    /// <summary>Builds file-select page templates and imports dynamic slot artwork, borders, cursor frames, and helmet frames.</summary>
    /// <param name="bus">Non-null cartridge import address space supplying native menu tilemaps, text patches, and spritemaps.</param>
    /// <returns>New UTF-8 JSON bytes containing pages, field patches, digit and slot-letter cells, tile-cell anchors, pixel-space sprite anchors, and frame durations.</returns>
    /// <remarks>Includes main, copy, and clear presentation variants without importing save contents, validation, or copy/clear control flow.</remarks>
    /// <exception cref="ArgumentNullException"><paramref name="bus"/> is null.</exception>
    /// <exception cref="InvalidDataException">Imported artwork escapes its page or fails presentation or spritemap validation.</exception>
    public static byte[] Extract(ISnesAddressSpace bus)
    {
        Ensure.NotNull(bus);
        var rawPatches = new Dictionary<ushort, RawPatch>();
        // Cache a decoded native text patch by its bank-local pointer across page variants.
        RawPatch Patch(ushort pointer)
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
        ushort[] CopyPage(ushort prompt, int promptDestination,
            bool confirmation = false, ushort? completion = null)
        {
            ushort[] page = DataBase(FileSelectTilemapSourceDefinitions.DataCopyMode,
                FileSelectNativeLayout.DataModeCopyDestination, prompt, promptDestination);
            if (confirmation)
                AddConfirmation(page);
            if (completion is ushort completed)
                Apply(page, Patch(completed), FileSelectNativeLayout.CopyCompletedDestination);
            return page;
        }

        // Build a clear-mode page with its prompt, optional confirmation, and optional cleared-result text.
        ushort[] ClearPage(ushort prompt, int promptDestination,
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
            [FileSelectPresentationDefinitions.BackgroundPageName] = Cells(
                RomDataReader.ReadFixedBank(CartridgeImportSource.Require(bus), MapSourceDefinitions.Menu.Background,
                    MapSourceDefinitions.Menu.TilemapBytes)),
            [FileSelectPresentationDefinitions.PageName(FileSelectPage.MainWithData)] = Cells(mainWithData),
            [FileSelectPresentationDefinitions.PageName(FileSelectPage.MainEmpty)] = Cells(mainEmpty),
            [FileSelectPresentationDefinitions.PageName(FileSelectPage.CopySource)] = Cells(CopyPage(
                FileSelectTilemapSourceDefinitions.CopyWhichData, FileSelectNativeLayout.CopySourcePromptDestination)),
            [FileSelectPresentationDefinitions.PageName(FileSelectPage.CopyDestination)] = Cells(CopyPage(
                FileSelectTilemapSourceDefinitions.CopySamusToWhere, FileSelectNativeLayout.CopyDestinationPromptDestination)),
            [FileSelectPresentationDefinitions.PageName(FileSelectPage.CopyConfirm)] = Cells(CopyPage(
                FileSelectTilemapSourceDefinitions.CopySamusToSamus,
                FileSelectNativeLayout.CopyConfirmationPromptDestination, confirmation: true)),
            [FileSelectPresentationDefinitions.PageName(FileSelectPage.CopyCompleted)] = Cells(CopyPage(
                FileSelectTilemapSourceDefinitions.CopySamusToSamus,
                FileSelectNativeLayout.CopyConfirmationPromptDestination, confirmation: true,
                completion: FileSelectTilemapSourceDefinitions.CopyCompleted)),
            [FileSelectPresentationDefinitions.PageName(FileSelectPage.ClearSelection)] = Cells(ClearPage(
                FileSelectTilemapSourceDefinitions.ClearWhichData, FileSelectNativeLayout.ClearPromptDestination)),
            [FileSelectPresentationDefinitions.PageName(FileSelectPage.ClearConfirm)] = Cells(ClearPage(
                FileSelectTilemapSourceDefinitions.ClearSamus, FileSelectNativeLayout.ClearPromptDestination,
                confirmation: true)),
            [FileSelectPresentationDefinitions.PageName(FileSelectPage.ClearCompleted)] = Cells(ClearPage(
                FileSelectTilemapSourceDefinitions.ClearSamus, FileSelectNativeLayout.ClearPromptDestination,
                confirmation: true, completed: true)),
        };

        var patches = new Dictionary<string, FileSelectPatchDocument>(StringComparer.Ordinal)
        {
            [FileSelectPresentationDefinitions.PatchName(FileSelectPatch.Energy)] = Document(Patch(FileSelectTilemapSourceDefinitions.Energy)),
            [FileSelectPresentationDefinitions.PatchName(FileSelectPatch.NoData)] = Document(Patch(FileSelectTilemapSourceDefinitions.NoData)),
            [FileSelectPresentationDefinitions.PatchName(FileSelectPatch.TimeColon)] = Document(Patch(FileSelectTilemapSourceDefinitions.TimeColon)),
        };
        var digits = Enumerable.Range(0, 10)
            .Select(value => Cell(unchecked((ushort)(FileSelectLayout.DigitTileBase + value))))
            .ToArray();
        var letters = Enumerable.Range(0, 3)
            .Select(value => Cell(unchecked((ushort)(FileSelectLayout.SamusLetterTileBase + value))))
            .ToArray();

        var sprites = new Dictionary<string, SpriteVisualPart[]>(StringComparer.Ordinal)
        {
            [FileSelectPresentationDefinitions.BorderSpriteName(FileSelectBorder.Main)] =
                MenuSpriteExtractor.Read(bus, FileSelectNativeLayout.NormalBorderSpritemap),
            [FileSelectPresentationDefinitions.BorderSpriteName(FileSelectBorder.Copy)] =
                MenuSpriteExtractor.Read(bus, FileSelectNativeLayout.CopyBorderSpritemap),
            [FileSelectPresentationDefinitions.BorderSpriteName(FileSelectBorder.Clear)] =
                MenuSpriteExtractor.Read(bus, FileSelectNativeLayout.ClearBorderSpritemap),
        };
        for (int frame = 0; frame < MenuMissileAnimationDefinitions.FrameCount; frame++)
            sprites.Add(FileSelectPresentationDefinitions.CursorSpriteName(frame),
                MenuSpriteExtractor.Read(bus, MenuMissileAnimationDefinitions.SpritemapId(frame)));
        for (int frame = 0; frame < FileSelectHelmetAnimation.FrameCount; frame++)
            sprites.Add(FileSelectPresentationDefinitions.HelmetSpriteName(frame),
                MenuSpriteExtractor.Read(bus, FileSelectHelmetAnimation.SpritemapId(frame)));

        using var output = new MemoryStream();
        FileSelectPresentation.Write(output, new()
        {
            Version = FileSelectPresentationDefinitions.Version,
            Pages = pages,
            Patches = patches,
            Digits = digits,
            SlotLetters = letters,
            MainSlots =
            [
                Slot(FileSelectNativeLayout.MainSlotDestination(0, FileSelectSlotField.Energy),
                    FileSelectNativeLayout.MainSlotDestination(0, FileSelectSlotField.TimeValue)),
                Slot(FileSelectNativeLayout.MainSlotDestination(1, FileSelectSlotField.Energy),
                    FileSelectNativeLayout.MainSlotDestination(1, FileSelectSlotField.TimeValue)),
                Slot(FileSelectNativeLayout.MainSlotDestination(2, FileSelectSlotField.Energy),
                    FileSelectNativeLayout.MainSlotDestination(2, FileSelectSlotField.TimeValue)),
            ],
            DataSlots =
            [
                Slot(FileSelectNativeLayout.DataSlotDestination(0, FileSelectSlotField.Energy),
                    FileSelectNativeLayout.DataSlotDestination(0, FileSelectSlotField.TimeValue)),
                Slot(FileSelectNativeLayout.DataSlotDestination(1, FileSelectSlotField.Energy),
                    FileSelectNativeLayout.DataSlotDestination(1, FileSelectSlotField.TimeValue)),
                Slot(FileSelectNativeLayout.DataSlotDestination(2, FileSelectSlotField.Energy),
                    FileSelectNativeLayout.DataSlotDestination(2, FileSelectSlotField.TimeValue)),
            ],
            Sprites = sprites,
            BorderAnchors = new(StringComparer.Ordinal)
            {
                [FileSelectPresentationDefinitions.BorderName(FileSelectBorder.Main)] = new(128, 16),
                [FileSelectPresentationDefinitions.BorderName(FileSelectBorder.Copy)] = new(128, 16),
                [FileSelectPresentationDefinitions.BorderName(FileSelectBorder.Clear)] = new(124, 16),
            },
            MainCursorAnchors = Enumerable.Range(0, FileSelectLayout.MainSelectionCount)
                .Select(FileSelectLayout.MainSelectionY)
                .Select(y => new Position16<SpritePixels>(14, y)).ToArray(),
            DataCursorAnchors = Enumerable.Range(0, FileSelectLayout.DataSelectionCount)
                .Select(FileSelectLayout.DataSelectionY)
                .Select(y => new Position16<SpritePixels>(22, y)).ToArray(),
            ConfirmationCursorAnchors =
                [new(94, 184), new(94, 208)],
            HelmetAnchors = Enumerable.Range(0, FileSelectLayout.SaveSlotCount)
                .Select(FileSelectLayout.HelmetY)
                .Select(y => new Position16<SpritePixels>(100, y)).ToArray(),
            DynamicAnchors = new(StringComparer.Ordinal)
            {
                [FileSelectPresentationDefinitions.AnchorName(FileSelectAnchor.CopyDestinationSource)] =
                    Point(FileSelectNativeLayout.CopyDestinationSourceLetterDestination),
                [FileSelectPresentationDefinitions.AnchorName(FileSelectAnchor.CopyConfirmSource)] =
                    Point(FileSelectNativeLayout.CopyConfirmationSourceLetterDestination),
                [FileSelectPresentationDefinitions.AnchorName(FileSelectAnchor.CopyConfirmDestination)] =
                    Point(FileSelectNativeLayout.CopyConfirmationDestinationLetterDestination),
                [FileSelectPresentationDefinitions.AnchorName(FileSelectAnchor.ClearConfirmSource)] =
                    Point(FileSelectNativeLayout.ClearConfirmationSourceLetterDestination),
            },
            ObjectPalette = MenuRenderDefinitions.ObjectPaletteBits >> 9,
            CursorFrameDuration = MenuMissileAnimationDefinitions.FrameDuration,
            HelmetFrameDuration = FileSelectHelmetAnimation.FrameDuration,
        });
        return output.ToArray();

        // Overlay the title, three slot labels/time fields, Exit, and copy/clear commands only for occupied menus.
        void AddMainStatic(ushort[] page, bool includeDataCommands)
        {
            Apply(page, Patch(FileSelectTilemapSourceDefinitions.SamusData), FileSelectNativeLayout.SamusDataDestination);
            Apply(page, Patch(FileSelectTilemapFormat.SlotLabel(0)), FileSelectNativeLayout.MainSlotDestination(0, FileSelectSlotField.Label));
            Apply(page, Patch(FileSelectTilemapSourceDefinitions.Time), FileSelectNativeLayout.MainSlotDestination(0, FileSelectSlotField.TimeLabel));
            Apply(page, Patch(FileSelectTilemapFormat.SlotLabel(1)), FileSelectNativeLayout.MainSlotDestination(1, FileSelectSlotField.Label));
            Apply(page, Patch(FileSelectTilemapSourceDefinitions.Time), FileSelectNativeLayout.MainSlotDestination(1, FileSelectSlotField.TimeLabel));
            Apply(page, Patch(FileSelectTilemapFormat.SlotLabel(2)), FileSelectNativeLayout.MainSlotDestination(2, FileSelectSlotField.Label));
            Apply(page, Patch(FileSelectTilemapSourceDefinitions.Time), FileSelectNativeLayout.MainSlotDestination(2, FileSelectSlotField.TimeLabel));
            if (includeDataCommands)
            {
                Apply(page, Patch(FileSelectTilemapSourceDefinitions.DataCopy), FileSelectNativeLayout.DataCopyDestination);
                Apply(page, Patch(FileSelectTilemapSourceDefinitions.DataClear), FileSelectNativeLayout.DataClearDestination);
            }
            Apply(page, Patch(FileSelectTilemapSourceDefinitions.Exit), FileSelectNativeLayout.ExitDestination);
        }

        // Start a copy/clear page with its mode title, prompt, Exit, and three data-slot label/time fields.
        ushort[] DataBase(ushort mode, int modeDestination, ushort prompt,
            int promptDestination)
        {
            ushort[] page = BlankPage();
            Apply(page, Patch(mode), modeDestination);
            Apply(page, Patch(prompt), promptDestination);
            Apply(page, Patch(FileSelectTilemapSourceDefinitions.Exit), FileSelectNativeLayout.ExitDestination);
            Apply(page, Patch(FileSelectTilemapFormat.SlotLabel(0)), FileSelectNativeLayout.DataSlotDestination(0, FileSelectSlotField.Label));
            Apply(page, Patch(FileSelectTilemapSourceDefinitions.Time), FileSelectNativeLayout.DataSlotDestination(0, FileSelectSlotField.TimeLabel));
            Apply(page, Patch(FileSelectTilemapFormat.SlotLabel(1)), FileSelectNativeLayout.DataSlotDestination(1, FileSelectSlotField.Label));
            Apply(page, Patch(FileSelectTilemapSourceDefinitions.Time), FileSelectNativeLayout.DataSlotDestination(1, FileSelectSlotField.TimeLabel));
            Apply(page, Patch(FileSelectTilemapFormat.SlotLabel(2)), FileSelectNativeLayout.DataSlotDestination(2, FileSelectSlotField.Label));
            Apply(page, Patch(FileSelectTilemapSourceDefinitions.Time), FileSelectNativeLayout.DataSlotDestination(2, FileSelectSlotField.TimeLabel));
            return page;
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
    private static RawPatch ReadPatch(ISnesAddressSpace bus, ushort pointer)
    {
        var cells = new List<RawPatchCell>();
        int address = FileSelectTilemapFormat.Bank | pointer;
        int x = 0, y = 0;
        while (true)
        {
            ushort word = RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus), address);
            address = FileSelectTilemapFormat.Bank | (SnesAddressMath.OffsetInBank(address + 2));
            if (Enum.IsDefined((FileSelectTextControl)word))
            {
                switch ((FileSelectTextControl)word)
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
            cells.Add(new(new(checked((short)(x++)), checked((short)y)), new SnesBgTilemapWord(word)));
        }
    }

    /// <summary>Places a relative text patch at a tilemap byte anchor and rejects cells escaping the page.</summary>
    private static void Apply(ushort[] page, RawPatch patch, int byteOffset)
    {
        Position16<TilemapCells> anchor = Point(byteOffset);
        foreach (RawPatchCell cell in patch.Cells)
        {
            int index = FileSelectPresentationDefinitions.PageSize.RowMajorIndex(anchor + cell.Offset);
            if ((uint)index >= page.Length)
                throw new InvalidDataException("Extracted file-select text escaped its page.");
            page[index] = cell.Word.Raw;
        }
    }

    /// <summary>Fills a complete file-select page with the cartridge's blank tile word.</summary>
    private static ushort[] BlankPage()
    {
        var page = new ushort[FileSelectPresentationDefinitions.CellCount];
        Array.Fill(page, FileSelectLayout.BlankTile);
        return page;
    }

    /// <summary>Derives one slot's energy, health, no-data, and time anchors from native tilemap offsets.</summary>
    private static FileSelectSlotFieldDocument Slot(int energyOffset, int timeOffset) => new()
    {
        EnergyAnchor = Point(energyOffset),
        HealthAnchor = Point(energyOffset + FileSelectNativeLayout.NextTilemapRowByteOffset + sizeof(ushort)),
        NoDataAnchor = Point(energyOffset + FileSelectNativeLayout.NextTilemapRowByteOffset),
        TimeValueAnchor = Point(timeOffset),
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
            words[index] = unchecked(SnesLittleEndian.Word(bytes[index * 2], bytes[index * 2 + 1]));
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
    private static MapPresentationCell Cell(ushort raw)
    {
        var word = new SnesBgTilemapWord(raw);
        return new()
        {
            TileColumn = word.CharacterIndex % FileSelectPresentationDefinitions.CharacterColumns,
            TileRow = word.CharacterIndex / FileSelectPresentationDefinitions.CharacterColumns,
            Palette = word.PaletteIndex,
            Priority = word.HasPriority,
            FlipX = word.FlipHorizontally,
            FlipY = word.FlipVertically,
        };
    }

    /// <summary>Maps a tilemap byte offset to column and row within the file-select page width.</summary>
    private static Position16<TilemapCells> Point(int byteOffset)
    {
        int cell = byteOffset / sizeof(ushort);
        return new(checked((ushort)(cell % FileSelectPresentationDefinitions.Width)),
            checked((ushort)(cell / FileSelectPresentationDefinitions.Width)));
    }

    /// <summary>Decoded relative tile cells from one native file-select text patch.</summary>
    private sealed record RawPatch(RawPatchCell[] Cells);
    /// <summary>One patch tile word and its row/column offset from the destination anchor.</summary>
    private readonly record struct RawPatchCell(Offset16<TilemapCells> Offset, SnesBgTilemapWord Word);
}
