using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.Core.Rooms.Artwork;

/// <summary>
/// The collectible one of the collectible graphics slots shows (#1466, #1513): the blocks a collectible in the slot writes
/// where it lies have a picture of each collectible their tileset's rooms place, drawn from the collectible's own graphics,
/// and of the room's load where a block shows it until a collectible does. Nothing else depends on the slot: no other
/// block or background cell is pictured over a collectible.
/// </summary>
/// <param name="Slot">The slot, among those the collectible-rotation animation step rotates through.</param>
public sealed record CollectibleSlotAppearance(int Slot) : AppearanceInput
{
    /// <summary>The slot.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The slot is not one collectibles rotate through.</exception>
    public int Slot { get; } = Ensure.BetweenInclusive(0, CollectibleObjects.GraphicsSlots - 1, Slot);

    /// <inheritdoc />
    public override string Token { get; } = $"{AppearanceInputTokens.CollectibleSlotPrefix}{Slot}";

    /// <summary>How a picture's name spells <paramref name="kind"/> showing in a slot.</summary>
    public static string ValueOf(InWorldCollectibleKind kind) => kind.ToString();

    /// <inheritdoc />
    internal override string ValueIn(RoomAppearanceView appearance) =>
        appearance.Room.CollectibleIn(Slot) is { } kind ? ValueOf(kind) : RoomAppearanceValues.Loaded;
}
