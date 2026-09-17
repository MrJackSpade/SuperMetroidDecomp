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
                RomDataReader.ReadFixedBank(bus, FileSelectMapRomData.InitialMenuBackground,
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
        for (int frame = 0; frame < FileSelectLayout.MissileSpritemapIds.Length; frame++)
            sprites.Add(FileSelectPresentationDefinitions.CursorFrameName(frame),
                MenuSpriteExtractor.Read(bus, FileSelectLayout.MissileSpritemapIds[frame]));
        for (int frame = 0; frame < 8; frame++)
            sprites.Add(FileSelectPresentationDefinitions.HelmetFrameName(frame),
                MenuSpriteExtractor.Read(bus, unchecked((ushort)(0x2c + frame))));

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
                Slot(FileSelectLayout.SlotAEnergyDestination,
                    FileSelectLayout.SlotATimeValueDestination),
                Slot(FileSelectLayout.SlotBEnergyDestination,
                    FileSelectLayout.SlotBTimeValueDestination),
                Slot(FileSelectLayout.SlotCEnergyDestination,
                    FileSelectLayout.SlotCTimeValueDestination),
            ],
            DataSlots =
            [
                Slot(FileSelectLayout.DataSlotAEnergyDestination,
                    FileSelectLayout.DataSlotATimeValueDestination),
                Slot(FileSelectLayout.DataSlotBEnergyDestination,
                    FileSelectLayout.DataSlotBTimeValueDestination),
                Slot(FileSelectLayout.DataSlotCEnergyDestination,
                    FileSelectLayout.DataSlotCTimeValueDestination),
            ],
            Sprites = sprites,
            BorderAnchors = new(StringComparer.Ordinal)
            {
                [FileSelectPresentationDefinitions.MainBorder] = new(128, 16),
                [FileSelectPresentationDefinitions.CopyBorder] = new(128, 16),
                [FileSelectPresentationDefinitions.ClearBorder] = new(124, 16),
            },
            MainCursorAnchors = FileSelectLayout.MainSelectionY
                .Select(y => new MapLabelPoint(14, y)).ToArray(),
            DataCursorAnchors = new ushort[] { 72, 104, 136, 211 }
                .Select(y => new MapLabelPoint(22, y)).ToArray(),
            ConfirmationCursorAnchors =
                [new(94, 184), new(94, 208)],
            HelmetAnchors = FileSelectLayout.HelmetY
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
            CursorFrameDuration = 8,
            HelmetFrameDuration = 8,
        });
        return output.ToArray();

        void AddMainStatic(ushort[] page, bool includeDataCommands)
        {
            Apply(page, Patch(FileSelectTilemaps.SamusData), FileSelectLayout.SamusDataDestination);
            Apply(page, Patch(FileSelectTilemaps.SamusA), FileSelectLayout.SlotALabelDestination);
            Apply(page, Patch(FileSelectTilemaps.Time), FileSelectLayout.SlotATimeLabelDestination);
            Apply(page, Patch(FileSelectTilemaps.SamusB), FileSelectLayout.SlotBLabelDestination);
            Apply(page, Patch(FileSelectTilemaps.Time), FileSelectLayout.SlotBTimeLabelDestination);
            Apply(page, Patch(FileSelectTilemaps.SamusC), FileSelectLayout.SlotCLabelDestination);
            Apply(page, Patch(FileSelectTilemaps.Time), FileSelectLayout.SlotCTimeLabelDestination);
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
            Apply(page, Patch(FileSelectTilemaps.SamusA), FileSelectLayout.DataSlotALabelDestination);
            Apply(page, Patch(FileSelectTilemaps.Time), FileSelectLayout.DataSlotATimeLabelDestination);
            Apply(page, Patch(FileSelectTilemaps.SamusB), FileSelectLayout.DataSlotBLabelDestination);
            Apply(page, Patch(FileSelectTilemaps.Time), FileSelectLayout.DataSlotBTimeLabelDestination);
            Apply(page, Patch(FileSelectTilemaps.SamusC), FileSelectLayout.DataSlotCLabelDestination);
            Apply(page, Patch(FileSelectTilemaps.Time), FileSelectLayout.DataSlotCTimeLabelDestination);
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

    private static RawPatch ReadPatch(ISnesAddressSpace bus, ushort pointer)
    {
        var cells = new List<RawPatchCell>();
        int address = FileSelectTilemapFormat.Bank | pointer;
        int x = 0, y = 0;
        while (true)
        {
            ushort word = RomDataReader.ReadWordFixedBank(bus, address);
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

    private static ushort[] BlankPage()
    {
        var page = new ushort[FileSelectPresentationDefinitions.CellCount];
        Array.Fill(page, FileSelectLayout.BlankTile);
        return page;
    }

    private static FileSelectSlotFieldDocument Slot(int energyOffset, int timeOffset) => new()
    {
        EnergyAnchor = Point(energyOffset),
        HealthAnchor = Point(energyOffset + 0x42),
        NoDataAnchor = Point(energyOffset + FileSelectLayout.NextTilemapRowByteOffset),
        TimeValueAnchor = Point(timeOffset),
    };

    private static FileSelectPatchDocument Document(RawPatch patch) => new()
    {
        Cells = patch.Cells.Select(cell => new FileSelectPatchCellDocument
        {
            X = cell.X,
            Y = cell.Y,
            Cell = Cell(cell.Word),
        }).ToArray(),
    };

    private static MapPresentationCell[] Cells(ReadOnlySpan<byte> bytes)
    {
        if (bytes.Length % sizeof(ushort) != 0)
            throw new InvalidDataException("File-select page contains a partial word.");
        var words = new ushort[bytes.Length / sizeof(ushort)];
        for (int index = 0; index < words.Length; index++)
            words[index] = unchecked((ushort)(bytes[index * 2] | bytes[index * 2 + 1] << 8));
        return Cells(words);
    }

    private static MapPresentationCell[] Cells(ReadOnlySpan<ushort> words)
    {
        var cells = new MapPresentationCell[words.Length];
        for (int index = 0; index < cells.Length; index++)
            cells[index] = Cell(words[index]);
        return cells;
    }

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

    private static MapLabelPoint Point(int byteOffset)
    {
        int cell = byteOffset / sizeof(ushort);
        return new(cell % FileSelectPresentationDefinitions.Width,
            cell / FileSelectPresentationDefinitions.Width);
    }

    private sealed record RawPatch(RawPatchCell[] Cells);
    private readonly record struct RawPatchCell(int X, int Y, ushort Word);
}
