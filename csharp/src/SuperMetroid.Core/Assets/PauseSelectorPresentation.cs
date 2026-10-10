using SuperMetroid.Core.Frontend;
using System.Text.Json;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Assets;

/// <summary>Equipment-selector anchors, visual animation and compositions; selection rules stay compiled.</summary>
public sealed class PauseSelectorPresentation
{
    /// <summary>Stores only authored anchor coordinates that differ from stock, with null retaining the stock axis value.</summary>
    private readonly Dictionary<string, (int? X, int? Y)> anchorOverrides;

    /// <summary>Base phase compositions for the reserve, beam, and suit/equipment selector categories.</summary>
    private readonly PhaseComposition reserve, beam, equipment;

    /// <summary>Stores authored phase durations only when they differ from the native menu timing rule.</summary>
    private readonly Dictionary<int, int>? durationOverrides;
    /// <summary>Gets the initial selector dwell time in accepted update ticks.</summary>
    public int InitialDurationTicks { get; }
    /// <summary>Gets the selected three-bit OBJ palette encoded in its attribute-word position.</summary>
    public ushort PaletteBits { get; }
    /// <summary>Gets the number of cyclic visual animation phases.</summary>
    public int PhaseCount { get; }
    /// <summary>Creates the validated selector presentation from sparse anchor, composition, and timing overrides.</summary>
    /// <param name="anchorOverrides">Coordinates that differ from stock anchors; null axes retain their stock values.</param>
    /// <param name="reserve">The reserve-category composition across animation phases.</param>
    /// <param name="beam">The beam-category composition across animation phases.</param>
    /// <param name="equipment">The suit/equipment-category composition across animation phases.</param>
    /// <param name="phaseCount">Number of entries in the cyclic selector animation.</param>
    /// <param name="durationOverrides">Non-native phase durations keyed by normalized phase index, if any.</param>
    /// <param name="initialDuration">Initial selector dwell duration in accepted update ticks.</param>
    /// <param name="palette">The OBJ palette index encoded for selector drawing.</param>
    private PauseSelectorPresentation(Dictionary<string, (int? X, int? Y)> anchorOverrides,
        PhaseComposition reserve, PhaseComposition beam, PhaseComposition equipment, int phaseCount,
        Dictionary<int, int>? durationOverrides, int initialDuration, int palette)
    { this.anchorOverrides = anchorOverrides; this.reserve = reserve; this.beam = beam; this.equipment = equipment; PhaseCount = phaseCount; this.durationOverrides = durationOverrides; InitialDurationTicks = initialDuration; PaletteBits = SnesObjAttributeWord.Create(0, palette, 0).PaletteBits; }
    /// <summary>Gets the selected screen-pixel anchor for an equipment category and item.</summary>
    public MapLabelPoint Anchor(int category, int item)
    {
        string name = PauseSelectorDefinitions.Anchor(category, item);
        var basis = PauseSelectorDefinitions.StockAnchor(category, item);
        return anchorOverrides.TryGetValue(name, out var value) ? new(value.X ?? basis.X, value.Y ?? basis.Y) : basis;
    }
    /// <summary>Wraps a nonnegative animation phase into the authored phase count.</summary>
    public int NormalizePhase(int phase) => phase >= 0 ? phase % PhaseCount : throw new ArgumentOutOfRangeException(nameof(phase));
    /// <summary>Calculates the shared native dwell rule after cyclic phase normalization.</summary>
    public int Duration(int phase)
    {
        int normalized = NormalizePhase(phase);
        return durationOverrides is not null && durationOverrides.TryGetValue(normalized, out int duration)
            ? duration : MenuSelectorTiming.Duration(normalized);
    }
    /// <summary>Draws the selected category's normalized selector composition at the item's anchor.</summary>
    public void Draw(OamBuffer oam, int category, int item, int phase)
    {
        var point = Anchor(category, item);
        int normalized = NormalizePhase(phase);
        var composition = (category switch { 0 => reserve, 1 => beam, _ => equipment }).Get(normalized);
        composition.DrawOnScreen(oam, (ushort)point.X, (ushort)point.Y, PaletteBits);
    }
    /// <summary>Loads and validates equipment-selector anchors, frames, animation, palette, and timing.</summary>
    public static PauseSelectorPresentation Load(Stream json)
    {
        PauseSelectorDocument document;
        try { document = JsonAssetDocument.Read<PauseSelectorDocument>(json, MapPresentationFormat.JsonOptions)
            ?? throw new InvalidDataException("Pause selector document is null."); }
        catch (JsonException error) { throw new InvalidDataException("Invalid pause selector JSON.", error); }
        if (document.Version != PauseSelectorDefinitions.Version || document.InitialDurationTicks is < 1 or > PauseSelectorDefinitions.MaximumDuration ||
            (uint)document.Palette > 7 || document.Anchors is null || document.Anchors.Count != PauseSelectorDefinitions.AnchorCount ||
            document.Frames is null || document.Frames.Count is < 1 or > PauseSelectorDefinitions.MaximumFrames ||
            document.Animation is null || document.Animation.Length is < 1 or > PauseSelectorDefinitions.MaximumPhases)
            throw new InvalidDataException("Pause selectors require version 1, sixteen anchors, palette 0..7, valid named frames and positive bounded timing.");
        var anchorOverrides = new Dictionary<string, (int? X, int? Y)>();
        foreach (var definition in PauseSelectorDefinitions.Anchors())
        {
            if (!document.Anchors.TryGetValue(definition.Name, out var point) || point is null || point.X is < 0 or > 255 || point.Y is < 0 or > 223)
                throw new InvalidDataException($"Pause selector {definition.Name} requires screen coordinates X=0..255/Y=0..223.");
            var basis = PauseSelectorDefinitions.StockAnchor(definition.Category, definition.Item);
            if (point.X != basis.X || point.Y != basis.Y)
                anchorOverrides.Add(definition.Name, (point.X == basis.X ? null : point.X, point.Y == basis.Y ? null : point.Y));
        }
        var frames = new Dictionary<string, PauseSelectorVisual>();
        foreach (var frame in document.Frames)
        {
            if (string.IsNullOrWhiteSpace(frame.Key) || frame.Value is null) throw new InvalidDataException("Pause selector frame requires a name and parts.");
            frames.Add(frame.Key, PauseSelectorVisual.Compile(frame.Value, frame.Key));
        }
        PhaseComposition? reservePhases = null, beamPhases = null, equipmentPhases = null;
        Dictionary<int, int>? durationOverrides = null;
        for (int i = 0; i < document.Animation.Length; i++)
        {
            var phase = document.Animation[i];
            if (phase is null || phase.DurationTicks is < 1 or > PauseSelectorDefinitions.MaximumDuration ||
                phase.Reserve is null || !frames.TryGetValue(phase.Reserve, out var reserve) ||
                phase.Beam is null || !frames.TryGetValue(phase.Beam, out var beam) ||
                phase.Equipment is null || !frames.TryGetValue(phase.Equipment, out var equipment))
                throw new InvalidDataException($"Pause selector phase {i} has invalid timing or an unknown frame.");
            if (i == 0)
            {
                reservePhases = new(reserve); beamPhases = new(beam); equipmentPhases = new(equipment);
            }
            else
            {
                reservePhases!.Capture(i, reserve); beamPhases!.Capture(i, beam); equipmentPhases!.Capture(i, equipment);
            }
            if (phase.DurationTicks != MenuSelectorTiming.Duration(i))
                (durationOverrides ??= new()).Add(i, phase.DurationTicks);
        }
        return new(anchorOverrides, reservePhases!, beamPhases!, equipmentPhases!, document.Animation.Length, durationOverrides, document.InitialDurationTicks, document.Palette);
    }
    /// <summary>Native $82:C137 has zero sprite offsets in every phase: each category
    /// holds its base composition. Capture only explicitly authored phase differences.</summary>
    /// <param name="basis">Base selector composition reused by phases without an override.</param>
    private sealed class PhaseComposition(PauseSelectorVisual basis)
    {
        /// <summary>Base composition reused in phases with no explicitly authored visual change.</summary>
        private readonly PauseSelectorVisual basis = basis;

