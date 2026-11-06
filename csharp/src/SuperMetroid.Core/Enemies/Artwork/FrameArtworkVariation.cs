using System.Collections.Immutable;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Enemies.Artwork;

/// <summary>
/// How a sheet frame follows one encounter's artwork sequence (#1513): for each step of the sequence at which the frame is
/// drawn, the step whose pre-assembled picture the frame shows then, or none while it shows the picture its room's load
/// gives it. Steps that leave the frame as an earlier step did share that step's picture, so the sheet holds each distinct
/// picture once. A step at which the frame is never drawn has no picture, and neither has the time before the first step
/// when the frame is never drawn then. The import composes the pictures and names the step each step shows.
/// </summary>
/// <param name="Sequence">The encounter's artwork sequence.</param>
/// <param name="Pictures">The step whose picture the frame shows at each step, or none for the loaded picture or a step it is not drawn at.</param>
/// <param name="Drawn">Whether the frame is drawn at each step.</param>
/// <param name="DrawnBeforeFirstStep">Whether the frame is drawn before the sequence reaches its first step, showing its loaded picture.</param>
public sealed record FrameArtworkVariation(EncounterArtworkSequence Sequence, ImmutableArray<int?> Pictures, ImmutableArray<bool> Drawn,
    bool DrawnBeforeFirstStep)
{
    /// <summary>Whether the frame is drawn at each step.</summary>
    public ImmutableArray<bool> Drawn { get; } = Drawn.Length == Pictures.Length
        && Enumerable.Range(0, Pictures.Length).All(step => Drawn[step] || Pictures[step] is null)
        ? Drawn
        : throw new InvalidDataException($"A frame following {Sequence} is drawn at {Drawn.Length} of its {Pictures.Length} steps' pictures, or pictured at a step it is not drawn at.");

    /// <summary>
    /// The step whose picture the frame shows before the sequence reaches its first step, or none for its loaded picture
    /// (#1513): a frame whose artwork a run of pages completes shows that picture then, as the room's load does not hold it.
    /// </summary>
    /// <exception cref="InvalidDataException">The frame is not drawn before the first step, or the sequence has no such step.</exception>
    public int? PictureBeforeFirstStep
    {
        get;
        init => field = value is not int step || DrawnBeforeFirstStep && (uint)step < (uint)Pictures.Length ? value
            : throw new InvalidDataException($"A frame following {Sequence} shows step {step}'s picture before the first step, at which it is not drawn or which the sequence lacks.");
    }

    /// <summary>Whether the frame is drawn at <paramref name="step"/>.</summary>
    /// <exception cref="InvalidDataException">The sequence has no such step.</exception>
    public bool DrawnAt(int step) => (uint)step < (uint)Pictures.Length ? Drawn[step]
        : throw new InvalidDataException($"{Sequence} has {Pictures.Length} steps; step {step} has no picture.");

    /// <summary>The step whose picture <paramref name="frame"/> shows at <paramref name="step"/>, or none for its loaded picture.</summary>
    /// <param name="step">The step the sequence has reached.</param>
    /// <param name="frame">The frame drawn, which names the failure.</param>
    /// <exception cref="InvalidDataException">The sequence has no such step, or the frame is never drawn at it.</exception>
    public int? PictureAt(int step, string frame) => DrawnAt(step) ? Pictures[step]
        : throw new InvalidDataException($"Sheet frame {frame} is drawn at step {step} of {Sequence}, a step at which it is never drawn; its sheet holds no picture of it there.");

    /// <summary>
    /// The name of the picture of <paramref name="frame"/> that shows each sequence <paramref name="shown"/> pairs with the
    /// step whose picture it shows: the frame's own name when it shows none.
    /// </summary>
    public static string PictureName(string frame, IEnumerable<EncounterArtworkStep> shown)
    {
        ArgumentException.ThrowIfNullOrEmpty(frame);
        return frame + string.Concat(Ensure.NotNull(shown).Select(entry => $"|{entry.Sequence}={entry.Step}"));
    }
}
