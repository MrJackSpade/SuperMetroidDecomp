using SuperMetroid.Core.Rooms.Artwork;

namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>
/// One image of a collectible's animations (<see cref="CollectibleSequenceAnimations"/>): a draw at the item's block, or
/// one of the item's own two visible looks (#2480), which the item's kind and block pair draw.
/// </summary>
internal readonly record struct CollectibleImage
{
    /// <summary>The image that draws <paramref name="draw"/>, or, with none, shows <paramref name="look"/>.</summary>
    private CollectibleImage(RoomObjectDraw? draw, CollectibleLook look)
    {
        this.draw = draw;
        this.look = look;
    }

    /// <summary>The draw the image draws; none for an image of one of the item's looks.</summary>
    private readonly RoomObjectDraw? draw;

    /// <summary>The look the image shows, for an image that draws no draw of its own.</summary>
    private readonly CollectibleLook look;

    /// <summary>The image that draws <paramref name="draw"/>.</summary>
    internal static CollectibleImage Drawn(RoomObjectDraw draw) => new(Ensure.NotNull(draw), default);

    /// <summary>The image of the item's own <paramref name="look"/>.</summary>
    internal static CollectibleImage Visible(CollectibleLook look) => new(null, look);

    /// <summary>
    /// What <paramref name="drawn"/> makes of the image's draw, or for an image of one of the item's looks what
    /// <paramref name="visible"/> makes of that look.
    /// </summary>
    internal TResult Select<TResult>(Func<RoomObjectDraw, TResult> drawn, Func<CollectibleLook, TResult> visible) =>
        draw is { } shown ? Ensure.NotNull(drawn)(shown) : Ensure.NotNull(visible)(look);
}