        /// <summary>Stores only phase compositions that differ by identity from the base composition.</summary>
        private Dictionary<int, PauseSelectorVisual>? overrides;

        /// <summary>Records an explicitly authored phase when it is a distinct composition from the base.</summary>
        /// <param name="phase">The zero-based animation phase index.</param>
        /// <param name="value">The compiled composition selected for that phase.</param>
        public void Capture(int phase, PauseSelectorVisual value)
        {
            if (!ReferenceEquals(basis, value)) (overrides ??= new()).Add(phase, value);
        }
        /// <summary>Returns the captured composition for a phase, or the shared base when that phase has no override.</summary>
        /// <param name="phase">The normalized zero-based animation phase index.</param>
        /// <returns>The phase-specific composition or the base composition.</returns>
        public PauseSelectorVisual Get(int phase) => overrides is not null && overrides.TryGetValue(phase, out var value)
            ? value : basis;
    }
    /// <summary>Validates and writes a pause-selector document as JSON.</summary>
    public static void Write(Stream output, PauseSelectorDocument document)
    {
        byte[] bytes = JsonSerializer.SerializeToUtf8Bytes(document, MapPresentationFormat.JsonOptions);
        _ = Load(new MemoryStream(bytes, writable: false)); output.Write(bytes);
    }
}
/// <summary>Defines editable pause equipment-selector placement and animation.</summary>
public sealed record PauseSelectorDocument
{
    /// <summary>Gets the document schema revision.</summary>
    public required int Version { get; init; }
    /// <summary>Gets the initial selector dwell time in update ticks.</summary>
    public required int InitialDurationTicks { get; init; }
    /// <summary>Gets the OBJ palette index from zero through seven.</summary>
    public required int Palette { get; init; }
    /// <summary>Gets all sixteen named selector anchors in screen pixels.</summary>
    public required Dictionary<string, MapLabelPoint> Anchors { get; init; }
    /// <summary>Gets named selector frames as sprite-part compositions.</summary>
    public required Dictionary<string, SpriteVisualPart[]> Frames { get; init; }
    /// <summary>Gets the ordered cyclic animation phases.</summary>
    public required PauseSelectorPhase[] Animation { get; init; }
}
/// <summary>Defines one timed pause-selector animation phase for all three categories.</summary>
public sealed record PauseSelectorPhase
{
    /// <summary>Gets the phase duration in accepted update ticks.</summary>
    public required int DurationTicks { get; init; }
    /// <summary>Gets the named frame used by the reserve selector.</summary>
    public required string Reserve { get; init; }
    /// <summary>Gets the named frame used by the beam selector.</summary>
    public required string Beam { get; init; }
    /// <summary>Gets the named frame used by the suit-and-equipment selector.</summary>
    public required string Equipment { get; init; }
}
