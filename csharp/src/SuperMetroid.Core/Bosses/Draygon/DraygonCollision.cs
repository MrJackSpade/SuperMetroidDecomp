using SuperMetroid.Core.Enemies;
using System.Collections.Immutable;
using SuperMetroid.Core.Enemies.Artwork;
using static SuperMetroid.Core.Bosses.Draygon.DraygonCollisionDefinitions;

namespace SuperMetroid.Core.Bosses.Draygon;

/// <summary>Finds the collision components of the Draygon frame shown.</summary>
internal static partial class DraygonCollision
{
    /// <summary>The collision components of Draygon frame <paramref name="frame"/>.</summary>
    /// <exception cref="InvalidDataException"><paramref name="frame"/> is not one of Draygon's frames.</exception>
    internal static ImmutableArray<EnemyCollisionComponent> ComponentsAt(EnemyFrameId? frame) =>
        frame is not { } shown ? []
            : Components.TryGetValue(shown, out ImmutableArray<EnemyCollisionComponent> components) ? components
            : throw new InvalidDataException($"Draygon frame {frame} has no collision identity.");

    /// <summary>
    /// Each frame's components, checked once (#2553) to name only the touches and shots Draygon answers: the ignoring touch
    /// or his own, the dud shot or his own.
    /// </summary>
    private static Dictionary<EnemyFrameId, ImmutableArray<EnemyCollisionComponent>> Components { get; } = EnemyHitboxReactionCheck.Checked(
        FrameComponents,
        FrameComponents.Values.SelectMany(components => components).SelectMany(component => component.Hitboxes),
        "Draygon",
        [EnemyReaction.Ignore, DraygonReactions.Touch],
        [EnemyReaction.DudShot, DraygonReactions.Shot]);
}
