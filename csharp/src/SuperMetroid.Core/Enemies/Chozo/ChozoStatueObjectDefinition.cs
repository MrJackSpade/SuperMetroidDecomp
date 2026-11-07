using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.Core.Enemies.Chozo;

/// <summary>One Chozo terrain PLM header paired with its first program.</summary>
internal readonly record struct ChozoStatueObjectDefinition(
    RoomObjectHeaderId Header,
    RoomObjectProgramStart Program);
