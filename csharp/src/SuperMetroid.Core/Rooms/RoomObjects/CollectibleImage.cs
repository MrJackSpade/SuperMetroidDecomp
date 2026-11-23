using SuperMetroid.Core.Rooms.Artwork;

namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>
/// One image of a collectible's animations (<see cref="CollectibleSequenceAnimations"/>, #2560): a draw at the item's block,
/// or one of the item's own two visible looks (#2480), which the item's kind and block pair draw. What it shows is one
/// closed choice (<see cref="Shown"/>); the image is a value, as an animation's frames are.
/// </summary>
/// <param name="What">What the image shows.</param>
internal readonly partial record struct CollectibleImage(CollectibleImage.Shown What)
{
    /// <summary>The image that draws <paramref name="draw"/>.</summary>
    internal static CollectibleImage Drawn(RoomObjectDraw draw) => new(new DrawnImage(Ensure.NotNull(draw)));

    /// <summary>The image of the item's own <paramref name="look"/>.</summary>
    internal static CollectibleImage Visible(CollectibleLook look) => new(new VisibleLook(look));

    /// <summary>What a collectible's image shows: a draw, or one of the item's own looks.</summary>
    internal abstract partial record Shown
    {
        /// <summary>Only the two cases below are shown.</summary>
        private protected Shown()
        {
        }
    }

    /// <summary>The image draws <paramref name="Draw"/> at the item's block.</summary>
    /// <param name="Draw">What the image draws.</param>
    internal sealed partial record DrawnImage(RoomObjectDraw Draw) : Shown;

    /// <summary>The image shows the item's own <paramref name="Look"/>.</summary>
    /// <param name="Look">The look its kind and block pair draw.</param>
    internal sealed partial record VisibleLook(CollectibleLook Look) : Shown;
}
