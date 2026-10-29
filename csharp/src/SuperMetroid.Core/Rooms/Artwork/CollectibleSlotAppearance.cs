using SuperMetroid.Core.Rooms.Plms;

namespace SuperMetroid.Core.Rooms.Artwork;

/// <summary>
/// The collectible one of the collectible graphics slots shows (#1466): the slot's blocks, and anything else drawn over
/// what the slot shows, have a picture for each collectible that can show there, and for the room's load where a block
/// shows it until a collectible does.
/// </summary>
/// <param name="Slot">The slot, among those instruction <c>$84:8764</c> rotates through.</param>
public sealed record CollectibleSlotAppearance(int Slot) : AppearanceInput
{
    /// <summary>The slot.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The slot is not one collectibles rotate through.</exception>
    public int Slot { get; } = Ensure.BetweenInclusive(0, CollectiblePlms.GraphicsSlots - 1, Slot);

    /// <inheritdoc />
    public override string Token => $"{AppearanceInputTokens.CollectibleSlotPrefix}{Slot}";

    /// <summary>How a picture's name spells <paramref name="kind"/> showing in a slot.</summary>
    public static string ValueOf(InWorldCollectibleKind kind) => kind.ToString();

    /// <inheritdoc />
    internal override string ValueIn(RoomAppearanceView appearance) =>
        appearance.Room.CollectibleIn(Slot) is { } kind ? ValueOf(kind) : RoomAppearanceValues.Loaded;
}
