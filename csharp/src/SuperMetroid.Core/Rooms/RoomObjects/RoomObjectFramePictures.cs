using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>
/// One room-object family's installed frame pictures (#1557): for every frame the family draws, keyed by the frame, the
/// pre-coloured block picture each of its cells shows, in run then cell order, exactly as installed. The import pictures
/// every frame at install, and the game shows what is installed: Core keeps no picture of its own and
/// nothing stands in for a frame the installation lacks. A frame's terrain, the collision and block behaviour it writes
/// into the level, stays the game's; only what each cell shows comes from here.
/// </summary>
internal sealed class RoomObjectFramePictures
{
    /// <summary>The family, naming it when a frame is missing.</summary>
    private readonly string family;

    /// <summary>Each installed frame's cell pictures in run then cell order, owned by this set.</summary>
    private readonly Dictionary<RoomObjectDraw, RoomBlockAppearance[]> frames;

    /// <summary>Holds a family's installed frames, copying each frame's cells.</summary>
    /// <param name="family">The family, naming it when a frame is missing.</param>
    /// <param name="frames">Every installed frame's cell pictures in run then cell order.</param>
    /// <exception cref="ArgumentNullException">The frames or one frame's cells are null.</exception>
    internal RoomObjectFramePictures(string family, IReadOnlyDictionary<RoomObjectDraw, RoomBlockAppearance[]> frames)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(family);
        this.family = family;
        this.frames = Ensure.NotNull(frames).ToDictionary(frame => frame.Key, frame => Ensure.NotNull(frame.Value).ToArray());
    }

    /// <summary>The picture cell <paramref name="cell"/> of <paramref name="frame"/> shows, counted in run then cell order.</summary>
    /// <exception cref="InvalidDataException">The installation holds no such frame.</exception>
    /// <exception cref="ArgumentOutOfRangeException">The frame has no such cell.</exception>
    internal RoomBlockAppearance At(RoomObjectDraw frame, int cell)
    {
        if (!frames.TryGetValue(frame, out RoomBlockAppearance[]? cells))
            throw new InvalidDataException($"The installed {family} visuals hold no frame {frame}.");
        return (uint)cell < (uint)cells.Length ? cells[cell] : throw new ArgumentOutOfRangeException(nameof(cell));
    }
}
