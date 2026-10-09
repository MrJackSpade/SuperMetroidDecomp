using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rom;

namespace SuperMetroid.AssetExtraction;

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
        ArgumentNullException.ThrowIfNull(bus);
        var rawPatches = new Dictionary<ushort, RawPatch>();
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
                .Select(y => new MapLabelPoint(14, y)).ToArray(),
            DataCursorAnchors = Enumerable.Range(0, FileSelectLayout.DataSelectionCount)
                .Select(FileSelectLayout.DataSelectionY)
                .Select(y => new MapLabelPoint(22, y)).ToArray(),
            ConfirmationCursorAnchors =
                [new(94, 184), new(94, 208)],
            HelmetAnchors = Enumerable.Range(0, FileSelectLayout.SaveSlotCount)
                .Select(FileSelectLayout.HelmetY)
                .Select(y => new MapLabelPoint(100, y)).ToArray(),
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

        void AddConfirmation(ushort[] page)
        {
            Apply(page, Patch(FileSelectTilemaps.IsThisOkay),
                FileSelectLayout.ConfirmationQuestionDestination);
            Apply(page, Patch(FileSelectTilemaps.Yes), FileSelectLayout.ConfirmationYesDestination);
            Apply(page, Patch(FileSelectTilemaps.No), FileSelectLayout.ConfirmationNoDestination);
        }
    }

    /// <summary>Decodes one native file-select text patch into relative tilemap cells.</summary>
    /// <param name="bus">Cartridge address space containing the fixed-bank tilemap stream.</param>
    /// <param name="pointer">Bank-local address of the patch.</param>
    /// <returns>Patch cells with row-relative coordinates and original tilemap words.</returns>
    private static RawPatch ReadPatch(ISnesAddressSpace bus, ushort pointer)
    {
        var cells = new List<RawPatchCell>();
        int address = FileSelectTilemapFormat.Bank | pointer;
        int x = 0, y = 0;
        while (true)
        {
            ushort word = RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus), address);
            address = FileSelectTilemapFormat.Bank | ((address + 2) & 0xffff);
            if (word == FileSelectTilemapFormat.End)
                return new(cells.ToArray());
            if (word == FileSelectTilemapFormat.NextRow)
            {
                x = 0;
                y++;
                continue;
            }
            cells.Add(new(x++, y, word));
        }
    }

    /// <summary>Places a decoded text patch onto a page and rejects cells outside its tilemap.</summary>
    /// <param name="page">Mutable file-select page words.</param>
    /// <param name="patch">Patch cells positioned relative to their anchor.</param>
    /// <param name="byteOffset">Byte offset locating the patch's first cell.</param>
    private static void Apply(ushort[] page, RawPatch patch, int byteOffset)
    {
        MapLabelPoint anchor = Point(byteOffset);
        foreach (RawPatchCell cell in patch.Cells)
        {
            int index = (anchor.Y + cell.Y) * FileSelectPresentationDefinitions.Width +
                anchor.X + cell.X;
            if ((uint)index >= page.Length)
                throw new InvalidDataException("Extracted file-select text escaped its page.");
            page[index] = cell.Word;
        }
    }

    /// <summary>Creates a page-sized tilemap initialized with the authored blank tile.</summary>
    /// <returns>The initialized file-select page words.</returns>
    private static ushort[] BlankPage()
    {
        var page = new ushort[FileSelectPresentationDefinitions.CellCount];
        Array.Fill(page, FileSelectLayout.BlankTile);
        return page;
    }

    /// <summary>Creates slot-field anchors from the energy and time tilemap locations.</summary>
    /// <param name="energyOffset">Byte offset of the slot's energy field.</param>
    /// <param name="timeOffset">Byte offset of the slot's displayed time value.</param>
    /// <returns>The derived energy, health, no-data, and time anchors.</returns>
    private static FileSelectSlotFieldDocument Slot(int energyOffset, int timeOffset) => new()
    {
        EnergyAnchor = Point(energyOffset),
        HealthAnchor = Point(energyOffset + 0x42),
        NoDataAnchor = Point(energyOffset + FileSelectLayout.NextTilemapRowByteOffset),
        TimeValueAnchor = Point(timeOffset),
    };

    /// <summary>Converts native patch cells into the serialized file-select patch document.</summary>
    /// <param name="patch">Decoded cells retaining native relative coordinates and tile words.</param>
    /// <returns>The editable patch document.</returns>
    private static FileSelectPatchDocument Document(RawPatch patch) => new()
    {
        Cells = patch.Cells.Select(cell => new FileSelectPatchCellDocument
        {
            X = cell.X,
            Y = cell.Y,
            Cell = Cell(cell.Word),
        }).ToArray(),
    };

    /// <summary>Decodes little-endian tilemap bytes into presentation cells.</summary>
    /// <param name="bytes">Even-length tilemap byte sequence.</param>
    /// <returns>Decoded cells in row-major order.</returns>
    private static MapPresentationCell[] Cells(ReadOnlySpan<byte> bytes)
    {
        if (bytes.Length % sizeof(ushort) != 0)
            throw new InvalidDataException("File-select page contains a partial word.");
        var words = new ushort[bytes.Length / sizeof(ushort)];
        for (int index = 0; index < words.Length; index++)
            words[index] = unchecked((ushort)(bytes[index * 2] | bytes[index * 2 + 1] << 8));
        return Cells(words);
    }

    /// <summary>Converts tilemap words into renderer-facing presentation cells.</summary>
    /// <param name="words">Native tilemap words in row-major order.</param>
    /// <returns>Presentation cells preserving character, palette, priority, and flips.</returns>
    private static MapPresentationCell[] Cells(ReadOnlySpan<ushort> words)
    {
        var cells = new MapPresentationCell[words.Length];
        for (int index = 0; index < cells.Length; index++)
            cells[index] = Cell(words[index]);
        return cells;
    }

    /// <summary>Converts one SNES background tilemap word into a presentation cell.</summary>
    /// <param name="raw">Packed native tilemap word.</param>
    /// <returns>The decoded character coordinate and visual attributes.</returns>
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

    /// <summary>Converts a byte offset in a page tilemap into cell coordinates.</summary>
    /// <param name="byteOffset">Byte offset from the start of the page.</param>
    /// <returns>Column and row of the corresponding tilemap cell.</returns>
    private static MapLabelPoint Point(int byteOffset)
    {
        int cell = byteOffset / sizeof(ushort);
        return new(cell % FileSelectPresentationDefinitions.Width,
            cell / FileSelectPresentationDefinitions.Width);
    }

    /// <summary>Native tilemap cells for one decoded file-select text patch.</summary>
    /// <param name="Cells">Cells in patch order with coordinates relative to its anchor.</param>
    private sealed record RawPatch(RawPatchCell[] Cells);

    /// <summary>One raw tilemap word and its patch-relative position.</summary>
    /// <param name="X">Column offset from the patch anchor.</param>
    /// <param name="Y">Row offset from the patch anchor.</param>
    /// <param name="Word">Packed SNES tilemap word.</param>
    private readonly record struct RawPatchCell(int X, int Y, ushort Word);
}
