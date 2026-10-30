using SuperMetroid.Core.Areas.Ceres;

namespace SuperMetroid.Core.Bosses.Ridley.Artwork;

/// <summary>
/// The floor updates each of the getaway's floor animation frames queues, as the cartridge's floor update lists describe
/// them (#1415, #1467): each run's tile source, tile count and first floor cell. Nothing draws from them: the floor shows the
/// frame's installed picture. They are the descriptors that outlive the update in the floor update residue, which Samus's
/// over-reading delay programs then read. They are read from the player's cartridge at install, so Core holds no cartridge
/// address or floor cell.
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

    /// <summary>Each capsule frame's runs' updates, by frame then run.</summary>
    private readonly FloorUpdate[][] capsule;

    /// <summary>Each wing frame's runs' updates, by frame then run.</summary>
    private readonly FloorUpdate[][] wings;

    /// <summary>Keeps validated updates.</summary>
    private CeresRidleyFloorUpdateSources(FloorUpdate[][] capsule, FloorUpdate[][] wings)
    {
        this.capsule = capsule;
        this.wings = wings;
    }

    /// <summary>Validates the installed updates: three capsule frames of two runs and two wing frames of six.</summary>
    /// <param name="capsule">The capsule frames' run updates, by frame then run.</param>
    /// <param name="wings">The wing frames' run updates, by frame then run.</param>
    /// <exception cref="InvalidDataException">A frame or run is missing or extra.</exception>
    public static CeresRidleyFloorUpdateSources Load(IReadOnlyList<IReadOnlyList<FloorUpdate>> capsule, IReadOnlyList<IReadOnlyList<FloorUpdate>> wings)
    {
        Ensure.NotNull(capsule);
        Ensure.NotNull(wings);
        return new CeresRidleyFloorUpdateSources(Validated(capsule, CapsuleFrameCount, CapsuleRunCount, "capsule"),
            Validated(wings, WingFrameCount, WingRunCount, "wing"));
    }

    /// <summary>The floor updates <paramref name="frame"/>'s runs queue, in the order queued.</summary>
    /// <exception cref="InvalidDataException"><paramref name="frame"/> is not a frame the getaway draws.</exception>
    public IReadOnlyList<FloorUpdate> Updates(CeresFloorAnimationFrame frame) => frame switch
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

    /// <summary>Copies <paramref name="frames"/>, requiring <paramref name="frameCount"/> frames of <paramref name="runCount"/> updates.</summary>
    private static FloorUpdate[][] Validated(IReadOnlyList<IReadOnlyList<FloorUpdate>> frames, int frameCount, int runCount, string name)
    {
        if (frames.Count != frameCount)
            throw new InvalidDataException($"Ceres Ridley floor updates need {frameCount} {name} frames; found {frames.Count}.");
        var copy = new FloorUpdate[frameCount][];
        for (int frame = 0; frame < frameCount; frame++)
        {
            IReadOnlyList<FloorUpdate> runs = frames[frame] ?? throw new InvalidDataException($"Ceres Ridley {name} frame {frame} has no runs.");
            if (runs.Count != runCount)
                throw new InvalidDataException($"Ceres Ridley {name} frame {frame} needs {runCount} runs; found {runs.Count}.");
            copy[frame] = [.. runs];
        }
        return copy;
    }
}
