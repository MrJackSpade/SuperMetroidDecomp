using System.Security.Cryptography;
using System.Text.Json;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Assets;

/// <summary>
/// Immutable file-select pages, dynamic field artwork, actor compositions and layout.
/// SRAM validation, menu navigation, copy/clear operations and fades remain compiled.
/// </summary>
public sealed class FileSelectPresentation
{
    private readonly Dictionary<string, ushort[]> pages;
    private readonly Dictionary<string, FileSelectCompiledPatch> patches;
    private readonly ushort[] digits;
    private readonly ushort[] slotLetters;
    private readonly Dictionary<string, SpriteComposition> sprites;
    private readonly FileSelectPresentationDocument document;

    private FileSelectPresentation(
        Dictionary<string, ushort[]> pages,
        Dictionary<string, FileSelectCompiledPatch> patches,
        ushort[] digits,
        ushort[] slotLetters,
        Dictionary<string, SpriteComposition> sprites,
        FileSelectPresentationDocument document,
        string contentIdentity)
    {
        this.pages = pages;
        this.patches = patches;
        this.digits = digits;
        this.slotLetters = slotLetters;
        this.sprites = sprites;
        this.document = document;
        ContentIdentity = contentIdentity;
    }

    public string ContentIdentity { get; }
    public int CursorFrameDuration => document.CursorFrameDuration;
    public int HelmetFrameDuration => document.HelmetFrameDuration;

    internal void LoadBackground(SnesVram vram) =>
        vram.ExecuteWordTransfer(pages[FileSelectPresentationDefinitions.BackgroundPage],
            MenuPpuState.Bg2TilemapWord, 1);

    internal void CopyPage(string name, Span<ushort> destination)
    {
        if (!pages.TryGetValue(name, out ushort[]? source))
            throw new InvalidDataException($"Unknown file-select page {name}.");
        if (destination.Length != FileSelectPresentationDefinitions.CellCount)
            throw new ArgumentException("File-select destination must contain 1024 cells.",
                nameof(destination));
        source.CopyTo(destination);
    }

    internal FileSelectSlotFieldDocument Slot(bool dataManagement, int slot)
    {
        FileSelectSlotFieldDocument[] layouts = dataManagement
            ? document.DataSlots
            : document.MainSlots;
        return (uint)slot < layouts.Length
            ? layouts[slot]
            : throw new ArgumentOutOfRangeException(nameof(slot));
    }

    internal void ApplyPatch(Span<ushort> tilemap, string name, MapLabelPoint anchor)
    {
        if (!patches.TryGetValue(name, out FileSelectCompiledPatch? patch))
            throw new InvalidDataException($"Unknown file-select patch {name}.");
        foreach (FileSelectCompiledPatchCell cell in patch.Cells)
            tilemap[(anchor.Y + cell.Y) * FileSelectPresentationDefinitions.Width +
                anchor.X + cell.X] = cell.Word;
    }

    internal void WriteDigit(Span<ushort> tilemap, MapLabelPoint anchor, int offset, int digit)
    {
        if ((uint)digit >= digits.Length)
            throw new ArgumentOutOfRangeException(nameof(digit));
        tilemap[anchor.Y * FileSelectPresentationDefinitions.Width + anchor.X + offset] =
            digits[digit];
    }

    internal void WriteSlotLetter(Span<ushort> tilemap, MapLabelPoint anchor, int slot)
    {
        if ((uint)slot >= slotLetters.Length)
            throw new ArgumentOutOfRangeException(nameof(slot));
        tilemap[anchor.Y * FileSelectPresentationDefinitions.Width + anchor.X] =
            slotLetters[slot];
    }

    internal MapLabelPoint DynamicAnchor(string name) =>
        document.DynamicAnchors.TryGetValue(name, out MapLabelPoint? anchor)
            ? anchor
            : throw new InvalidDataException($"Unknown file-select dynamic anchor {name}.");

    internal MapLabelPoint CursorPosition(bool main, bool confirmation, int selected) =>
        Point(main
            ? document.MainCursorAnchors
            : confirmation
                ? document.ConfirmationCursorAnchors
                : document.DataCursorAnchors,
            selected, "cursor");

