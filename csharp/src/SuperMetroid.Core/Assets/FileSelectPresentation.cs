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
    /// <summary>Compiled foreground tilemaps keyed by their schema page names.</summary>
    private readonly Dictionary<string, ushort[]> pages;
    /// <summary>Compiled ENERGY label patch applied relative to each slot's energy anchor.</summary>
    private readonly FileSelectCompiledPatch energyPatch;
    /// <summary>Compiled NO DATA label patch applied to empty save slots.</summary>
    private readonly FileSelectCompiledPatch noDataPatch;
    /// <summary>Compiled colon glyph patch inserted into the save-time field.</summary>
    private readonly FileSelectCompiledPatch timeColonPatch;
    /// <summary>Optional authored digit words; null uses the stock consecutive tile sequence.</summary>
    private readonly ushort[]? digits;
    /// <summary>Optional authored save-slot letter words; null uses the stock consecutive tile sequence.</summary>
    private readonly ushort[]? slotLetters;
    /// <summary>Compiled sprite compositions addressed by their required schema identities.</summary>
    private readonly Dictionary<string, SpriteComposition> sprites;
    /// <summary>Parsed layout retained for dynamic anchors and menu animation durations.</summary>
    private readonly FileSelectPresentationDocument document;

    /// <summary>Assembles validated page, patch, glyph, sprite, and layout data into the loaded presentation.</summary>
    /// <param name="pages">Compiled menu tilemaps indexed by page key.</param>
    /// <param name="patches">Compiled dynamic text patches indexed by patch key.</param>
    /// <param name="digits">Compiled digit glyph words in numeric order.</param>
    /// <param name="slotLetters">Compiled save-slot letters in A-to-C order.</param>
    /// <param name="sprites">Compiled border and menu actor compositions by sprite key.</param>
    /// <param name="document">Parsed document retaining the validated dynamic layout.</param>
    /// <param name="contentIdentity">Uppercase digest identifying the exact source JSON bytes.</param>
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
        energyPatch = patches[FileSelectPresentationDefinitions.EnergyPatch];
        noDataPatch = patches[FileSelectPresentationDefinitions.NoDataPatch];
        timeColonPatch = patches[FileSelectPresentationDefinitions.TimeColonPatch];
        this.digits = PreserveEditedGlyphs(digits, FileSelectLayout.DigitTileBase);
        this.slotLetters = PreserveEditedGlyphs(slotLetters, FileSelectLayout.SamusLetterTileBase);
        this.sprites = sprites;
        this.document = document;
        ContentIdentity = contentIdentity;
    }

    /// <summary>Uses stock consecutive tile IDs when supplied glyph words exactly match that sequence.</summary>
    /// <param name="values">Compiled glyph words in display order.</param>
    /// <param name="first">Stock tile word expected at index zero.</param>
    /// <returns>The supplied array when any word is edited; otherwise null signals stock calculation.</returns>
    private static ushort[]? PreserveEditedGlyphs(ushort[] values, ushort first)
    {
        for (int index = 0; index < values.Length; index++)
            if (values[index] != first + index)
                return values;
        return null;
    }
    /// <summary>Uppercase SHA-256 digest of the loaded JSON bytes, identifying the selected presentation including its encoding.</summary>
    public string ContentIdentity { get; }
    /// <summary>Menu updates per missile-cursor animation frame, in the range 1-65535.</summary>
    public int CursorFrameDuration => document.CursorFrameDuration;
    /// <summary>Menu updates per selected-slot helmet animation frame, in the range 1-65535.</summary>
    public int HelmetFrameDuration => document.HelmetFrameDuration;

    /// <summary>Transfers the shared file-select background page into the BG2 tilemap.</summary>
    /// <param name="vram">Video memory receiving the compiled background page.</param>
    internal void LoadBackground(SnesVram vram) =>
        vram.ExecuteWordTransfer(pages[FileSelectPresentationDefinitions.BackgroundPage],
            MenuPpuState.Bg2TilemapWord, 1);

    /// <summary>Copies a compiled foreground page into a caller-provided 32-by-32 tilemap buffer.</summary>
    /// <param name="name">Required page key from the presentation schema.</param>
    /// <param name="destination">Exactly 1024 words receiving the row-major page.</param>
    /// <exception cref="InvalidDataException">The page key is not present in this presentation.</exception>
    /// <exception cref="ArgumentException">The destination does not contain exactly 1024 cells.</exception>
    internal void CopyPage(string name, Span<ushort> destination)
    {
        if (!pages.TryGetValue(name, out ushort[]? source))
            throw new InvalidDataException($"Unknown file-select page {name}.");
        if (destination.Length != FileSelectPresentationDefinitions.CellCount)
            throw new ArgumentException("File-select destination must contain 1024 cells.",
                nameof(destination));
        source.CopyTo(destination);
    }

    /// <summary>Gets the dynamic field anchors for a selected save slot on the requested menu family.</summary>
    /// <param name="dataManagement">Selects copy/clear layout when true, otherwise the main-menu layout.</param>
    /// <param name="slot">Zero-based save slot index.</param>
    /// <returns>The validated field layout for that slot.</returns>
    /// <exception cref="ArgumentOutOfRangeException">The slot is outside the three available entries.</exception>
    internal FileSelectSlotFieldDocument Slot(bool dataManagement, int slot)
    {
        FileSelectSlotFieldDocument[] layouts = dataManagement
            ? document.DataSlots
            : document.MainSlots;
        return (uint)slot < layouts.Length
            ? layouts[slot]
            : throw new ArgumentOutOfRangeException(nameof(slot));
    }

    /// <summary>Writes one named sparse text patch into a tilemap relative to its validated anchor.</summary>
    /// <param name="tilemap">Destination page tilemap.</param>
    /// <param name="name">Energy, no-data, or time-colon patch identity.</param>
    /// <param name="anchor">Tile coordinate at which the patch's offsets begin.</param>
    /// <exception cref="InvalidDataException">The patch name is not one of the compiled patch identities.</exception>
    internal void ApplyPatch(Span<ushort> tilemap, string name, MapLabelPoint anchor)
    {
        FileSelectCompiledPatch patch = name switch
        {
            FileSelectPresentationDefinitions.EnergyPatch => energyPatch,
            FileSelectPresentationDefinitions.NoDataPatch => noDataPatch,
            FileSelectPresentationDefinitions.TimeColonPatch => timeColonPatch,
            _ => throw new InvalidDataException($"Unknown file-select patch {name}."),
        };
        for (int index = 0; index < patch.Count; index++)
        {
            FileSelectCompiledPatchCell cell = patch.Cell(index);
            tilemap[(anchor.Y + cell.Y) * FileSelectPresentationDefinitions.Width +
                anchor.X + cell.X] = cell.Word;
        }
    }

    /// <summary>Writes a decimal glyph at a horizontal offset from a dynamic field anchor.</summary>
    /// <param name="tilemap">Destination page tilemap.</param>
    /// <param name="anchor">Origin of the dynamic numeric field.</param>
    /// <param name="offset">Horizontal tile offset from the anchor.</param>
    /// <param name="digit">Digit value from zero through nine.</param>
    /// <exception cref="ArgumentOutOfRangeException">The digit is outside zero through nine.</exception>
    internal void WriteDigit(Span<ushort> tilemap, MapLabelPoint anchor, int offset, int digit)
    {
        if ((uint)digit >= FileSelectPresentationDefinitions.DigitCount)
            throw new ArgumentOutOfRangeException(nameof(digit));
        tilemap[anchor.Y * FileSelectPresentationDefinitions.Width + anchor.X + offset] =
            digits is null ? (ushort)(FileSelectLayout.DigitTileBase + digit) : digits[digit];
    }

    /// <summary>Writes the A, B, or C glyph identifying a save slot at the given anchor.</summary>
    /// <param name="tilemap">Destination page tilemap.</param>
    /// <param name="anchor">Tile coordinate receiving the glyph.</param>
    /// <param name="slot">Zero-based save slot index.</param>
    /// <exception cref="ArgumentOutOfRangeException">The slot is outside zero through two.</exception>
    internal void WriteSlotLetter(Span<ushort> tilemap, MapLabelPoint anchor, int slot)
    {
        if ((uint)slot >= FileSelectPresentationDefinitions.SlotLetterCount)
            throw new ArgumentOutOfRangeException(nameof(slot));
        tilemap[anchor.Y * FileSelectPresentationDefinitions.Width + anchor.X] =
            slotLetters is null ? (ushort)(FileSelectLayout.SamusLetterTileBase + slot) : slotLetters[slot];
    }

    /// <summary>Returns a named tile-space anchor used for dynamic copy or clear slot lettering.</summary>
    /// <param name="name">Required dynamic-anchor schema key.</param>
    /// <returns>The validated tile coordinate.</returns>
    /// <exception cref="InvalidDataException">The key is not present in the loaded document.</exception>
    internal MapLabelPoint DynamicAnchor(string name) =>
        document.DynamicAnchors.TryGetValue(name, out MapLabelPoint? anchor)
            ? anchor
            : throw new InvalidDataException($"Unknown file-select dynamic anchor {name}.");

    /// <summary>Resolves the cursor's pixel-space origin for the active menu and selection.</summary>
    /// <param name="main">Selects the main-menu cursor layout.</param>
    /// <param name="confirmation">Selects confirmation choices when not on the main menu.</param>
    /// <param name="selected">Selected option index in the chosen cursor layout.</param>
    /// <returns>The pixel coordinate used to draw the cursor.</returns>
    internal MapLabelPoint CursorPosition(bool main, bool confirmation, int selected) =>
        Point(main
            ? document.MainCursorAnchors
            : confirmation
                ? document.ConfirmationCursorAnchors
                : document.DataCursorAnchors,
            selected, "cursor");

    /// <summary>Draws the border composition associated with a menu page.</summary>
    /// <param name="oam">Sprite buffer receiving the border parts.</param>
    /// <param name="page">Page identity selecting its border anchor and composition.</param>
    internal void DrawBorder(OamBuffer oam, string page)
    {
        MapLabelPoint anchor = document.BorderAnchors[page];
        sprites[FileSelectPresentationDefinitions.BorderFrameName(page)].DrawOnScreen(
            oam, checked((ushort)anchor.X), checked((ushort)anchor.Y),
            PaletteBits(document.ObjectPalette));
    }

    /// <summary>Draws one cursor animation frame at its resolved pixel-space origin.</summary>
    /// <param name="oam">Sprite buffer receiving cursor parts.</param>
    /// <param name="frame">Cursor frame index from zero through three.</param>
    /// <param name="anchor">Pixel origin for the cursor composition.</param>
    internal void DrawCursor(OamBuffer oam, int frame, MapLabelPoint anchor) =>
        sprites[FileSelectPresentationDefinitions.CursorFrameName(frame)].DrawOnScreen(
            oam, checked((ushort)anchor.X), checked((ushort)anchor.Y),
            PaletteBits(document.ObjectPalette));

    /// <summary>Draws the selected save slot's helmet animation frame at its configured origin.</summary>
    /// <param name="oam">Sprite buffer receiving helmet parts.</param>
    /// <param name="frame">Helmet frame index from zero through seven.</param>
    /// <param name="slot">Zero-based save slot selecting the pixel-space anchor.</param>
    internal void DrawHelmet(OamBuffer oam, int frame, int slot)
    {
        MapLabelPoint anchor = Point(document.HelmetAnchors, slot, "helmet");
        sprites[FileSelectPresentationDefinitions.HelmetFrameName(frame)].DrawOnScreen(
            oam, checked((ushort)anchor.X), checked((ushort)anchor.Y),
            PaletteBits(document.ObjectPalette));
    }

    /// <summary>Reads and validates a complete file-select JSON document, compiling its tilemaps, patches, glyphs, and sprite compositions.</summary>
    /// <param name="source">Readable JSON stream, consumed from its current position to the end and left open.</param>
    /// <returns>A presentation owning the parsed layout and compiled artwork, independent of the source stream.</returns>
    /// <exception cref="InvalidDataException">The JSON, schema version, required names, artwork, anchors, durations, or patch placements are invalid.</exception>
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
            document = JsonAssetDocument.Read<FileSelectPresentationDocument>(
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
            patches.Add(name, new(name, compiled));
        }

        ushort[] digits = document.Digits.Select((cell, index) =>
            CompileCell(cell, $"file-select digit {index}")).ToArray();
        ushort[] letters = document.SlotLetters.Select((cell, index) =>
            CompileCell(cell, $"file-select slot letter {index}")).ToArray();
        var sprites = new Dictionary<string, SpriteComposition>(StringComparer.Ordinal);
        foreach (string name in FileSelectPresentationDefinitions.SpriteNames)
            sprites.Add(name, MenuHeadingBorderDefinitions.CalculateIfMatching(name, FileSelectHelmetParts.CalculateIfMatching(name, MenuBorderParts.CalculateIfMatching(name, MenuCursorParts.CalculateIfMatching(name, MenuSpriteCompiler.Compile(document.Sprites[name],
                $"file-select {name}"))))));

        foreach (FileSelectSlotFieldDocument slot in document.MainSlots.Concat(document.DataSlots))
            ValidateSlot(slot);
        ValidatePatchPlacements(document, patches);
        return new(pages, patches, digits, letters, sprites, document,
            Convert.ToHexString(SHA256.HashData(bytes)));
    }

    /// <summary>Serializes an authored document and validates the serialized presentation before writing any bytes to the output.</summary>
    /// <param name="output">Writable stream receiving UTF-8 JSON at its current position; left open.</param>
    /// <param name="document">Authored layout and artwork to validate and serialize; its collections are not modified.</param>
    /// <exception cref="InvalidDataException">The serialized document fails file-select presentation validation.</exception>
    public static void Write(Stream output, FileSelectPresentationDocument document)
    {
        byte[] bytes = JsonSerializer.SerializeToUtf8Bytes(document,
            MapPresentationFormat.JsonOptions);
        _ = Load(new MemoryStream(bytes, writable: false));
        output.Write(bytes);
    }

    /// <summary>Checks each slot's dynamic text patches and time field fit within a 32-by-32 page.</summary>
    /// <param name="document">Parsed layout containing all main and data-management slot anchors.</param>
    /// <param name="patches">Compiled patches whose sparse cell offsets determine their extents.</param>
    /// <exception cref="InvalidDataException">A patch or time field extends beyond its page.</exception>
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

        // Verify every compiled sparse patch cell remains inside the page when placed at
        // the supplied tile-coordinate anchor, rejecting an escaped cell as invalid data.
        static void ValidatePatch(MapLabelPoint anchor, FileSelectCompiledPatch patch)
        {
            for (int index = 0; index < patch.Count; index++)
            {
                FileSelectCompiledPatchCell cell = patch.Cell(index);
                if (anchor.X + cell.X >= FileSelectPresentationDefinitions.Width ||
                    anchor.Y + cell.Y >= FileSelectPresentationDefinitions.Height)
                    throw new InvalidDataException("File-select patch placement escapes its page.");
            }
        }
    }

    /// <summary>Compiles all page cells into SNES tilemap words while identifying invalid entries by owner.</summary>
    /// <param name="cells">Parsed cells in row-major order.</param>
    /// <param name="owner">Page description included in any validation error.</param>
    /// <returns>Compiled tilemap words in the original order.</returns>
    private static ushort[] CompileCells(MapPresentationCell[] cells, string owner)
    {
        var words = new ushort[cells.Length];
        for (int index = 0; index < words.Length; index++)
            words[index] = CompileCell(cells[index], $"{owner} cell {index}");
        return words;
    }

    /// <summary>Validates one character-sheet coordinate and packs its tile, palette, priority, and flips.</summary>
    /// <param name="cell">Authored background tile entry.</param>
    /// <param name="owner">Cell description included in any validation error.</param>
    /// <returns>The SNES tilemap word for this cell.</returns>
    /// <exception cref="InvalidDataException">The cell is null or references an invalid tile or palette.</exception>
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

    /// <summary>Requires all four dynamic field anchors for a save slot to lie inside the tile page.</summary>
    /// <param name="slot">Parsed slot layout to validate.</param>
    /// <exception cref="InvalidDataException">The layout is null or an anchor falls outside the 32-by-32 page.</exception>
    private static void ValidateSlot(FileSelectSlotFieldDocument? slot)
    {
        if (slot is null)
            throw new InvalidDataException("File-select slot layout is null.");
        ValidateTilePoint(slot.EnergyAnchor, "slot energy");
        ValidateTilePoint(slot.HealthAnchor, "slot health");
        ValidateTilePoint(slot.NoDataAnchor, "slot no-data");
        ValidateTilePoint(slot.TimeValueAnchor, "slot time");
    }

    /// <summary>Validates the required number and screen-space bounds of a cursor or helmet anchor list.</summary>
    /// <param name="points">Authored anchor coordinates.</param>
    /// <param name="count">Exact number of coordinates required.</param>
    /// <param name="owner">Anchor role included in validation errors.</param>
    /// <exception cref="InvalidDataException">The list is null, has the wrong size, or contains an invalid point.</exception>
    private static void ValidatePoints(MapLabelPoint[]? points, int count, string owner)
    {
        if (points is null || points.Length != count)
            throw new InvalidDataException($"File-select {owner} requires {count} points.");
        foreach (MapLabelPoint? point in points)
            ValidatePoint(point, owner);
    }

    /// <summary>Validates a screen-space anchor against the SNES's wrapped X and byte-sized Y ranges.</summary>
    /// <param name="point">Pixel coordinate to validate.</param>
    /// <param name="owner">Anchor role included in validation errors.</param>
    /// <exception cref="InvalidDataException">The point is null or outside X=0..511 and Y=0..255.</exception>
    private static void ValidatePoint(MapLabelPoint? point, string owner)
    {
        if (point is null || point.X < 0 || point.X > 0x1ff ||
            (uint)point.Y > byte.MaxValue)
            throw new InvalidDataException(
                $"File-select {owner} requires X=0..511 and Y=0..255.");
    }

    /// <summary>Validates a tile-space coordinate within the menu's 32-by-32 page.</summary>
    /// <param name="point">Tile coordinate to validate.</param>
    /// <param name="owner">Anchor role included in validation errors.</param>
    /// <exception cref="InvalidDataException">The point is null or outside the tile page.</exception>
    private static void ValidateTilePoint(MapLabelPoint? point, string owner)
    {
        if (point is null || (uint)point.X >= FileSelectPresentationDefinitions.Width ||
            (uint)point.Y >= FileSelectPresentationDefinitions.Height)
            throw new InvalidDataException(
                $"File-select {owner} requires a coordinate inside the 32x32 page.");
    }

    /// <summary>Requires a JSON object to contain exactly the supplied schema identities.</summary>
    /// <typeparam name="T">Value type stored for each identity.</typeparam>
    /// <param name="values">Parsed object to validate.</param>
    /// <param name="expected">Required keys, with no additional entries allowed.</param>
    /// <param name="owner">Object role included in validation errors.</param>
    /// <exception cref="InvalidDataException">The object is null, has a different count, or lacks a required key.</exception>
    private static void ValidateExactKeys<T>(Dictionary<string, T>? values,
        IEnumerable<string> expected, string owner)
    {
        int count = expected.Count();
        if (values is null || values.Count != count)
            throw new InvalidDataException($"{owner} requires exactly {count} entries.");
        foreach (string name in expected)
            if (!values.ContainsKey(name))
                throw new InvalidDataException($"{owner} is missing {name}.");
    }

    /// <summary>Returns an indexed anchor or reports the selection role when the index is invalid.</summary>
    /// <param name="points">Validated anchors for one menu context.</param>
    /// <param name="index">Selected anchor index.</param>
    /// <param name="owner">Selection role used as the argument name on failure.</param>
    /// <returns>The selected pixel-space anchor.</returns>
    private static MapLabelPoint Point(MapLabelPoint[] points, int index, string owner) =>
        (uint)index < points.Length ? points[index] :
            throw new ArgumentOutOfRangeException(owner);

    /// <summary>Encodes the selected OBJ palette index into SNES sprite attribute bits.</summary>
    /// <param name="index">OBJ palette number in the supported range zero through seven.</param>
    /// <returns>Attribute bits ready to combine with each sprite part.</returns>
    private static ushort PaletteBits(int index) =>
        SnesObjAttributeWord.Create(0, index, 0).PaletteBits;
}

