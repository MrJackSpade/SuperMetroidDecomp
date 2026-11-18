using System.Collections.Immutable;
using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.Core.Rooms.Artwork;

/// <summary>
/// One look of the picture a collectible kind shows where it lies (#1538, #1994): every item but the four tanks, whose
/// pictures are blocks the room loads. A collectible shows its own kind's picture whatever was set up before it.
/// </summary>
/// <param name="Kind">The collectible kind, one that shows its own picture.</param>
/// <param name="Look">Which of its two alternating looks the picture shows.</param>
public readonly record struct CollectiblePicture(InWorldCollectibleKind Kind, CollectibleLook Look)
{
    /// <summary>Every look of every kind that shows its own picture, kind by kind in kind order, each kind's looks in order.</summary>
    public static ImmutableArray<CollectiblePicture> All { get; } =
    [
        .. Enum.GetValues<InWorldCollectibleKind>().Where(kind => CollectibleDefinitions.Items[kind].Picture == CollectiblePictureSource.OwnPicture)
            .SelectMany(kind => Enum.GetValues<CollectibleLook>().Select(look => new CollectiblePicture(kind, look))),
    ];

    /// <summary>The collectible kind.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The kind's picture is a block the room loads, not its own.</exception>
    public InWorldCollectibleKind Kind { get; } = CollectibleDefinitions.Items[Kind].Picture == CollectiblePictureSource.OwnPicture ? Kind
        : throw new ArgumentOutOfRangeException(nameof(Kind), Kind, "The collectible's picture is a block the room loads, not its own.");

    /// <summary>The look.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The look is undefined.</exception>
    public CollectibleLook Look { get; } = Enum.IsDefined(Look) ? Look
        : throw new ArgumentOutOfRangeException(nameof(Look), Look, "Undefined collectible look.");
}
