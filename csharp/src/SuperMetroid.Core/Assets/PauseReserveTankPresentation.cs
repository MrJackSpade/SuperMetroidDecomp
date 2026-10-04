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
    internal int StoredAnchorComponentCount => anchorOverrides.Values.Sum(value => (value.X.HasValue ? 1 : 0) + (value.Y.HasValue ? 1 : 0));
    public MapLabelPoint Anchor(int index)
    {
        var basis = PauseReserveTankDefinitions.StockAnchor(index);
        return anchorOverrides.TryGetValue(index, out var value) ? new(value.X ?? basis.X, value.Y ?? basis.Y) : basis;
    }
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
        frame.DrawOnScreen(oam, (ushort)anchor.X, (ushort)anchor.Y, paletteBits);
    }
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
        var frames = new FrameSet(Require("Full"), Require("EndCap"), Require("Empty"),
            Require("Fill1"), Require("Fill2"), Require("Fill3"), Require("Fill4"), Require("Fill5"), Require("Fill6"), Require("Fill7"));
        SpriteComposition Require(string name)
        {
            if (!document.Frames.TryGetValue(name, out var parts) || parts is null)
                throw new InvalidDataException($"Missing reserve tank frame {name}.");
            return MenuSpriteCompiler.Compile(parts, name);
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
    private sealed record FrameSet(SpriteComposition Full, SpriteComposition EndCap, SpriteComposition Empty,
        SpriteComposition Fill1, SpriteComposition Fill2, SpriteComposition Fill3, SpriteComposition Fill4,
        SpriteComposition Fill5, SpriteComposition Fill6, SpriteComposition Fill7);
    public static void Write(Stream output, PauseReserveTankDocument document)
    {
        byte[] bytes = JsonSerializer.SerializeToUtf8Bytes(document, MapPresentationFormat.JsonOptions);
        _ = Load(new MemoryStream(bytes, writable: false)); output.Write(bytes);
    }
}

public sealed record PauseReserveTankDocument
{
    public required int Version { get; init; }
    public required int Palette { get; init; }
    public required MapLabelPoint[] Anchors { get; init; }
    public required Dictionary<string, SpriteVisualPart[]> Frames { get; init; }
}
