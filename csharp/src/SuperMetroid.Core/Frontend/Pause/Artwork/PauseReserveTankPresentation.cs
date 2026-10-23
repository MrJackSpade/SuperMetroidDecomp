using SuperMetroid.Core.Frontend.Map.Artwork;
using System.Collections.Immutable;
using System.Text.Json;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend.Menus.Artwork;

namespace SuperMetroid.Core.Frontend.Pause.Artwork;

/// <summary>Editable reserve-strip artwork and origins, independent of reserve energy and fill selection.</summary>
public sealed class PauseReserveTankPresentation
{
    /// <summary>Per-coordinate deviations from the six stock reserve-strip anchors.</summary>
    internal readonly Dictionary<int, AnchorOverride> anchorOverrides;
    /// <summary>Edited sprite compositions for the ten native reserve visual roles.</summary>
    internal readonly FrameSet frames;
    /// <summary>Object attributes selecting the OBJ palette applied to every reserve-strip part, with no tile, priority or flip.</summary>
    private readonly SnesObjAttributeWord objectPalette;
    /// <summary>Stores independent anchor edits, frame art, and selected reserve palette.</summary>
    private PauseReserveTankPresentation(Dictionary<int, AnchorOverride> anchorOverrides, FrameSet frames, int palette)
    { this.anchorOverrides = anchorOverrides; this.frames = frames; objectPalette = SnesObjAttributeWord.Create(0, palette, 0); }

    /// <summary>Gets one reserve-strip screen anchor, falling back independently to each stock coordinate.</summary>
    /// <param name="index">Zero-based tank or trailing-cap position from 0 through 5.</param>
    /// <returns>The configured screen-pixel anchor for that strip position.</returns>
    public Position16<OamPixels> Anchor(int index)
    {
        var basis = PauseReserveTankDefinitions.StockAnchor(index);
        return anchorOverrides.TryGetValue(index, out var value) ? new(checked((ushort)(value.X ?? basis.X)), checked((ushort)(value.Y ?? basis.Y))) : basis;
    }
    /// <summary>The OBJ palette row the strip draws in.</summary>
    internal int Palette => objectPalette.PaletteIndex;

    /// <summary>Draws one full, empty, partially filled, or end-cap reserve frame of the pause sheet at a strip position.</summary>
    /// <param name="entries">The screen's sprite entries.</param>
    /// <param name="sheet">The pause sheet.</param>
    /// <param name="visual">The one of the ten reserve visuals to draw.</param>
    /// <param name="index">Zero-based tank or trailing-cap anchor index.</param>
    /// <exception cref="InvalidOperationException">The native visual identity is unknown.</exception>
    public void Draw(ScreenSprites entries, SpriteSheet sheet, PauseReserveTankVisual visual, int index)
    {
        // The strip position is checked before the visual.
        Position16<OamPixels> anchor = Anchor(index);
        string name = PauseReserveTankDefinitions.Frames().Where(frame => frame.Id == visual).Select(frame => frame.Name)
            .SingleOrDefault() ?? throw new InvalidOperationException($"Undefined PauseReserveTankVisual {visual}.");
        Ensure.NotNull(entries).AddImage(Ensure.NotNull(sheet).Frame(PauseReserveTankDefinitions.SheetFrameName(name)), anchor);
    }

    /// <summary>
    /// One reserve visual as a whole frame (#1392): its authored composition, or the stock single character, which takes
    /// its draw's palette.
    /// </summary>
    internal SpriteFrame ToFrame(PauseReserveTankVisual visual, string frameName) =>
        Composition(visual) is { } composition ? composition.ToFrame(frameName, 0)
            : new(frameName, [ObjPartFrames.Part(SnesSpritemapXWord.Create(0, false), new SnesSpritemapYByte(0),
                SnesObjAttributeWord.Create(PauseReserveTankDefinitions.StockTile(visual), 0, 3), 0, ObjPartFrames.StandardSizes,
                inheritPalette: true)]);

