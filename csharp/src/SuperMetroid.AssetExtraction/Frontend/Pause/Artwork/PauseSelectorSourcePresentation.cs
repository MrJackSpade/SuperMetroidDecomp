using System.Text.Json;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend.Map.Artwork;
using SuperMetroid.Core.Frontend.Menus;
using SuperMetroid.Core.Frontend.Pause.Artwork;

namespace SuperMetroid.AssetExtraction.Frontend.Pause.Artwork;

/// <summary>
/// The installed equipment-selector presentation as the import pipeline reads it (#1463): beside the anchors, frame
/// sequences and timing the game binds, each named frame's composition the pause sheet is composed from.
/// </summary>
public sealed class PauseSelectorSourcePresentation : PauseSelectorPresentation
{
    /// <summary>Each named frame's recognised or authored composition.</summary>
    internal readonly Dictionary<string, PauseSelectorVisual> visuals;

    /// <summary>Retains the selector presentation, its frames' compositions and the sprite set they are composed in.</summary>
    private PauseSelectorSourcePresentation(PauseSelectorParts parts, Dictionary<string, PauseSelectorVisual> visuals,
        int spriteSet) : base(parts)
    {
        this.visuals = visuals;
        SpriteSet = spriteSet;
    }

    /// <summary>The sprite set the pause sheet composes every selector frame in.</summary>
    internal int SpriteSet { get; }

    /// <summary>Every distinct visual the animation shows, in the order the animation first shows it.</summary>
    internal IEnumerable<PauseSelectorVisual> Visuals => FrameNames.Select(name => visuals[name]);

    /// <summary>Loads and validates equipment-selector anchors, frames, animation, palette, and timing.</summary>
    public static PauseSelectorSourcePresentation Load(Stream json)
    {
        PauseSelectorDocument document;
        try { document = JsonAssetDocument.Read<PauseSelectorDocument>(json, ArtworkJson.Options)
            ?? throw new InvalidDataException("Pause selector document is null."); }
        catch (JsonException error) { throw new InvalidDataException("Invalid pause selector JSON.", error); }
        if (document.Version != PauseSelectorDefinitions.Version || document.InitialDurationTicks is < 1 or > PauseSelectorDefinitions.MaximumDuration ||
            (uint)document.Palette > 7 || document.Anchors is null || document.Anchors.Count != PauseSelectorDefinitions.AnchorCount ||
            document.Frames is null || document.Frames.Count is < 1 or > PauseSelectorDefinitions.MaximumFrames ||
            document.Animation is null || document.Animation.Length is < 1 or > PauseSelectorDefinitions.MaximumPhases)
            throw new InvalidDataException("Pause selectors require version 1, sixteen anchors, palette 0..7, valid named frames and positive bounded timing.");
        var anchorOverrides = new Dictionary<string, AnchorOverride>();
        foreach (var definition in PauseSelectorAnchors.Anchors())
        {
            if (!document.Anchors.TryGetValue(definition.Name, out var point) || point.X is < 0 or > 255 || point.Y is < 0 or > 223)
                throw new InvalidDataException($"Pause selector {definition.Name} requires screen coordinates X=0..255/Y=0..223.");
            var basis = PauseSelectorAnchors.StockAnchor(definition.Category, definition.Item);
            if (point.X != basis.X || point.Y != basis.Y)
                anchorOverrides.Add(definition.Name, new AnchorOverride(point.X == basis.X ? null : point.X, point.Y == basis.Y ? null : point.Y));
        }
        var frames = new Dictionary<string, PauseSelectorVisual>();
        foreach (var frame in document.Frames)
        {
            if (string.IsNullOrWhiteSpace(frame.Key) || frame.Value is null) throw new InvalidDataException("Pause selector frame requires a name and parts.");
            frames.Add(frame.Key, PauseSelectorVisual.Compile(frame.Value, frame.Key));
        }
        PhaseFrames? reservePhases = null, beamPhases = null, equipmentPhases = null;
        Dictionary<int, int>? durationOverrides = null;
        for (int i = 0; i < document.Animation.Length; i++)
        {
            var phase = document.Animation[i];
            if (phase is null || phase.DurationTicks is < 1 or > PauseSelectorDefinitions.MaximumDuration ||
                phase.Reserve is null || !frames.ContainsKey(phase.Reserve) ||
                phase.Beam is null || !frames.ContainsKey(phase.Beam) ||
                phase.Equipment is null || !frames.ContainsKey(phase.Equipment))
                throw new InvalidDataException($"Pause selector phase {i} has invalid timing or an unknown frame.");
            if (reservePhases is null || beamPhases is null || equipmentPhases is null)
            {
                reservePhases = new(phase.Reserve); beamPhases = new(phase.Beam); equipmentPhases = new(phase.Equipment);
            }
            else
            {
                reservePhases.Capture(i, phase.Reserve); beamPhases.Capture(i, phase.Beam); equipmentPhases.Capture(i, phase.Equipment);
            }
            if (phase.DurationTicks != MenuSelectorTiming.Duration(i))
                (durationOverrides ??= new()).Add(i, phase.DurationTicks);
        }
        if (reservePhases is null || beamPhases is null || equipmentPhases is null)
            throw new InvalidDataException("Pause selector animation requires at least one phase.");
        return new(new PauseSelectorParts { AnchorOverrides = anchorOverrides, Reserve = reservePhases, Beam = beamPhases, Equipment = equipmentPhases, PhaseCount = document.Animation.Length, DurationOverrides = durationOverrides, InitialDuration = document.InitialDurationTicks },
            frames, document.Palette);
    }

    /// <summary>Validates and writes a pause-selector document as JSON.</summary>
    public static void Write(Stream output, PauseSelectorDocument document)
    {
        byte[] bytes = JsonSerializer.SerializeToUtf8Bytes(document, ArtworkJson.Options);
        _ = Load(new MemoryStream(bytes, writable: false)); output.Write(bytes);
    }

    /// <summary>The import-side presentation <paramref name="selectors"/> was read with.</summary>
    /// <exception cref="InvalidDataException">The selector presentation was not read from installed artwork.</exception>
    public static PauseSelectorSourcePresentation Of(PauseSelectorPresentation selectors) => selectors as PauseSelectorSourcePresentation
        ?? throw new InvalidDataException("The pause selector presentation was not read from installed artwork.");
}
