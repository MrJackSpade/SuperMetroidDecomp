namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>The room-object kinds retail room populations place; every other kind is rejected from a population.</summary>
internal static class RoomObjectKinds
{
    /// <summary>Returns <paramref name="kind"/> when a retail room population places it, and rejects it otherwise.</summary>
    /// <exception cref="InvalidDataException">No retail room population places <paramref name="kind"/>.</exception>
    /// <exception cref="InvalidOperationException"><paramref name="kind"/> names no room object.</exception>
    internal static RoomObjectKind RequirePlaced(RoomObjectKind kind) =>
        !Enum.IsDefined(kind) ? throw new InvalidOperationException($"Undefined room-object kind {kind}.")
        : RoomObjectKindDefinitions.Placed.Contains(kind)
            ? kind
            : throw new InvalidDataException($"No retail room population places room-object kind {kind}.");

    /// <summary>The room object that breaks up <paramref name="crumble"/> once Samus lands on it.</summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="crumble"/> names no crumble block.</exception>
    internal static RoomObjectKind ContactCrumbleOf(ContactCrumbleBlock crumble) => crumble switch
    {
        ContactCrumbleBlock.Respawning1x1 => RoomObjectKind.ContactCrumble1x1Respawning,
        ContactCrumbleBlock.Respawning2x1 => RoomObjectKind.ContactCrumble2x1Respawning,
        ContactCrumbleBlock.Respawning1x2 => RoomObjectKind.ContactCrumble1x2Respawning,
        ContactCrumbleBlock.Respawning2x2 => RoomObjectKind.ContactCrumble2x2Respawning,
        ContactCrumbleBlock.Permanent1x1 => RoomObjectKind.ContactCrumble1x1Permanent,
        ContactCrumbleBlock.Permanent2x1 => RoomObjectKind.ContactCrumble2x1Permanent,
        ContactCrumbleBlock.Permanent1x2 => RoomObjectKind.ContactCrumble1x2Permanent,
        ContactCrumbleBlock.Permanent2x2 => RoomObjectKind.ContactCrumble2x2Permanent,
        _ => throw new ArgumentOutOfRangeException(nameof(crumble), crumble, "Undefined contact crumble block."),
    };
}
