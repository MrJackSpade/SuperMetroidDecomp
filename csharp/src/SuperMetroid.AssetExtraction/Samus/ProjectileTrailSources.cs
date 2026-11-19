using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Samus.Projectiles;

namespace SuperMetroid.AssetExtraction.Samus;

/// <summary>
/// Where the bank-$90 trail programs store each sparkle animation's beats (#2348): a frame's timed record takes four bytes,
/// a move instruction two, and each program ends with a two-byte end word after its last beat.
/// </summary>
internal static class ProjectileTrailSources
{
    /// <summary>The bank-$90 address of <paramref name="program"/>'s first beat.</summary>
    private static ushort Start(ProjectileTrailSegment program) =>
        ProjectileTrailSourceDefinitions.Starts.ContainsKey(program)
            ? ProjectileTrailSourceDefinitions.Starts[program]
            : throw new ArgumentOutOfRangeException(nameof(program), program, "Undefined trail program.");

    /// <summary>
    /// The bank-$90 address of beat <paramref name="beat"/> of <paramref name="program"/>; at the beat count, the program's
    /// end word.
    /// </summary>
    private static ushort Address(ProjectileTrailSegment program, int beat)
    {
        IReadOnlyList<AnimationBeat<ProjectileTrailFrame, ProjectileTrailCue>> beats = ProjectileTrailSparkleAnimations.PlayedBeats(program);
        if (beat < 0 || beat > beats.Count)
            throw new ArgumentOutOfRangeException(nameof(beat), beat, $"Trail program {program} has no beat there.");
        int address = Start(program);
        for (int before = 0; before < beat; before++)
            address += beats[before] is AnimationBeat<ProjectileTrailFrame, ProjectileTrailCue>.Frame ? 4 : 2;
        return checked((ushort)address);
    }

    /// <summary>
    /// Every timed record the trail programs store, in address order, with the frame it shows: a frame shown again is shown
    /// from every record that stores it.
    /// </summary>
    internal static IReadOnlyList<KeyValuePair<ushort, ProjectileTrailFrame>> Records { get; } = ReadRecords();

    /// <summary>Finds every timed record the programs store, from each program's frame beats, in address order.</summary>
    private static List<KeyValuePair<ushort, ProjectileTrailFrame>> ReadRecords()
    {
        var records = new List<KeyValuePair<ushort, ProjectileTrailFrame>>();
        foreach (ProjectileTrailSegment program in Enum.GetValues<ProjectileTrailSegment>())
        {
            IReadOnlyList<AnimationBeat<ProjectileTrailFrame, ProjectileTrailCue>> beats = ProjectileTrailSparkleAnimations.PlayedBeats(program);
            for (int beat = 0; beat < beats.Count; beat++)
            {
                if (beats[beat] is AnimationBeat<ProjectileTrailFrame, ProjectileTrailCue>.Frame frame)
                    records.Add(KeyValuePair.Create(Address(program, beat), frame.Shown));
            }
        }
        return [.. records.OrderBy(record => record.Key)];
    }
}
