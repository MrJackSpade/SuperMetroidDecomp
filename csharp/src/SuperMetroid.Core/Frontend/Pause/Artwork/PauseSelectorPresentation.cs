using SuperMetroid.Core.Frontend.Map.Artwork;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend.Menus;

namespace SuperMetroid.Core.Frontend.Pause.Artwork;

/// <summary>
/// Equipment-selector anchors, visual animation and timing; selection rules stay compiled. Each phase selects a frame of
/// the installed pause sheet by name; the frames' compositions belong to the import pipeline that reads the installed
/// presentation (#1463).
/// </summary>
public abstract class PauseSelectorPresentation
{
    /// <summary>Only anchors differing from compiled stock coordinates.</summary>
    internal readonly Dictionary<string, AnchorOverride> anchorOverrides;
    /// <summary>Category frame sequences sharing their base frame unless a phase overrides it.</summary>
    internal readonly PhaseFrames reserve, beam, equipment;
    /// <summary>Authored durations differing from the stock phase timing rule.</summary>
    internal readonly Dictionary<int, int>? durationOverrides;
    /// <summary>Dwell time before the selector enters its cyclic animation.</summary>
    public int InitialDurationTicks { get; }
    /// <summary>Number of phases used to wrap the live selector counter.</summary>
    public int PhaseCount { get; }
    /// <summary>Retains only authored anchor, frame, and timing differences.</summary>
    /// <param name="parts">The validated selector anchors, frame sequences and timing.</param>
    internal PauseSelectorPresentation(PauseSelectorParts parts)
    { Ensure.NotNull(parts); this.anchorOverrides = parts.AnchorOverrides; this.reserve = parts.Reserve; this.beam = parts.Beam; this.equipment = parts.Equipment; PhaseCount = parts.PhaseCount; this.durationOverrides = parts.DurationOverrides; InitialDurationTicks = parts.InitialDuration; }
    /// <summary>Applies a named screen-pixel override to the category and item's compiled stock anchor.</summary>
    public Position16<SpritePixels> Anchor(PauseEquipmentCategory category, int item)
    {
        string name = PauseSelectorAnchors.Anchor(category, item);
        var basis = PauseSelectorAnchors.StockAnchor(category, item);
        return anchorOverrides.TryGetValue(name, out var value) ? new(value.X ?? basis.X, value.Y ?? basis.Y) : basis;
    }
    /// <summary>Wraps a nonnegative animation phase into the authored phase count.</summary>
    public int NormalizePhase(int phase) => phase >= 0 ? phase % PhaseCount : throw new ArgumentOutOfRangeException(nameof(phase));
    /// <summary>Calculates the shared dwell rule after cyclic phase normalization.</summary>
    public int Duration(int phase)
    {
        int normalized = NormalizePhase(phase);
        return durationOverrides is not null && durationOverrides.TryGetValue(normalized, out int duration)
            ? duration : MenuSelectorTiming.Duration(normalized);
    }
    /// <summary>
    /// Draws the selected category's normalized selector frame of the pause sheet at the item's anchor, in the selector's
    /// own colours (#1544).
    /// </summary>
    public void Draw(ScreenSprites entries, SpriteSheet sheet, PauseEquipmentCategory category, int item, int phase)
    {
        var point = Anchor(category, item);
        int normalized = NormalizePhase(phase);
        string frame = (category switch
        {
            PauseEquipmentCategory.Reserves => reserve,
            PauseEquipmentCategory.Beams => beam,
            PauseEquipmentCategory.Suits or PauseEquipmentCategory.Boots => equipment,
            _ => throw new InvalidOperationException($"Undefined PauseEquipmentCategory {category}."),
        }).Get(normalized);
        Ensure.NotNull(entries).AddImage(Ensure.NotNull(sheet).Frame(PauseSelectorAnchors.SheetFrameName(frame)), point, PictureShading.Standard);
    }
    /// <summary>The stock selector has zero sprite offsets in every phase: each category
    /// holds its base frame. Only explicitly authored phase differences are retained.</summary>
    /// <param name="basis">The base frame shared by phases without an authored override.</param>
    /// <param name="overrides">The animation phases the import found selecting a different frame, or null.</param>
    internal sealed class PhaseFrames(string basis, Dictionary<int, string>? overrides)
    {
        /// <summary>Base frame shared by phases without an authored override.</summary>
        internal readonly string basis = basis;
        /// <summary>Only animation phases selecting a different frame.</summary>
        internal Dictionary<int, string>? Overrides { get; } = overrides;
        /// <summary>Resolves an authored phase frame or falls back to the base frame.</summary>
        public string Get(int phase) => Overrides is not null && Overrides.TryGetValue(phase, out var value)
            ? value : basis;
    }

    /// <summary>Validated selector parts passed into the compact installed representation.</summary>
    internal sealed record PauseSelectorParts
    {
        /// <summary>Named X and Y differences from compiled stock selector anchors.</summary>
        public required Dictionary<string, AnchorOverride> AnchorOverrides { get; init; }

        /// <summary>Reserve category frame sequence.</summary>
        public required PhaseFrames Reserve { get; init; }

        /// <summary>Beam category frame sequence.</summary>
        public required PhaseFrames Beam { get; init; }

        /// <summary>Shared suit and boot category frame sequence.</summary>
        public required PhaseFrames Equipment { get; init; }

        /// <summary>Cycle length used when normalizing the live phase.</summary>
        public required int PhaseCount { get; init; }

        /// <summary>Phases whose dwell times differ from the stock menu timing.</summary>
        public required Dictionary<int, int>? DurationOverrides { get; init; }

        /// <summary>Delay before the first selector animation phase.</summary>
        public required int InitialDuration { get; init; }
    }
}
