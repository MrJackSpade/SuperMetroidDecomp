using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Frontend.Pause.Artwork;

/// <summary>
/// The installed equipment-selector anchors, visual animation and timing; selection rules stay compiled. The installation
/// is the anchors', frames' and timing's only source (#1596, #1615): each phase names, for each category, the frame of the
/// installed pause sheet it shows. The frames' compositions belong to the import pipeline that reads the installed
/// presentation and never reach the game (#1463, #1616).
/// </summary>
public sealed class PauseSelectorPresentation
{
    /// <summary>The installed anchor of each equipment control, by its stable anchor name.</summary>
    internal readonly Dictionary<string, Position16<SpritePixels>> anchors;
    /// <summary>The frame each animation phase shows for a category, in phase order.</summary>
    internal readonly string[] reserve, beam, equipment;
    /// <summary>The installed dwell of each animation phase, in ticks.</summary>
    internal readonly int[] durations;
    /// <summary>Dwell time before the selector enters its cyclic animation.</summary>
    public int InitialDurationTicks { get; }
    /// <summary>Number of phases used to wrap the live selector counter.</summary>
    public int PhaseCount => durations.Length;
    /// <summary>Keeps the complete installed anchors, frame sequences and timing.</summary>
    /// <param name="parts">The validated selector anchors, frame sequences and timing.</param>
    /// <exception cref="InvalidDataException">A category's frame sequence does not name a frame for every phase.</exception>
    internal PauseSelectorPresentation(PauseSelectorParts parts)
    {
        Ensure.NotNull(parts);
        anchors = new Dictionary<string, Position16<SpritePixels>>(parts.Anchors, StringComparer.Ordinal);
        durations = [.. parts.Durations];
        reserve = PhaseFrames(parts.Reserve, nameof(parts.Reserve));
        beam = PhaseFrames(parts.Beam, nameof(parts.Beam));
        equipment = PhaseFrames(parts.Equipment, nameof(parts.Equipment));
        InitialDurationTicks = parts.InitialDuration;
    }
    /// <summary>A category's frame for every phase, rejected unless there is exactly one per phase.</summary>
    private string[] PhaseFrames(IReadOnlyList<string> frames, string category) => frames.Count == durations.Length
        ? [.. frames]
        : throw new InvalidDataException($"The installed pause selector's {category} sequence names {frames.Count} frames for {durations.Length} phases.");
    /// <summary>The installed screen-pixel anchor of <paramref name="control"/>.</summary>
    /// <exception cref="InvalidDataException">The installation has no anchor for the control.</exception>
    public Position16<SpritePixels> Anchor(PauseEquipmentControl control)
    {
        string name = Ensure.NotNull(control).Anchor;
        return anchors.TryGetValue(name, out Position16<SpritePixels> anchor) ? anchor
            : throw new InvalidDataException($"The installed pause selector has no anchor {name}.");
    }
    /// <summary>Wraps a nonnegative animation phase into the authored phase count.</summary>
    public int NormalizePhase(int phase) => phase >= 0 ? phase % PhaseCount : throw new ArgumentOutOfRangeException(nameof(phase));
    /// <summary>The installed dwell of a phase after cyclic phase normalization.</summary>
    public int Duration(int phase) => durations[NormalizePhase(phase)];
    /// <summary>
    /// Draws the normalized selector frame of <paramref name="control"/>'s column from the pause sheet at the control's
    /// anchor, in the selector's own colours (#1544).
    /// </summary>
    public void Draw(ScreenSprites entries, SpriteSheet sheet, PauseEquipmentControl control, int phase)
    {
        var point = Anchor(control);
        int normalized = NormalizePhase(phase);
        PauseEquipmentCategory category = control.Category;
        string frame = (category switch
        {
            PauseEquipmentCategory.Reserves => reserve,
            PauseEquipmentCategory.Beams => beam,
            PauseEquipmentCategory.Suits or PauseEquipmentCategory.Boots => equipment,
            _ => throw new InvalidOperationException($"Undefined PauseEquipmentCategory {category}."),
        })[normalized];
        Ensure.NotNull(entries).AddImage(Ensure.NotNull(sheet).Frame(PauseSelectorAnchors.SheetFrameName(frame)), point, PictureShading.Standard);
    }

    /// <summary>Validated selector parts passed into the installed representation.</summary>
    internal sealed record PauseSelectorParts
    {
        /// <summary>The installed anchor of each equipment control, by its stable anchor name.</summary>
        public required IReadOnlyDictionary<string, Position16<SpritePixels>> Anchors { get; init; }

        /// <summary>The reserve category's frame for each phase, in phase order.</summary>
        public required IReadOnlyList<string> Reserve { get; init; }

        /// <summary>The beam category's frame for each phase, in phase order.</summary>
        public required IReadOnlyList<string> Beam { get; init; }

        /// <summary>The shared suit and boot category's frame for each phase, in phase order.</summary>
        public required IReadOnlyList<string> Equipment { get; init; }

        /// <summary>Each animation phase's dwell in ticks, in phase order; its length is the cycle length.</summary>
        public required IReadOnlyList<int> Durations { get; init; }

        /// <summary>Delay before the first selector animation phase.</summary>
        public required int InitialDuration { get; init; }
    }
}
