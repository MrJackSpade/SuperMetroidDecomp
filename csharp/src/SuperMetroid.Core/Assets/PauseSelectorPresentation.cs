using System.Text.Json;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Assets;

/// <summary>Equipment-selector anchors, visual animation and compositions; selection rules stay compiled.</summary>
public sealed class PauseSelectorPresentation
{
    private readonly Dictionary<string, MapLabelPoint> anchors;
    private readonly (int Duration, SpriteComposition Reserve, SpriteComposition Beam, SpriteComposition Equipment)[] phases;
    public int InitialDurationTicks { get; }
    public ushort PaletteBits { get; }
    public int PhaseCount => phases.Length;
    private PauseSelectorPresentation(Dictionary<string, MapLabelPoint> anchors,
        (int, SpriteComposition, SpriteComposition, SpriteComposition)[] phases, int initialDuration, int palette)
    { this.anchors = anchors; this.phases = phases; InitialDurationTicks = initialDuration; PaletteBits = SnesObjAttributeWord.Create(0, palette, 0).PaletteBits; }
    public MapLabelPoint Anchor(int category, int item) => anchors[PauseSelectorDefinitions.Anchor(category, item)];
    public int NormalizePhase(int phase) => phase >= 0 ? phase % phases.Length : throw new ArgumentOutOfRangeException(nameof(phase));
    public int Duration(int phase) => phases[NormalizePhase(phase)].Duration;
    public void Draw(OamBuffer oam, int category, int item, int phase)
    {
        var point = Anchor(category, item);
        var current = phases[NormalizePhase(phase)];
        var composition = category switch { 0 => current.Reserve, 1 => current.Beam, _ => current.Equipment };
        composition.DrawOnScreen(oam, (ushort)point.X, (ushort)point.Y, PaletteBits);
    }
    public static PauseSelectorPresentation Load(Stream json)
    {
        PauseSelectorDocument document;
        try { document = JsonSerializer.Deserialize<PauseSelectorDocument>(json, MapPresentationFormat.JsonOptions)
            ?? throw new InvalidDataException("Pause selector document is null."); }
        catch (JsonException error) { throw new InvalidDataException("Invalid pause selector JSON.", error); }
        if (document.Version != PauseSelectorDefinitions.Version || document.InitialDurationTicks is < 1 or > PauseSelectorDefinitions.MaximumDuration ||
            (uint)document.Palette > 7 || document.Anchors is null || document.Anchors.Count != PauseSelectorDefinitions.AnchorCount ||
            document.Frames is null || document.Frames.Count is < 1 or > PauseSelectorDefinitions.MaximumFrames ||
            document.Animation is null || document.Animation.Length is < 1 or > PauseSelectorDefinitions.MaximumPhases)
            throw new InvalidDataException("Pause selectors require version 1, sixteen anchors, palette 0..7, valid named frames and positive bounded timing.");
        var anchors = new Dictionary<string, MapLabelPoint>();
        foreach (var definition in PauseSelectorDefinitions.Anchors())
        {
            if (!document.Anchors.TryGetValue(definition.Name, out var point) || point is null || point.X is < 0 or > 255 || point.Y is < 0 or > 223)
                throw new InvalidDataException($"Pause selector {definition.Name} requires screen coordinates X=0..255/Y=0..223.");
            anchors.Add(definition.Name, point);
        }
        var frames = new Dictionary<string, SpriteComposition>();
        foreach (var frame in document.Frames)
        {
            if (string.IsNullOrWhiteSpace(frame.Key) || frame.Value is null) throw new InvalidDataException("Pause selector frame requires a name and parts.");
            frames.Add(frame.Key, MenuSpriteCompiler.Compile(frame.Value, frame.Key));
        }
        var phases = new (int, SpriteComposition, SpriteComposition, SpriteComposition)[document.Animation.Length];
        for (int i = 0; i < phases.Length; i++)
        {
            var phase = document.Animation[i];
            if (phase is null || phase.DurationTicks is < 1 or > PauseSelectorDefinitions.MaximumDuration ||
                phase.Reserve is null || !frames.TryGetValue(phase.Reserve, out var reserve) ||
                phase.Beam is null || !frames.TryGetValue(phase.Beam, out var beam) ||
                phase.Equipment is null || !frames.TryGetValue(phase.Equipment, out var equipment))
                throw new InvalidDataException($"Pause selector phase {i} has invalid timing or an unknown frame.");
            phases[i] = (phase.DurationTicks, reserve, beam, equipment);
        }
        return new(anchors, phases, document.InitialDurationTicks, document.Palette);
    }
    public static void Write(Stream output, PauseSelectorDocument document)
    {
        byte[] bytes = JsonSerializer.SerializeToUtf8Bytes(document, MapPresentationFormat.JsonOptions);
        _ = Load(new MemoryStream(bytes, writable: false)); output.Write(bytes);
    }
}
public sealed record PauseSelectorDocument
{
    public required int Version { get; init; }
    public required int InitialDurationTicks { get; init; }
    public required int Palette { get; init; }
    public required Dictionary<string, MapLabelPoint> Anchors { get; init; }
    public required Dictionary<string, SpriteVisualPart[]> Frames { get; init; }
    public required PauseSelectorPhase[] Animation { get; init; }
}
public sealed record PauseSelectorPhase
{
    public required int DurationTicks { get; init; }
    public required string Reserve { get; init; }
    public required string Beam { get; init; }
    public required string Equipment { get; init; }
}
