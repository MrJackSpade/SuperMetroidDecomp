namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>The room objects retail room populations place; every other room object is rejected.</summary>
internal static class RoomObjectHeaders
{
    /// <summary>Selects a retail definition or rejects a room object absent from room populations.</summary>
    /// <exception cref="InvalidDataException">No retail room population places <paramref name="header"/>.</exception>
    /// <exception cref="InvalidOperationException"><paramref name="header"/> names no room object.</exception>
    internal static RoomObjectHeaderDefinition Get(RoomObjectHeaderId header) =>
        !Enum.IsDefined(header) ? throw new InvalidOperationException($"Undefined RoomObjectHeaderId {header}.")
        : RoomObjectHeaderDefinitions.RetailHeaders.Contains(header)
            ? new(header)
            : throw new InvalidDataException($"Compiled room-object headers lack retail header {header}.");

    /// <summary>The room object that breaks up <paramref name="crumble"/> once Samus lands on it.</summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="crumble"/> names no crumble block.</exception>
    internal static RoomObjectHeaderId ContactCrumbleOf(ContactCrumbleBlock crumble) => crumble switch
    {
        ContactCrumbleBlock.Respawning1x1 => RoomObjectHeaderId.ContactCrumble1x1Respawning,
        ContactCrumbleBlock.Respawning2x1 => RoomObjectHeaderId.ContactCrumble2x1Respawning,
        ContactCrumbleBlock.Respawning1x2 => RoomObjectHeaderId.ContactCrumble1x2Respawning,
        ContactCrumbleBlock.Respawning2x2 => RoomObjectHeaderId.ContactCrumble2x2Respawning,
        ContactCrumbleBlock.Permanent1x1 => RoomObjectHeaderId.ContactCrumble1x1Permanent,
        ContactCrumbleBlock.Permanent2x1 => RoomObjectHeaderId.ContactCrumble2x1Permanent,
        ContactCrumbleBlock.Permanent1x2 => RoomObjectHeaderId.ContactCrumble1x2Permanent,
        ContactCrumbleBlock.Permanent2x2 => RoomObjectHeaderId.ContactCrumble2x2Permanent,
        _ => throw new ArgumentOutOfRangeException(nameof(crumble), crumble, "Undefined contact crumble block."),
    };
}
