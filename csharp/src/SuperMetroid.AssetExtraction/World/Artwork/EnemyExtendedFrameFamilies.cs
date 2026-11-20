using System.Collections.Frozen;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.AssetExtraction.World.Artwork;

/// <summary>
/// The enemy family of each extended frame (#1623), read from the document's family runs
/// (<see cref="EnemyExtendedFrameDefinitions.FamilyRuns"/>) rather than from the frames' names.
/// </summary>
internal static class EnemyExtendedFrameFamilies
{
    /// <summary>Each extended frame's run of its family's frames.</summary>
    private static readonly Lazy<FrozenDictionary<EnemyFrameId, EnemyExtendedFrameRun>> Runs = new(Build);

    /// <summary>The family <paramref name="frame"/> belongs to.</summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="frame"/> is not an extended frame.</exception>
    internal static EnemyExtendedFrameFamily Of(EnemyFrameId frame) => RunOf(frame).Family;

    /// <summary>The run of its family's frames <paramref name="frame"/> lies in, which states how its components are laid out.</summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="frame"/> is not an extended frame.</exception>
    internal static EnemyExtendedFrameRun RunOf(EnemyFrameId frame) =>
        Runs.Value.TryGetValue(frame, out EnemyExtendedFrameRun run)
            ? run
            : throw new ArgumentOutOfRangeException(nameof(frame), frame, "The frame is not an extended frame.");

    /// <summary>Assigns the document's frames, in order, to the family runs.</summary>
    /// <exception cref="InvalidDataException">The runs do not hold exactly the document's frames.</exception>
    private static FrozenDictionary<EnemyFrameId, EnemyExtendedFrameRun> Build()
    {
        var families = new Dictionary<EnemyFrameId, EnemyExtendedFrameRun>();
        using IEnumerator<EnemyFrameName> frames = ((IEnumerable<EnemyFrameName>)EnemyExtendedFrameDefinitions.Frames).GetEnumerator();
        foreach (EnemyExtendedFrameRun run in EnemyExtendedFrameDefinitions.FamilyRuns)
        {
            for (int index = 0; index < run.Count; index++)
            {
                if (!frames.MoveNext())
                    throw new InvalidDataException("The extended frames' family runs hold more frames than the document lists.");
                families.Add(frames.Current.Frame, run);
            }
        }
        if (frames.MoveNext())
            throw new InvalidDataException("The extended frames' family runs leave frames of the document without a family.");
        return families.ToFrozenDictionary();
    }
}
