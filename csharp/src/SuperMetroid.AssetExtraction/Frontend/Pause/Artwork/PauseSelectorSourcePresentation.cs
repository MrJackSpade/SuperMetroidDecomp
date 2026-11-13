using System.Text.Json;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.Frontend.Menus;
using SuperMetroid.Core.Frontend.Pause.Artwork;
using SuperMetroid.Core.Spatial;

namespace SuperMetroid.AssetExtraction.Frontend.Pause.Artwork;

/// <summary>
/// The equipment-selector presentation as the import pipeline reads it (#1463, #1638), from the extracted artwork document in
/// the component's import data: the anchors, frame sequences and timing (<see cref="Presentation"/>), which the installed
/// layout holds for the game (<see cref="PauseSelectorLayoutFile"/>), and each named frame's composition the pause sheet is
/// composed from, which the game never receives (#1616).
/// </summary>
public sealed class PauseSelectorSourcePresentation
{
    /// <summary>Each named frame's installed composition.</summary>
    private readonly Dictionary<string, SpriteComposition> visuals;

    /// <summary>Retains the selector presentation, its frames' compositions and the sprite set they are composed in.</summary>
    private PauseSelectorSourcePresentation(PauseSelectorPresentation presentation, Dictionary<string, SpriteComposition> visuals,
        int spriteSet)
    {
        Presentation = presentation;
        this.visuals = visuals;
        SpriteSet = spriteSet;
    }

    /// <summary>The anchors, frame sequences and timing the game binds.</summary>
    public PauseSelectorPresentation Presentation { get; }

    /// <summary>The sprite set the pause sheet composes every selector frame in.</summary>
    internal int SpriteSet { get; }

    /// <summary>
    /// The name of every distinct frame the animation shows, in the order the animation first shows it: the reserve
    /// category's phases, then the beam category's, then the equipment category's.
    /// </summary>
    internal IEnumerable<string> FrameNames => FrameNamesOf(Presentation);

    /// <summary>
    /// The name of every distinct frame <paramref name="presentation"/>'s animation shows, in the order it first shows it: the
    /// reserve category's phases, then the beam category's, then the equipment category's.
    /// </summary>
    internal static IEnumerable<string> FrameNamesOf(PauseSelectorPresentation presentation) =>
        presentation.reserve.Concat(presentation.beam).Concat(presentation.equipment).Distinct(StringComparer.Ordinal);

    /// <summary>The installed composition of the frame named <paramref name="name"/> as a whole frame of the pause sheet (#1392).</summary>
    /// <param name="name">The installed frame name.</param>
    /// <param name="frameName">The pause sheet's name for the frame.</param>
    internal SpriteFrame Frame(string name, string frameName) => visuals[name].ToFrame(frameName, 0);

    /// <summary>Loads and validates equipment-selector anchors, frames, animation, palette, and timing.</summary>
    public static PauseSelectorSourcePresentation Load(Stream json)
    {
        PauseSelectorDocument document;
        try { document = JsonAssetDocument.Read<PauseSelectorDocument>(json, ArtworkJson.Options)
            ?? throw new InvalidDataException("Pause selector document is null."); }
        catch (JsonException error) { throw new InvalidDataException("Invalid pause selector JSON.", error); }
        if (document.Version != PauseSelectorFormat.Version || (uint)document.Palette > 7 ||
            document.Frames is null || document.Frames.Count is < 1 or > PauseSelectorFormat.MaximumFrames)
            throw new InvalidDataException("Pause selectors require version 1, sixteen anchors, palette 0..7, valid named frames and positive bounded timing.");
        var frames = new Dictionary<string, SpriteComposition>();
        foreach (var frame in document.Frames)
        {
            if (string.IsNullOrWhiteSpace(frame.Key) || frame.Value is null) throw new InvalidDataException("Pause selector frame requires a name and parts.");
            frames.Add(frame.Key, MenuSpriteCompiler.Compile(frame.Value, frame.Key));
        }
        return new(PauseSelectorLayoutFile.Presentation(document.InitialDurationTicks, document.Anchors, document.Animation, frames.ContainsKey),
            frames, document.Palette);
    }

    /// <summary>Writes a pause-selector document as JSON.</summary>
    public static void Write(Stream output, PauseSelectorDocument document)
    {
        byte[] bytes = JsonSerializer.SerializeToUtf8Bytes(document, ArtworkJson.Options);
        output.Write(bytes);
    }
}