    internal void DrawBorder(OamBuffer oam, string page)
    {
        MapLabelPoint anchor = document.BorderAnchors[page];
        sprites[FileSelectPresentationDefinitions.BorderFrameName(page)].DrawOnScreen(
            oam, checked((ushort)anchor.X), checked((ushort)anchor.Y),
            PaletteBits(document.ObjectPalette));
    }

    internal void DrawCursor(OamBuffer oam, int frame, MapLabelPoint anchor) =>
        sprites[FileSelectPresentationDefinitions.CursorFrameName(frame)].DrawOnScreen(
            oam, checked((ushort)anchor.X), checked((ushort)anchor.Y),
            PaletteBits(document.ObjectPalette));

    internal void DrawHelmet(OamBuffer oam, int frame, int slot)
    {
        MapLabelPoint anchor = Point(document.HelmetAnchors, slot, "helmet");
        sprites[FileSelectPresentationDefinitions.HelmetFrameName(frame)].DrawOnScreen(
            oam, checked((ushort)anchor.X), checked((ushort)anchor.Y),
            PaletteBits(document.ObjectPalette));
    }

    public static FileSelectPresentation Load(Stream source)
    {
        byte[] bytes;
        using (var copy = new MemoryStream())
        {
            source.CopyTo(copy);
            bytes = copy.ToArray();
        }
        FileSelectPresentationDocument document;
        try
        {
            document = JsonSerializer.Deserialize<FileSelectPresentationDocument>(
                bytes, MapPresentationFormat.JsonOptions) ??
                throw new InvalidDataException("File-select presentation document is null.");
        }
        catch (JsonException error)
        {
            throw new InvalidDataException("Invalid file-select presentation JSON.", error);
        }

        if (document.Version != FileSelectPresentationDefinitions.Version)
            throw new InvalidDataException(
                $"File-select presentation version must be {FileSelectPresentationDefinitions.Version}.");
        ValidateExactKeys(document.Pages, FileSelectPresentationDefinitions.PageNames,
            "file-select pages");
        ValidateExactKeys(document.Patches, FileSelectPresentationDefinitions.PatchNames,
            "file-select patches");
        ValidateExactKeys(document.Sprites, FileSelectPresentationDefinitions.SpriteNames,
            "file-select sprites");
        ValidateExactKeys(document.BorderAnchors,
            FileSelectPresentationDefinitions.BorderNames, "file-select borders");
        ValidateExactKeys(document.DynamicAnchors,
            FileSelectPresentationDefinitions.DynamicAnchorNames,
            "file-select dynamic anchors");
        if (document.Digits is null || document.Digits.Length != 10)
            throw new InvalidDataException("File select requires ten digit cells.");
        if (document.SlotLetters is null || document.SlotLetters.Length != 3)
            throw new InvalidDataException("File select requires three slot-letter cells.");
        if (document.MainSlots is null || document.MainSlots.Length != 3 ||
            document.DataSlots is null || document.DataSlots.Length != 3)
            throw new InvalidDataException("File select requires three main and three data slot layouts.");
        ValidatePoints(document.MainCursorAnchors, 6, "main cursor");
        ValidatePoints(document.DataCursorAnchors, 4, "data cursor");
        ValidatePoints(document.ConfirmationCursorAnchors, 2, "confirmation cursor");
        ValidatePoints(document.HelmetAnchors, 3, "helmet");
        foreach (string name in FileSelectPresentationDefinitions.BorderNames)
            ValidatePoint(document.BorderAnchors[name], $"{name} border");
        foreach (string name in FileSelectPresentationDefinitions.DynamicAnchorNames)
            ValidateTilePoint(document.DynamicAnchors[name], $"{name} dynamic anchor");
        if ((uint)document.ObjectPalette >= MapPresentationFormat.PaletteCount)
            throw new InvalidDataException("File-select object palette must be 0..7.");
        if (document.CursorFrameDuration is < 1 or > ushort.MaxValue ||
            document.HelmetFrameDuration is < 1 or > ushort.MaxValue)
            throw new InvalidDataException("File-select actor durations must be 1..65535.");

        var pages = new Dictionary<string, ushort[]>(StringComparer.Ordinal);
        foreach (string name in FileSelectPresentationDefinitions.PageNames)
        {
            MapPresentationCell[] cells = document.Pages[name];
            if (cells is null || cells.Length != FileSelectPresentationDefinitions.CellCount)
                throw new InvalidDataException($"File-select page {name} requires 1024 cells.");
            pages.Add(name, CompileCells(cells, $"file-select page {name}"));
        }
        var patches = new Dictionary<string, FileSelectCompiledPatch>(StringComparer.Ordinal);
        foreach (string name in FileSelectPresentationDefinitions.PatchNames)
        {
            FileSelectPatchDocument patch = document.Patches[name] ??
                throw new InvalidDataException($"File-select patch {name} is null.");
            if (patch.Cells is null || patch.Cells.Length == 0)
                throw new InvalidDataException($"File-select patch {name} is empty.");
            var claimed = new HashSet<(int X, int Y)>();
            var compiled = new FileSelectCompiledPatchCell[patch.Cells.Length];
            for (int index = 0; index < compiled.Length; index++)
            {
                FileSelectPatchCellDocument cell = patch.Cells[index] ??
                    throw new InvalidDataException($"File-select patch {name} cell {index} is null.");
                if (cell.X < 0 || cell.Y < 0 ||
                    !claimed.Add((cell.X, cell.Y)))
                    throw new InvalidDataException(
                        $"File-select patch {name} contains an invalid or duplicate coordinate.");
                compiled[index] = new(cell.X, cell.Y,
                    CompileCell(cell.Cell, $"file-select patch {name} cell {index}"));
            }
            patches.Add(name, new(compiled));
        }

        ushort[] digits = document.Digits.Select((cell, index) =>
            CompileCell(cell, $"file-select digit {index}")).ToArray();
        ushort[] letters = document.SlotLetters.Select((cell, index) =>
            CompileCell(cell, $"file-select slot letter {index}")).ToArray();
        var sprites = new Dictionary<string, SpriteComposition>(StringComparer.Ordinal);
        foreach (string name in FileSelectPresentationDefinitions.SpriteNames)
            sprites.Add(name, MenuSpriteCompiler.Compile(document.Sprites[name],
                $"file-select {name}"));

        foreach (FileSelectSlotFieldDocument slot in document.MainSlots.Concat(document.DataSlots))
            ValidateSlot(slot);
        ValidatePatchPlacements(document, patches);
        return new(pages, patches, digits, letters, sprites, document,
            Convert.ToHexString(SHA256.HashData(bytes)));
    }

