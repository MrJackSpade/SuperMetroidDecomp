using SuperMetroid.Core.Frontend;
using System.Text.Json;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Assets;

/// <summary>Editable reserve-strip artwork and origins, independent of reserve energy and fill selection.</summary>
public sealed class PauseReserveTankPresentation
{
    private readonly Dictionary<int, (int? X, int? Y)> anchorOverrides;
    private readonly FrameSet frames;
    private readonly ushort paletteBits;
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
    public void Draw(OamBuffer oam, PauseReserveTankVisual nativeIdentity, int index)
    {
        var anchor = Anchor(index);
        var frame = nativeIdentity switch
        {
            PauseReserveTankVisual.Full => frames.Full,
            PauseReserveTankVisual.EndCap => frames.EndCap,
            PauseReserveTankVisual.Empty => frames.Empty,
            PauseReserveTankVisual.Fill1 => frames.Fill1,
            PauseReserveTankVisual.Fill2 => frames.Fill2,
            PauseReserveTankVisual.Fill3 => frames.Fill3,
            PauseReserveTankVisual.Fill4 => frames.Fill4,
            PauseReserveTankVisual.Fill5 => frames.Fill5,
            PauseReserveTankVisual.Fill6 => frames.Fill6,
            PauseReserveTankVisual.Fill7 => frames.Fill7,
            _ => throw new InvalidOperationException($"Undefined PauseReserveTankVisual {nativeIdentity}."),
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
        var frames = new FrameSet(Require("Full", PauseReserveTankVisual.Full), Require("EndCap", PauseReserveTankVisual.EndCap), Require("Empty", PauseReserveTankVisual.Empty),
            Require("Fill1", PauseReserveTankVisual.Fill1), Require("Fill2", PauseReserveTankVisual.Fill2), Require("Fill3", PauseReserveTankVisual.Fill3), Require("Fill4", PauseReserveTankVisual.Fill4), Require("Fill5", PauseReserveTankVisual.Fill5), Require("Fill6", PauseReserveTankVisual.Fill6), Require("Fill7", PauseReserveTankVisual.Fill7));
        SpriteComposition? Require(string name, PauseReserveTankVisual identity)
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
