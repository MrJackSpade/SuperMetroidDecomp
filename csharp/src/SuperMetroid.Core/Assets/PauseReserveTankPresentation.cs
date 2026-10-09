using SuperMetroid.Core.Frontend;
using System.Text.Json;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Assets;

/// <summary>Editable reserve-strip artwork and origins, independent of reserve energy and fill selection.</summary>
public sealed class PauseReserveTankPresentation
{
    /// <summary>Only anchor coordinates that differ from their stock positions.</summary>
    private readonly Dictionary<int, (int? X, int? Y)> anchorOverrides;
    /// <summary>Compiled sprite compositions for the reserve strip's ten visual roles.</summary>
    private readonly FrameSet frames;
    /// <summary>OBJ palette bits applied when reserve-strip sprite parts are drawn.</summary>
    private readonly ushort paletteBits;

    /// <summary>Creates a presentation from validated anchor overrides, sprite frames, and palette selection.</summary>
    /// <param name="anchorOverrides">Per-anchor coordinates that replace the corresponding stock components.</param>
    /// <param name="frames">Compiled compositions or stock-frame markers for each reserve visual role.</param>
    /// <param name="palette">OBJ palette index selected by the presentation document.</param>
    private PauseReserveTankPresentation(Dictionary<int, (int? X, int? Y)> anchorOverrides, FrameSet frames, int palette)
    { this.anchorOverrides = anchorOverrides; this.frames = frames; paletteBits = SnesObjAttributeWord.Create(0, palette, 0).PaletteBits; }