    public static void Write(Stream output, FileSelectPresentationDocument document)
    {
        byte[] bytes = JsonSerializer.SerializeToUtf8Bytes(document,
            MapPresentationFormat.JsonOptions);
        _ = Load(new MemoryStream(bytes, writable: false));
        output.Write(bytes);
    }

    private static void ValidatePatchPlacements(
        FileSelectPresentationDocument document,
        Dictionary<string, FileSelectCompiledPatch> patches)
    {
        foreach (FileSelectSlotFieldDocument slot in document.MainSlots.Concat(document.DataSlots))
        {
            ValidatePatch(slot.EnergyAnchor, patches[FileSelectPresentationDefinitions.EnergyPatch]);
            ValidatePatch(slot.NoDataAnchor, patches[FileSelectPresentationDefinitions.NoDataPatch]);
            ValidatePatch(new(slot.TimeValueAnchor.X + 2, slot.TimeValueAnchor.Y),
                patches[FileSelectPresentationDefinitions.TimeColonPatch]);
            if (slot.TimeValueAnchor.X + 5 > FileSelectPresentationDefinitions.Width)
                throw new InvalidDataException("File-select time field escapes its page.");
        }

        static void ValidatePatch(MapLabelPoint anchor, FileSelectCompiledPatch patch)
        {
            if (patch.Cells.Any(cell =>
                    anchor.X + cell.X >= FileSelectPresentationDefinitions.Width ||
                    anchor.Y + cell.Y >= FileSelectPresentationDefinitions.Height))
                throw new InvalidDataException("File-select patch placement escapes its page.");
        }
    }

    private static ushort[] CompileCells(MapPresentationCell[] cells, string owner)
    {
        var words = new ushort[cells.Length];
        for (int index = 0; index < words.Length; index++)
            words[index] = CompileCell(cells[index], $"{owner} cell {index}");
        return words;
    }