/// <summary>Authored JSON layout and artwork for file-select menus, with tile-space background fields and pixel-space sprite anchors.</summary>
/// <remarks>Init-only properties hold caller-owned mutable arrays and dictionaries. A loaded presentation parses its own document and compiles artwork from the supplied JSON stream.</remarks>
public sealed record FileSelectPresentationDocument
{
    /// <summary>Schema version; must equal <see cref="FileSelectPresentationDefinitions.Version"/>.</summary>
    public required int Version { get; init; }
    /// <summary>Exactly the named menu pages, each containing 1024 row-major cells for a 32-by-32 tilemap.</summary>
    public required Dictionary<string, MapPresentationCell[]> Pages { get; init; }
    /// <summary>Exactly the energy, empty-slot, and time-colon patches, placed relative to dynamic slot-field anchors.</summary>
    public required Dictionary<string, FileSelectPatchDocument> Patches { get; init; }
    /// <summary>Ten tile cells in digit-value order 0-9, including each glyph's palette and tile attributes.</summary>
    public required MapPresentationCell[] Digits { get; init; }
    /// <summary>Three tile cells in save-slot order A-C, used when copy or clear text identifies a slot.</summary>
    public required MapPresentationCell[] SlotLetters { get; init; }
    /// <summary>Three dynamic save-field layouts in slot order for the main file-select page.</summary>
    public required FileSelectSlotFieldDocument[] MainSlots { get; init; }
    /// <summary>Three dynamic save-field layouts in slot order for copy and clear pages.</summary>
    public required FileSelectSlotFieldDocument[] DataSlots { get; init; }
    /// <summary>Exactly the named border compositions, four cursor frames, and eight helmet frames, using sprite-part offsets relative to their drawing anchors.</summary>
    public required Dictionary<string, SpriteVisualPart[]> Sprites { get; init; }
    /// <summary>Main, copy, and clear border origins in screen pixels, with X=0-511 and Y=0-255.</summary>
    public required Dictionary<string, MapLabelPoint> BorderAnchors { get; init; }
    /// <summary>Six missile-cursor origins in main-menu selection order, in screen pixels with X=0-511 and Y=0-255.</summary>
    public required MapLabelPoint[] MainCursorAnchors { get; init; }
    /// <summary>Four missile-cursor origins in data-management selection order, in screen pixels with X=0-511 and Y=0-255.</summary>
    public required MapLabelPoint[] DataCursorAnchors { get; init; }
    /// <summary>Two missile-cursor origins in confirmation-option order, in screen pixels with X=0-511 and Y=0-255.</summary>
    public required MapLabelPoint[] ConfirmationCursorAnchors { get; init; }
    /// <summary>Three helmet origins in save-slot order, in screen pixels with X=0-511 and Y=0-255.</summary>
    public required MapLabelPoint[] HelmetAnchors { get; init; }
    /// <summary>Exactly the four named copy/clear slot-letter anchors, in tile coordinates inside the 32-by-32 page.</summary>
    public required Dictionary<string, MapLabelPoint> DynamicAnchors { get; init; }
    /// <summary>OBJ palette selector 0-7 applied to border, cursor, and helmet compositions.</summary>
    public required int ObjectPalette { get; init; }
    /// <summary>Menu-update duration 1-65535 for each of the four looping missile-cursor frames.</summary>
    public required int CursorFrameDuration { get; init; }
    /// <summary>Menu-update duration 1-65535 for each selected-slot helmet frame; the engine stops the animation on its last frame.</summary>
    public required int HelmetFrameDuration { get; init; }
}

