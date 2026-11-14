using System.Collections.Immutable;
using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.Core.Rooms.Artwork;

/// <summary>
/// One animation frame of the picture a collectible kind shows where it lies (#1538): every item but the four tanks, whose
/// pictures are blocks the room loads. A collectible shows its own kind's picture whatever was set up before it.
/// </summary>
/// <param name="Kind">The collectible kind, one that shows its own picture.</param>
/// <param name="Frame">The animation frame, zero or one.</param>
public readonly record struct CollectiblePicture(InWorldCollectibleKind Kind, int Frame)
{
    /// <summary>The animation frames each kind's picture has.</summary>
    public const int Frames = 2;

    /// <summary>Every frame of every kind that shows its own picture, kind by kind in kind order, each kind's frames in order.</summary>
    public static ImmutableArray<CollectiblePicture> All { get; } =
    [
        .. Enum.GetValues<InWorldCollectibleKind>().Where(CollectibleObjects.ShowsOwnPicture)
            .SelectMany(kind => Enumerable.Range(0, Frames).Select(frame => new CollectiblePicture(kind, frame))),
    ];

    /// <summary>The collectible kind.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The kind's picture is a block the room loads, not its own.</exception>
    public InWorldCollectibleKind Kind { get; } = CollectibleObjects.ShowsOwnPicture(Kind) ? Kind
        : throw new ArgumentOutOfRangeException(nameof(Kind), Kind, "The collectible's picture is a block the room loads, not its own.");

    /// <summary>The animation frame.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The frame is not zero or one.</exception>
    public int Frame { get; } = Ensure.BetweenInclusive(0, Frames - 1, Frame);
}
