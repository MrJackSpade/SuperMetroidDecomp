using System.Text.Json;
using SuperMetroid.AssetExtraction.Frontend.Menus;
using SuperMetroid.Core;
using SuperMetroid.Core.Frontend.Pause;
using SuperMetroid.Core.Frontend.Pause.Artwork;
using SuperMetroid.Core.Spatial;
using SuperMetroid.AssetExtraction.Artwork;

namespace SuperMetroid.AssetExtraction.Frontend.Pause.Artwork;

/// <summary>
/// The reserve-strip presentation as the import pipeline reads it (#1463, #1638), from the extracted artwork document in the
/// component's import data: the anchors (<see cref="Presentation"/>), which the installed layout holds for the game
/// (<see cref="PauseReserveTankLayoutFile"/>), and the reserve visuals' compositions the pause sheet is composed from, which
/// the game never receives (#1616).
/// </summary>
public sealed class PauseReserveTankSourcePresentation
{
    /// <summary>The installed composition of each of the ten reserve visual roles.</summary>
    private readonly Dictionary<PauseReserveTankVisual, SpriteComposition> frames;

    /// <summary>Stores the installed anchors, frame art, and the sprite set the strip's frames are composed in.</summary>
    private PauseReserveTankSourcePresentation(Position16<SpritePixels>[] anchors,
        Dictionary<PauseReserveTankVisual, SpriteComposition> frames, int spriteSet)
    {
        Presentation = new PauseReserveTankPresentation(anchors);
        this.frames = frames;
        SpriteSet = spriteSet;
    }

    /// <summary>The strip anchors the game binds.</summary>
    public PauseReserveTankPresentation Presentation { get; }

    /// <summary>The sprite set the pause sheet composes every reserve-strip frame in.</summary>
    internal int SpriteSet { get; }

    /// <summary>One reserve visual's installed composition as a whole frame (#1392).</summary>
    internal SpriteFrame ToFrame(PauseReserveTankVisual visual, string frameName) =>
        frames.TryGetValue(visual, out SpriteComposition? composition) ? composition.ToFrame(frameName, 0)
            : throw new InvalidOperationException($"Undefined PauseReserveTankVisual {visual}.");

    /// <summary>Loads and validates the reserve-strip palette, six anchors, and ten named sprite frames.</summary>
    /// <param name="json">The caller-owned stream containing the editable presentation document.</param>
    /// <returns>The validated reserve-tank presentation.</returns>
    /// <exception cref="InvalidDataException">The JSON, version, palette, anchors, or frames are invalid.</exception>
    public static PauseReserveTankSourcePresentation Load(Stream json)
    {
        PauseReserveTankDocument document;
        try { document = JsonAssetDocument.Read<PauseReserveTankDocument>(json, ArtworkJson.Options)
            ?? throw new InvalidDataException("Reserve tank document is null."); }
        catch (JsonException error) { throw new InvalidDataException("Invalid reserve tank JSON.", error); }
        if (document.Version != PauseReserveTankFormat.Version || (uint)document.Palette > 7 ||
            document.Frames is null || document.Frames.Count != PauseReserveTankParts.Frames().Count())
            throw new InvalidDataException("Reserve tanks require version 1, palette 0..7, six anchors and ten named frames.");
        Position16<SpritePixels>[] anchors = PauseReserveTankLayoutFile.Anchors(document.Anchors);
        var frames = new Dictionary<PauseReserveTankVisual, SpriteComposition>();
        foreach (var frame in PauseReserveTankParts.Frames())
        {
            if (!document.Frames.TryGetValue(frame.Name, out var parts) || parts is null)
                throw new InvalidDataException($"Missing reserve tank frame {frame.Name}.");
            frames.Add(frame.Id, MenuSpriteCompiler.Compile(parts, frame.Name));
        }
        return new(anchors, frames, document.Palette);
    }

    /// <summary>Writes an editable reserve-tank presentation document as JSON.</summary>
    /// <param name="output">The caller-owned destination stream.</param>
    /// <param name="document">The presentation document to serialize.</param>
    public static void Write(Stream output, PauseReserveTankDocument document)
    {
        byte[] bytes = JsonSerializer.SerializeToUtf8Bytes(document, ArtworkJson.Options);
        output.Write(bytes);
    }
}
