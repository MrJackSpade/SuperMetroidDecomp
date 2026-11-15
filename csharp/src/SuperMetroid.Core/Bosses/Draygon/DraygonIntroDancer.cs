using SuperMetroid.Core.Enemies.EffectActors;

namespace SuperMetroid.Core.Bosses.Draygon;

/// <summary>One Evir of Draygon's opening dance, spawned in the room's effect actors, with the delay it waits before its first step.</summary>
/// <param name="Sprite">The effect actor the Evir was spawned into; the dance moves it while it is active.</param>
/// <param name="StartDelay">How many dance updates the Evir waits before it takes the traced path's first step.</param>
internal sealed record DraygonIntroDancer(RoomEffectActor Sprite, int StartDelay);
