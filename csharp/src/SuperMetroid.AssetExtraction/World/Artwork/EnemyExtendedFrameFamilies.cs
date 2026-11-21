using System.Collections.Frozen;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.AssetExtraction.World.Artwork;

/// <summary>
/// The enemy family of each extended frame (#1623), read from the document's family runs
/// (<see cref="EnemyExtendedFrameDefinitions.FamilyRuns"/>), each of which names its frames, rather than from the frames' names.
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

    /// <summary>Each family run's frames, each with the run that names it.</summary>
    /// <exception cref="ArgumentException">Two runs name one frame.</exception>
    private static FrozenDictionary<EnemyFrameId, EnemyExtendedFrameRun> Build()
    {
        var families = new Dictionary<EnemyFrameId, EnemyExtendedFrameRun>();
        foreach (EnemyExtendedFrameRun run in EnemyExtendedFrameDefinitions.FamilyRuns)
        {
            foreach (EnemyFrameId frame in run.Frames)
                families.Add(frame, run);
        }
        return families.ToFrozenDictionary();
    }
}