/// <summary>Authored sparse tilemap patch whose cells are positioned relative to a slot-field anchor.</summary>
public sealed record FileSelectPatchDocument
{
    /// <summary>Nonempty caller-owned cell array; coordinates must be unique and the placed patch must fit its page.</summary>
    public required FileSelectPatchCellDocument[] Cells { get; init; }
}

/// <summary>One authored glyph or artwork cell in a sparse file-select tilemap patch.</summary>
public sealed record FileSelectPatchCellDocument
{
    /// <summary>Nonnegative horizontal tile offset from the patch's placement anchor.</summary>
    public required int X { get; init; }
    /// <summary>Nonnegative vertical tile offset from the patch's placement anchor.</summary>
    public required int Y { get; init; }
    /// <summary>Character-sheet coordinates and palette, priority, and flip attributes compiled into the destination tilemap word.</summary>
    public required MapPresentationCell Cell { get; init; }
}

/// <summary>Dynamic display-field anchors for one save slot; all coordinates are tiles inside a 32-by-32 page.</summary>
public sealed record FileSelectSlotFieldDocument
{
    /// <summary>Origin of the ENERGY label patch; the engine also derives energy-tank placement from this anchor.</summary>
    public required MapLabelPoint EnergyAnchor { get; init; }
    /// <summary>Origin of the two decimal health digits, written at horizontal offsets zero and one.</summary>
    public required MapLabelPoint HealthAnchor { get; init; }
    /// <summary>Origin of the empty-slot NO DATA patch, used when the slot contains no valid save.</summary>
    public required MapLabelPoint NoDataAnchor { get; init; }
    /// <summary>Origin of the five-tile HH:MM field: hour digits at offsets 0-1, colon at 2, and minute digits at 3-4.</summary>
    public required MapLabelPoint TimeValueAnchor { get; init; }
}

