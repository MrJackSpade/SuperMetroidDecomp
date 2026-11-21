using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Frontend.Pause.Artwork;

/// <summary>
/// The installed equipment-selector anchors, visual animation and timing; selection rules are the game's own. The installation
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
    internal PauseSelectorPresentation(PauseSelectorParts parts)
    {
        Ensure.NotNull(parts);
        anchors = new Dictionary<string, Offset16<ScreenPixels>>(parts.Anchors, StringComparer.Ordinal);
        Looks = [.. parts.Steps.Select(step => step.Look)];
        Segments = new(new Dictionary<PauseSelectorSegment, AnimationBeat<PauseSelectorLook, NoAnimationCue>[]>
        {
            [PauseSelectorSegment.Cycle] =
                [.. parts.Steps.Select(step => new AnimationBeat<PauseSelectorLook, NoAnimationCue>.Frame(step.Look, step.Duration))],
        });
        Cycling = Animation<PauseSelectorSegment>.Looping([Segments.Whole(PauseSelectorSegment.Cycle)]);
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
    private Offset16<ScreenPixels> Anchor(PauseEquipmentControl control)
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

        /// <summary>The animation's steps in order, each with its look and its hold.</summary>
        public required IReadOnlyList<PauseSelectorStep> Steps { get; init; }

        /// <summary>How long the first look shows on entering the equipment page.</summary>
        public required int InitialDuration { get; init; }
    }

    /// <summary>One step of the selector's animation (#2565): its look and how many updates it holds.</summary>
    /// <param name="Look">The step's reserve, beam and equipment frames.</param>
    /// <param name="Duration">The step's hold in updates.</param>
    internal sealed record PauseSelectorStep(PauseSelectorLook Look, int Duration);
}
