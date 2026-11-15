using SuperMetroid.Core.Rooms.Artwork;

namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>The draw a collectible's block shows in each of its two alternating looks (#1994).</summary>
/// <param name="Primary">The draw of the look it shows first.</param>
/// <param name="Alternate">The draw of the look it changes to.</param>
internal readonly record struct CollectibleLookDraws(CollectibleDraw Primary, CollectibleDraw Alternate)
{
    /// <summary>The draw of <paramref name="look"/>.</summary>
    /// <exception cref="InvalidOperationException">The look is undefined.</exception>
    internal CollectibleDraw Of(CollectibleLook look) => look switch
    {
        CollectibleLook.Primary => Primary,
        CollectibleLook.Alternate => Alternate,
        _ => throw new InvalidOperationException($"Undefined {nameof(CollectibleLook)} {look}."),
    };
}