/// <summary>Provides sparse tilemap patch cells from authored data or the matching stock definition.</summary>
internal sealed class FileSelectCompiledPatch
{
    /// <summary>Schema patch identity used to calculate its stock cells when no edits are present.</summary>
    private readonly string name;
    /// <summary>Authored cells retained when their count or contents differ from the stock patch.</summary>
    private readonly FileSelectCompiledPatchCell[]? suppliedCells;

    /// <summary>Stores authored patch cells only when they differ from the calculated stock patch.</summary>
    /// <param name="name">Required patch identity that determines the stock cell sequence.</param>
    /// <param name="cells">Compiled cells supplied by the loaded document.</param>
    internal FileSelectCompiledPatch(string name, FileSelectCompiledPatchCell[] cells)
    {
        this.name = name;
        if (cells.Length != FileSelectPresentationDefinitions.PatchCellCount(name))
        {
            suppliedCells = cells;
            return;
        }
        for (int index = 0; index < cells.Length; index++)
            if (cells[index] != FileSelectPresentationDefinitions.PatchCell(name, index))
            {
                suppliedCells = cells;
                return;
            }
    }

    /// <summary>Number of sparse cells in the authored patch or its stock equivalent.</summary>
    internal int Count => suppliedCells?.Length ?? FileSelectPresentationDefinitions.PatchCellCount(name);
    /// <summary>Gets a patch cell from retained artwork or computes the matching stock entry.</summary>
    /// <param name="index">Zero-based cell index in the patch.</param>
    /// <returns>The tile offset and compiled tilemap word.</returns>
    internal FileSelectCompiledPatchCell Cell(int index) => suppliedCells is null
        ? FileSelectPresentationDefinitions.PatchCell(name, index) : suppliedCells[index];
}
/// <summary>One compiled sparse-patch cell in tile offsets relative to the patch anchor.</summary>
/// <param name="X">Horizontal tile offset from the patch placement anchor.</param>
/// <param name="Y">Vertical tile offset from the patch placement anchor.</param>
/// <param name="Word">Packed SNES tilemap word written at the resulting page coordinate.</param>
internal readonly record struct FileSelectCompiledPatchCell(int X, int Y, ushort Word);

