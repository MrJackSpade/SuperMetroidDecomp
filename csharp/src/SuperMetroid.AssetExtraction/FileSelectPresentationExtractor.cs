using SuperMetroid.Core;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rom;
using SuperMetroid.Core.Spatial;
using SuperMetroid.Core.Frontend.FileSelect;
using SuperMetroid.Core.Frontend.Menus;
using SuperMetroid.Core.Frontend.FileSelect.Artwork;
using SuperMetroid.Core.Frontend.Map.Artwork;

namespace SuperMetroid.AssetExtraction;

/// <summary>
/// Imports file-select page templates, dynamic slot-field artwork and menu actors.
/// Save validation and copy/clear behavior are deliberately not asset data.
/// </summary>
[CpuBusBoundary("codec: ROM file-select page templates, slot-field artwork and menu actors → file-select presentation")]
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
            ushort[] page = DataBase(FileSelectTilemaps.DataCopyMode,
                FileSelectLayout.DataModeCopyDestination, prompt, promptDestination);
            if (confirmation)
                AddConfirmation(page);
            if (completion is ushort completed)
                Apply(page, Patch(completed), FileSelectLayout.CopyCompletedDestination);
            return page;
        }

        // Build a clear-mode page with its prompt, optional confirmation, and optional cleared-result text.
        ushort[] ClearPage(ushort prompt, int promptDestination,
            bool confirmation = false, bool completed = false)
        {
            ushort[] page = DataBase(FileSelectTilemaps.DataClearMode,
                FileSelectLayout.DataModeClearDestination, prompt, promptDestination);
            if (confirmation)
                AddConfirmation(page);
            if (completed)
                Apply(page, Patch(FileSelectTilemaps.DataCleared),
                    FileSelectLayout.DataClearedDestination);
            return page;
        }

        var pages = new Dictionary<string, MapPresentationCell[]>(StringComparer.Ordinal)
        {
            [FileSelectPresentationDefinitions.BackgroundPage] = Cells(
                RomDataReader.ReadFixedBank(CartridgeImportSource.Require(bus), FileSelectMapRomData.InitialMenuBackground,
                    FileSelectMapRomData.TilemapBytes)),
            [FileSelectPresentationDefinitions.MainWithDataPage] = Cells(mainWithData),
            [FileSelectPresentationDefinitions.MainEmptyPage] = Cells(mainEmpty),
            [FileSelectPresentationDefinitions.CopySourcePage] = Cells(CopyPage(
                FileSelectTilemaps.CopyWhichData, FileSelectLayout.CopySourcePromptDestination)),
            [FileSelectPresentationDefinitions.CopyDestinationPage] = Cells(CopyPage(
                FileSelectTilemaps.CopySamusToWhere, FileSelectLayout.CopyDestinationPromptDestination)),
            [FileSelectPresentationDefinitions.CopyConfirmPage] = Cells(CopyPage(
                FileSelectTilemaps.CopySamusToSamus,
                FileSelectLayout.CopyConfirmationPromptDestination, confirmation: true)),
            [FileSelectPresentationDefinitions.CopyCompletedPage] = Cells(CopyPage(
                FileSelectTilemaps.CopySamusToSamus,
                FileSelectLayout.CopyConfirmationPromptDestination, confirmation: true,
                completion: FileSelectTilemaps.CopyCompleted)),
            [FileSelectPresentationDefinitions.ClearSelectionPage] = Cells(ClearPage(
                FileSelectTilemaps.ClearWhichData, FileSelectLayout.ClearPromptDestination)),
            [FileSelectPresentationDefinitions.ClearConfirmPage] = Cells(ClearPage(
                FileSelectTilemaps.ClearSamus, FileSelectLayout.ClearPromptDestination,
                confirmation: true)),
            [FileSelectPresentationDefinitions.ClearCompletedPage] = Cells(ClearPage(
                FileSelectTilemaps.ClearSamus, FileSelectLayout.ClearPromptDestination,
                confirmation: true, completed: true)),
        };

        var patches = new Dictionary<string, FileSelectPatchDocument>(StringComparer.Ordinal)
        {
            [FileSelectPresentationDefinitions.EnergyPatch] = Document(Patch(FileSelectTilemaps.Energy)),
            [FileSelectPresentationDefinitions.NoDataPatch] = Document(Patch(FileSelectTilemaps.NoData)),
            [FileSelectPresentationDefinitions.TimeColonPatch] = Document(Patch(FileSelectTilemaps.TimeColon)),
        };
        var digits = Enumerable.Range(0, 10)
            .Select(value => Cell(unchecked((ushort)(FileSelectLayout.DigitTileBase + value))))
            .ToArray();
        var letters = Enumerable.Range(0, 3)
            .Select(value => Cell(unchecked((ushort)(FileSelectLayout.SamusLetterTileBase + value))))
            .ToArray();

        var sprites = new Dictionary<string, SpriteVisualPart[]>(StringComparer.Ordinal)
        {
            [FileSelectPresentationDefinitions.BorderFrameName(FileSelectPresentationDefinitions.MainBorder)] =
                MenuSpriteExtractor.Read(bus, FileSelectLayout.NormalBorderSpritemap),
            [FileSelectPresentationDefinitions.BorderFrameName(FileSelectPresentationDefinitions.CopyBorder)] =
                MenuSpriteExtractor.Read(bus, FileSelectLayout.CopyBorderSpritemap),
            [FileSelectPresentationDefinitions.BorderFrameName(FileSelectPresentationDefinitions.ClearBorder)] =
                MenuSpriteExtractor.Read(bus, FileSelectLayout.ClearBorderSpritemap),
        };
        for (int frame = 0; frame < MenuMissileAnimationDefinitions.FrameCount; frame++)
            sprites.Add(FileSelectPresentationDefinitions.CursorFrameName(frame),
                MenuSpriteExtractor.Read(bus, MenuMissileAnimationDefinitions.SpritemapId(frame)));
        for (int frame = 0; frame < FileSelectHelmetAnimation.FrameCount; frame++)
            sprites.Add(FileSelectPresentationDefinitions.HelmetFrameName(frame),
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
                Slot(FileSelectLayout.MainSlotDestination(0, FileSelectSlotField.Energy),
                    FileSelectLayout.MainSlotDestination(0, FileSelectSlotField.TimeValue)),
                Slot(FileSelectLayout.MainSlotDestination(1, FileSelectSlotField.Energy),
                    FileSelectLayout.MainSlotDestination(1, FileSelectSlotField.TimeValue)),
                Slot(FileSelectLayout.MainSlotDestination(2, FileSelectSlotField.Energy),
                    FileSelectLayout.MainSlotDestination(2, FileSelectSlotField.TimeValue)),
            ],
            DataSlots =
            [
                Slot(FileSelectLayout.DataSlotDestination(0, FileSelectSlotField.Energy),
                    FileSelectLayout.DataSlotDestination(0, FileSelectSlotField.TimeValue)),
                Slot(FileSelectLayout.DataSlotDestination(1, FileSelectSlotField.Energy),
                    FileSelectLayout.DataSlotDestination(1, FileSelectSlotField.TimeValue)),
                Slot(FileSelectLayout.DataSlotDestination(2, FileSelectSlotField.Energy),
                    FileSelectLayout.DataSlotDestination(2, FileSelectSlotField.TimeValue)),
            ],
            Sprites = sprites,
            BorderAnchors = new(StringComparer.Ordinal)
            {
                [FileSelectPresentationDefinitions.MainBorder] = new(128, 16),
                [FileSelectPresentationDefinitions.CopyBorder] = new(128, 16),
                [FileSelectPresentationDefinitions.ClearBorder] = new(124, 16),
            },
            MainCursorAnchors = Enumerable.Range(0, FileSelectLayout.MainSelectionCount)
                .Select(FileSelectLayout.MainSelectionY)
                .Select(y => new Position16<OamPixels>(14, y)).ToArray(),
            DataCursorAnchors = Enumerable.Range(0, FileSelectLayout.DataSelectionCount)
                .Select(FileSelectLayout.DataSelectionY)
                .Select(y => new Position16<OamPixels>(22, y)).ToArray(),
            ConfirmationCursorAnchors =
                [new(94, 184), new(94, 208)],
            HelmetAnchors = Enumerable.Range(0, FileSelectLayout.SaveSlotCount)
                .Select(FileSelectLayout.HelmetY)
                .Select(y => new Position16<OamPixels>(100, y)).ToArray(),
            DynamicAnchors = new(StringComparer.Ordinal)
            {
                [FileSelectPresentationDefinitions.CopyDestinationSourceAnchor] =
                    Point(FileSelectLayout.CopyDestinationSourceLetterDestination),
                [FileSelectPresentationDefinitions.CopyConfirmSourceAnchor] =
                    Point(FileSelectLayout.CopyConfirmationSourceLetterDestination),
                [FileSelectPresentationDefinitions.CopyConfirmDestinationAnchor] =
                    Point(FileSelectLayout.CopyConfirmationDestinationLetterDestination),
                [FileSelectPresentationDefinitions.ClearConfirmSourceAnchor] =
                    Point(FileSelectLayout.ClearConfirmationSourceLetterDestination),
            },
            ObjectPalette = MenuPpuState.ObjectPaletteBits >> 9,
            CursorFrameDuration = MenuMissileAnimationDefinitions.FrameDuration,
            HelmetFrameDuration = FileSelectHelmetAnimation.FrameDuration,
        });
        return output.ToArray();

        // Overlay the title, three slot labels/time fields, Exit, and copy/clear commands only for occupied menus.
        void AddMainStatic(ushort[] page, bool includeDataCommands)
        {
            Apply(page, Patch(FileSelectTilemaps.SamusData), FileSelectLayout.SamusDataDestination);
            Apply(page, Patch(FileSelectTilemaps.SlotLabel(0)), FileSelectLayout.MainSlotDestination(0, FileSelectSlotField.Label));
            Apply(page, Patch(FileSelectTilemaps.Time), FileSelectLayout.MainSlotDestination(0, FileSelectSlotField.TimeLabel));
            Apply(page, Patch(FileSelectTilemaps.SlotLabel(1)), FileSelectLayout.MainSlotDestination(1, FileSelectSlotField.Label));
            Apply(page, Patch(FileSelectTilemaps.Time), FileSelectLayout.MainSlotDestination(1, FileSelectSlotField.TimeLabel));
            Apply(page, Patch(FileSelectTilemaps.SlotLabel(2)), FileSelectLayout.MainSlotDestination(2, FileSelectSlotField.Label));
            Apply(page, Patch(FileSelectTilemaps.Time), FileSelectLayout.MainSlotDestination(2, FileSelectSlotField.TimeLabel));
            if (includeDataCommands)
            {
                Apply(page, Patch(FileSelectTilemaps.DataCopy), FileSelectLayout.DataCopyDestination);
                Apply(page, Patch(FileSelectTilemaps.DataClear), FileSelectLayout.DataClearDestination);
            }
            Apply(page, Patch(FileSelectTilemaps.Exit), FileSelectLayout.ExitDestination);
        }

        // Start a copy/clear page with its mode title, prompt, Exit, and three data-slot label/time fields.
        ushort[] DataBase(ushort mode, int modeDestination, ushort prompt,
            int promptDestination)
        {
            ushort[] page = BlankPage();
            Apply(page, Patch(mode), modeDestination);
            Apply(page, Patch(prompt), promptDestination);
            Apply(page, Patch(FileSelectTilemaps.Exit), FileSelectLayout.ExitDestination);
            Apply(page, Patch(FileSelectTilemaps.SlotLabel(0)), FileSelectLayout.DataSlotDestination(0, FileSelectSlotField.Label));
            Apply(page, Patch(FileSelectTilemaps.Time), FileSelectLayout.DataSlotDestination(0, FileSelectSlotField.TimeLabel));
            Apply(page, Patch(FileSelectTilemaps.SlotLabel(1)), FileSelectLayout.DataSlotDestination(1, FileSelectSlotField.Label));
            Apply(page, Patch(FileSelectTilemaps.Time), FileSelectLayout.DataSlotDestination(1, FileSelectSlotField.TimeLabel));
            Apply(page, Patch(FileSelectTilemaps.SlotLabel(2)), FileSelectLayout.DataSlotDestination(2, FileSelectSlotField.Label));
            Apply(page, Patch(FileSelectTilemaps.Time), FileSelectLayout.DataSlotDestination(2, FileSelectSlotField.TimeLabel));
            return page;
        }

        // Overlay the shared question and YES/NO choices on a copy or clear page.
        void AddConfirmation(ushort[] page)
        {
            Apply(page, Patch(FileSelectTilemaps.IsThisOkay),
                FileSelectLayout.ConfirmationQuestionDestination);
            Apply(page, Patch(FileSelectTilemaps.Yes), FileSelectLayout.ConfirmationYesDestination);
            Apply(page, Patch(FileSelectTilemaps.No), FileSelectLayout.ConfirmationNoDestination);
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
            if (word == FileSelectTilemapFormat.End)
                return new(cells.ToArray());
            if (word == FileSelectTilemapFormat.NextRow)
            {
                x = 0;
                y++;
                continue;
            }
            cells.Add(new(new(checked((short)(x++)), checked((short)y)), word));
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
            page[index] = cell.Word;
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
        HealthAnchor = Point(energyOffset + 0x42),
        NoDataAnchor = Point(energyOffset + FileSelectLayout.NextTilemapRowByteOffset),
        TimeValueAnchor = Point(timeOffset),
    };

    /// <summary>Converts a decoded text patch to editable tile cells and relative coordinates.</summary>
    private static FileSelectPatchDocument Document(RawPatch patch) => new()
    {
        Cells = patch.Cells.Select(cell => new FileSelectPatchCellDocument
        {
            X = cell.Offset.X,
            Y = cell.Offset.Y,
            Cell = Cell(cell.Word),
        }).ToArray(),
    };

    /// <summary>Reads complete little-endian tile words from a native page before converting their visual fields.</summary>
    private static MapPresentationCell[] Cells(ReadOnlySpan<byte> bytes)
    {
        if (bytes.Length % sizeof(ushort) != 0)
            throw new InvalidDataException("File-select page contains a partial word.");
        var words = new ushort[bytes.Length / sizeof(ushort)];
        for (int index = 0; index < words.Length; index++)
            words[index] = unchecked((ushort)(SnesLittleEndian.Word(bytes[index * 2], bytes[index * 2 + 1])));
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
    private readonly record struct RawPatchCell(Offset16<TilemapCells> Offset, ushort Word);
}
