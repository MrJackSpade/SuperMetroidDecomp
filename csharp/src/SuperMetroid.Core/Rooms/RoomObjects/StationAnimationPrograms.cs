using static SuperMetroid.Core.Rooms.RoomObjects.StationAnimationProgramDefinitions;

namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>Reads the stations' animations (<see cref="StationAnimationProgramDefinitions.Frames"/>) by image.</summary>
internal static class StationAnimationPrograms
{
    /// <summary>How many images <paramref name="list"/> shows: three for the map and resource cycles, one for each save image.</summary>
    internal static int FrameCount(StationAnimationList list) => Animation(list).Count;

    /// <summary>Image <paramref name="frameIndex"/> of <paramref name="list"/>; any other index is rejected.</summary>
    /// <exception cref="InvalidDataException">The animation has no such image.</exception>
    internal static Frame Resolve(StationAnimationList list, int frameIndex)
    {
        IReadOnlyList<Frame> frames = Animation(list);
        if ((uint)frameIndex >= (uint)frames.Count)
            throw new InvalidDataException($"Station animation {list} frame {frameIndex} is not compiled.");
        return frames[frameIndex];
    }

    /// <summary>The images of <paramref name="list"/>.</summary>
    private static IReadOnlyList<Frame> Animation(StationAnimationList list) =>
        Frames.TryGetValue(list, out IReadOnlyList<Frame>? frames)
            ? frames
            : throw new InvalidOperationException($"Undefined StationAnimationList {list}.");
}
