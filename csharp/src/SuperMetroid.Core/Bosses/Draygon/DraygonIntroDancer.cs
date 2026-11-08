using SuperMetroid.Core.Enemies.SpriteObjects;

namespace SuperMetroid.Core.Bosses.Draygon;

/// <summary>One Evir of Draygon's opening dance, spawned in the room's sprite objects, with the delay it waits before its first step.</summary>
/// <param name="Sprite">The sprite object the Evir was spawned into; the dance moves it while it is active.</param>
/// <param name="StartDelay">How many dance updates the Evir waits before it takes the traced path's first step.</param>
internal sealed record DraygonIntroDancer(RoomSpriteObjectSlot Sprite, int StartDelay);
