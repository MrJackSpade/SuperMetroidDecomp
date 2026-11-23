using SuperMetroid.Core.Enemies.EffectActors;
using static SuperMetroid.Core.Bosses.Draygon.DraygonIntroDanceDefinitions;

namespace SuperMetroid.Core.Bosses.Draygon;

/// <summary>
/// One Evir of Draygon's opening dance, spawned in the room's effect actors: the delay it waits before its first step and
/// the part of the traced path it has still to take.
/// </summary>
/// <param name="sprite">The effect actor the Evir was spawned into; the dance moves it while it is active.</param>
/// <param name="startDelay">How many dance updates the Evir waits before it takes the traced path's first step.</param>
internal sealed partial class DraygonIntroDancer(RoomEffectActor sprite, int startDelay)
{
    /// <summary>The moves of the traced path the Evir has not yet taken.</summary>
    private ReadOnlyMemory<Offset16<RoomPixels>> _untaken = TracedPath;

    /// <summary>The effect actor the Evir was spawned into.</summary>
    internal RoomEffectActor Sprite => sprite;

    /// <summary>How many dance updates the Evir waits before it takes the traced path's first step.</summary>
    internal int StartDelay => startDelay;

    /// <summary>
    /// Takes the Evir's next step: the path's next move, or, once it has taken every move, the delete that ends it, so no
    /// step lies past the path.
    /// </summary>
    internal void Step()
    {
        if (_untaken.IsEmpty)
        {
            sprite.End();
            return;
        }

        sprite.Position += _untaken.Span[0];
        _untaken = _untaken[1..];
    }
}