    private static ushort CompileCell(MapPresentationCell? cell, string owner)
    {
        if (cell is null ||
            (uint)cell.TileColumn >= FileSelectPresentationDefinitions.CharacterColumns ||
            (uint)cell.TileRow >= FileSelectPresentationDefinitions.CharacterRows ||
            (uint)cell.Palette >= MapPresentationFormat.PaletteCount)
            throw new InvalidDataException($"{owner} has an invalid character or palette.");
        return checked((ushort)(
            cell.TileRow * FileSelectPresentationDefinitions.CharacterColumns + cell.TileColumn |
            cell.Palette << MapPresentationFormat.PaletteShift |
            (cell.Priority ? MapPresentationFormat.PriorityBit : 0) |
            (cell.FlipX ? MapPresentationFormat.FlipXBit : 0) |
            (cell.FlipY ? MapPresentationFormat.FlipYBit : 0)));
    }

    private static void ValidateSlot(FileSelectSlotFieldDocument? slot)
    {
        if (slot is null)
            throw new InvalidDataException("File-select slot layout is null.");
        ValidateTilePoint(slot.EnergyAnchor, "slot energy");
        ValidateTilePoint(slot.HealthAnchor, "slot health");
        ValidateTilePoint(slot.NoDataAnchor, "slot no-data");
        ValidateTilePoint(slot.TimeValueAnchor, "slot time");
    }

    private static void ValidatePoints(MapLabelPoint[]? points, int count, string owner)
    {
        if (points is null || points.Length != count)
            throw new InvalidDataException($"File-select {owner} requires {count} points.");
        foreach (MapLabelPoint? point in points)
            ValidatePoint(point, owner);
    }

    private static void ValidatePoint(MapLabelPoint? point, string owner)
    {
        if (point is null || point.X < 0 || point.X > 0x1ff ||
            (uint)point.Y > byte.MaxValue)
            throw new InvalidDataException(
                $"File-select {owner} requires X=0..511 and Y=0..255.");
    }

    private static void ValidateTilePoint(MapLabelPoint? point, string owner)
    {
        if (point is null || (uint)point.X >= FileSelectPresentationDefinitions.Width ||
            (uint)point.Y >= FileSelectPresentationDefinitions.Height)
            throw new InvalidDataException(
                $"File-select {owner} requires a coordinate inside the 32x32 page.");
    }

    private static void ValidateExactKeys<T>(Dictionary<string, T>? values,
        ReadOnlySpan<string> expected, string owner)
    {
        if (values is null || values.Count != expected.Length)
            throw new InvalidDataException($"{owner} requires exactly {expected.Length} entries.");
        foreach (string name in expected)
            if (!values.ContainsKey(name))
                throw new InvalidDataException($"{owner} is missing {name}.");
    }

    private static MapLabelPoint Point(MapLabelPoint[] points, int index, string owner) =>
        (uint)index < points.Length ? points[index] :
            throw new ArgumentOutOfRangeException(owner);

    private static ushort PaletteBits(int index) =>
        SnesObjAttributeWord.Create(0, index, 0).PaletteBits;
}

public sealed record FileSelectPresentationDocument
{
    public required int Version { get; init; }
    public required Dictionary<string, MapPresentationCell[]> Pages { get; init; }
    public required Dictionary<string, FileSelectPatchDocument> Patches { get; init; }
    public required MapPresentationCell[] Digits { get; init; }
    public required MapPresentationCell[] SlotLetters { get; init; }
    public required FileSelectSlotFieldDocument[] MainSlots { get; init; }
    public required FileSelectSlotFieldDocument[] DataSlots { get; init; }
    public required Dictionary<string, SpriteVisualPart[]> Sprites { get; init; }
    public required Dictionary<string, MapLabelPoint> BorderAnchors { get; init; }
    public required MapLabelPoint[] MainCursorAnchors { get; init; }
    public required MapLabelPoint[] DataCursorAnchors { get; init; }
    public required MapLabelPoint[] ConfirmationCursorAnchors { get; init; }
    public required MapLabelPoint[] HelmetAnchors { get; init; }
    public required Dictionary<string, MapLabelPoint> DynamicAnchors { get; init; }
    public required int ObjectPalette { get; init; }
    public required int CursorFrameDuration { get; init; }
    public required int HelmetFrameDuration { get; init; }
}

