using SuperMetroid.Core.Areas.Ceres;

namespace SuperMetroid.Core.Bosses.Ridley.Artwork;

/// <summary>
/// Where each of the getaway's floor runs takes its tile numbers from, as the floor updates it queues name them
/// (#1415). The value is not drawn: it is the source the update's descriptor records, which outlives the update in the
/// floor update residue and which Samus's over-reading delay programs then read. It is read from the player's
/// cartridge at install, so Core holds no cartridge address.
/// </summary>
public sealed class CeresRidleyFloorUpdateSources
{
    /// <summary>The Baby capsule's three frames, each two runs.</summary>
    public const int CapsuleFrameCount = 3;

    /// <summary>The runs of one capsule frame.</summary>
    public const int CapsuleRunCount = 2;

    /// <summary>Ridley's wings' two frames, each six runs.</summary>
    public const int WingFrameCount = 2;

    /// <summary>The runs of one wing frame.</summary>
    public const int WingRunCount = 6;

    /// <summary>Each capsule frame's runs' sources, by frame then run.</summary>
    private readonly int[][] capsule;

    /// <summary>Each wing frame's runs' sources, by frame then run.</summary>
    private readonly int[][] wings;

    /// <summary>Keeps validated sources.</summary>
    private CeresRidleyFloorUpdateSources(int[][] capsule, int[][] wings)
    {
        this.capsule = capsule;
        this.wings = wings;
    }

    /// <summary>Validates the installed sources: three capsule frames of two runs, two wing frames of six, each 0 through 65535.</summary>
    /// <param name="capsule">The capsule frames' run sources, by frame then run.</param>
    /// <param name="wings">The wing frames' run sources, by frame then run.</param>
    /// <exception cref="InvalidDataException">A frame or run is missing or extra, or a source is outside 0 through 65535.</exception>
    public static CeresRidleyFloorUpdateSources Load(IReadOnlyList<IReadOnlyList<int>> capsule, IReadOnlyList<IReadOnlyList<int>> wings)
    {
        Ensure.NotNull(capsule);
        Ensure.NotNull(wings);
        return new CeresRidleyFloorUpdateSources(Validated(capsule, CapsuleFrameCount, CapsuleRunCount, "capsule"),
            Validated(wings, WingFrameCount, WingRunCount, "wing"));
    }

    /// <summary>Where run <paramref name="run"/> of <paramref name="frame"/> takes its tile numbers from.</summary>
    /// <exception cref="InvalidDataException"><paramref name="frame"/> is not a frame the getaway draws.</exception>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="run"/> is not one of the frame's runs.</exception>
    public int TileSource(CeresFloorAnimationFrame frame, int run)
    {
        int[] runs = frame switch
        {
            CeresFloorAnimationFrame.BabyFrame0 => capsule[0],
            CeresFloorAnimationFrame.BabyFrame1 => capsule[1],
            CeresFloorAnimationFrame.BabyFrame2 => capsule[2],
            CeresFloorAnimationFrame.WingFrame0 => wings[0],
            CeresFloorAnimationFrame.WingFrame1 => wings[1],
            CeresFloorAnimationFrame.ElevatorLight or CeresFloorAnimationFrame.ElevatorDark =>
                throw new InvalidDataException($"The getaway draws no {frame} floor update."),
            _ => throw new InvalidDataException($"Unknown Ceres floor animation frame {frame}."),
        };
        return (uint)run < (uint)runs.Length ? runs[run] : throw new ArgumentOutOfRangeException(nameof(run));
    }

    /// <summary>Copies <paramref name="frames"/>, requiring <paramref name="frameCount"/> frames of <paramref name="runCount"/> sixteen-bit sources.</summary>
    private static int[][] Validated(IReadOnlyList<IReadOnlyList<int>> frames, int frameCount, int runCount, string name)
    {
        if (frames.Count != frameCount)
            throw new InvalidDataException($"Ceres Ridley floor updates need {frameCount} {name} frames; found {frames.Count}.");
        var copy = new int[frameCount][];
        for (int frame = 0; frame < frameCount; frame++)
        {
            IReadOnlyList<int> runs = frames[frame] ?? throw new InvalidDataException($"Ceres Ridley {name} frame {frame} has no runs.");
            if (runs.Count != runCount)
                throw new InvalidDataException($"Ceres Ridley {name} frame {frame} needs {runCount} runs; found {runs.Count}.");
            copy[frame] = new int[runCount];
            for (int run = 0; run < runCount; run++)
                copy[frame][run] = runs[run] is >= 0 and <= ushort.MaxValue ? runs[run]
                    : throw new InvalidDataException($"Ceres Ridley {name} frame {frame} run {run} source {runs[run]} is outside 0 through 65535.");
        }
        return copy;
    }
}