/// <summary>Schema names and fixed dimensions for <c>file-select.json</c>.</summary>
public static class FileSelectPresentationDefinitions
{
    /// <summary>$81:B4AC-B4C0, the literal small-font empty-slot label including its padding.</summary>
    private const string NoDataLabel = " NO DATA   ";
    /// <summary>$81:B4B6: tile $6A is the small-font A; following alphabet glyphs are consecutive.</summary>
    private const ushort SmallLetterA = 0x206a;
    /// <summary>$81:B4B2 and B4BC-B4C0: the small-font paletted space.</summary>
    private const ushort SmallSpace = 0x200f;
    /// <summary>$81:B496-B49A: three consecutive tiles composing the ENERGY label.</summary>
    private const ushort EnergyLabelFirst = 0x209d;
    /// <summary>$81:B49C: the separately placed final glyph of the ENERGY patch.</summary>
    private const ushort EnergyLabelFinal = 0x20cc;
    /// <summary>$81:B4A8: the time-field colon glyph.</summary>
    private const ushort TimeColonGlyph = 0x208c;

    /// <summary>Native one-row patch extents at $81:B496, B4A8 and B4AC, excluding terminators.</summary>
    internal static int PatchCellCount(string name) => name switch
    {
        EnergyPatch => 4,
        TimeColonPatch => 1,
        NoDataPatch => NoDataLabel.Length,
        _ => throw new InvalidDataException($"Unknown file-select patch {name}."),
    };

