using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Frontend.Pause.Artwork;

/// <summary>
/// The installed equipment-selector anchors, visual animation and timing; selection rules stay compiled. The installation
/// is the anchors', frames' and timing's only source (#1596, #1615): each step of the animation names, for each category,
/// the frame of the installed pause sheet it shows, and how long it holds (#2359). The frames' compositions belong to the
/// import pipeline that reads the installed presentation and never reach the game (#1463, #1616).
/// </summary>
public sealed class PauseSelectorPresentation
{
    /// <summary>The installed anchor of each equipment control, by its stable anchor name.</summary>
    internal readonly Dictionary<string, Offset16<ScreenPixels>> anchors;

    /// <summary>Keeps the complete installed anchors, looks and timing.</summary>
    /// <param name="parts">The validated selector anchors, frame sequences and timing.</param>
    /// <exception cref="InvalidDataException">A category's frame sequence does not name a frame for every step.</exception>
    internal PauseSelectorPresentation(PauseSelectorParts parts)
    {
        Ensure.NotNull(parts);
        anchors = new Dictionary<string, Offset16<ScreenPixels>>(parts.Anchors, StringComparer.Ordinal);
        int steps = parts.Durations.Count;
        if (parts.Reserve.Count != steps || parts.Beam.Count != steps || parts.Equipment.Count != steps)
            throw new InvalidDataException($"The installed pause selector's frame sequences do not each name one frame for its {steps} steps.");
        Looks = [.. Enumerable.Range(0, steps).Select(step => new PauseSelectorLook(parts.Reserve[step], parts.Beam[step], parts.Equipment[step]))];
        Segments = new(new Dictionary<PauseSelectorSegment, AnimationBeat<PauseSelectorLook, NoAnimationCue>[]>
        {
            [PauseSelectorSegment.Cycle] =
                [.. Enumerable.Range(0, steps).Select(step => new AnimationBeat<PauseSelectorLook, NoAnimationCue>.Frame(Looks[step], parts.Durations[step]))],
        });
        Cycling = new([Segments.Whole(PauseSelectorSegment.Cycle)], AnimationEnd.LoopsFrom(0));
        InitialDurationTicks = parts.InitialDuration;
    }

    /// <summary>The selector's look at each step of its animation, in order.</summary>
    internal IReadOnlyList<PauseSelectorLook> Looks { get; }

    /// <summary>The selector animation's one segment: each step's look for its installed hold.</summary>
    internal AnimationSegments<PauseSelectorSegment, PauseSelectorLook, NoAnimationCue> Segments { get; }

    /// <summary>The selector's animation: its steps over and over.</summary>
    internal Animation<PauseSelectorSegment> Cycling { get; }

    /// <summary>How long the selector shows its first look on entering the equipment page, before its animation moves on.</summary>
    public int InitialDurationTicks { get; }

    /// <summary>The installed screen-pixel anchor of <paramref name="control"/>.</summary>
    /// <exception cref="InvalidDataException">The installation has no anchor for the control.</exception>
    public Offset16<ScreenPixels> Anchor(PauseEquipmentControl control)
    {
        string name = Ensure.NotNull(control).Anchor;
        return anchors.TryGetValue(name, out Offset16<ScreenPixels> anchor) ? anchor
            : throw new InvalidDataException($"The installed pause selector has no anchor {name}.");
    }

    /// <summary>
    /// Draws the frame <paramref name="look"/> shows for <paramref name="control"/>'s column from the pause sheet at the
    /// control's anchor, in the selector's own colours (#1544).
    /// </summary>
    public void Draw(ScreenSprites entries, SpriteSheet sheet, PauseEquipmentControl control, PauseSelectorLook look)
    {
        Offset16<ScreenPixels> point = Anchor(control);
        string frame = look.FrameOf(control.Category);
        Ensure.NotNull(entries).AddImage(Ensure.NotNull(sheet).Frame(PauseSelectorAnchors.SheetFrameName(frame)), point, PictureShading.Standard);
    }

    /// <summary>Validated selector parts passed into the installed representation.</summary>
    internal sealed record PauseSelectorParts
    {
        /// <summary>The installed anchor of each equipment control, by its stable anchor name.</summary>
        public required IReadOnlyDictionary<string, Offset16<ScreenPixels>> Anchors { get; init; }

        /// <summary>The reserve category's frame for each step, in order.</summary>
        public required IReadOnlyList<string> Reserve { get; init; }

        /// <summary>The beam category's frame for each step, in order.</summary>
        public required IReadOnlyList<string> Beam { get; init; }

        /// <summary>The shared suit and boot category's frame for each step, in order.</summary>
        public required IReadOnlyList<string> Equipment { get; init; }

        /// <summary>Each step's hold in updates, in order; its length is the animation's length.</summary>
        public required IReadOnlyList<int> Durations { get; init; }

        /// <summary>How long the first look shows on entering the equipment page.</summary>
        public required int InitialDuration { get; init; }
    }
}
