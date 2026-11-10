using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Frontend.Pause.Artwork;

/// <summary>
/// The installed equipment-selector anchors, visual animation and timing; selection rules stay compiled. The installation
/// is the anchors' and timing's only source (#1596). Each phase selects a frame of the installed pause sheet by name; the
/// frames' compositions belong to the import pipeline that reads the installed presentation (#1463).
/// </summary>
public abstract class PauseSelectorPresentation
{
    /// <summary>The installed anchor of each equipment control, by its stable anchor name.</summary>
    internal readonly Dictionary<string, Position16<SpritePixels>> anchors;
    /// <summary>Category frame sequences sharing their base frame unless a phase overrides it.</summary>
    internal readonly PhaseFrames reserve, beam, equipment;
    /// <summary>The installed dwell of each animation phase, in ticks.</summary>
    internal readonly int[] durations;
    /// <summary>Dwell time before the selector enters its cyclic animation.</summary>
    public int InitialDurationTicks { get; }
    /// <summary>Number of phases used to wrap the live selector counter.</summary>
    public int PhaseCount => durations.Length;
    /// <summary>Keeps the complete installed anchors, frame sequences and timing.</summary>
    /// <param name="parts">The validated selector anchors, frame sequences and timing.</param>
    internal PauseSelectorPresentation(PauseSelectorParts parts)
    {
        Ensure.NotNull(parts);
        anchors = new Dictionary<string, Position16<SpritePixels>>(parts.Anchors, StringComparer.Ordinal);
        reserve = parts.Reserve;
        beam = parts.Beam;
        equipment = parts.Equipment;
        durations = [.. parts.Durations];
        InitialDurationTicks = parts.InitialDuration;
    }
    /// <summary>The installed screen-pixel anchor of the category's item.</summary>
    /// <exception cref="InvalidDataException">The installation has no anchor for the item.</exception>
    public Position16<SpritePixels> Anchor(PauseEquipmentCategory category, int item)
    {
        string name = PauseSelectorAnchors.Anchor(category, item);
        return anchors.TryGetValue(name, out Position16<SpritePixels> anchor) ? anchor
            : throw new InvalidDataException($"The installed pause selector has no anchor {name}.");
    }
    /// <summary>Wraps a nonnegative animation phase into the authored phase count.</summary>
    public int NormalizePhase(int phase) => phase >= 0 ? phase % PhaseCount : throw new ArgumentOutOfRangeException(nameof(phase));
    /// <summary>The installed dwell of a phase after cyclic phase normalization.</summary>
    public int Duration(int phase) => durations[NormalizePhase(phase)];
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
        /// <summary>The installed anchor of each equipment control, by its stable anchor name.</summary>
        public required IReadOnlyDictionary<string, Position16<SpritePixels>> Anchors { get; init; }

        /// <summary>Reserve category frame sequence.</summary>
        public required PhaseFrames Reserve { get; init; }

        /// <summary>Beam category frame sequence.</summary>
        public required PhaseFrames Beam { get; init; }

        /// <summary>Shared suit and boot category frame sequence.</summary>
        public required PhaseFrames Equipment { get; init; }

        /// <summary>Each animation phase's dwell in ticks, in phase order; its length is the cycle length.</summary>
        public required IReadOnlyList<int> Durations { get; init; }

        /// <summary>Delay before the first selector animation phase.</summary>
        public required int InitialDuration { get; init; }
    }
}