    /// <summary>Calculates text glyph selection and left-to-right cell placement for one native patch.</summary>
    internal static FileSelectCompiledPatchCell PatchCell(string name, int index)
    {
        if ((uint)index >= PatchCellCount(name)) throw new IndexOutOfRangeException();
        ushort word = name switch
        {
            EnergyPatch => index < 3 ? (ushort)(EnergyLabelFirst + index) : EnergyLabelFinal,
            TimeColonPatch => TimeColonGlyph,
            NoDataPatch => index == 0 ? FileSelectLayout.BlankTile
                : NoDataLabel[index] == ' ' ? SmallSpace
                : (ushort)(SmallLetterA + NoDataLabel[index] - 'A'),
            _ => throw new InvalidDataException($"Unknown file-select patch {name}."),
        };
        return new(index, 0, word);
    }

    /// <summary>Supported file-select presentation JSON schema version.</summary>
    public const int Version = 1;
    /// <summary>Extracted JSON filename for menu pages, fields, sprite compositions, and layout.</summary>
    public const string FileName = "file-select.json";
    /// <summary>Width of each menu tilemap page in tile cells.</summary>
    public const int Width = 32;
    /// <summary>Height of each menu tilemap page in tile cells.</summary>
    public const int Height = 32;
    /// <summary>Number of row-major tile cells required in each menu page.</summary>
    public const int CellCount = Width * Height;
    /// <summary>Width in characters of the sheet addressed by authored background tile cells.</summary>
    public const int CharacterColumns = 32;
    /// <summary>Height in characters of the sheet addressed by authored background tile cells.</summary>
    public const int CharacterRows = 32;
    /// <summary>Ten consecutive digit cells beginning at FileSelectLayout.DigitTileBase.</summary>
    public const int DigitCount = 10;
    /// <summary>Three consecutive save-slot letters beginning at FileSelectLayout.SamusLetterTileBase.</summary>
    public const int SlotLetterCount = 3;

