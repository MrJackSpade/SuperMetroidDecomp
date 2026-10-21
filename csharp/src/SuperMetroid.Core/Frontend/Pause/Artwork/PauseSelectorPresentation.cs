using SuperMetroid.Core.Frontend.Map.Artwork;
using System.Text.Json;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend.Menus;

namespace SuperMetroid.Core.Frontend.Pause.Artwork;

/// <summary>Equipment-selector anchors, visual animation and compositions; selection rules stay compiled.</summary>
public sealed class PauseSelectorPresentation
{
    /// <summary>Only anchors differing from compiled stock coordinates.</summary>
    internal readonly Dictionary<string, AnchorOverride> anchorOverrides;
    /// <summary>Category compositions sharing their native base frame unless a phase overrides it.</summary>
    internal readonly PhaseComposition reserve, beam, equipment;
    /// <summary>Authored durations differing from the native phase timing rule.</summary>
    internal readonly Dictionary<int, int>? durationOverrides;
    /// <summary>Dwell time before the selector enters its cyclic animation.</summary>
    public int InitialDurationTicks { get; }
    /// <summary>Object attributes selecting the authored OBJ palette, with no tile, priority or flip.</summary>
    public SnesObjAttributeWord ObjectPalette { get; }
    /// <summary>Number of phases used to wrap the live selector counter.</summary>
    public int PhaseCount { get; }
    /// <summary>Retains only authored anchor, composition, and timing differences.</summary>
    private PauseSelectorPresentation(PauseSelectorParts parts)
    { this.anchorOverrides = parts.AnchorOverrides; this.reserve = parts.Reserve; this.beam = parts.Beam; this.equipment = parts.Equipment; PhaseCount = parts.PhaseCount; this.durationOverrides = parts.DurationOverrides; InitialDurationTicks = parts.InitialDuration; ObjectPalette = SnesObjAttributeWord.Create(0, parts.Palette, 0); }
    /// <summary>Applies a named screen-pixel override to the category and item's compiled stock anchor.</summary>
    public Position16<OamPixels> Anchor(PauseEquipmentCategory category, int item)
    {
        string name = PauseSelectorDefinitions.Anchor(category, item);
        var basis = PauseSelectorDefinitions.StockAnchor(category, item);
        return anchorOverrides.TryGetValue(name, out var value) ? new(checked((ushort)(value.X ?? basis.X)), checked((ushort)(value.Y ?? basis.Y))) : basis;
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
    public void Draw(ScreenSprites entries, PauseEquipmentCategory category, int item, int phase)
    {
        var point = Anchor(category, item);
        int normalized = NormalizePhase(phase);
        var composition = (category switch
        {
            PauseEquipmentCategory.Reserves => reserve,
            PauseEquipmentCategory.Beams => beam,
            PauseEquipmentCategory.Suits or PauseEquipmentCategory.Boots => equipment,
            _ => throw new InvalidOperationException($"Undefined PauseEquipmentCategory {category}."),
        }).Get(normalized);
        composition.Draw(entries, point, ObjectPalette.PaletteBits);
    }
    /// <summary>Loads and validates equipment-selector anchors, frames, animation, palette, and timing.</summary>
    public static PauseSelectorPresentation Load(Stream json)
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
        foreach (var definition in PauseSelectorDefinitions.Anchors())
        {
            if (!document.Anchors.TryGetValue(definition.Name, out var point) || point.X is < 0 or > 255 || point.Y is < 0 or > 223)
                throw new InvalidDataException($"Pause selector {definition.Name} requires screen coordinates X=0..255/Y=0..223.");
            var basis = PauseSelectorDefinitions.StockAnchor(definition.Category, definition.Item);
            if (point.X != basis.X || point.Y != basis.Y)
                anchorOverrides.Add(definition.Name, new AnchorOverride(point.X == basis.X ? null : point.X, point.Y == basis.Y ? null : point.Y));
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
            if (reservePhases is null || beamPhases is null || equipmentPhases is null)
            {
                reservePhases = new(reserve); beamPhases = new(beam); equipmentPhases = new(equipment);
            }
            else
            {
                reservePhases.Capture(i, reserve); beamPhases.Capture(i, beam); equipmentPhases.Capture(i, equipment);
            }
            if (phase.DurationTicks != MenuSelectorTiming.Duration(i))
                (durationOverrides ??= new()).Add(i, phase.DurationTicks);
        }
        if (reservePhases is null || beamPhases is null || equipmentPhases is null)
            throw new InvalidDataException("Pause selector animation requires at least one phase.");
        return new(new PauseSelectorParts { AnchorOverrides = anchorOverrides, Reserve = reservePhases, Beam = beamPhases, Equipment = equipmentPhases, PhaseCount = document.Animation.Length, DurationOverrides = durationOverrides, InitialDuration = document.InitialDurationTicks, Palette = document.Palette });
    }
    /// <summary>Native $82:C137 has zero sprite offsets in every phase: each category
    /// holds its base composition. Capture only explicitly authored phase differences.</summary>
    internal sealed class PhaseComposition(PauseSelectorVisual basis)
    {
        /// <summary>Native base composition shared by phases without an authored override.</summary>
        internal readonly PauseSelectorVisual basis = basis;
        /// <summary>Only animation phases selecting a different composition.</summary>
        internal Dictionary<int, PauseSelectorVisual>? Overrides { get; private set; }
        /// <summary>Retains a phase image only when it differs from the base image object.</summary>
        public void Capture(int phase, PauseSelectorVisual value)
        {
            if (!ReferenceEquals(basis, value)) (Overrides ??= new()).Add(phase, value);
        }
        /// <summary>Resolves an authored phase image or falls back to the native base composition.</summary>
        public PauseSelectorVisual Get(int phase) => Overrides is not null && Overrides.TryGetValue(phase, out var value)
            ? value : basis;
    }
    /// <summary>Validates and writes a pause-selector document as JSON.</summary>
    public static void Write(Stream output, PauseSelectorDocument document)
    {
        byte[] bytes = JsonSerializer.SerializeToUtf8Bytes(document, ArtworkJson.Options);
        _ = Load(new MemoryStream(bytes, writable: false)); output.Write(bytes);
    }

    /// <summary>Validated selector parts passed into the compact installed representation.</summary>
    private sealed record PauseSelectorParts
    {
        /// <summary>Named X and Y differences from compiled stock selector anchors.</summary>
        public required Dictionary<string, AnchorOverride> AnchorOverrides { get; init; }

        /// <summary>Reserve category frame sequence.</summary>
        public required PhaseComposition Reserve { get; init; }

        /// <summary>Beam category frame sequence.</summary>
        public required PhaseComposition Beam { get; init; }

        /// <summary>Shared suit and boot category frame sequence.</summary>
        public required PhaseComposition Equipment { get; init; }

        /// <summary>Cycle length used when normalizing the live phase.</summary>
        public required int PhaseCount { get; init; }

        /// <summary>Phases whose dwell times differ from native menu timing.</summary>
        public required Dictionary<int, int>? DurationOverrides { get; init; }

        /// <summary>Delay before the first selector animation phase.</summary>
        public required int InitialDuration { get; init; }

        /// <summary>OBJ palette index encoded when drawing a selector frame.</summary>
        public required int Palette { get; init; }
    }
}
