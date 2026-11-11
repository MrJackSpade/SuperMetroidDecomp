using System.Text.Json;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.Frontend.Menus;
using SuperMetroid.Core.Frontend.Pause.Artwork;
using SuperMetroid.Core.Spatial;

namespace SuperMetroid.AssetExtraction.Frontend.Pause.Artwork;

/// <summary>
/// The installed equipment-selector presentation as the import pipeline reads it (#1463): beside the anchors, frame
/// sequences and timing the game binds, each named frame's composition the pause sheet is composed from, kept as
/// installed (#1615).
/// </summary>
public sealed class PauseSelectorSourcePresentation : PauseSelectorPresentation
{
    /// <summary>Each named frame's installed composition.</summary>
    private readonly Dictionary<string, SpriteComposition> visuals;

    /// <summary>Retains the selector presentation, its frames' compositions and the sprite set they are composed in.</summary>
    private PauseSelectorSourcePresentation(PauseSelectorParts parts, Dictionary<string, SpriteComposition> visuals,
        int spriteSet) : base(parts)
    {
        this.visuals = visuals;
        SpriteSet = spriteSet;
    }

    /// <summary>The sprite set the pause sheet composes every selector frame in.</summary>
    internal int SpriteSet { get; }

    /// <summary>
    /// The name of every distinct frame the animation shows, in the order the animation first shows it: the reserve
    /// category's phases, then the beam category's, then the equipment category's.
    /// </summary>
    internal IEnumerable<string> FrameNames => reserve.Concat(beam).Concat(equipment).Distinct(StringComparer.Ordinal);

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
        if (document.Version != PauseSelectorFormat.Version || document.InitialDurationTicks is < 1 or > PauseSelectorFormat.MaximumDuration ||
            (uint)document.Palette > 7 || document.Anchors is null || document.Anchors.Count != PauseSelectorFormat.AnchorCount ||
            document.Frames is null || document.Frames.Count is < 1 or > PauseSelectorFormat.MaximumFrames ||
            document.Animation is null || document.Animation.Length is < 1 or > PauseSelectorFormat.MaximumPhases)
            throw new InvalidDataException("Pause selectors require version 1, sixteen anchors, palette 0..7, valid named frames and positive bounded timing.");
        var anchors = new Dictionary<string, Position16<SpritePixels>>(StringComparer.Ordinal);
        foreach (var definition in PauseSelectorAnchors.Anchors())
        {
            string name = PauseSelectorAnchors.Anchor(definition.Category, definition.Item);
            if (!document.Anchors.TryGetValue(name, out var point) || point.X is < 0 or > 255 || point.Y is < 0 or > 223)
                throw new InvalidDataException($"Pause selector {name} requires screen coordinates X=0..255/Y=0..223.");
            anchors.Add(name, point);
        }
        var frames = new Dictionary<string, SpriteComposition>();
        foreach (var frame in document.Frames)
        {
            if (string.IsNullOrWhiteSpace(frame.Key) || frame.Value is null) throw new InvalidDataException("Pause selector frame requires a name and parts.");
            frames.Add(frame.Key, MenuSpriteCompiler.Compile(frame.Value, frame.Key));
        }
        var reserve = new string[document.Animation.Length];
        var beam = new string[document.Animation.Length];
        var equipment = new string[document.Animation.Length];
        var durations = new int[document.Animation.Length];
        for (int i = 0; i < document.Animation.Length; i++)
        {
            var phase = document.Animation[i];
            if (phase is null || phase.DurationTicks is < 1 or > PauseSelectorFormat.MaximumDuration ||
                phase.Reserve is null || !frames.ContainsKey(phase.Reserve) ||
                phase.Beam is null || !frames.ContainsKey(phase.Beam) ||
                phase.Equipment is null || !frames.ContainsKey(phase.Equipment))
                throw new InvalidDataException($"Pause selector phase {i} has invalid timing or an unknown frame.");
            reserve[i] = phase.Reserve;
            beam[i] = phase.Beam;
            equipment[i] = phase.Equipment;
            durations[i] = phase.DurationTicks;
        }
        return new(new PauseSelectorParts { Anchors = anchors, Reserve = reserve, Beam = beam, Equipment = equipment,
            Durations = durations, InitialDuration = document.InitialDurationTicks }, frames, document.Palette);
    }

    /// <summary>Writes a pause-selector document as JSON.</summary>
    public static void Write(Stream output, PauseSelectorDocument document)
    {
        byte[] bytes = JsonSerializer.SerializeToUtf8Bytes(document, ArtworkJson.Options);
        output.Write(bytes);
    }

    /// <summary>The import-side presentation <paramref name="selectors"/> was read with.</summary>
    /// <exception cref="InvalidDataException">The selector presentation was not read from installed artwork.</exception>
    public static PauseSelectorSourcePresentation Of(PauseSelectorPresentation selectors) => selectors as PauseSelectorSourcePresentation
        ?? throw new InvalidDataException("The pause selector presentation was not read from installed artwork.");
}
