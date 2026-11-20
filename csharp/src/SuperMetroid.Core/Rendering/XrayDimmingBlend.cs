using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Rendering;

/// <summary>
/// The X-ray's dimming of the room outside its cone (#1514): the room's dimming (<see cref="XrayRoomBlendMode"/>) toward
/// its liquid or fog lying behind it, or toward <see cref="Tint"/> where nothing lies behind, brightening or darkening as
/// the room's own liquid does.
/// </summary>
/// <param name="Mode">How the room dims outside the cone.</param>
/// <param name="RoomDarkens">Whether the room's own effect darkens what it blends, so that the dimming darkens.</param>
/// <param name="Tint">The colour the room dims toward where nothing lies behind it: the X-ray's grey, or a darkness's shade.</param>
public sealed record XrayDimmingBlend(XrayRoomBlendMode Mode, bool RoomDarkens, RgbColor Tint) : GameplayBlend;