    /// <summary>The authored composition of one reserve visual, or null where the stock character stands.</summary>
    private SpriteComposition? Composition(PauseReserveTankVisual visual) => visual switch
        {
            PauseReserveTankVisual.Full => frames.Full,
            PauseReserveTankVisual.EndCap => frames.EndCap,
            PauseReserveTankVisual.Empty => frames.Empty,
            PauseReserveTankVisual.Fill1 => frames.Fills[0],
            PauseReserveTankVisual.Fill2 => frames.Fills[1],
            PauseReserveTankVisual.Fill3 => frames.Fills[2],
            PauseReserveTankVisual.Fill4 => frames.Fills[3],
            PauseReserveTankVisual.Fill5 => frames.Fills[4],
            PauseReserveTankVisual.Fill6 => frames.Fills[5],
            PauseReserveTankVisual.Fill7 => frames.Fills[6],
            _ => throw new InvalidOperationException($"Undefined PauseReserveTankVisual {visual}."),
        };
    /// <summary>Loads and validates the reserve-strip palette, six anchors, and ten named sprite frames.</summary>
    /// <param name="json">The caller-owned stream containing the editable presentation document.</param>
    /// <returns>The validated reserve-tank presentation.</returns>
    /// <exception cref="InvalidDataException">The JSON, version, palette, anchors, or frames are invalid.</exception>
    public static PauseReserveTankPresentation Load(Stream json)
    {
        PauseReserveTankDocument document;
        try { document = JsonAssetDocument.Read<PauseReserveTankDocument>(json, ArtworkJson.Options)
            ?? throw new InvalidDataException("Reserve tank document is null."); }
        catch (JsonException error) { throw new InvalidDataException("Invalid reserve tank JSON.", error); }
        if (document.Version != PauseReserveTankDefinitions.Version || (uint)document.Palette > 7 ||
            document.Anchors is null || document.Anchors.Length != PauseReserveTankDefinitions.AnchorCount ||
            document.Frames is null || document.Frames.Count != PauseReserveTankDefinitions.Frames().Count())
            throw new InvalidDataException("Reserve tanks require version 1, palette 0..7, six anchors and ten named frames.");
        foreach (var point in document.Anchors)
            if (point.X is < 0 or > 255 || point.Y is < 0 or > 223)
                throw new InvalidDataException("Reserve tank anchor requires screen coordinates X=0..255/Y=0..223.");
        var frames = new FrameSet(Require("Full", PauseReserveTankVisual.Full), Require("EndCap", PauseReserveTankVisual.EndCap), Require("Empty", PauseReserveTankVisual.Empty),
            [Require("Fill1", PauseReserveTankVisual.Fill1), Require("Fill2", PauseReserveTankVisual.Fill2), Require("Fill3", PauseReserveTankVisual.Fill3), Require("Fill4", PauseReserveTankVisual.Fill4), Require("Fill5", PauseReserveTankVisual.Fill5), Require("Fill6", PauseReserveTankVisual.Fill6), Require("Fill7", PauseReserveTankVisual.Fill7)]);
        SpriteComposition? Require(string name, PauseReserveTankVisual identity)
        {
            if (!document.Frames.TryGetValue(name, out var parts) || parts is null)
                throw new InvalidDataException($"Missing reserve tank frame {name}.");
            return parts.Length == 1 && parts[0] == PauseReserveTankDefinitions.StockPart(identity)
                ? null : MenuSpriteCompiler.Compile(parts, name);
        }
        var anchorOverrides = new Dictionary<int, AnchorOverride>();
        for (int index = 0; index < document.Anchors.Length; index++)
        {
            var point = document.Anchors[index];
            var basis = PauseReserveTankDefinitions.StockAnchor(index);
            if (point.X != basis.X || point.Y != basis.Y)
                anchorOverrides.Add(index, new AnchorOverride(point.X == basis.X ? null : point.X, point.Y == basis.Y ? null : point.Y));
        }
        return new(anchorOverrides, frames, document.Palette);
    }
    /// <summary>Distinct reserve strip roles selected by the native fill renderer.</summary>
    /// <param name="Full">The full strip.</param>
    /// <param name="EndCap">The strip's end cap.</param>
    /// <param name="Empty">The empty strip.</param>
    /// <param name="Fills">The seven partial-fill frames, indexed by fill level less one.</param>
    internal sealed record FrameSet(SpriteComposition? Full, SpriteComposition? EndCap, SpriteComposition? Empty,
        ImmutableArray<SpriteComposition?> Fills);
    /// <summary>Validates and writes an editable reserve-tank presentation document as JSON.</summary>
    /// <param name="output">The caller-owned destination stream.</param>
    /// <param name="document">The presentation document to validate and serialize.</param>
    public static void Write(Stream output, PauseReserveTankDocument document)
    {
        byte[] bytes = JsonSerializer.SerializeToUtf8Bytes(document, ArtworkJson.Options);
        _ = Load(new MemoryStream(bytes, writable: false)); output.Write(bytes);
    }
}
