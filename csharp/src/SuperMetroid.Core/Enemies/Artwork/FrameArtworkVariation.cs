using System.Collections.Immutable;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Enemies.Artwork;

/// <summary>
/// How a sheet frame follows one encounter's artwork sequence (#1513): for each step of the sequence, the step whose
/// pre-assembled picture the frame shows then, or none while it shows the picture its room's load gives it. Steps that
/// leave the frame as an earlier step did share that step's picture, so the sheet holds each distinct picture once. The
/// import composes the pictures and names the step each step shows.
/// </summary>
/// <param name="Sequence">The encounter's artwork sequence.</param>
/// <param name="Pictures">The step whose picture the frame shows at each step, or none for the loaded picture.</param>
public sealed record FrameArtworkVariation(EncounterArtworkSequence Sequence, ImmutableArray<int?> Pictures)
{
    /// <summary>The step whose picture the frame shows at <paramref name="step"/>, or none for its loaded picture.</summary>
    /// <exception cref="InvalidDataException">The sequence has no such step.</exception>
    public int? PictureAt(int step) => (uint)step < (uint)Pictures.Length ? Pictures[step]
        : throw new InvalidDataException($"{Sequence} has {Pictures.Length} steps; step {step} has no picture.");

    /// <summary>
    /// The name of the picture of <paramref name="frame"/> that shows each sequence <paramref name="shown"/> pairs with the
    /// step whose picture it shows: the frame's own name when it shows none.
    /// </summary>
    public static string PictureName(string frame, IEnumerable<(EncounterArtworkSequence Sequence, int Step)> shown)
    {
        ArgumentException.ThrowIfNullOrEmpty(frame);
        return frame + string.Concat(Ensure.NotNull(shown).Select(entry => $"|{entry.Sequence}={entry.Step}"));
    }
}