    /// <summary>Gets one reserve-strip screen anchor, falling back independently to each stock coordinate.</summary>
    /// <param name="index">Zero-based tank or trailing-cap position from 0 through 5.</param>
    /// <returns>The configured screen-pixel anchor for that strip position.</returns>
    public MapLabelPoint Anchor(int index)
    {
        var basis = PauseReserveTankDefinitions.StockAnchor(index);
        return anchorOverrides.TryGetValue(index, out var value) ? new(value.X ?? basis.X, value.Y ?? basis.Y) : basis;
    }
    /// <summary>Draws one full, empty, partially filled, or end-cap reserve visual at a strip position.</summary>
    /// <param name="oam">The OAM buffer that receives the authored or stock sprite parts.</param>
    /// <param name="nativeIdentity">The native bank-$82 spritemap identity selecting one of the ten visual roles.</param>
    /// <param name="index">Zero-based tank or trailing-cap anchor index.</param>
    /// <exception cref="InvalidDataException">The native visual identity is unknown.</exception>
    public void Draw(OamBuffer oam, ushort nativeIdentity, int index)
    {
        var anchor = Anchor(index);
        var frame = nativeIdentity switch
        {
            PauseReserveTankRomData.FullMap => frames.Full,
            PauseReserveTankRomData.EndCapMap => frames.EndCap,
            PauseReserveTankRomData.EmptyMap => frames.Empty,
            PauseReserveTankRomData.EmptyMap + 1 => frames.Fill1,
            PauseReserveTankRomData.EmptyMap + 2 => frames.Fill2,
            PauseReserveTankRomData.EmptyMap + 3 => frames.Fill3,
            PauseReserveTankRomData.EmptyMap + 4 => frames.Fill4,
            PauseReserveTankRomData.EmptyMap + 5 => frames.Fill5,
            PauseReserveTankRomData.EmptyMap + 6 => frames.Fill6,
            PauseReserveTankRomData.EmptyMap + 7 => frames.Fill7,
            _ => throw new InvalidDataException($"Unknown reserve visual {nativeIdentity:X4}."),
        };
        if (frame is not null) frame.DrawOnScreen(oam, (ushort)anchor.X, (ushort)anchor.Y, paletteBits);
        else oam.AddOnScreenSpritePart(SnesSpritemapXWord.Create(0, false), 0,
            SnesObjAttributeWord.Create(PauseReserveTankDefinitions.StockTile(nativeIdentity), 0, 3).WithPaletteBits(paletteBits),
            (ushort)anchor.X, (ushort)anchor.Y);
    }
    /// <summary>Loads and validates the reserve-strip palette, six anchors, and ten named sprite frames.</summary>
    /// <param name="json">The caller-owned stream containing the editable presentation document.</param>
    /// <returns>The validated reserve-tank presentation.</returns>
    /// <exception cref="InvalidDataException">The JSON, version, palette, anchors, or frames are invalid.</exception>
    public static PauseReserveTankPresentation Load(Stream json)
    {
        PauseReserveTankDocument document;
        try { document = JsonAssetDocument.Read<PauseReserveTankDocument>(json, MapPresentationFormat.JsonOptions)
            ?? throw new InvalidDataException("Reserve tank document is null."); }
        catch (JsonException error) { throw new InvalidDataException("Invalid reserve tank JSON.", error); }
        if (document.Version != PauseReserveTankDefinitions.Version || (uint)document.Palette > 7 ||
            document.Anchors is null || document.Anchors.Length != PauseReserveTankDefinitions.AnchorCount ||
            document.Frames is null || document.Frames.Count != PauseReserveTankDefinitions.Frames().Count())
            throw new InvalidDataException("Reserve tanks require version 1, palette 0..7, six anchors and ten named frames.");
        foreach (var point in document.Anchors)
            if (point is null || point.X is < 0 or > 255 || point.Y is < 0 or > 223)
                throw new InvalidDataException("Reserve tank anchor requires screen coordinates X=0..255/Y=0..223.");
        var frames = new FrameSet(Require("Full", PauseReserveTankRomData.FullMap), Require("EndCap", PauseReserveTankRomData.EndCapMap), Require("Empty", PauseReserveTankRomData.EmptyMap),
            Require("Fill1", PauseReserveTankRomData.EmptyMap + 1), Require("Fill2", PauseReserveTankRomData.EmptyMap + 2), Require("Fill3", PauseReserveTankRomData.EmptyMap + 3), Require("Fill4", PauseReserveTankRomData.EmptyMap + 4), Require("Fill5", PauseReserveTankRomData.EmptyMap + 5), Require("Fill6", PauseReserveTankRomData.EmptyMap + 6), Require("Fill7", PauseReserveTankRomData.EmptyMap + 7));
        SpriteComposition? Require(string name, ushort identity)
        {
            if (!document.Frames.TryGetValue(name, out var parts) || parts is null)
                throw new InvalidDataException($"Missing reserve tank frame {name}.");
            return parts.Length == 1 && parts[0] == PauseReserveTankDefinitions.StockPart(identity)
                ? null : MenuSpriteCompiler.Compile(parts, name);
        }
        var anchorOverrides = new Dictionary<int, (int? X, int? Y)>();
        for (int index = 0; index < document.Anchors.Length; index++)
        {
            var point = document.Anchors[index];
            var basis = PauseReserveTankDefinitions.StockAnchor(index);
            if (point.X != basis.X || point.Y != basis.Y)
                anchorOverrides.Add(index, (point.X == basis.X ? null : point.X, point.Y == basis.Y ? null : point.Y));
        }
        return new(anchorOverrides, frames, document.Palette);
    }
    /// <summary>Distinct reserve strip roles selected by the native fill renderer.</summary>
    /// <param name="Full">Composition for a full reserve tank.</param>
    /// <param name="EndCap">Composition for the strip's trailing cap.</param>
    /// <param name="Empty">Composition for an empty tank.</param>
    /// <param name="Fill1">Composition for the first partial-fill level.</param>
    /// <param name="Fill2">Composition for the second partial-fill level.</param>
    /// <param name="Fill3">Composition for the third partial-fill level.</param>
    /// <param name="Fill4">Composition for the fourth partial-fill level.</param>
    /// <param name="Fill5">Composition for the fifth partial-fill level.</param>
    /// <param name="Fill6">Composition for the sixth partial-fill level.</param>
    /// <param name="Fill7">Composition for the seventh partial-fill level.</param>
    private sealed record FrameSet(SpriteComposition? Full, SpriteComposition? EndCap, SpriteComposition? Empty,
        SpriteComposition? Fill1, SpriteComposition? Fill2, SpriteComposition? Fill3, SpriteComposition? Fill4,
        SpriteComposition? Fill5, SpriteComposition? Fill6, SpriteComposition? Fill7);
    /// <summary>Validates and writes an editable reserve-tank presentation document as JSON.</summary>
    /// <param name="output">The caller-owned destination stream.</param>
    /// <param name="document">The presentation document to validate and serialize.</param>
    public static void Write(Stream output, PauseReserveTankDocument document)
    {
        byte[] bytes = JsonSerializer.SerializeToUtf8Bytes(document, MapPresentationFormat.JsonOptions);
        _ = Load(new MemoryStream(bytes, writable: false)); output.Write(bytes);
    }
}

/// <summary>Editable JSON schema for the pause screen's reserve-energy strip artwork and placement.</summary>
public sealed record PauseReserveTankDocument
{
    /// <summary>Gets the reserve-tank presentation schema version.</summary>
    public required int Version { get; init; }

    /// <summary>Gets the OBJ palette index, from zero through seven, applied to every reserve-strip part.</summary>
    public required int Palette { get; init; }

    /// <summary>Gets the six screen-pixel anchors for five tank positions and the trailing cap.</summary>
    public required MapLabelPoint[] Anchors { get; init; }

    /// <summary>Gets the ten named full, end-cap, empty, and partial-fill sprite compositions.</summary>
    public required Dictionary<string, SpriteVisualPart[]> Frames { get; init; }
}
