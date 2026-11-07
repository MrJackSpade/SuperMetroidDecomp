using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.Core.Areas.Tourian;

/// <summary>One Tourian access-floor PLM header and its initial instruction list.</summary>
internal readonly record struct TourianAccessObjectDefinition(
    RoomObjectHeaderId Header,
    TourianAccessProgram Program);