    /// <summary>Schema identity of the common BG2 background tilemap, separate from foreground menu pages.</summary>
    public const string BackgroundPage = "Background";
    /// <summary>Schema identity of the main foreground page used when at least one save slot contains data.</summary>
    public const string MainWithDataPage = "Main.WithData";
    /// <summary>Schema identity of the main foreground page used when every save slot is empty.</summary>
    public const string MainEmptyPage = "Main.Empty";
    /// <summary>Schema identity of the copy page for choosing the source save slot.</summary>
    public const string CopySourcePage = "Copy.Source";
    /// <summary>Schema identity of the copy page for choosing a destination, with a dynamic source-slot letter.</summary>
    public const string CopyDestinationPage = "Copy.Destination";
    /// <summary>Schema identity of the copy confirmation page, displaying both selected slot letters.</summary>
    public const string CopyConfirmPage = "Copy.Confirm";
    /// <summary>Schema identity of the completed-copy page, retaining source and destination slot-letter fields.</summary>
    public const string CopyCompletedPage = "Copy.Completed";
    /// <summary>Schema identity of the clear page for choosing the save slot to erase.</summary>
    public const string ClearSelectionPage = "Clear.Selection";
    /// <summary>Schema identity of the clear confirmation page with its selected slot-letter field.</summary>
    public const string ClearConfirmPage = "Clear.Confirm";
    /// <summary>Schema identity of the completed-clear page with its selected slot-letter field.</summary>
    public const string ClearCompletedPage = "Clear.Completed";

    /// <summary>Schema identity of the ENERGY label patch; stock glyphs are calculated from the native label at $81:B496.</summary>
    public const string EnergyPatch = "Energy";
    /// <summary>Schema identity of the padded empty-slot NO DATA patch corresponding to $81:B4AC-B4C0.</summary>
    public const string NoDataPatch = "NoData";
    /// <summary>Schema identity of the time-field colon patch corresponding to the native glyph at $81:B4A8.</summary>
    public const string TimeColonPatch = "TimeColon";
    /// <summary>Layout identity of the main-menu border anchor and corresponding border sprite composition.</summary>
    public const string MainBorder = "Main";
    /// <summary>Layout identity of the copy-menu border anchor and corresponding border sprite composition.</summary>
    public const string CopyBorder = "Copy";
    /// <summary>Layout identity of the clear-menu border anchor and corresponding border sprite composition.</summary>
    public const string ClearBorder = "Clear";
    /// <summary>Tile-anchor identity for the source-slot letter on the copy destination-selection page.</summary>
    public const string CopyDestinationSourceAnchor = "Copy.Destination.Source";
    /// <summary>Tile-anchor identity for the source-slot letter on copy confirmation and completed-copy pages.</summary>
    public const string CopyConfirmSourceAnchor = "Copy.Confirm.Source";
    /// <summary>Tile-anchor identity for the destination-slot letter on copy confirmation and completed-copy pages.</summary>
    public const string CopyConfirmDestinationAnchor = "Copy.Confirm.Destination";
    /// <summary>Tile-anchor identity for the erased-slot letter on clear confirmation and completed-clear pages.</summary>
    public const string ClearConfirmSourceAnchor = "Clear.Confirm.Source";

    /// <summary>Exact required schema identities, enumerated in the original definition order.
    /// Each case names a distinct menu page or asset role; these are not numerical samples.
    /// The enumerators evaluate cases directly and retain no generated name arrays.</summary>
    public static IEnumerable<string> PageNames => Names(10, PageName);
    /// <summary>Enumerates the three exact required patch identities in energy, empty-slot, and time-colon order.</summary>
    public static IEnumerable<string> PatchNames => Names(3, PatchName);
    /// <summary>Enumerates the three exact required border layout identities in main, copy, and clear order.</summary>
    public static IEnumerable<string> BorderNames => Names(3, BorderName);
    /// <summary>Enumerates the four exact required tile-anchor identities for dynamic copy and clear slot letters.</summary>
    public static IEnumerable<string> DynamicAnchorNames => Names(4, DynamicAnchorName);
    /// <summary>Enumerates fifteen required sprite identities: three borders, four cursor frames, then eight helmet frames.</summary>
    public static IEnumerable<string> SpriteNames => Names(15, SpriteName);