public sealed record FileSelectPatchDocument
{
    public required FileSelectPatchCellDocument[] Cells { get; init; }
}

public sealed record FileSelectPatchCellDocument
{
    public required int X { get; init; }
    public required int Y { get; init; }
    public required MapPresentationCell Cell { get; init; }
}

public sealed record FileSelectSlotFieldDocument
{
    public required MapLabelPoint EnergyAnchor { get; init; }
    public required MapLabelPoint HealthAnchor { get; init; }
    public required MapLabelPoint NoDataAnchor { get; init; }
    public required MapLabelPoint TimeValueAnchor { get; init; }
}

internal sealed record FileSelectCompiledPatch(FileSelectCompiledPatchCell[] Cells);
internal readonly record struct FileSelectCompiledPatchCell(int X, int Y, ushort Word);

/// <summary>Schema names and fixed dimensions for <c>file-select.json</c>.</summary>
public static class FileSelectPresentationDefinitions
{
    public const int Version = 1;
    public const string FileName = "file-select.json";
    public const int Width = 32;
    public const int Height = 32;
    public const int CellCount = Width * Height;
    public const int CharacterColumns = 32;
    public const int CharacterRows = 32;

    public const string BackgroundPage = "Background";
    public const string MainWithDataPage = "Main.WithData";
    public const string MainEmptyPage = "Main.Empty";
    public const string CopySourcePage = "Copy.Source";
    public const string CopyDestinationPage = "Copy.Destination";
    public const string CopyConfirmPage = "Copy.Confirm";
    public const string CopyCompletedPage = "Copy.Completed";
    public const string ClearSelectionPage = "Clear.Selection";
    public const string ClearConfirmPage = "Clear.Confirm";
    public const string ClearCompletedPage = "Clear.Completed";

    public const string EnergyPatch = "Energy";
    public const string NoDataPatch = "NoData";
    public const string TimeColonPatch = "TimeColon";
    public const string MainBorder = "Main";
    public const string CopyBorder = "Copy";
    public const string ClearBorder = "Clear";
    public const string CopyDestinationSourceAnchor = "Copy.Destination.Source";
    public const string CopyConfirmSourceAnchor = "Copy.Confirm.Source";
    public const string CopyConfirmDestinationAnchor = "Copy.Confirm.Destination";
    public const string ClearConfirmSourceAnchor = "Clear.Confirm.Source";

    private static readonly string[] pageNames =
    [
        BackgroundPage, MainWithDataPage, MainEmptyPage,
        CopySourcePage, CopyDestinationPage, CopyConfirmPage, CopyCompletedPage,
        ClearSelectionPage, ClearConfirmPage, ClearCompletedPage,
    ];
    private static readonly string[] patchNames = [EnergyPatch, NoDataPatch, TimeColonPatch];
    private static readonly string[] borderNames = [MainBorder, CopyBorder, ClearBorder];
    private static readonly string[] dynamicAnchorNames =
    [
        CopyDestinationSourceAnchor, CopyConfirmSourceAnchor,
        CopyConfirmDestinationAnchor, ClearConfirmSourceAnchor,
    ];
    private static readonly string[] spriteNames =
    [
        "Border.Main", "Border.Copy", "Border.Clear",
        "Cursor.0", "Cursor.1", "Cursor.2", "Cursor.3",
        "Helmet.0", "Helmet.1", "Helmet.2", "Helmet.3",
        "Helmet.4", "Helmet.5", "Helmet.6", "Helmet.7",
    ];

    public static ReadOnlySpan<string> PageNames => pageNames;
    public static ReadOnlySpan<string> PatchNames => patchNames;
    public static ReadOnlySpan<string> BorderNames => borderNames;
    public static ReadOnlySpan<string> DynamicAnchorNames => dynamicAnchorNames;
    public static ReadOnlySpan<string> SpriteNames => spriteNames;
    public static string BorderFrameName(string name) => $"Border.{name}";
    public static string CursorFrameName(int frame) => (uint)frame < 4
        ? $"Cursor.{frame}" : throw new ArgumentOutOfRangeException(nameof(frame));
    public static string HelmetFrameName(int frame) => (uint)frame < 8
        ? $"Helmet.{frame}" : throw new ArgumentOutOfRangeException(nameof(frame));
}