    /// <summary>Returns a required page identity in the fixed schema enumeration order.</summary>
    /// <param name="index">Zero-based page index 0-9.</param>
    /// <returns>The corresponding background, main, copy, or clear page key.</returns>
    /// <exception cref="ArgumentOutOfRangeException">The index is outside 0-9.</exception>
    public static string PageName(int index) => index switch
    {
        0 => BackgroundPage,
        1 => MainWithDataPage,
        2 => MainEmptyPage,
        3 => CopySourcePage,
        4 => CopyDestinationPage,
        5 => CopyConfirmPage,
        6 => CopyCompletedPage,
        7 => ClearSelectionPage,
        8 => ClearConfirmPage,
        9 => ClearCompletedPage,
        _ => throw new ArgumentOutOfRangeException(nameof(index)),
    };
    /// <summary>Returns the patch identity at a fixed schema index.</summary>
    /// <param name="index">Zero-based index 0-2, selecting energy, empty-slot, or time-colon artwork.</param>
    /// <returns>The required patch dictionary key.</returns>
    /// <exception cref="ArgumentOutOfRangeException">The index is outside 0-2.</exception>
    public static string PatchName(int index) => index switch
    {
        0 => EnergyPatch,
        1 => NoDataPatch,
        2 => TimeColonPatch,
        _ => throw new ArgumentOutOfRangeException(nameof(index)),
    };
    /// <summary>Returns the border layout identity at a fixed schema index.</summary>
    /// <param name="index">Zero-based index 0-2, selecting main, copy, or clear.</param>
    /// <returns>The required border-anchor key, without the sprite's <c>Border.</c> prefix.</returns>
    /// <exception cref="ArgumentOutOfRangeException">The index is outside 0-2.</exception>
    public static string BorderName(int index) => index switch
    {
        0 => MainBorder,
        1 => CopyBorder,
        2 => ClearBorder,
        _ => throw new ArgumentOutOfRangeException(nameof(index)),
    };
    /// <summary>Returns a required copy or clear slot-letter tile-anchor identity in fixed schema order.</summary>
    /// <param name="index">Zero-based dynamic-anchor index 0-3.</param>
    /// <returns>The required dynamic-anchor dictionary key.</returns>
    /// <exception cref="ArgumentOutOfRangeException">The index is outside 0-3.</exception>
    public static string DynamicAnchorName(int index) => index switch
    {
        0 => CopyDestinationSourceAnchor,
        1 => CopyConfirmSourceAnchor,
        2 => CopyConfirmDestinationAnchor,
        3 => ClearConfirmSourceAnchor,
        _ => throw new ArgumentOutOfRangeException(nameof(index)),
    };
    /// <summary>Returns a required sprite composition identity in border, cursor, then helmet order.</summary>
    /// <param name="index">Zero-based index: 0-2 for borders, 3-6 for cursor frames, or 7-14 for helmet frames.</param>
    /// <returns>The prefixed sprite dictionary key.</returns>
    /// <exception cref="ArgumentOutOfRangeException">The index is outside 0-14.</exception>
    public static string SpriteName(int index) => index switch
    {
        >= 0 and < 3 => BorderFrameName(BorderName(index)),
        >= 3 and < 7 => CursorFrameName(index - 3),
        >= 7 and < 15 => HelmetFrameName(index - 7),
        _ => throw new ArgumentOutOfRangeException(nameof(index)),
    };
    /// <summary>Lazily yields indexed schema names without allocating a backing name array.</summary>
    /// <param name="count">Number of names to enumerate.</param>
    /// <param name="name">Function mapping each zero-based index to its required key.</param>
    /// <returns>The names for indices from zero through <paramref name="count"/> minus one.</returns>
    private static IEnumerable<string> Names(int count, Func<int, string> name)
    {
        for (int index = 0; index < count; index++) yield return name(index);
    }
    /// <summary>Prefixes a border layout name to form its sprite composition key, without validating the name.</summary>
    /// <param name="name">Border layout name, normally main, copy, or clear as defined by the schema.</param>
    /// <returns>The supplied name prefixed with <c>Border.</c>.</returns>
    public static string BorderFrameName(string name) => $"Border.{name}";
    /// <summary>Forms the sprite composition key for one of the four looping missile-cursor frames.</summary>
    /// <param name="frame">Zero-based animation frame 0-3.</param>
    /// <returns><c>Cursor.</c> followed by the frame number.</returns>
    /// <exception cref="ArgumentOutOfRangeException">The frame is outside 0-3.</exception>
    public static string CursorFrameName(int frame) => (uint)frame < 4
        ? $"Cursor.{frame}" : throw new ArgumentOutOfRangeException(nameof(frame));
    /// <summary>Forms the sprite composition key for one of the eight selected-slot helmet frames.</summary>
    /// <param name="frame">Zero-based animation frame 0-7.</param>
    /// <returns><c>Helmet.</c> followed by the frame number.</returns>
    /// <exception cref="ArgumentOutOfRangeException">The frame is outside 0-7.</exception>
    public static string HelmetFrameName(int frame) => (uint)frame < 8
        ? $"Helmet.{frame}" : throw new ArgumentOutOfRangeException(nameof(frame));
}
